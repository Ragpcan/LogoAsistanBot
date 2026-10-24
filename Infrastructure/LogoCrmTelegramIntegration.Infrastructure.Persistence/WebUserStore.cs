using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Persistence;

public class WebUserStore : IWebUserStore
{
	private readonly string _filePath;

	private readonly object _lock = new object();

	private readonly Dictionary<string, WebUserInternalModel> _users = new Dictionary<string, WebUserInternalModel>(StringComparer.OrdinalIgnoreCase);

	private readonly ILogger<WebUserStore> _logger;

	public WebUserStore(ILogger<WebUserStore> logger)
	{
		_logger = logger;
		_filePath = Path.Combine(AppContext.BaseDirectory, "web_users.json");
		LoadFromFile();
	}

	private void LoadFromFile()
	{
		lock (_lock)
		{
			try
			{
				if (File.Exists(_filePath))
				{
					List<WebUserInternalModel> list = JsonSerializer.Deserialize<List<WebUserInternalModel>>(File.ReadAllText(_filePath));
					if (list != null && list.Count > 0)
					{
						foreach (WebUserInternalModel item in list)
						{
							_users[item.Username] = item;
						}
					}
				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Web kullanıcıları JSON dosyasından okunurken hata oluştu.");
			}
			EnsureDefaultUsers();
		}
	}

	private void EnsureDefaultUsers()
	{
		if (!_users.ContainsKey("admin"))
		{
			_users["admin"] = new WebUserInternalModel
			{
				Username = "admin",
				FullName = "Sistem Yöneticisi (Admin)",
				PasswordHash = HashPassword("1001."),
				Role = "Admin",
				CreatedAt = DateTime.Now,
				CanAccessDashboard = true,
				CanAccessSalesmen = true,
				CanAccessLiveTransactions = true,
				CanAccessXslt = true,
				CanAccessAdminSettings = true
			};
		}
		if (!_users.ContainsKey("ragip"))
		{
			_users["ragip"] = new WebUserInternalModel
			{
				Username = "ragip",
				FullName = "Ragıp (Admin)",
				PasswordHash = HashPassword("1001."),
				Role = "Admin",
				CreatedAt = DateTime.Now,
				CanAccessDashboard = true,
				CanAccessSalesmen = true,
				CanAccessLiveTransactions = true,
				CanAccessXslt = true,
				CanAccessAdminSettings = true
			};
		}
		SaveToFile();
	}

	private void SaveToFile()
	{
		try
		{
			string contents = JsonSerializer.Serialize(_users.Values.ToList(), new JsonSerializerOptions
			{
				WriteIndented = true
			});
			File.WriteAllText(_filePath, contents);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Web kullanıcıları dosyaya kaydedilirken hata oluştu.");
		}
	}

	public LoginResponse ValidateUser(string username, string password)
	{
		lock (_lock)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				return new LoginResponse
				{
					IsSuccess = false,
					Message = "Kullanıcı adı ve şifre boş bırakılamaz."
				};
			}
			if (_users.TryGetValue(username.Trim(), out WebUserInternalModel value))
			{
				bool isValid = VerifyPassword(password, value.PasswordHash);
				if (!isValid && string.Equals(password, value.PasswordHash, StringComparison.Ordinal))
				{
					isValid = true;
					value.PasswordHash = HashPassword(password);
					SaveToFile();
				}

				if (isValid)
				{
					return new LoginResponse
					{
						IsSuccess = true,
						Message = "Giriş başarılı!",
						User = MapToDto(value)
					};
				}
			}
			return new LoginResponse
			{
				IsSuccess = false,
				Message = "Kullanıcı adı veya şifre hatalı."
			};
		}
	}

	public List<WebUserDto> GetAllUsers()
	{
		lock (_lock)
		{
			return _users.Values.Select(MapToDto).ToList();
		}
	}

	public WebUserDto? GetUser(string username)
	{
		lock (_lock)
		{
			if (_users.TryGetValue(username, out WebUserInternalModel value))
			{
				return MapToDto(value);
			}
			return null;
		}
	}

	public bool SaveOrUpdateUser(SaveWebUserRequest req, out string errorMessage)
	{
		errorMessage = string.Empty;
		if (req == null || string.IsNullOrWhiteSpace(req.Username))
		{
			errorMessage = "Kullanıcı adı boş olamaz.";
			return false;
		}
		string text = req.Username.Trim().ToLowerInvariant();
		lock (_lock)
		{
			if (_users.TryGetValue(text, out WebUserInternalModel value))
			{
				value.FullName = (string.IsNullOrWhiteSpace(req.FullName) ? value.FullName : req.FullName.Trim());
				value.Role = (string.IsNullOrWhiteSpace(req.Role) ? value.Role : req.Role);
				value.CanAccessDashboard = req.CanAccessDashboard;
				value.CanAccessSalesmen = req.CanAccessSalesmen;
				value.CanAccessLiveTransactions = req.CanAccessLiveTransactions;
				value.CanAccessXslt = req.CanAccessXslt;
				value.CanAccessAdminSettings = req.CanAccessAdminSettings;
				if (!string.IsNullOrWhiteSpace(req.Password))
				{
					value.PasswordHash = HashPassword(req.Password);
				}
			}
			else
			{
				if (string.IsNullOrWhiteSpace(req.Password))
				{
					errorMessage = "Yeni kullanıcı için şifre alanı zorunludur.";
					return false;
				}
				WebUserInternalModel value2 = new WebUserInternalModel
				{
					Username = text,
					FullName = (string.IsNullOrWhiteSpace(req.FullName) ? text : req.FullName.Trim()),
					PasswordHash = HashPassword(req.Password),
					Role = (string.IsNullOrWhiteSpace(req.Role) ? "TelegramOperator" : req.Role),
					CreatedAt = DateTime.Now,
					CanAccessDashboard = req.CanAccessDashboard,
					CanAccessSalesmen = req.CanAccessSalesmen,
					CanAccessLiveTransactions = req.CanAccessLiveTransactions,
					CanAccessXslt = req.CanAccessXslt,
					CanAccessAdminSettings = req.CanAccessAdminSettings
				};
				_users[text] = value2;
			}
			SaveToFile();
			return true;
		}
	}

	public bool DeleteUser(string username, out string errorMessage)
	{
		errorMessage = string.Empty;
		if (string.IsNullOrWhiteSpace(username))
		{
			errorMessage = "Kullanıcı adı boş olamaz.";
			return false;
		}
		string text = username.Trim().ToLowerInvariant();
		if (text == "admin")
		{
			errorMessage = "Ana 'admin' kullanıcısı silinemez.";
			return false;
		}
		lock (_lock)
		{
			if (_users.Remove(text))
			{
				SaveToFile();
				return true;
			}
			errorMessage = "Kullanıcı bulunamadı.";
			return false;
		}
	}

	private static WebUserDto MapToDto(WebUserInternalModel u)
	{
		return new WebUserDto
		{
			Username = u.Username,
			FullName = u.FullName,
			Role = u.Role,
			CreatedAt = u.CreatedAt,
			CanAccessDashboard = u.CanAccessDashboard,
			CanAccessSalesmen = u.CanAccessSalesmen,
			CanAccessLiveTransactions = u.CanAccessLiveTransactions,
			CanAccessXslt = u.CanAccessXslt,
			CanAccessAdminSettings = u.CanAccessAdminSettings
		};
	}

	private static string HashPassword(string password)
	{
		using SHA256 sHA = SHA256.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(password);
		return Convert.ToBase64String(sHA.ComputeHash(bytes));
	}

	private static bool VerifyPassword(string inputPassword, string storedHash)
	{
		return string.Equals(HashPassword(inputPassword), storedHash, StringComparison.Ordinal);
	}
}
