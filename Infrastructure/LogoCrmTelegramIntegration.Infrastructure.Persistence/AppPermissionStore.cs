using System;
using System.IO;
using System.Text.Json;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Persistence;

public class AppPermissionStore : IAppPermissionStore
{
	private readonly string _filePath;

	private readonly object _lock = new object();

	private AppPermissionsDto _permissions;

	private readonly ILogger<AppPermissionStore> _logger;

	public AppPermissionStore(ILogger<AppPermissionStore> logger)
	{
		_logger = logger;
		_filePath = Path.Combine(AppContext.BaseDirectory, "app_permissions.json");
		_permissions = new AppPermissionsDto();
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
					AppPermissionsDto appPermissionsDto = JsonSerializer.Deserialize<AppPermissionsDto>(File.ReadAllText(_filePath));
					if (appPermissionsDto != null)
					{
						_permissions = appPermissionsDto;
					}
				}
				else
				{
					SaveToFile();
				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Uygulama yetkileri dosyasından yüklenirken hata oluştu.");
			}
		}
	}

	private void SaveToFile()
	{
		try
		{
			string contents = JsonSerializer.Serialize(_permissions, new JsonSerializerOptions
			{
				WriteIndented = true
			});
			File.WriteAllText(_filePath, contents);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Uygulama yetkileri dosyaya kaydedilirken hata oluştu.");
		}
	}

	public AppPermissionsDto GetPermissions()
	{
		lock (_lock)
		{
			return new AppPermissionsDto
			{
				EnableCariModule = _permissions.EnableCariModule,
				EnableSalesInvoice = _permissions.EnableSalesInvoice,
				EnableSalesDispatch = _permissions.EnableSalesDispatch,
				EnableSalesOrder = _permissions.EnableSalesOrder,
				EnablePurchaseInvoice = _permissions.EnablePurchaseInvoice,
				EnablePurchaseDispatch = _permissions.EnablePurchaseDispatch,
				EnablePurchaseOrder = _permissions.EnablePurchaseOrder,
				EnableStockModule = _permissions.EnableStockModule,
				EnableEBelgeSending = _permissions.EnableEBelgeSending
			};
		}
	}

	public void SavePermissions(AppPermissionsDto permissions)
	{
		lock (_lock)
		{
			_permissions = permissions ?? new AppPermissionsDto();
			SaveToFile();
			_logger.LogInformation("Uygulama modül yetkileri başarıyla güncellendi.");
		}
	}
}
