using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Persistence;

public class TelegramUserStore : ITelegramUserStore
{
	private readonly string _filePath;

	private readonly object _lock = new object();

	private readonly Dictionary<long, TelegramUserMappingDto> _users = new Dictionary<long, TelegramUserMappingDto>();

	private readonly ILogger<TelegramUserStore> _logger;

	public TelegramUserStore(ILogger<TelegramUserStore> logger)
	{
		_logger = logger;
		string baseDirectory = AppContext.BaseDirectory;
		_filePath = Path.Combine(baseDirectory, "telegram_user_mappings.json");
		LoadFromFile();
	}

	private void LoadFromFile()
	{
		lock (_lock)
		{
			try
			{
				if (!File.Exists(_filePath))
				{
					return;
				}
				List<TelegramUserMappingDto> list = JsonSerializer.Deserialize<List<TelegramUserMappingDto>>(File.ReadAllText(_filePath));
				if (list != null)
				{
					foreach (TelegramUserMappingDto item in list)
					{
						if (string.IsNullOrWhiteSpace(item.Status))
						{
							item.Status = "Approved";
						}
						_users[item.ChatId] = item;
					}
				}
				_logger.LogInformation("{Count} adet Telegram kullanıcısı JSON dosyasından yüklendi.", _users.Count);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Telegram kullanıcı haritası dosyadan okunurken hata oluştu.");
			}
		}
	}

	private void SaveToFile()
	{
		lock (_lock)
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
				_logger.LogError(exception, "Telegram kullanıcı haritası dosyaya kaydedilirken hata oluştu.");
			}
		}
	}

	public void RegisterOrUpdateUser(long chatId, string username, string fullName)
	{
		lock (_lock)
		{
			if (_users.TryGetValue(chatId, out TelegramUserMappingDto value))
			{
				value.Username = (string.IsNullOrWhiteSpace(username) ? value.Username : username);
				value.FullName = (string.IsNullOrWhiteSpace(fullName) ? value.FullName : fullName);
				value.LastSeen = DateTime.Now;
				if (string.IsNullOrWhiteSpace(value.Status))
				{
					value.Status = "Approved";
				}
			}
			else
			{
				string status = ((_users.Count == 0) ? "Approved" : "Pending");
				_users[chatId] = new TelegramUserMappingDto
				{
					ChatId = chatId,
					Username = username,
					FullName = fullName,
					RegisteredAt = DateTime.Now,
					LastSeen = DateTime.Now,
					Status = status
				};
			}
			SaveToFile();
		}
	}

	public List<TelegramUserMappingDto> GetAllUsers()
	{
		lock (_lock)
		{
			return _users.Values.OrderByDescending((TelegramUserMappingDto u) => u.LastSeen).ToList();
		}
	}

	public TelegramUserMappingDto? GetUser(long chatId)
	{
		lock (_lock)
		{
			_users.TryGetValue(chatId, out TelegramUserMappingDto value);
			return value;
		}
	}

	public void MapUserToSalesman(long chatId, int? salesmanRef, string? salesmanCode, string? salesmanName)
	{
		lock (_lock)
		{
			if (_users.TryGetValue(chatId, out TelegramUserMappingDto value))
			{
				value.MappedSalesmanRef = salesmanRef;
				value.MappedSalesmanCode = salesmanCode;
				value.MappedSalesmanName = salesmanName;
				SaveToFile();
				_logger.LogInformation("Telegram kullanıcısı (ChatId: {ChatId}) Satış Temsilcisine eşleştirildi: {SalesmanName} (Ref: {Ref})", chatId, salesmanName, salesmanRef);
			}
		}
	}

	public void UpdateUserStatus(long chatId, string status)
	{
		lock (_lock)
		{
			if (_users.TryGetValue(chatId, out TelegramUserMappingDto value))
			{
				value.Status = status;
				SaveToFile();
				_logger.LogInformation("Telegram kullanıcısı (ChatId: {ChatId}) erişim durumu güncellendi: {Status}", chatId, status);
			}
		}
	}

	public bool UpdateUserPermissions(UpdateUserPermissionsRequest request)
	{
		lock (_lock)
		{
			if (_users.TryGetValue(request.ChatId, out TelegramUserMappingDto value))
			{
				value.CanViewAllFirms = request.CanViewAllFirms;
				value.AllowedFirmCyphCode = request.AllowedFirmCyphCode?.Trim() ?? string.Empty;
				value.CanViewAllItems = request.CanViewAllItems;
				value.AllowedItemCyphCode = request.AllowedItemCyphCode?.Trim() ?? string.Empty;
				value.CanViewAllOrders = request.CanViewAllOrders;
				value.AllowedOrderCyphCode = request.AllowedOrderCyphCode?.Trim() ?? string.Empty;
				value.CanCreateSalesOrder = request.CanCreateSalesOrder;
				value.CanViewSalesOrders = request.CanViewSalesOrders;
				value.CanCreateDispatch = request.CanCreateDispatch;
				value.CanViewSalesDispatches = request.CanViewSalesDispatches;
				value.CanCreateSalesInvoice = request.CanCreateSalesInvoice;
				value.CanViewSalesInvoices = request.CanViewSalesInvoices;
				value.CanViewPurchaseOrders = request.CanViewPurchaseOrders;
				value.CanViewPurchaseDispatches = request.CanViewPurchaseDispatches;
				value.CanViewPurchaseInvoices = request.CanViewPurchaseInvoices;
				value.CanSendEBelge = request.CanSendEBelge;
				value.CanCreateCollection = request.CanCreateCollection;
				value.CanViewSalesmanTarget = request.CanViewSalesmanTarget;
				value.CanSendSms = request.CanSendSms;
				value.CanSendEmail = request.CanSendEmail;
				SaveToFile();
				_logger.LogInformation("Telegram kullanıcısı (ChatId: {ChatId}) yetki kısıtlamaları güncellendi.", request.ChatId);
				return true;
			}
			return false;
		}
	}
}
