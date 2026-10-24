using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogoCrmTelegramIntegration.Infrastructure.Telegram;

public interface ITelegramBotManager
{
	Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));

	Task StopAsync(CancellationToken cancellationToken = default(CancellationToken));

	BotStatus GetStatus();

	string? GetBotUsername();

	Exception? GetLastError();

	Task SendTextMessageAsync(long chatId, string messageHtml, CancellationToken ct = default);
}
