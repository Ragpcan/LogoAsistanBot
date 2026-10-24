using System;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Infrastructure.Telegram;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BotController : ControllerBase
{
	private readonly ITelegramBotManager _telegramBotManager;

	private readonly ILogger<BotController> _logger;

	public BotController(ITelegramBotManager telegramBotManager, ILogger<BotController> logger)
	{
		_telegramBotManager = telegramBotManager;
		_logger = logger;
	}

	[HttpGet("status")]
	public IActionResult GetStatus()
	{
		BotStatus status = _telegramBotManager.GetStatus();
		string botUsername = _telegramBotManager.GetBotUsername();
		string lastError = _telegramBotManager.GetLastError()?.Message;
		return Ok(new
		{
			status = status.ToString(),
			botUsername = botUsername,
			lastError = lastError
		});
	}

	[HttpPost("start")]
	public async Task<IActionResult> StartBot()
	{
		try
		{
			await _telegramBotManager.StartAsync();
			return Ok(new
			{
				isSuccess = true,
				message = "Telegram Bot başarıyla başlatıldı!",
				status = _telegramBotManager.GetStatus().ToString(),
				botUsername = _telegramBotManager.GetBotUsername()
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Bot başlatılırken hata oluştu.");
			string text = ex.ToString();
			string text2 = ex.Message;
			if (text.Contains("api.telegram.org") || text.Contains("Bilinen böyle bir ana bilgisayar yok"))
			{
				text2 = "Telegram sunucularına (api.telegram.org) bağlanılamadı. Lütfen bilgisayarınızın İnternet bağlantısını veya DNS/VPN ayarlarını kontrol edip 'Botu Başlat' butonuna tekrar tıklayın.";
			}
			return BadRequest(new
			{
				isSuccess = false,
				message = "Bot başlatılamadı: " + text2
			});
		}
	}

	[HttpPost("stop")]
	public async Task<IActionResult> StopBot()
	{
		try
		{
			await _telegramBotManager.StopAsync();
			return Ok(new
			{
				isSuccess = true,
				message = "Telegram Bot durduruldu.",
				status = _telegramBotManager.GetStatus().ToString()
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Bot durdurulurken hata oluştu.");
			return BadRequest(new
			{
				isSuccess = false,
				message = "Bot durdurulamadı: " + ex.Message
			});
		}
	}
}
