using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.Interfaces;
using LogoCrmTelegramIntegration.Infrastructure.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.WebUI.Services;

public class ExecutiveReportHostedService : BackgroundService
{
    private readonly IExecutiveReportService _executiveReportService;
    private readonly ITelegramBotManager _telegramBotManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExecutiveReportHostedService> _logger;
    private DateTime _lastSentDate = DateTime.MinValue;

    public ExecutiveReportHostedService(
        IExecutiveReportService executiveReportService,
        ITelegramBotManager telegramBotManager,
        IConfiguration configuration,
        ILogger<ExecutiveReportHostedService> logger)
    {
        _executiveReportService = executiveReportService;
        _telegramBotManager = telegramBotManager;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Otomatik Günlük Yönetici Rapor Servisi başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool isEnabled = bool.TryParse(_configuration["ExecutiveReport:Enabled"], out var en) ? en : true;
                string scheduleTime = _configuration["ExecutiveReport:ScheduleTime"] ?? "08:30";

                if (isEnabled)
                {
                    var now = DateTime.Now;
                    string currentTimeStr = now.ToString("HH:mm");

                    if (currentTimeStr == scheduleTime && _lastSentDate.Date != now.Date)
                    {
                        _logger.LogInformation("Günlük Yönetici Raporu saati geldi ({Time}), rapor gönderiliyor...", currentTimeStr);

                        var report = await _executiveReportService.GenerateDailySummaryAsync(stoppingToken);
                        if (!string.IsNullOrWhiteSpace(report.FormattedTelegramHtml))
                        {
                            var targetChatsStr = _configuration["ExecutiveReport:AdminChatIds"];
                            if (!string.IsNullOrWhiteSpace(targetChatsStr))
                            {
                                var chatIds = targetChatsStr.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var chatIdStr in chatIds)
                                {
                                    if (long.TryParse(chatIdStr.Trim(), out long chatId))
                                    {
                                        await _telegramBotManager.SendTextMessageAsync(chatId, report.FormattedTelegramHtml, stoppingToken);
                                    }
                                }
                            }
                        }

                        _lastSentDate = now.Date;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Günlük Yönetici Rapor servisi döngüsünde hata.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
