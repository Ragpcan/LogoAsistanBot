using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesmenController : ControllerBase
{
	private readonly ILogoErpRepository _logoErpRepository;

	private readonly ITelegramUserStore _telegramUserStore;

	private readonly IConfiguration _configuration;

	private readonly ILogger<SalesmenController> _logger;

	public SalesmenController(ILogoErpRepository logoErpRepository, ITelegramUserStore telegramUserStore, IConfiguration configuration, ILogger<SalesmenController> logger)
	{
		_logoErpRepository = logoErpRepository;
		_telegramUserStore = telegramUserStore;
		_configuration = configuration;
		_logger = logger;
	}

	[HttpGet("logo-salesmen")]
	public async Task<IActionResult> GetLogoSalesmen(CancellationToken ct)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		return Ok(await _logoErpRepository.GetSalesmenAsync(firmNo, ct));
	}

	[HttpGet("telegram-users")]
	public IActionResult GetTelegramUsers()
	{
		List<TelegramUserMappingDto> allUsers = _telegramUserStore.GetAllUsers();
		return Ok(allUsers);
	}

	[HttpPost("map-user")]
	public async Task<IActionResult> MapUser([FromBody] MapSalesmanRequest req, CancellationToken ct)
	{
		if (req == null || req.ChatId <= 0)
		{
			return BadRequest(new
			{
				isSuccess = false,
				message = "Geçersiz ChatId parametresi."
			});
		}
		string code = null;
		string name = null;
		if (req.SalesmanRef.HasValue && req.SalesmanRef.Value > 0)
		{
			string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
			LogoErpSalesmanDto logoErpSalesmanDto = (await _logoErpRepository.GetSalesmenAsync(firmNo, ct)).FirstOrDefault((LogoErpSalesmanDto s) => s.LogicalRef == req.SalesmanRef.Value);
			if (logoErpSalesmanDto != null)
			{
				code = logoErpSalesmanDto.Code;
				name = logoErpSalesmanDto.Definition;
			}
		}
		_telegramUserStore.MapUserToSalesman(req.ChatId, req.SalesmanRef, code, name);
		return Ok(new
		{
			isSuccess = true,
			message = ((req.SalesmanRef.HasValue && req.SalesmanRef.Value > 0) ? $"Kullanıcı, Satış Temsilcisi '{name}' ({code}) ile başarıyla eşleştirildi!" : "Kullanıcının Satış Temsilcisi eşleşmesi kaldırıldı.")
		});
	}

	[HttpPost("update-status")]
	public IActionResult UpdateUserStatus([FromBody] UpdateUserStatusRequest req)
	{
		if (req == null || req.ChatId <= 0 || string.IsNullOrWhiteSpace(req.Status))
		{
			return BadRequest(new
			{
				isSuccess = false,
				message = "Geçersiz parametreler."
			});
		}
		_telegramUserStore.UpdateUserStatus(req.ChatId, req.Status);
		string value = req.Status switch
		{
			"Approved" => "Onaylandı (Erişim Verildi)", 
			"Blocked" => "Engellendi (Erişim Kesildi)", 
			"Pending" => "Beklemeye Alındı", 
			_ => req.Status, 
		};
		return Ok(new
		{
			isSuccess = true,
			message = $"Kullanıcı (ChatId: {req.ChatId}) erişim durumu güncellendi: {value}"
		});
	}

	[HttpPost("update-permissions")]
	public IActionResult UpdateUserPermissions([FromBody] UpdateUserPermissionsRequest req)
	{
		if (req == null || req.ChatId <= 0)
		{
			return BadRequest(new
			{
				isSuccess = false,
				message = "Geçersiz ChatId parametresi."
			});
		}
		if (!_telegramUserStore.UpdateUserPermissions(req))
		{
			return NotFound(new
			{
				isSuccess = false,
				message = "Kullanıcı bulunamadı."
			});
		}
		return Ok(new
		{
			isSuccess = true,
			message = $"Kullanıcı (ChatId: {req.ChatId}) yetki kısıtlamaları başarıyla güncellendi."
		});
	}
}
