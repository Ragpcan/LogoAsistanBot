using System;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Infrastructure.Telegram;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.WebUI.Services;

public class TelegramHostedService : BackgroundService
{
	private readonly ITelegramBotManager _botManager;

	private readonly ILogger<TelegramHostedService> _logger;

	public TelegramHostedService(ITelegramBotManager botManager, ILogger<TelegramHostedService> logger)
	{
		_botManager = botManager;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			_logger.LogInformation("WebUI Telegram Bot Hizmeti Başlatılıyor...");
			await _botManager.StartAsync(stoppingToken);
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
		}
		catch (Exception exception)
		{
			if (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					_logger.LogWarning(exception, "Otomatik bot başlatma sırasında uyarı: Konfigürasyon bekleniyor.");
					return;
				}
				catch
				{
					return;
				}
			}
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogInformation("WebUI Telegram Bot Hizmeti Durduruluyor...");
			await _botManager.StopAsync(cancellationToken);
		}
		catch
		{
		}
		await base.StopAsync(cancellationToken);
	}
}
