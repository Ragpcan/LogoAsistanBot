using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using LogoCrmTelegramIntegration.Application.Services;
using LogoCrmTelegramIntegration.Infrastructure.Persistence;
using LogoCrmTelegramIntegration.Infrastructure.Services;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace LogoCrmTelegramIntegration.Infrastructure.Telegram;

public class TelegramBotManager : ITelegramBotManager
{
	private readonly IProposalAppService _proposalAppService;

	private readonly ILogoErpRepository _logoErpRepository;

	private readonly IOrderPdfGenerator _orderPdfGenerator;

	private readonly IStandardPdfGenerator _standardPdfGenerator;

	private readonly IDraftOrderStore _draftOrderStore;

	private readonly IXsltDispatchService _xsltDispatchService;

	private readonly IXsltInvoiceService _xsltInvoiceService;

	private readonly IEBelgeDispatcherService _ebelgeDispatcherService;

	private readonly ITelegramUserStore _telegramUserStore;

	private readonly IProposalLogStore _logStore;

	private readonly IVoiceSpeechTranscriber _transcriber;

	private readonly IAppPermissionStore _appPermissionStore;

	private readonly ISmsService _smsService;

	private readonly IEmailService _emailService;

	private readonly IConfiguration _configuration;

	private readonly ILogger<TelegramBotManager> _logger;

	private readonly IAiAssistantService? _aiAssistant;

	private readonly IExecutiveReportService? _executiveReport;

	private readonly ICustomReportStore? _customReportStore;

	private readonly IDynamicSqlExecutor? _dynamicSqlExecutor;

	private readonly IReportHistoryStore? _reportHistoryStore;

	private readonly ConcurrentDictionary<long, OrderWizardState> _wizardStates = new ConcurrentDictionary<long, OrderWizardState>();
	private readonly ConcurrentDictionary<long, string> _awaitingAiReportPrompt = new();

	private ITelegramBotClient? _botClient;

	private CancellationTokenSource? _cts;

	private bool _isRunning;

	private string _botUsername = string.Empty;

	private Exception? _lastError;

	public bool IsRunning => _isRunning;

	public string BotUsername => _botUsername;

	public Exception? LastError => _lastError;

	public TelegramBotManager(IProposalAppService proposalAppService, ILogoErpRepository logoErpRepository, IOrderPdfGenerator orderPdfGenerator, IStandardPdfGenerator standardPdfGenerator, IDraftOrderStore draftOrderStore, IXsltDispatchService xsltDispatchService, IXsltInvoiceService xsltInvoiceService, IEBelgeDispatcherService ebelgeDispatcherService, ITelegramUserStore telegramUserStore, IProposalLogStore logStore, IVoiceSpeechTranscriber transcriber, IAppPermissionStore appPermissionStore, ISmsService smsService, IEmailService emailService, IConfiguration configuration, ILogger<TelegramBotManager> logger, IAiAssistantService? aiAssistant = null, IExecutiveReportService? executiveReport = null, ICustomReportStore? customReportStore = null, IDynamicSqlExecutor? dynamicSqlExecutor = null, IReportHistoryStore? reportHistoryStore = null)
	{
		_proposalAppService = proposalAppService;
		_logoErpRepository = logoErpRepository;
		_orderPdfGenerator = orderPdfGenerator;
		_standardPdfGenerator = standardPdfGenerator;
		_draftOrderStore = draftOrderStore;
		_xsltDispatchService = xsltDispatchService;
		_xsltInvoiceService = xsltInvoiceService;
		_ebelgeDispatcherService = ebelgeDispatcherService;
		_telegramUserStore = telegramUserStore;
		_logStore = logStore;
		_transcriber = transcriber;
		_appPermissionStore = appPermissionStore;
		_smsService = smsService;
		_emailService = emailService;
		_configuration = configuration;
		_logger = logger;
		_aiAssistant = aiAssistant;
		_executiveReport = executiveReport;
		_customReportStore = customReportStore;
		_dynamicSqlExecutor = dynamicSqlExecutor;
		_reportHistoryStore = reportHistoryStore;
	}

	public BotStatus GetStatus()
	{
		if (!_isRunning)
		{
			return BotStatus.Stopped;
		}
		return BotStatus.Running;
	}

	public string? GetBotUsername()
	{
		return _botUsername;
	}

	public Exception? GetLastError()
	{
		return _lastError;
	}

	public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_isRunning)
		{
			return;
		}
		string text = _configuration["Telegram:BotToken"];
		if (string.IsNullOrWhiteSpace(text) || text.Contains("YOUR_TELEGRAM_BOT_TOKEN"))
		{
			_logger.LogWarning("Telegram BotToken yapılandırılmadığı için Telegram Bot hizmeti başlatılamadı.");
			return;
		}
		try
		{
			_botClient = new TelegramBotClient(text);
			User me = null;
			int maxAttempts = 3;
			for (int attempt = 1; attempt <= maxAttempts; attempt++)
			{
				try
				{
					me = await _botClient.GetMe(cancellationToken);
					if (me != null)
					{
						break;
					}
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					return;
				}
				catch (Exception ex2) when (attempt < maxAttempts && !cancellationToken.IsCancellationRequested)
				{
					SafeLogWarning(ex2, "Telegram API sunucusuna erişim deneniyor... ({Attempt}/{Max})", attempt, maxAttempts);
					await Task.Delay(2000, cancellationToken);
				}
			}
			if (me == null && !cancellationToken.IsCancellationRequested)
			{
				me = await _botClient.GetMe(cancellationToken);
			}
			if (me != null)
			{
				_botUsername = me.Username ?? "BilinmeyenBot";
				_cts = new CancellationTokenSource();
				ReceiverOptions receiverOptions = new ReceiverOptions();
				receiverOptions.AllowedUpdates = new UpdateType[2]
				{
					UpdateType.Message,
					UpdateType.CallbackQuery
				};
				ReceiverOptions receiverOptions2 = receiverOptions;
				_botClient.StartReceiving((Func<ITelegramBotClient, Update, CancellationToken, Task>)HandleUpdateAsync, (Func<ITelegramBotClient, Exception, CancellationToken, Task>)HandlePollingErrorAsync, receiverOptions2, _cts.Token);
				_isRunning = true;
				_lastError = null;
				try
				{
					BotCommand[] commands = new BotCommand[8]
					{
						new BotCommand
						{
							Command = "start",
							Description = "Botu başlatır ve komut kılavuzunu görüntüler"
						},
						new BotCommand
						{
							Command = "listele",
							Description = "Açık siparişler, irsaliyeler ve hazır raporlar"
						},
						new BotCommand
						{
							Command = "irsaliyeler",
							Description = "Toptan Satış İrsaliyeleri listesi"
						},
						new BotCommand
						{
							Command = "irsaliye",
							Description = "GİB e-İrsaliye PDF indir (Örn: /irsaliye IRS000001)"
						},
						new BotCommand
						{
							Command = "excel",
							Description = "Örnek sipariş dosya şablonunu indir"
						},
						new BotCommand
						{
							Command = "stoklistesi",
							Description = "Stok listesini Excel olarak indir"
						},
						new BotCommand
						{
							Command = "carilistesi",
							Description = "Cari hesap listesini Excel olarak indir"
						},
						new BotCommand
						{
							Command = "iptal",
							Description = "Devam eden aktif siparişi iptal et"
						}
					};
					await _botClient.SetMyCommands(commands, null, null, _cts.Token);
				}
				catch (Exception ex3)
				{
					SafeLogWarning(ex3, "Telegram bot komut menüsü kaydedilirken ikincil uyarı alındı.");
				}
				SafeLogInformation("Telegram Bot başarıyla başlatıldı. Bot Kullanıcı Adı: @{BotUsername} (ID: {BotId})", _botUsername, me.Id);
			}
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			_isRunning = false;
		}
		catch (Exception lastError)
		{
			Exception ex5 = (_lastError = lastError);
			_isRunning = false;
			if (!cancellationToken.IsCancellationRequested)
			{
				SafeLogError(ex5, "Telegram Bot başlatılırken hata oluştu.");
			}
			throw;
		}
	}

	public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!_isRunning)
		{
			return Task.CompletedTask;
		}
		try
		{
			_cts?.Cancel();
			_cts?.Dispose();
			_cts = null;
			_isRunning = false;
			_logger.LogInformation("Telegram Bot durduruldu.");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Telegram Bot durdurulurken hata oluştu.");
		}
		return Task.CompletedTask;
	}

	private void LogActivity(long? chatId, string rawMessage, string firmCode, string productCode, decimal amount, bool isSuccess, string message)
	{
		string telegramUser = "Telegram Bot / WebUI";
		if (chatId.HasValue && chatId.Value != 0L)
		{
			TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId.Value);
			telegramUser = ((user == null) ? $"ID:{chatId.Value}" : ((!string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(user.Username)) ? (user.FullName + " (@" + user.Username + ")") : ((!string.IsNullOrWhiteSpace(user.FullName)) ? user.FullName : (string.IsNullOrWhiteSpace(user.Username) ? $"ID:{chatId.Value}" : ("@" + user.Username)))));
		}
		_logStore.AddLog(new ProposalLogItem
		{
			Timestamp = DateTime.Now,
			TelegramUser = telegramUser,
			RawMessage = rawMessage,
			FirmCode = (string.IsNullOrWhiteSpace(firmCode) ? "-" : firmCode),
			ProductCode = (string.IsNullOrWhiteSpace(productCode) ? "-" : productCode),
			Amount = amount,
			IsSuccess = isSuccess,
			Message = message
		});
	}

	private static string? ExtractFicheNo(string message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return null;
		}
		Match match = Regex.Match(message, "Fiş No:\\s*`?([A-Za-z0-9]+)`?");
		if (match.Success)
		{
			return match.Groups[1].Value;
		}
		match = Regex.Match(message, "(BOT\\d+|TG\\d+|FAT\\d+|IRS\\d+|SIP\\d+|DNM\\d+|[A-Za-z]{2,8}\\d{3,14})");
		if (!match.Success)
		{
			return null;
		}
		return match.Value;
	}

	private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
	{
		string text = _configuration["SystemLicense:ExpirationDate"];
		if (!string.IsNullOrWhiteSpace(text) && DateTime.TryParse(text, out var result) && DateTime.Now.Date > result.Date)
		{
			string text2 = $"⚠\ufe0f **LİSANS SÜRESİ DOLMUŞTUR**\n\nBu yazılımın kullanım süresi **{result:dd.MM.yyyy}** tarihinde dolmuştur.\nSistem otomatik olarak durdurulmuştur. Lütfen sistem yöneticiniz ile iletişime geçerek lisans sürenizi uzatınız.";
			long num = 0L;
			if (update.Message != null)
			{
				num = update.Message.Chat.Id;
			}
			else if (update.CallbackQuery?.Message != null)
			{
				num = update.CallbackQuery.Message.Chat.Id;
			}
			if (num != 0L)
			{
				await SendReplyAsync(botClient, num, text2, cancellationToken);
			}
			return;
		}
		if (update.Type == UpdateType.CallbackQuery)
		{
			CallbackQuery callbackQuery = update.CallbackQuery;
			if (callbackQuery != null)
			{
				await HandleCallbackQueryAsync(botClient, callbackQuery, cancellationToken);
				return;
			}
		}
		Document document = update.Message?.Document;
		if (document != null)
		{
			await HandleDocumentUploadAsync(botClient, update.Message, document, cancellationToken);
			return;
		}
		Voice voice = update.Message?.Voice;
		if (voice != null)
		{
			await HandleVoiceMessageAsync(botClient, update.Message, voice, cancellationToken);
			return;
		}
		Message message = update.Message;
		if (message == null)
		{
			return;
		}
		string messageText = message.Text;
		if (messageText == null)
		{
			return;
		}
		long chatId = message.Chat.Id;
		string text3 = message.From?.Username ?? "";
		string text4 = (message.From?.FirstName + " " + message.From?.LastName).Trim();
		_telegramUserStore.RegisterOrUpdateUser(chatId, text3, text4);
		_logger.LogInformation("Telegram'dan mesaj alındı. ChatId: {ChatId}, Gönderen: {Username}, Mesaj: {MessageText}", chatId, text3, messageText);
		TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId);
		if (user != null)
		{
			if (user.IsBlocked)
			{
				await SendReplyAsync(botClient, chatId, "\ud83d\udeab **ERİŞİM ENGELLENDİ**\n\nTelegram hesabınız yönetici tarafından engellenmiştir. Sorgulama veya sipariş yapamazsınız.", cancellationToken);
				LogActivity(chatId, messageText, "-", "-", 0m, isSuccess: false, "Engellenmiş kullanıcı deneme yaptı.");
				return;
			}
			if (!user.IsApproved)
			{
				string text5 = $"⚠️ **YETKİSİZ ERİŞİM ENGELENDİ!**\n\nBu Telegram hesabı (ChatID: {chatId}) henüz yetkilendirilmemiştir.\n\n🔒 Güvenlik gereği veritabanı sorgusu çalıştırılamaz.\nKayıt isteğiniz yöneticinize iletildi. Onay verildikten sonra botu kullanabilirsiniz.";
				await SendReplyAsync(botClient, chatId, text5, cancellationToken);
				LogActivity(chatId, messageText, "-", "-", 0m, isSuccess: false, "Onay bekleyen kullanıcı bota erişmeye çalıştı.");

				try
				{
					var approvedAdmins = _telegramUserStore.GetAllUsers()
						.Where(u => u.ChatId != chatId && u.IsApproved)
						.ToList();

					foreach (var admin in approvedAdmins)
					{
						var adminNotification = $"🔔 **YENİ KULLANICI ERİŞİM İSTEĞİ!**\n\n" +
												$"• **Ad Soyad:** {text4}\n" +
												$"• **Kullanıcı Adı:** @{(string.IsNullOrWhiteSpace(text3) ? "Yok" : text3)}\n" +
												$"• **ChatID:** `{chatId}`\n" +
												$"• **Tarih:** {DateTime.Now:dd.MM.yyyy HH:mm}\n\n" +
												$"💡 *Aşağıdaki butona tıklayarak veya `/onay {chatId}` komutu ile kullanıcıya anında erişim izni verebilirsiniz.*";

						var approveMarkup = new InlineKeyboardMarkup(new[]
						{
							new[] { InlineKeyboardButton.WithCallbackData($"✅ Erişim Ver ({text4})", $"usr_approve_{chatId}") }
						});

						await SendReplyAsync(botClient, admin.ChatId, adminNotification, cancellationToken, approveMarkup);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Yöneticiye yetkisiz erişim bildirimi gönderilirken hata oluştu.");
				}
				return;
			}
		}
		string trimmedText = messageText.Trim();
		if (trimmedText.Equals("/iptal", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("iptal", StringComparison.OrdinalIgnoreCase))
		{
			_wizardStates.TryRemove(chatId, out OrderWizardState _);
			await SendReplyAsync(botClient, chatId, "❌ Sipariş oluşturma sihirbazı iptal edildi.", cancellationToken);
			LogActivity(chatId, trimmedText, "-", "-", 0m, isSuccess: true, "İşlem iptal edildi.");
			return;
		}
		if (trimmedText.Equals("/irsaliyeler", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("irsaliyeler", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("/irsaliye_listesi", StringComparison.OrdinalIgnoreCase))
		{
			await SendDispatchListAsync(botClient, chatId, null, cancellationToken);
			return;
		}
		if (trimmedText.StartsWith("/irsaliye ", StringComparison.OrdinalIgnoreCase) || trimmedText.StartsWith("irsaliye ", StringComparison.OrdinalIgnoreCase))
		{
			string[] array = trimmedText.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 2)
			{
				string ficheNo = array[1].Trim();
				await SendDispatchPdfDocumentAsync(botClient, chatId, ficheNo, cancellationToken);
				return;
			}
		}
		if (trimmedText.Equals("/listele", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("listele", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("/raporlar", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("raporlar", StringComparison.OrdinalIgnoreCase))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("\ud83d\udcca **HAZIR RAPOR VE İŞLEM MENÜSÜ**\n");
			stringBuilder.AppendLine("Aşağıdaki butonlara tıklayarak veya komut linklerini kullanarak belgeleri anında indirebilirsiniz:\n");
			stringBuilder.AppendLine("• /irsaliyeler - Toptan Satış İrsaliyeleri");
			stringBuilder.AppendLine("• /irsaliye IRS000001 - GİB e-İrsaliye PDF İndir");
			stringBuilder.AppendLine("• /stoklistesi - Tüm Stok Listesi");
			stringBuilder.AppendLine("• /carilistesi - Tüm Cari Hesaplar");
			stringBuilder.AppendLine("• /borclular - Borçlu Cariler");
			stringBuilder.AppendLine("• /alacaklilar - Alacaklı Cariler\n");
			InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[6][]
			{
				new InlineKeyboardButton[2]
				{
					InlineKeyboardButton.WithCallbackData("\ud83d\uded2 Sipariş Oluştur", "menu_create_order"),
					InlineKeyboardButton.WithCallbackData("\ud83d\ude9a İrsaliye Kes", "menu_create_dispatch")
				},
				new InlineKeyboardButton[2]
				{
					InlineKeyboardButton.WithCallbackData("\ud83d\udce6 Açık Siparişler", "acik_siparis_listesi"),
					InlineKeyboardButton.WithCallbackData("\ud83d\ude9a Satış İrsaliyeleri", "irsaliye_listesi")
				},
				new InlineKeyboardButton[2]
				{
					InlineKeyboardButton.WithCallbackData("\ud83d\udce6 Stok Listesi Excel", "rpt_stok"),
					InlineKeyboardButton.WithCallbackData("\ud83c\udfe2 Cari Liste Excel", "rpt_cari")
				},
				new InlineKeyboardButton[2]
				{
					InlineKeyboardButton.WithCallbackData("\ud83d\udd34 Borçlular Excel", "rpt_borc"),
					InlineKeyboardButton.WithCallbackData("\ud83d\udfe2 Alacaklılar Excel", "rpt_alacak")
				},
				new InlineKeyboardButton[2]
				{
					InlineKeyboardButton.WithCallbackData("📊 Dashboard", "menu_dashboard"),
					InlineKeyboardButton.WithCallbackData("⏱️ Vade Takip", "menu_vade_takip")
				},
				new InlineKeyboardButton[2]
				{
					InlineKeyboardButton.WithCallbackData("📱 SMS Gönder", "menu_sms"),
					InlineKeyboardButton.WithCallbackData("\ud83d\udcc4 Örnek Sipariş Şablonu", "rpt_excel_sample")
				}
			});
			await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, replyMarkup);
			LogActivity(chatId, trimmedText, "-", "-", 0m, isSuccess: true, "Rapor ve İrsaliye buton menüsü gönderildi.");
			return;
		}
		if (trimmedText.Equals("/excel", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("/ornek", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("örnek excel", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("ornek excel", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("excel şablonu", StringComparison.OrdinalIgnoreCase))
		{
			await SendSampleExcelTemplateAsync(botClient, chatId, null, null, "ALL", cancellationToken);
			LogActivity(chatId, trimmedText, "-", "-", 0m, isSuccess: true, "Örnek Excel şablonu Telegram'a gönderildi.");
			return;
		}
		if (trimmedText.Equals("/stoklistesi", StringComparison.OrdinalIgnoreCase))
		{
			using (MemoryStream ms = new MemoryStream(await _proposalAppService.GetStockListExcelAsync(cancellationToken)))
			{
				ChatId chatId2 = chatId;
				InputFileStream document2 = InputFile.FromStream(ms, "StokListesi.csv");
				CancellationToken cancellationToken2 = cancellationToken;
				await botClient.SendDocument(chatId2, document2, "\ud83d\udce6 **Tüm Stok Listesi (Excel)**", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken2);
				LogActivity(chatId, trimmedText, "-", "-", 0m, isSuccess: true, "Stok Listesi Excel gönderildi.");
				return;
			}
		}
		if (trimmedText.Equals("/carilistesi", StringComparison.OrdinalIgnoreCase))
		{
			using (MemoryStream ms = new MemoryStream(await _proposalAppService.GetFirmListExcelAsync("ALL", cancellationToken)))
			{
				ChatId chatId3 = chatId;
				InputFileStream document3 = InputFile.FromStream(ms, "CariHesapListesi.csv");
				CancellationToken cancellationToken2 = cancellationToken;
				await botClient.SendDocument(chatId3, document3, "\ud83c\udfe2 **Tüm Cari Hesap Listesi (Excel)**\n\nBakiyeler Döviz Cinslerine Göre Ayrılmıştır.", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken2);
				LogActivity(chatId, trimmedText, "-", "-", 0m, isSuccess: true, "Cari Hesap Listesi Excel gönderildi.");
				return;
			}
		}
		if (trimmedText.Equals("/borclular", StringComparison.OrdinalIgnoreCase))
		{
			using (MemoryStream ms = new MemoryStream(await _proposalAppService.GetFirmListExcelAsync("DEBT", cancellationToken)))
			{
				ChatId chatId4 = chatId;
				InputFileStream document4 = InputFile.FromStream(ms, "BorcluCariler.csv");
				CancellationToken cancellationToken2 = cancellationToken;
				await botClient.SendDocument(chatId4, document4, "\ud83d\udd34 **Borçlu Cari Listesi (Excel)**\n\nBakiyeler Döviz Cinslerine Göre Ayrılmıştır.", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken2);
				LogActivity(chatId, trimmedText, "-", "-", 0m, isSuccess: true, "Borçlu Cari Listesi Excel gönderildi.");
				return;
			}
		}
		if (trimmedText.Equals("/alacaklilar", StringComparison.OrdinalIgnoreCase))
		{
			using (MemoryStream ms = new MemoryStream(await _proposalAppService.GetFirmListExcelAsync("CREDIT", cancellationToken)))
			{
				ChatId chatId5 = chatId;
				InputFileStream document5 = InputFile.FromStream(ms, "AlacakliCariler.csv");
				CancellationToken cancellationToken2 = cancellationToken;
				await botClient.SendDocument(chatId5, document5, "\ud83d\udfe2 **Alacaklı Cari Listesi (Excel)**\n\nBakiyeler Döviz Cinslerine Göre Ayrılmıştır.", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken2);
				LogActivity(chatId, trimmedText, "-", "-", 0m, isSuccess: true, "Alacaklı Cari Listesi Excel gönderildi.");
				return;
			}
		}
		if (trimmedText.Equals("/aciksiparisler", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("/siparisler", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("/bekleyensiparisler", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("açık siparişler", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("acik siparisler", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("siparişler", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("siparisler", StringComparison.OrdinalIgnoreCase))
		{
			await SendDateFilterMenuAsync(botClient, chatId, null, cancellationToken);
			return;
		}
		string[] array2 = trimmedText.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array2.Length == 2 && (array2[1].Equals("SIPARIS", StringComparison.OrdinalIgnoreCase) || array2[1].Equals("SİPARİŞ", StringComparison.OrdinalIgnoreCase) || array2[1].Equals("SIPARISLER", StringComparison.OrdinalIgnoreCase) || array2[1].Equals("SİPARİŞLER", StringComparison.OrdinalIgnoreCase)))
		{
			string firmCodeFilter = array2[0];
			await SendDateFilterMenuAsync(botClient, chatId, firmCodeFilter, cancellationToken);
			return;
		}
		if (_wizardStates.TryGetValue(chatId, out OrderWizardState wizard))
		{
			if (wizard.Step == "AwaitingFirmSearchForOrder" || wizard.Step == "AwaitingFirmSearchForDispatch")
			{
				bool isDispatch = wizard.Step == "AwaitingFirmSearchForDispatch";
				List<LogoErpFirmDto> list = await _proposalAppService.SearchFirmsAsync(trimmedText, cancellationToken);
				if (list == null || list.Count == 0)
				{
					await SendReplyAsync(botClient, chatId, "❌ '" + trimmedText + "' aramanıza uygun cari hesap bulunamadı. Lütfen tekrar farklı bir **Cari Unvanı veya Kodu** yazınız:", cancellationToken);
				}
				else if (list.Count == 1)
				{
					LogoErpFirmDto logoErpFirmDto = list[0];
					wizard.FirmCode = logoErpFirmDto.FirmCode;
					wizard.FirmName = logoErpFirmDto.FirmName;
					wizard.IsDispatchMode = isDispatch;
					wizard.Step = "AwaitingMethodSelection";
					await SendOrderOrDispatchMethodSelectionAsync(botClient, chatId, wizard, cancellationToken);
				}
				else
				{
					await SendCariSelectionKeyboardAsync(botClient, chatId, trimmedText, list, cancellationToken);
				}
				return;
			}
			if (wizard.Step == "AwaitingCarrierInfo")
			{
				if (string.IsNullOrWhiteSpace(trimmedText))
				{
					await SendReplyAsync(botClient, chatId, "❌ e-İrsaliye GİB standartları gereği **Taşıyıcı Kodu** veya **Şöför Bilgileri (Ad Soyad / TCKN / Plaka)** girmek ZORUNLUDUR.\n\nLütfen taşıyıcı veya şöför bilgisini yazınız (Örn: `34 ABC 123 - Ahmet Yılmaz` veya `ARAS KARGO`):", cancellationToken);
					return;
				}
				wizard.CarrierOrDriverInfo = trimmedText.Trim();
				await SendReplyAsync(botClient, chatId, "✅ **Taşıyıcı / Şoför Bilgisi Kaydedildi:** `" + wizard.CarrierOrDriverInfo + "`", cancellationToken);
				await SendDraftDispatchConfirmationAsync(botClient, chatId, wizard, cancellationToken);
				return;
			}
			if (wizard.IsCollectionMode && wizard.Step == "AwaitingCollectionAmount")
			{
				if (decimal.TryParse(trimmedText.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valAmnt) && valAmnt > 0m)
				{
					wizard.CollectionAmount = valAmnt;
					wizard.Step = "AwaitingCollectionDescription";
					InlineKeyboardMarkup confirmMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
					{
						new[] { InlineKeyboardButton.WithCallbackData("✅ Direk Onayla & ERP'ye İşle", "ths_confirm") },
						new[] { InlineKeyboardButton.WithCallbackData("❌ İptal Et", "ths_cancel") }
					});
					await SendReplyAsync(botClient, chatId, $"✅ **Tahsilat Tutarı:** `{valAmnt:N2} TL`\n\n📝 **Açıklama / Not Giriniz:**\n(İsteğe bağlı, örn: `Makbuz No: 45872`) veya aşağıdaki buton ile direkt onaylayınız:", cancellationToken, confirmMarkup);
				}
				else
				{
					await SendReplyAsync(botClient, chatId, "❌ Geçersiz tutar. Lütfen sadece rakam olarak giriniz (Örn: `5000` veya `1250.50`):", cancellationToken);
				}
				return;
			}
			if (wizard.IsCollectionMode && wizard.Step == "AwaitingCollectionDescription")
			{
				wizard.Description = trimmedText.Trim();
				await ExecuteFinalCollectionAsync(botClient, chatId, wizard, cancellationToken);
				return;
			}
			if (wizard.Step == "AwaitingGeneralDescription")
			{
				wizard.Description = trimmedText.Trim();
				wizard.Step = string.Empty;
				await SendReplyAsync(botClient, chatId, "✅ **Genel Açıklama / Not Kaydedildi:** `" + wizard.Description + "`", cancellationToken);
				await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
				return;
			}
			if (wizard.Step == "AwaitingGeneralDiscount" || wizard.Step == "AwaitingExcelGeneralDiscount")
			{
				bool isDispatch = wizard.Step == "AwaitingExcelGeneralDiscount";
				if (!decimal.TryParse(trimmedText.Replace('%', ' ').Replace("tl", "", StringComparison.OrdinalIgnoreCase).Replace("lira", "", StringComparison.OrdinalIgnoreCase)
					.Replace(',', '.')
					.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result2) || !(result2 > 0m))
				{
					await SendReplyAsync(botClient, chatId, "❌ Geçersiz iskonto formatı. Lütfen örn: `%10` için `10` veya `500 TL` indirim için `500TL` yazınız:", cancellationToken);
					return;
				}
				if (trimmedText.ToLowerInvariant().Contains("tl") || trimmedText.ToLowerInvariant().Contains("lira"))
				{
					wizard.GeneralDiscountAmount = result2;
					wizard.GeneralDiscountRate = 0m;
					await SendReplyAsync(botClient, chatId, $"✅ Tüm siparişe `{result2:N2} TL` **Genel İskonto (Tutar)** uygulandı.", cancellationToken);
				}
				else
				{
					if (result2 > 0m && result2 < 1m)
					{
						result2 *= 100m;
					}
					wizard.GeneralDiscountRate = result2;
					wizard.GeneralDiscountAmount = 0m;
					await SendReplyAsync(botClient, chatId, $"✅ Tüm siparişe `%{result2:N1}` **Genel İskonto (Oran)** uygulandı.", cancellationToken);
				}
				if (isDispatch)
				{
					wizard.Step = "ExcelPendingConfirmation";
					await SendReplyAsync(botClient, chatId, "\ud83d\udcca **Genel İskonto Güncellendi!**\nSiparişinizin yeni toplam tutarı hesaplandı. Onaylamak için `[ ✅ İskontolu Siparişi Onayla ]` butonuna basabilirsiniz.", cancellationToken);
				}
				else
				{
					wizard.Step = "AwaitingQuantity";
					await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
				}
				return;
			}
			if (wizard.Step == "AwaitingExcelGroupCode")
			{
				wizard.Step = "AwaitingExcelUpload";
				await SendSampleExcelTemplateAsync(botClient, chatId, wizard.FirmCode, wizard.FirmName, trimmedText, cancellationToken);
				LogActivity(chatId, trimmedText, wizard.FirmCode, "-", 0m, isSuccess: true, "Excel sipariş kataloğu grup filtresi '" + trimmedText + "' ile oluşturulup gönderildi.");
				return;
			}
			if (wizard.Step == "AwaitingProductSearch")
			{
				if (trimmedText.Contains(","))
				{
					await ProcessMultiProductCommaSearchAsync(botClient, chatId, wizard, trimmedText, cancellationToken);
					return;
				}
				List<LogoErpItemStockDto> stocks = await _proposalAppService.SearchItemStockAsync(trimmedText, cancellationToken);
				if (stocks == null || stocks.Count == 0)
				{
					await SendReplyAsync(botClient, chatId, "❌ '" + trimmedText + "' aramanıza uygun malzeme veritabanında bulunamadı. Lütfen tekrar farklı bir **Malzeme Adı veya Kodu** yazınız (Virgülle birden fazla malzeme aramak için örn: `a malz, b malz, c malz`):", cancellationToken);
					return;
				}
				if (stocks.Count > 10)
				{
					await SendReplyAsync(botClient, chatId, $"⚠\ufe0f **ÇOK FAZLA MALZEME BULUNDU** (`{trimmedText}` için **{stocks.Count}** Adet Malzeme Eşleşti)\n\nLütfen aramayı daraltmak için malzeme adına daha yakın ve detaylı bir kelime yazınız (Örn: `intel i5`, `intel i7`, `laptop`):", cancellationToken);
					LogActivity(chatId, trimmedText, wizard.FirmCode, "-", 0m, isSuccess: true, $"Sipariş sihirbazı malzeme arandı: '{trimmedText}' ({stocks.Count} ürün bulundu, 10'dan fazla olduğu için kelime daraltma istendi).");
					return;
				}
				List<InlineKeyboardButton[]> list2 = new List<InlineKeyboardButton[]>();
				foreach (LogoErpItemStockDto item in stocks)
				{
					string value2 = ((item.UnitPrice > 0m) ? $" - {item.UnitPrice:N2} TL" : "");
					string text6 = $"\ud83d\udce6 {item.ItemCode} - {item.ItemName}{value2}";
					if (text6.Length > 40)
					{
						text6 = text6.Substring(0, 37) + "...";
					}
					list2.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData(text6, "ord_prod:" + item.ItemCode) });
				}
				list2.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ İptal", "ord_cancel") });
				InlineKeyboardMarkup replyMarkup2 = new InlineKeyboardMarkup(list2);
				await SendReplyAsync(botClient, chatId, $"\ud83d\udd0d **LOGO ERP MALZEME SEÇİMİ** (`Arama: {trimmedText}` - {stocks.Count} Sonuç)\nCari: **{wizard.FirmCode} - {wizard.FirmName}**\n\nLütfen eklemek istediğiniz malzemeyi seçiniz:", cancellationToken, replyMarkup2);
				LogActivity(chatId, trimmedText, wizard.FirmCode, "-", 0m, isSuccess: true, $"Sipariş sihirbazı malzeme arandı: '{trimmedText}' ({stocks.Count} ürün bulundu).");
				return;
			}
			if (wizard.Step == "AwaitingQuantity")
			{
				string[] array3 = trimmedText.Split(new char[4] { ' ', '\t', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
				decimal qty;
				if (array3.Length >= 2 && decimal.TryParse(array3[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var qtyMulti) && decimal.TryParse(array3[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var priceMulti) && qtyMulti > 0m)
				{
					wizard.Lines.Add(new OrderWizardLine
					{
						ProductCode = wizard.CurrentProductCode,
						ProductName = wizard.CurrentProductName,
						Quantity = qtyMulti,
						ListPrice = priceMulti,
						UnitPrice = priceMulti
					});
					await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
					LogActivity(chatId, trimmedText, wizard.FirmCode, wizard.CurrentProductCode, qtyMulti, isSuccess: true, $"Sepete tek mesajda Miktar+Fiyat eklendi: {wizard.CurrentProductCode} x {qtyMulti} @ {priceMulti} TL.");
				}
				else if (!decimal.TryParse(trimmedText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out qty) || qty <= 0m)
				{
					await SendReplyAsync(botClient, chatId, "❌ Geçersiz miktar. Lütfen pozitif bir sayı giriniz (Örn: `10` veya tek mesajda Miktar+Fiyat için: `10 250`):", cancellationToken);
				}
				else
				{
					wizard.CurrentQuantity = qty;
					await SendPricePromptAsync(botClient, chatId, wizard, cancellationToken);
					LogActivity(chatId, trimmedText, wizard.FirmCode, wizard.CurrentProductCode, qty, isSuccess: true, $"Sipariş sihirbazı miktar girildi: {qty}");
				}
				return;
			}
			if (wizard.Step == "AwaitingPrice")
			{
				decimal qty = 0m;
				decimal.TryParse(trimmedText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out qty);
				wizard.Lines.Add(new OrderWizardLine
				{
					ProductCode = wizard.CurrentProductCode,
					ProductName = wizard.CurrentProductName,
					Quantity = wizard.CurrentQuantity,
					ListPrice = qty,
					UnitPrice = qty
				});
				await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
				LogActivity(chatId, trimmedText, wizard.FirmCode, wizard.CurrentProductCode, wizard.CurrentQuantity, isSuccess: true, $"Sepete kalem eklendi: {wizard.CurrentProductCode} x {wizard.CurrentQuantity} @ {qty} TL.");
				return;
			}
			if (wizard.Step == "AwaitingLineEditQuantity")
			{
				if (wizard.EditingLineIndex < 0 || wizard.EditingLineIndex >= wizard.Lines.Count)
				{
					await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
					return;
				}
				string[] array4 = trimmedText.Split(new char[5] { ' ', '\t', ';', ',', '*' }, StringSplitOptions.RemoveEmptyEntries);
				decimal result5;
				if (array4.Length >= 2 && decimal.TryParse(array4[0].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result3) && decimal.TryParse(array4[1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result4) && result3 > 0m)
				{
					OrderWizardLine orderWizardLine = wizard.Lines[wizard.EditingLineIndex];
					orderWizardLine.Quantity = result3;
					orderWizardLine.ListPrice = result4;
					orderWizardLine.UnitPrice = result4;
					wizard.EditingLineIndex = -1;
					wizard.Step = string.Empty;
					await SendReplyAsync(botClient, chatId, $"✅ `{orderWizardLine.ProductName}` güncellendi: `{result3:N2} ADET x {result4:N2} TL`", cancellationToken);
					await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
				}
				else if (!decimal.TryParse(trimmedText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result5) || result5 <= 0m)
				{
					await SendReplyAsync(botClient, chatId, "❌ Geçersiz miktar. Lütfen pozitif bir sayı giriniz (Örn: `10` veya tek mesajda Miktar*Fiyat için: `10 250` veya `10*250`):", cancellationToken);
				}
				else
				{
					wizard.Lines[wizard.EditingLineIndex].Quantity = result5;
					wizard.Step = "AwaitingLineEditPrice";
					await SendReplyAsync(botClient, chatId, $"\ud83d\udd22 **Yeni Miktar:** `{result5} ADET`\n\nLütfen yeni **Birim Fiyatı** giriniz (Mevcut fiyatı korumak için **/gec** yazabilirsiniz):", cancellationToken);
				}
				return;
			}
			if (wizard.Step == "AwaitingLineEditPrice")
			{
				if (wizard.EditingLineIndex < 0 || wizard.EditingLineIndex >= wizard.Lines.Count)
				{
					await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
					return;
				}
				OrderWizardLine orderWizardLine2 = wizard.Lines[wizard.EditingLineIndex];
				if (!trimmedText.Equals("/gec", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(trimmedText) && decimal.TryParse(trimmedText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result6))
				{
					orderWizardLine2.ListPrice = result6;
					orderWizardLine2.UnitPrice = result6;
				}
				wizard.EditingLineIndex = -1;
				wizard.Step = string.Empty;
				await SendReplyAsync(botClient, chatId, $"✅ `{orderWizardLine2.ProductName}` güncellendi: `{orderWizardLine2.Quantity:N2} ADET x {orderWizardLine2.EffectiveUnitPrice:N2} TL`", cancellationToken);
				await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
				return;
			}
		}
		if (trimmedText.Equals("/alissiparisleri", StringComparison.OrdinalIgnoreCase))
		{
			await SendPurchaseOrderListAsync(botClient, chatId, null, cancellationToken);
			return;
		}
		if (trimmedText.Equals("/alisirsaliyeleri", StringComparison.OrdinalIgnoreCase))
		{
			await SendPurchaseDispatchListAsync(botClient, chatId, null, cancellationToken);
			return;
		}
		if (trimmedText.Equals("/alisfaturalari", StringComparison.OrdinalIgnoreCase))
		{
			await SendPurchaseInvoiceListAsync(botClient, chatId, null, cancellationToken);
			return;
		}
		if (trimmedText.Equals("/satisfaturalari", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("/faturalar", StringComparison.OrdinalIgnoreCase))
		{
			await SendInvoiceListAsync(botClient, chatId, null, cancellationToken);
			return;
		}
		if (trimmedText.Equals("/hedef", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("/ciro", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("hedef", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("ciro", StringComparison.OrdinalIgnoreCase))
		{
			await SendSalesmanTargetReportAsync(botClient, chatId, cancellationToken);
			return;
		}
		string cleanCmd = trimmedText;
		int atIdx = cleanCmd.IndexOf('@');
		if (atIdx > 0)
		{
			cleanCmd = cleanCmd.Substring(0, atIdx).Trim();
		}
		string cleanCmdLower = cleanCmd.ToLowerInvariant();

		if (cleanCmdLower == "/risk" || cleanCmdLower == "risk" || cleanCmdLower.StartsWith("/risk ") || cleanCmdLower.StartsWith("risk "))
		{
			string param = "";
			if (cleanCmdLower.StartsWith("/risk")) param = cleanCmd.Length > 5 ? cleanCmd.Substring(5).Trim() : "";
			else if (cleanCmdLower.StartsWith("risk")) param = cleanCmd.Length > 4 ? cleanCmd.Substring(4).Trim() : "";
			await HandleRiskCommandAsync(botClient, chatId, param, cancellationToken);
			return;
		}
		if (cleanCmdLower == "/rapor" || cleanCmdLower == "rapor" || cleanCmdLower == "/ozet" || cleanCmdLower == "ozet" || cleanCmdLower == "/ozetrapor" || cleanCmdLower == "ozetrapor" || cleanCmdLower.StartsWith("/rapor ") || cleanCmdLower.StartsWith("rapor ") || cleanCmdLower.StartsWith("/ozet ") || cleanCmdLower.StartsWith("ozet "))
		{
			await HandleExecutiveReportCommandAsync(botClient, chatId, cancellationToken);
			return;
		}
		if (cleanCmdLower == "/firmam" || cleanCmdLower == "firmam" || cleanCmdLower == "/firma" || cleanCmdLower == "firma" || cleanCmdLower == "/kokpit" || cleanCmdLower == "kokpit")
		{
			await SendFirmamMenuAsync(botClient, chatId, cancellationToken);
			return;
		}
		if (cleanCmdLower == "/raporlar" || cleanCmdLower == "raporlar" || cleanCmdLower == "/raporlistesi")
		{
			await SendReportsMenuAsync(botClient, chatId, null, cancellationToken);
			return;
		}
		if (cleanCmdLower == "/kritikstok" || cleanCmdLower == "kritikstok" || cleanCmdLower == "/kritik" || cleanCmdLower == "kritik" || cleanCmdLower.StartsWith("/kritikstok ") || cleanCmdLower.StartsWith("kritikstok "))
		{
			await HandleCriticalStockCommandAsync(botClient, chatId, cancellationToken);
			return;
		}
		if (cleanCmdLower == "/sohbet" || cleanCmdLower == "sohbet" || cleanCmdLower == "/chat" || cleanCmdLower == "/aichat")
		{
			if (_aiAssistant != null)
			{
				bool isNowActive = _aiAssistant.ToggleSohbetMode(chatId);
				if (isNowActive)
				{
					var sohbetMarkup = new InlineKeyboardMarkup(new[]
					{
						new[] { InlineKeyboardButton.WithCallbackData("🔴 Sohbeti Kapat", "sohbet_kapat") },
						new[] { InlineKeyboardButton.WithCallbackData("🗑️ Geçmişi Temizle", "sohbet_temizle") },
						new[] { InlineKeyboardButton.WithCallbackData("🏢 ERP Menüsü", "firmam_menu") }
					});
					await SendReplyAsync(botClient, chatId,
						"🤖 **AI Sohbet Modu Açıldı!**\n\n💬 Artık benimle direkt konuşabilirsiniz.\nYazılarınız Gemini'ye iletilecek ve gerçek zamanlı yanıt alacaksınız.\n\n📌 *ERP işlemleri için `/erp` yazın veya aşağıdaki butonları kullanın.*",
						cancellationToken, sohbetMarkup);
				}
				else
				{
					await SendReplyAsync(botClient, chatId,
						"✅ **ERP Modu aktif.** Normal ERP işlemlerinize devam edebilirsiniz.\n\nTekrar AI sohbet modu için `/sohbet` yazın.",
						cancellationToken);
				}
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "❌ AI servisi aktif değil.", cancellationToken);
			}
			return;
		}
		if (cleanCmdLower == "/erp" || cleanCmdLower == "erp")
		{
			if (_aiAssistant != null && _aiAssistant.IsSohbetModeActive(chatId))
			{
				_aiAssistant.ClearChatHistory(chatId);
				await SendReplyAsync(botClient, chatId, "✅ **ERP moduna geçildi.** Sohbet geçmişi temizlendi.", cancellationToken);
			}
			else
			{
				await SendFirmamMenuAsync(botClient, chatId, cancellationToken);
			}
			return;
		}
		// ─── AI CARİ RAPORU İÇİN İSTEK BEKLEME DURUMU ───
		if (_awaitingAiReportPrompt.TryRemove(chatId, out string targetFirmCode))
		{
			if (cleanCmdLower == "iptal" || cleanCmdLower == "/iptal")
			{
				await SendCariActionMenuAsync(botClient, chatId, targetFirmCode, targetFirmCode, cancellationToken);
				return;
			}
			await SendReplyAsync(botClient, chatId, $"⏳ 🤖 **{targetFirmCode}** carisi için AI raporu hazırlanıyor...", cancellationToken);
			if (_aiAssistant != null)
			{
				try
				{
					var aiReport = await _aiAssistant.AnswerUserQueryWithButtonsAsync(chatId, $"{targetFirmCode} carisine {trimmedText}", cancellationToken);
					var aiMarkup = new InlineKeyboardMarkup(new[]
					{
						new[] { InlineKeyboardButton.WithCallbackData("👤 Cari Menüsüne Dön", $"cari_sel:{targetFirmCode}") },
						new[] { InlineKeyboardButton.WithCallbackData("🏢 Firmam Kokpiti", "firmam_menu") }
					});
					await SendReplyAsync(botClient, chatId, aiReport.MessageText, cancellationToken, aiMarkup);
				}
				catch (Exception ex)
				{
					await SendReplyAsync(botClient, chatId, $"⚠️ AI raporu oluşturulurken hata oluştu: {ex.Message}", cancellationToken);
				}
			}
			return;
		}
		// ─── SOHBET MODU AKTİFSE: Tüm mesajları direkt AI'ya gönder ───
		if (_aiAssistant != null && _aiAssistant.IsSohbetModeActive(chatId))
		{
			string chatReply = await _aiAssistant.DirectChatAsync(chatId, trimmedText, cancellationToken);
			var exitMarkup = new InlineKeyboardMarkup(new[]
			{
				new[] { InlineKeyboardButton.WithCallbackData("🔴 Sohbeti Kapat", "sohbet_kapat") },
				new[] { InlineKeyboardButton.WithCallbackData("🏢 ERP Moduna Geç", "firmam_menu") }
			});
			await SendReplyAsync(botClient, chatId, chatReply, cancellationToken, exitMarkup);
			return;
		}
		if (cleanCmdLower.StartsWith("bot:") || cleanCmdLower.StartsWith("yapayzeka:") || cleanCmdLower.StartsWith("/bot") || cleanCmdLower.StartsWith("/ai"))
		{
			await SendReplyAsync(botClient, chatId, "⏳ 🤖 **Yapay Zeka sorgulama yapıyor...**", cancellationToken);
			if (_aiAssistant != null)
			{
				var aiResult = await _aiAssistant.AnswerUserQueryWithButtonsAsync(chatId, cleanCmd, cancellationToken);
				InlineKeyboardMarkup? markup = null;
				if (aiResult.Buttons != null && aiResult.Buttons.Count > 0)
				{
					var btnRows = aiResult.Buttons.Select(b => new[] { InlineKeyboardButton.WithCallbackData(b.Text, b.CallbackData) }).ToArray();
					markup = new InlineKeyboardMarkup(btnRows);
				}
				await SendReplyAsync(botClient, chatId, aiResult.MessageText, cancellationToken, markup);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "❌ Yapay Zeka servisi aktif değil.", cancellationToken);
			}
			return;
		}
		if (cleanCmdLower.StartsWith("/onay") || cleanCmdLower.StartsWith("onay"))
		{
			string[] partsCmd = cleanCmd.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (partsCmd.Length > 1 && long.TryParse(partsCmd[1], out long targetChatId))
			{
				_telegramUserStore.UpdateUserStatus(targetChatId, "Approved");
				await SendReplyAsync(botClient, chatId, $"✅ **KULLANICI ERİŞİMİ ONAYLANDI!**\n\nChatID `{targetChatId}` için erişim yetkisi başarıyla tanımlandı.", cancellationToken);
				try
				{
					await SendReplyAsync(botClient, targetChatId, "🎉 **TEBRİKLER!**\n\nTelegram hesabınız yönetici tarafından onaylandı. Artık Logo Asistan Botunu ve tüm sorgulamaları kullanabilirsiniz!", cancellationToken);
				}
				catch { }
				return;
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "⚠️ **Kullanım:** `/onay <ChatID>`\n*Örnek:* `/onay 8993190481`", cancellationToken);
				return;
			}
		}
		if (trimmedText.StartsWith("/gib", StringComparison.OrdinalIgnoreCase))
		{
			string fNo = trimmedText.Substring(Math.Min(4, trimmedText.Length)).Trim();
			await HandleGibStatusCheckAsync(botClient, chatId, fNo, cancellationToken);
			return;
		}
		if (trimmedText.Equals("/start", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("/help", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("/baslat", StringComparison.OrdinalIgnoreCase) || trimmedText.Equals("/yardim", StringComparison.OrdinalIgnoreCase))
		{
			string comprehensiveWelcomeMessage = GetComprehensiveWelcomeMessage();
			await SendReplyAsync(botClient, chatId, comprehensiveWelcomeMessage, cancellationToken);
			LogActivity(chatId, trimmedText, "-", "-", 0m, isSuccess: true, "Yardım menüsü gönderildi.");
			return;
		}
		string[] parts = trimmedText.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 1 && !trimmedText.StartsWith("/"))
		{
			List<LogoErpFirmDto> matchedFirms = await _proposalAppService.SearchFirmsAsync(trimmedText.ToUpperInvariant(), cancellationToken);
			List<LogoErpItemStockDto> matchedItems = await _proposalAppService.SearchItemStockAsync(trimmedText.ToUpperInvariant(), cancellationToken);

			int totalMatches = (matchedFirms?.Count ?? 0) + (matchedItems?.Count ?? 0);
			if (totalMatches > 0)
			{
				if (matchedFirms != null && matchedFirms.Count == 1 && (matchedItems == null || matchedItems.Count == 0))
				{
					LogoErpFirmDto singleFirm = matchedFirms[0];
					await SendCariActionMenuAsync(botClient, chatId, singleFirm.FirmCode, singleFirm.FirmName, cancellationToken);
					LogActivity(chatId, trimmedText, singleFirm.FirmCode, "-", 0m, isSuccess: true, "Cari İşlem Menüsü açıldı: " + singleFirm.FirmCode + " - " + singleFirm.FirmName);
					return;
				}

				StringBuilder sbSearch = new StringBuilder();
				sbSearch.AppendLine($"🔍 **'{trimmedText}' İÇİN ARAMA SONUÇLARI:**\n");
				sbSearch.AppendLine("👇 *İşlem yapmak istediğiniz cari hesabı veya malzemeyi aşağıdaki butonlara tıklayarak seçiniz:*\n");

				List<InlineKeyboardButton[]> btnList = new List<InlineKeyboardButton[]>();

				if (matchedFirms != null && matchedFirms.Count > 0)
				{
					sbSearch.AppendLine($"👤 **Eşleşen Cari Hesaplar ({matchedFirms.Count} Adet):**");
					foreach (var f in matchedFirms.Take(10))
					{
						sbSearch.AppendLine($"• `{f.FirmCode}` - {f.FirmName}");
						string btnText = $"👤 {f.FirmCode} - {(f.FirmName.Length > 24 ? f.FirmName.Substring(0, 21) + "..." : f.FirmName)}";
						btnList.Add(new[] { InlineKeyboardButton.WithCallbackData(btnText, "cari_sel:" + f.FirmCode) });
					}
					sbSearch.AppendLine();
				}

				if (matchedItems != null && matchedItems.Count > 0)
				{
					sbSearch.AppendLine($"📦 **Eşleşen Malzemeler ({matchedItems.Count} Adet):**");
					foreach (var itm in matchedItems.Take(10))
					{
						string prcStr = itm.UnitPrice > 0 ? $" ({itm.UnitPrice:N2} TL)" : "";
						sbSearch.AppendLine($"• `{itm.ItemCode}` - {itm.ItemName}{prcStr}");
						string btnText = $"📦 {itm.ItemCode} - {(itm.ItemName.Length > 24 ? itm.ItemName.Substring(0, 21) + "..." : itm.ItemName)}";
						btnList.Add(new[] { InlineKeyboardButton.WithCallbackData(btnText, "stk_sel:" + itm.ItemCode) });
					}
				}

				InlineKeyboardMarkup searchMarkup = new InlineKeyboardMarkup(btnList);
				await SendReplyAsync(botClient, chatId, sbSearch.ToString(), cancellationToken, searchMarkup);
				LogActivity(chatId, trimmedText, "-", "-", 0m, isSuccess: true, $"Arama '{trimmedText}': {matchedFirms?.Count ?? 0} cari, {matchedItems?.Count ?? 0} malzeme listelendi ve butonlar gösterildi.");
				return;
			}
		}
		ProposalCreateResult result7 = await _proposalAppService.ProcessTelegramMessageAsync(messageText, chatId, cancellationToken);
		if (result7.PdfData != null && result7.PdfData.Length != 0)
		{
			await SendPdfDocumentAsync(botClient, chatId, result7.PdfData, result7.PdfFileName ?? "Ekstre.pdf", result7.Message, cancellationToken);
			LogActivity(chatId, messageText, "-", "-", 0m, isSuccess: true, "PDF Ekstresi oluşturulup Telegram'a gönderildi.");
			return;
		}
		if (!result7.IsSuccess && (result7.Message.Contains("KILAVUZU") || result7.Message.Contains("Geçersiz miktar") || result7.Message.Contains("anlaşılamadı") || result7.Message.Contains("bulunamadı")))
		{
			if (_aiAssistant != null)
			{
				try
				{
					var aiResult = await _aiAssistant.AnswerUserQueryWithButtonsAsync(chatId, trimmedText, cancellationToken);
					if (aiResult != null && !string.IsNullOrWhiteSpace(aiResult.MessageText))
					{
						InlineKeyboardMarkup? markup = null;
						if (aiResult.Buttons != null && aiResult.Buttons.Count > 0)
						{
							var btnRows = aiResult.Buttons.Select(b => new[] { InlineKeyboardButton.WithCallbackData(b.Text, b.CallbackData) }).ToArray();
							markup = new InlineKeyboardMarkup(btnRows);
						}
						await SendReplyAsync(botClient, chatId, aiResult.MessageText, cancellationToken, markup);
						LogActivity(chatId, messageText, "-", "-", 0m, isSuccess: true, "Doğal dil AI yanıtı verildi (Fallback).");
						return;
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Doğal dil AI yanıtı alınırken hata oluştu.");
				}
			}

			string text7 = ((trimmedText.Length > 40) ? trimmedText.Substring(0, 40) : trimmedText);
			InlineKeyboardMarkup replyMarkup3 = new InlineKeyboardMarkup(new InlineKeyboardButton[2][]
			{
				new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83c\udfe2 Carilerde Ara", "search_firm:" + text7) },
				new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udce6 Malzemelerde Ara", "search_item:" + text7) }
			});
			await SendReplyAsync(botClient, chatId, "❓ **'" + text7 + "' anlaşılamadı.**\n\nHatalı bir komut girdiniz veya bir arama yapmak istiyorsunuz. Lütfen aşağıdan ne aradığınızı seçiniz:", cancellationToken, replyMarkup3);
			LogActivity(chatId, messageText, "-", "-", 0m, isSuccess: false, "Bilinmeyen komut, arama seçenekleri sunuldu.");
			return;
		}
		await SendReplyAsync(botClient, chatId, result7.Message, cancellationToken);
		LogActivity(chatId, messageText, "-", "-", 0m, result7.IsSuccess, result7.Message);
		if (result7.IsSuccess && result7.Message.Contains("Siparişi Başarıyla Oluşturuldu"))
		{
			string ficheNo2 = ExtractFicheNo(result7.Message) ?? "BOT_SIPARIS";
			LogoErpOrderPrintDto orderPrintDto = new LogoErpOrderPrintDto
			{
				FicheNo = ficheNo2,
				Date = DateTime.Now,
				FirmCode = ((parts.Length != 0) ? parts[0] : "-"),
				FirmName = "Sipariş Hesabı",
				SalesmanName = GetSalesmanDisplayName(chatId),
				Lines = new List<LogoErpOrderPrintLineDto>
				{
					new LogoErpOrderPrintLineDto
					{
						ItemCode = ((parts.Length > 1) ? parts[1] : "-"),
						ItemName = "Sipariş Kalemi",
						Amount = ((parts.Length > 2 && decimal.TryParse(parts[2], out var result8)) ? result8 : 1m),
						UnitPrice = ((parts.Length > 3 && decimal.TryParse(parts[3], out var result9)) ? result9 : 0m)
					}
				}
			};
			byte[] array5 = await _proposalAppService.GetOrderPdfAsync(orderPrintDto, cancellationToken);
			if (array5 != null && array5.Length != 0)
			{
				await SendPdfDocumentAsync(botClient, chatId, array5, "Siparis_" + ficheNo2 + ".pdf", "\ud83d\udcc4 **Oluşturulan Sipariş Fişi (PDF)** ektedir. (Fiş No: `" + ficheNo2 + "`)\nİstediğiniz kişi/grupla paylaşabilirsiniz.", cancellationToken);
			}
		}
	}

	private async Task HandleDocumentUploadAsync(ITelegramBotClient botClient, Message message, Document document, CancellationToken cancellationToken)
	{
		long chatId = message.Chat.Id;
		TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId);
		if (user != null && (!user.IsApproved || user.IsBlocked))
		{
			await SendReplyAsync(botClient, chatId, "⚠\ufe0f Hesabınız yetkili değil veya yönetici onayı bekliyor.", cancellationToken);
			return;
		}
		string fileName = document.FileName ?? "Dosya";
		string caption = message.Caption?.Trim() ?? "";
		string ext = Path.GetExtension(fileName).ToLowerInvariant();
		if (ext != ".xlsx" && ext != ".xls" && ext != ".csv" && ext != ".txt" && ext != ".tsv")
		{
			await SendReplyAsync(botClient, chatId, "⚠\ufe0f Yalnızca `.xlsx`, `.csv`, `.txt` veya `.tsv` uzantılı sipariş/katalog dosyalarını otomatik işleyebiliriz.", cancellationToken);
			return;
		}
		try
		{
			await SendReplyAsync(botClient, chatId, "\ud83d\udcc4 **" + fileName + "** sipariş kataloğu taranıyor...", cancellationToken);
			TGFile tGFile = await botClient.GetFile(document.FileId, cancellationToken);
			using MemoryStream ms = new MemoryStream();
			await botClient.DownloadFile(tGFile.FilePath, ms, cancellationToken);
			ms.Position = 0L;
			List<List<string>> rawRows = new List<List<string>>();
			if (ext == ".xlsx" || ext == ".xls")
			{
				rawRows = ExcelDocumentReader.ReadXlsxRows(ms);
			}
			else
			{
				using StreamReader reader = new StreamReader(ms, Encoding.UTF8);
				string[] array = (await reader.ReadToEndAsync(cancellationToken)).Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					List<string> list = (from c in array[i].Split(new char[3] { ';', ',', '\t' })
						select c.Trim()).ToList();
					if (list.Any((string c) => !string.IsNullOrWhiteSpace(c)))
					{
						rawRows.Add(list);
					}
				}
			}
			if (rawRows.Count == 0)
			{
				await SendReplyAsync(botClient, chatId, "❌ `" + fileName + "` Excel dosyasında okunabilir veri bulunamadı.", cancellationToken);
				return;
			}
			string firmCode = "";
			string firmName = "";
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value) && !string.IsNullOrWhiteSpace(value.FirmCode))
			{
				firmCode = value.FirmCode;
				firmName = value.FirmName;
			}
			else if (!string.IsNullOrWhiteSpace(caption))
			{
				List<LogoErpFirmDto> list2 = await _proposalAppService.SearchFirmsAsync(caption, cancellationToken);
				if (list2 != null && list2.Count > 0)
				{
					if (list2.Count != 1)
					{
						await ProcessExcelPendingCariDisambiguationAsync(botClient, chatId, caption, list2, rawRows, cancellationToken);
						return;
					}
					firmCode = list2[0].FirmCode;
					firmName = list2[0].FirmName;
				}
				else
				{
					firmCode = caption;
				}
			}
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			int num4 = -1;
			int num5 = -1;
			int num6 = -1;
			int num7 = -1;
			int num8 = 0;
			for (int num9 = 0; num9 < Math.Min(rawRows.Count, 5); num9++)
			{
				List<string> list3 = rawRows[num9];
				for (int num10 = 0; num10 < list3.Count; num10++)
				{
					string text = list3[num10].Trim().ToLowerInvariant();
					if (text.Contains("cari") || text.Contains("firm"))
					{
						num5 = num10;
					}
					if (text.Contains("kod") || text.Contains("code") || text.Contains("stok"))
					{
						num = num10;
					}
					if (text.Contains("ad") || text.Contains("name") || text.Contains("açıklama") || text.Contains("unvan"))
					{
						num2 = num10;
					}
					if (text.Contains("miktar") || text.Contains("adet") || text.Contains("qty") || text.Contains("sayı"))
					{
						num3 = num10;
					}
					if (text.Contains("liste fiyat") || text.Contains("b.fiyat") || text.Contains("fiyat") || text.Contains("price") || text.Contains("tutar"))
					{
						num4 = num10;
					}
					if ((text.Contains("isk") || text.Contains("indirim")) && (text.Contains("tl") || text.Contains("tutar")))
					{
						num7 = num10;
					}
					else if (text.Contains("isk") || text.Contains("%") || text.Contains("indirim") || text.Contains("discount") || text.Contains("oran"))
					{
						num6 = num10;
					}
				}
				if (num != -1 && num3 != -1)
				{
					num8 = num9 + 1;
					break;
				}
			}
			List<OrderWizardLine> validLines = new List<OrderWizardLine>();
			for (int num11 = num8; num11 < rawRows.Count; num11++)
			{
				List<string> list4 = rawRows[num11];
				string text2 = "";
				string text3 = "";
				decimal num12 = 0m;
				decimal num13 = 0m;
				decimal discountRate = 0m;
				decimal discountAmount = 0m;
				if (num != -1 && num3 != -1)
				{
					if (num < list4.Count)
					{
						text2 = list4[num].Trim();
					}
					if (num2 != -1 && num2 < list4.Count)
					{
						text3 = list4[num2].Trim();
					}
					if (num3 < list4.Count && decimal.TryParse(list4[num3].Replace(',', '.').Replace("%", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
					{
						num12 = result;
					}
					if (num4 != -1 && num4 < list4.Count && decimal.TryParse(list4[num4].Replace(',', '.').Replace("%", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
					{
						num13 = result2;
					}
					if (num6 != -1 && num6 < list4.Count && decimal.TryParse(list4[num6].Replace('%', ' ').Replace("isk", "", StringComparison.OrdinalIgnoreCase).Replace(',', '.')
						.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result3))
					{
						if (result3 > 0m && result3 < 1m)
						{
							result3 *= 100m;
						}
						discountRate = result3;
					}
					if (num7 != -1 && num7 < list4.Count && decimal.TryParse(list4[num7].Replace(',', '.').Replace("%", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var result4))
					{
						discountAmount = result4;
					}
					if (num5 != -1 && num5 < list4.Count && string.IsNullOrWhiteSpace(firmCode))
					{
						string text4 = list4[num5].Trim();
						if (!string.IsNullOrWhiteSpace(text4) && !text4.Equals("Cari Kodu", StringComparison.OrdinalIgnoreCase))
						{
							firmCode = text4;
						}
					}
				}
				else
				{
					foreach (string item in list4)
					{
						string text5 = item.Trim();
						if (string.IsNullOrWhiteSpace(text5))
						{
							continue;
						}
						if (string.IsNullOrWhiteSpace(firmCode) && (text5.StartsWith("CARI:", StringComparison.OrdinalIgnoreCase) || text5.StartsWith("CARI KODU:", StringComparison.OrdinalIgnoreCase)))
						{
							string text6 = text5.Split(':').Last().Trim();
							if (!string.IsNullOrWhiteSpace(text6))
							{
								firmCode = text6;
								continue;
							}
						}
						if (decimal.TryParse(text5.Replace(',', '.').Replace("%", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var result5) && result5 > 0m)
						{
							if (num12 == 0m)
							{
								num12 = result5;
							}
							else if (num13 == 0m)
							{
								num13 = result5;
							}
						}
						else if (text5.Length >= 2 && !text5.Equals("Miktar", StringComparison.OrdinalIgnoreCase) && !text5.Equals("Fiyat", StringComparison.OrdinalIgnoreCase) && !text5.Equals("Malzeme Kodu", StringComparison.OrdinalIgnoreCase) && !text5.Equals("Cari Kodu", StringComparison.OrdinalIgnoreCase))
						{
							if (string.IsNullOrWhiteSpace(text2))
							{
								text2 = text5;
							}
							else if (string.IsNullOrWhiteSpace(text3))
							{
								text3 = text5;
							}
						}
					}
				}
				if (!string.IsNullOrWhiteSpace(text2) && num12 > 0m)
				{
					validLines.Add(new OrderWizardLine
					{
						ProductCode = text2,
						ProductName = (string.IsNullOrWhiteSpace(text3) ? text2 : text3),
						Quantity = num12,
						ListPrice = num13,
						DiscountRate = discountRate,
						DiscountAmount = discountAmount,
						UnitPrice = num13
					});
				}
			}
			if (validLines.Count == 0)
			{
				await SendReplyAsync(botClient, chatId, $"⚠\ufe0f `{fileName}` dosyasındaki **{rawRows.Count} satırlık kataloğunda** miktar yazılmış (**Miktar > 0**) herhangi bir malzeme bulunamadı.\nLütfen sipariş vermek istediğiniz ürünlerin **Miktar** sütununu doldurup tekrar yükleyiniz.", cancellationToken);
				return;
			}
			if (string.IsNullOrWhiteSpace(firmCode))
			{
				_wizardStates[chatId] = new OrderWizardState
				{
					ChatId = chatId,
					Step = "AwaitingFirmSelection",
					Lines = validLines
				};
				await SendReplyAsync(botClient, chatId, $"\ud83c\udfe2 `{fileName}` Excel kataloğundan **{validLines.Count} kalem sipariş** tespit edildi.\n\nLütfen siparişin oluşturulacağı **Cari Kodu veya Cari Unvanını** sohbetten yazınız (Örn: `ragıp`, `X.01.016`):", cancellationToken);
				return;
			}
			List<OrderWizardLine> invalidItems = new List<OrderWizardLine>();
			List<OrderWizardLine> verifiedLines = new List<OrderWizardLine>();
			(OrderWizardLine, List<LogoErpItemStockDto>)[] array2 = await Task.WhenAll(validLines.Select(async (OrderWizardLine orderWizardLine) => (Line: orderWizardLine, Stocks: await _proposalAppService.SearchItemStockAsync(orderWizardLine.ProductCode, cancellationToken))).ToList());
			foreach ((OrderWizardLine, List<LogoErpItemStockDto>) tuple in array2)
			{
				var (line, list5) = tuple;
				if (list5 == null || list5.Count == 0)
				{
					invalidItems.Add(line);
					continue;
				}
				LogoErpItemStockDto logoErpItemStockDto = list5.FirstOrDefault((LogoErpItemStockDto s) => s.ItemCode.Equals(line.ProductCode, StringComparison.OrdinalIgnoreCase)) ?? list5.First();
				line.ProductCode = logoErpItemStockDto.ItemCode;
				line.ProductName = logoErpItemStockDto.ItemName;
				if (line.ListPrice <= 0m && logoErpItemStockDto.UnitPrice > 0m)
				{
					line.ListPrice = logoErpItemStockDto.UnitPrice;
					line.UnitPrice = logoErpItemStockDto.UnitPrice;
				}
				verifiedLines.Add(line);
			}
			StringBuilder stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler;
			if (invalidItems.Count > 0)
			{
				_wizardStates[chatId] = new OrderWizardState
				{
					ChatId = chatId,
					FirmCode = firmCode,
					FirmName = firmName,
					Step = "ExcelValidationFailed",
					Lines = verifiedLines
				};
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("⚠\ufe0f **EXCEL SİPARİŞİ DOĞRULAMA UYARISI** (Sipariş Henüz Kaydedilmedi)\n");
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(28, 2, stringBuilder2);
				handler.AppendLiteral("• **Cari Hesabı:** `");
				handler.AppendFormatted(firmCode);
				handler.AppendLiteral("` - **");
				handler.AppendFormatted(firmName);
				handler.AppendLiteral("**");
				stringBuilder3.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(65, 2, stringBuilder2);
				handler.AppendLiteral("• **Okunan Satır:** Toplam `");
				handler.AppendFormatted(rawRows.Count);
				handler.AppendLiteral("` satırdan `");
				handler.AppendFormatted(validLines.Count);
				handler.AppendLiteral("` kalem miktar girilmiş.\n");
				stringBuilder4.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder5 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(64, 1, stringBuilder2);
				handler.AppendLiteral("❌ **Aşağıdaki ");
				handler.AppendFormatted(invalidItems.Count);
				handler.AppendLiteral(" adet malzeme Logo ERP veritabanında bulunamadı:**");
				stringBuilder5.AppendLine(ref handler);
				foreach (OrderWizardLine item2 in invalidItems)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder6 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(45, 1, stringBuilder2);
					handler.AppendLiteral("• Kod: `");
					handler.AppendFormatted(item2.ProductCode);
					handler.AppendLiteral("` (Veritabanında Yok / Pasif Malzeme)");
					stringBuilder6.AppendLine(ref handler);
				}
				stringBuilder.AppendLine("\nSiparişi kaydetmeden önce dosyanızı düzeltebilir veya hatalı kalemleri atlayıp kalanları kaydetmek için butonlara tıklayabilirsiniz:");
				InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[2][]
				{
					new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData($"⚡ Hatalı Kalemleri Atla ({verifiedLines.Count} Kalemi Onayla)", "excel_confirm_submit") },
					new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ Siparişi İptal Et", "ord_cancel") }
				});
				await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, replyMarkup);
				LogActivity(chatId, $"Excel Doğrulama Uyarısı ({invalidItems.Count} Hatalı Ürün)", firmCode, "-", 0m, isSuccess: false, "Logo ERP veritabanında bulunamayan malzeme var.");
				return;
			}
			_wizardStates[chatId] = new OrderWizardState
			{
				ChatId = chatId,
				FirmCode = firmCode,
				FirmName = firmName,
				Step = "ExcelPendingConfirmation",
				Lines = verifiedLines
			};
			StringBuilder sbPreview = new StringBuilder();
			sbPreview.AppendLine("\ud83d\udcca **İSKONTOLU EXCEL SİPARİŞ KATALOĞU ÖNİZLEME** (Sipariş Henüz Kaydedilmedi)\n");
			stringBuilder2 = sbPreview;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(39, 2, stringBuilder2);
			handler.AppendLiteral("• **Dosya:** `");
			handler.AppendFormatted(fileName);
			handler.AppendLiteral("` (Toplam ");
			handler.AppendFormatted(rawRows.Count);
			handler.AppendLiteral(" Satır Katalog)");
			stringBuilder7.AppendLine(ref handler);
			stringBuilder2 = sbPreview;
			StringBuilder stringBuilder8 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(28, 2, stringBuilder2);
			handler.AppendLiteral("• **Cari Hesabı:** `");
			handler.AppendFormatted(firmCode);
			handler.AppendLiteral("` - **");
			handler.AppendFormatted(firmName);
			handler.AppendLiteral("**");
			stringBuilder8.AppendLine(ref handler);
			stringBuilder2 = sbPreview;
			StringBuilder stringBuilder9 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(79, 1, stringBuilder2);
			handler.AppendLiteral("• **Tespit Edilen Sipariş:** **");
			handler.AppendFormatted(verifiedLines.Count);
			handler.AppendLiteral(" Kalem Ürün** (Miktarı girilen ürünler süzüldü)\n");
			stringBuilder9.AppendLine(ref handler);
			sbPreview.AppendLine("\ud83d\udccb **Taranan Sipariş Kalemleri & İskonto Detayı:**");
			int num14 = 1;
			foreach (OrderWizardLine item3 in verifiedLines.Take(15))
			{
				string value2 = (string.IsNullOrWhiteSpace(item3.ProductName) ? item3.ProductCode : item3.ProductName);
				string value3 = "";
				if (item3.DiscountRate > 0m)
				{
					value3 = $" (İsk: %{item3.DiscountRate:N1})";
				}
				else if (item3.DiscountAmount > 0m)
				{
					value3 = $" (İsk: {item3.DiscountAmount:N2} TL)";
				}
				string value4 = ((item3.EffectiveListPrice > 0m) ? $" x {item3.EffectiveListPrice:N2} TL{value3} -> Net: {item3.EffectiveUnitPrice:N2} TL = {item3.LineTotal:N2} TL" : "");
				stringBuilder2 = sbPreview;
				StringBuilder stringBuilder10 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(23, 5, stringBuilder2);
				handler.AppendLiteral("**");
				handler.AppendFormatted(num14);
				handler.AppendLiteral(".** `");
				handler.AppendFormatted(item3.ProductCode);
				handler.AppendLiteral("` - ");
				handler.AppendFormatted(value2);
				handler.AppendLiteral(" — **");
				handler.AppendFormatted(item3.Quantity, "N2");
				handler.AppendLiteral(" ADET**");
				handler.AppendFormatted(value4);
				stringBuilder10.AppendLine(ref handler);
				num14++;
			}
			if (verifiedLines.Count > 15)
			{
				stringBuilder2 = sbPreview;
				StringBuilder stringBuilder11 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
				handler.AppendLiteral("_...ve ");
				handler.AppendFormatted(verifiedLines.Count - 15);
				handler.AppendLiteral(" kalem daha_");
				stringBuilder11.AppendLine(ref handler);
			}
			decimal num15 = verifiedLines.Sum((OrderWizardLine l) => l.Quantity * l.EffectiveListPrice);
			decimal grandNet = verifiedLines.Sum((OrderWizardLine l) => l.LineTotal);
			decimal num16 = Math.Max(0m, num15 - grandNet);
			stringBuilder2 = sbPreview;
			StringBuilder stringBuilder12 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder2);
			handler.AppendLiteral("\n\ud83d\udcb0 **Brüt Toplam:** `");
			handler.AppendFormatted(num15, "N2");
			handler.AppendLiteral(" TL`");
			stringBuilder12.AppendLine(ref handler);
			if (num16 > 0m)
			{
				stringBuilder2 = sbPreview;
				StringBuilder stringBuilder13 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(29, 1, stringBuilder2);
				handler.AppendLiteral("\ud83d\udcc9 **Toplam İskonto:** `-");
				handler.AppendFormatted(num16, "N2");
				handler.AppendLiteral(" TL`");
				stringBuilder13.AppendLine(ref handler);
			}
			stringBuilder2 = sbPreview;
			StringBuilder stringBuilder14 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(30, 1, stringBuilder2);
			handler.AppendLiteral("\ud83d\udcb5 **Net Sepet Tutarı:** `");
			handler.AppendFormatted(grandNet, "N2");
			handler.AppendLiteral(" TL`");
			stringBuilder14.AppendLine(ref handler);
			sbPreview.AppendLine("\nSiparişi oluşturmadan önce PDF çıktısını inceleyebilir veya onaylayıp Logo ERP veritabanına aktarabilirsiniz:");
			string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
			int valueOrDefault = (await _logoErpRepository.GetFirmLogicalRefAsync(firmCode, firmNo, cancellationToken)).GetValueOrDefault();
			List<LogoErpOrderLineInput> lines = verifiedLines.Select((OrderWizardLine l) => new LogoErpOrderLineInput
			{
				ProductCode = l.ProductCode,
				Amount = l.Quantity,
				UnitPrice = ((l.ListPrice > 0m) ? l.ListPrice : l.UnitPrice),
				DiscountRate = l.DiscountRate,
				ItemDetails = new LogoErpItemDetails(0, 0, 0, 20.0)
			}).ToList();
			string text7 = _draftOrderStore.SaveDraft(chatId, firmCode, firmName, valueOrDefault, lines);
			InlineKeyboardMarkup replyMarkup2 = new InlineKeyboardMarkup(new InlineKeyboardButton[3][]
			{
				new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udcc4 PDF Taslağı Önizle & İndir (Sipariş Oluşturmadan)", "draft_pdf_" + text7) },
				new InlineKeyboardButton[2]
				{
					InlineKeyboardButton.WithCallbackData($"✅ Siparişi Logo ERP'ye Aktar ({verifiedLines.Count} Kalem)", "draft_confirm_" + text7),
					InlineKeyboardButton.WithCallbackData("\ud83d\udcc9 Genel İskonto Ekle", "excel_add_gen_disc")
				},
				new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ İptal Et", "draft_cancel_" + text7) }
			});
			await SendReplyAsync(botClient, chatId, sbPreview.ToString(), cancellationToken, replyMarkup2);
			LogActivity(chatId, $"Excel Katalog Önizleme ({verifiedLines.Count}/{rawRows.Count} Kalem)", firmCode, "-", grandNet, isSuccess: true, "Excel sipariş önizleme ekrana basıldı, onay bekleniyor.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Excel sipariş dosyası işlenirken hata oluştu.");
			await SendReplyAsync(botClient, chatId, "❌ `" + fileName + "` Excel dosyası okunamadı: " + ex.Message, cancellationToken);
		}
	}

	private async Task ProcessExcelPendingCariDisambiguationAsync(ITelegramBotClient botClient, long chatId, string searchTerm, List<LogoErpFirmDto> matchedFirms, List<List<string>> rawRows, CancellationToken cancellationToken)
	{
		await SendCariSelectionKeyboardAsync(botClient, chatId, searchTerm, matchedFirms, cancellationToken);
	}

	private async Task ExecuteFinalMultiLineOrderAsync(ITelegramBotClient botClient, long chatId, string firmCode, string firmName, string fileName, List<OrderWizardLine> validLines, int totalRows, CancellationToken cancellationToken)
	{
		decimal num = (_wizardStates.TryGetValue(chatId, out OrderWizardState value) ? value.GeneralDiscountRate : 0m);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(firmCode);
		foreach (OrderWizardLine validLine in validLines)
		{
			decimal effectiveListPrice = validLine.EffectiveListPrice;
			decimal num2 = validLine.DiscountRate;
			if (num > 0m)
			{
				if (num2 > 0m)
				{
					decimal num3 = (1m - num2 / 100m) * (1m - num / 100m);
					num2 = (1m - num3) * 100m;
				}
				else
				{
					num2 = num;
				}
			}
			else if (num2 <= 0m && validLine.DiscountAmount > 0m && effectiveListPrice > 0m)
			{
				num2 = validLine.DiscountAmount / effectiveListPrice * 100m;
			}
			if (num2 > 0m && effectiveListPrice > 0m)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(4, 4, stringBuilder2);
				handler.AppendFormatted(validLine.ProductCode);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(validLine.Quantity);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(effectiveListPrice.ToString("0.00", CultureInfo.InvariantCulture));
				handler.AppendLiteral(" ");
				handler.AppendFormatted(num2.ToString("0.00", CultureInfo.InvariantCulture));
				handler.AppendLiteral("%");
				stringBuilder3.AppendLine(ref handler);
			}
			else
			{
				decimal effectiveUnitPrice = validLine.EffectiveUnitPrice;
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 3, stringBuilder2);
				handler.AppendFormatted(validLine.ProductCode);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(validLine.Quantity);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(effectiveUnitPrice.ToString("0.00", CultureInfo.InvariantCulture));
				stringBuilder4.AppendLine(ref handler);
			}
		}
		ProposalCreateResult orderResult = await _proposalAppService.ProcessTelegramMessageAsync(stringBuilder.ToString().Trim(), chatId, cancellationToken);
		string ficheNo = ExtractFicheNo(orderResult.Message) ?? "BOT_SIPARIS";
		string text = $"\ud83d\udcca **İSKONTOLU EXCEL KATALOG SİPARİŞİ BAŞARIYLA OLUŞTURULDU!**\n\n• **Dosya:** `{fileName}` (Toplam {totalRows} Satır Katalog)\n• **Aktarılan Ürün:** **{validLines.Count} Kalem** (Miktarı girilen ürünler seçildi)\n• **Cari Hesabı:** `{firmCode}` - **{firmName}**\n• **Fiş No:** `{ficheNo}`\n\n" + orderResult.Message;
		await SendReplyAsync(botClient, chatId, text, cancellationToken);
		LogoErpOrderPrintDto orderPrintDto = new LogoErpOrderPrintDto
		{
			FicheNo = ficheNo,
			Date = DateTime.Now,
			FirmCode = firmCode,
			FirmName = (string.IsNullOrWhiteSpace(firmName) ? firmCode : firmName),
			SalesmanName = GetSalesmanDisplayName(chatId),
			Lines = validLines.Select((OrderWizardLine l) => new LogoErpOrderPrintLineDto
			{
				ItemCode = l.ProductCode,
				ItemName = l.ProductName,
				Amount = l.Quantity,
				ListPrice = l.EffectiveListPrice,
				DiscountRate = l.DiscountRate,
				DiscountAmount = l.DiscountAmount,
				UnitPrice = l.EffectiveUnitPrice,
				VatRate = l.VatRate
			}).ToList()
		};
		byte[] array = await _proposalAppService.GetOrderPdfAsync(orderPrintDto, cancellationToken);
		if (array != null && array.Length != 0)
		{
			await SendPdfDocumentAsync(botClient, chatId, array, "Siparis_" + ficheNo + ".pdf", "\ud83d\udcc4 **İskontolu Excel Sipariş Formu (PDF)** ektedir. (Fiş No: `" + ficheNo + "`)\nPaylaş seçeneği ile dilediğiniz kişiyle paylaşabilirsiniz.", cancellationToken);
		}
		LogActivity(chatId, $"Excel Katalog Siparişi ({validLines.Count}/{totalRows} Kalem)", firmCode, "-", validLines.Sum((OrderWizardLine l) => l.LineTotal), orderResult.IsSuccess, orderResult.Message);
	}

	private async Task HandleVoiceMessageAsync(ITelegramBotClient botClient, Message message, Voice voice, CancellationToken cancellationToken)
	{
		long chatId = message.Chat.Id;
		try
		{
			await SendReplyAsync(botClient, chatId, "\ud83c\udf99\ufe0f **Sesli Mesajınız Alındı!**\nSes kaydınız taranıyor ve metne dönüştürülüyor...", cancellationToken);
			TGFile tGFile = await botClient.GetFile(voice.FileId, cancellationToken);
			using MemoryStream ms = new MemoryStream();
			await botClient.DownloadFile(tGFile.FilePath, ms, cancellationToken);
			byte[] audioBytes = ms.ToArray();
			string transcribedText = await _transcriber.TranscribeAudioAsync(audioBytes, "voice.ogg", cancellationToken);
			if (!string.IsNullOrWhiteSpace(transcribedText))
			{
				await SendReplyAsync(botClient, chatId, "\ud83c\udf99\ufe0f **Sesli Mesajınız Metne Dönüştürüldü:**\n_\"" + transcribedText + "\"_\n\n⚙\ufe0f Talimatınız Logo ERP üzerinde işleniyor...", cancellationToken);
				ProposalCreateResult result = await _proposalAppService.ProcessTelegramMessageAsync(transcribedText, chatId, cancellationToken);
				await SendReplyAsync(botClient, chatId, result.Message, cancellationToken);
				if (result.IsSuccess && result.Message.Contains("Siparişi Başarıyla Oluşturuldu"))
				{
					string ficheNo = ExtractFicheNo(result.Message) ?? "BOT_SIPARIS";
					LogoErpOrderPrintDto orderPrintDto = new LogoErpOrderPrintDto
					{
						FicheNo = ficheNo,
						Date = DateTime.Now,
						FirmCode = "SESLI_SIPARIS",
						FirmName = "Sesli Mesaj Hesabı",
						Lines = new List<LogoErpOrderPrintLineDto>
						{
							new LogoErpOrderPrintLineDto
							{
								ItemCode = "SESLI_KALEM",
								ItemName = transcribedText,
								Amount = 1m,
								UnitPrice = 0m
							}
						}
					};
					byte[] array = await _proposalAppService.GetOrderPdfAsync(orderPrintDto, cancellationToken);
					if (array != null && array.Length != 0)
					{
						await SendPdfDocumentAsync(botClient, chatId, array, "Siparis_" + ficheNo + ".pdf", "\ud83d\udcc4 **Sesli Mesajla Oluşturulan Sipariş Formu (PDF)** ektedir. (Fiş No: `" + ficheNo + "`)", cancellationToken);
					}
				}
				LogActivity(chatId, "Sesli Mesaj İşlendi: '" + transcribedText + "'", "-", "-", 0m, result.IsSuccess, result.Message);
				return;
			}
			await SendReplyAsync(botClient, chatId, "\ud83c\udf99\ufe0f **Sesli Mesajınız Başarıyla İndirildi!**\n\n⚠\ufe0f Ses kayıtlarınızın doğrudan Türkçe metne dönüştürülüp otomatik sipariş oluşturulabilmesi için Web Panelinizdeki **'Speech-to-Text (Whisper)' API Anahtarını** aktif edebilirsiniz.\n\n\ud83d\udca1 *Şu an sözel sipariş ve sorgularınızı doğrudan metin olarak göndererek anında çalıştırabilirsiniz:*\n• `onur carisine sipariş oluştur a malzemesi 30 tane 25 liradan b malzemesi 20 tane 15 liradan`\n• `Onur carisinin ekstresini getir`\n• `intel stok miktarı kaç`", cancellationToken);
			LogActivity(chatId, "Sesli Mesaj Alındı (STT Bekleniyor)", "-", "-", 0m, isSuccess: true, "Telegram ses kaydı indirildi.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Sesli mesaj işlenirken hata oluştu.");
			await SendReplyAsync(botClient, chatId, "❌ Sesli mesaj okunamadı: " + ex.Message, cancellationToken);
		}
	}

	private async Task SendSampleExcelTemplateAsync(ITelegramBotClient botClient, long chatId, string? firmCode = null, string? firmName = null, string? groupFilter = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		string cCode = (string.IsNullOrWhiteSpace(firmCode) ? "X.01.016" : firmCode);
		string filterTerm = (string.IsNullOrWhiteSpace(groupFilter) ? "ALL" : groupFilter.Trim());
		List<LogoErpItemStockDto> list = await _proposalAppService.SearchItemStockAsync(filterTerm, cancellationToken);
		if (list == null || list.Count == 0)
		{
			list = new List<LogoErpItemStockDto>
			{
				new LogoErpItemStockDto
				{
					ItemCode = "A.001.01",
					ItemName = "A Malzemesi",
					UnitPrice = 35.00m
				},
				new LogoErpItemStockDto
				{
					ItemCode = "INTEL-I5",
					ItemName = "Intel i5 İşlemci",
					UnitPrice = 3500.00m
				},
				new LogoErpItemStockDto
				{
					ItemCode = "LAPTOP-01",
					ItemName = "Dell Laptop",
					UnitPrice = 15000.00m
				}
			};
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Cari Kodu;Malzeme Kodu;Malzeme Adı;Miktar;Birim Fiyat;İskonto %;İskonto TL");
		foreach (LogoErpItemStockDto item in list)
		{
			string value = ((item.UnitPrice > 0m) ? item.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture) : "");
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(6, 4, stringBuilder2);
			handler.AppendFormatted(cCode);
			handler.AppendLiteral(";");
			handler.AppendFormatted(item.ItemCode);
			handler.AppendLiteral(";");
			handler.AppendFormatted(item.ItemName);
			handler.AppendLiteral(";;");
			handler.AppendFormatted(value);
			handler.AppendLiteral(";;");
			stringBuilder2.AppendLine(ref handler);
		}
		byte[] buffer = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(stringBuilder.ToString())).ToArray();
		using MemoryStream ms = new MemoryStream(buffer);
		string text = ((filterTerm.Equals("ALL", StringComparison.OrdinalIgnoreCase) || filterTerm.Equals("*") || filterTerm.Equals("TÜMÜ", StringComparison.OrdinalIgnoreCase)) ? "KATALOG" : filterTerm.ToUpperInvariant());
		string fileName = ((!string.IsNullOrWhiteSpace(firmCode)) ? $"Ornek_Siparis_{firmCode}_{text}.csv" : ("Ornek_Siparis_" + text + ".csv"));
		InputFileStream document = InputFile.FromStream(ms, fileName);
		string caption = (string.IsNullOrWhiteSpace(firmCode) ? $"\ud83d\udcc4 **Logo ERP'den Çekilen Örnek İskontolu Excel Sipariş Kataloğu** ({list.Count} Malzeme - Filtre: `{text}`)\n\n• **Tanımlı Son Satış Fiyatları** otomatik doldurulmuştur.\n• Ürünlerin **Miktar** ve isteğe bağlı **İskonto %** sütununu doldurup geri gönderebilirsiniz." : $"\ud83d\udcc4 **{firmName ?? firmCode} Hesabı İçin Hazırlanan İskontolu Excel Sipariş Kataloğu** ({list.Count} Malzeme - Filtre: `{text}`)\n\nLogo ERP veritabanından çekilen tüm bu ürünlerin **Miktar** ve isteğe bağlı **İskonto %** sütununu doldurduktan sonra doğrudan bu sohbete dosya olarak atabilirsiniz.");
		await botClient.SendDocument(chatId, document, caption, ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
	}

	private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
	{
		if (callbackQuery.Message == null || string.IsNullOrEmpty(callbackQuery.Data))
		{
			return;
		}
		try
		{
			string id = callbackQuery.Id;
			CancellationToken cancellationToken2 = cancellationToken;
			await botClient.AnswerCallbackQuery(id, null, showAlert: false, null, null, cancellationToken2);
		}
		catch
		{
		}
		long chatId = callbackQuery.Message.Chat.Id;
		string data = callbackQuery.Data;
		_awaitingAiReportPrompt.TryRemove(chatId, out _);
		TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId);
		if (user != null && (!user.IsApproved || user.IsBlocked))
		{
			string id2 = callbackQuery.Id;
			CancellationToken cancellationToken2 = cancellationToken;
			await botClient.AnswerCallbackQuery(id2, "⚠\ufe0f Hesabınız yetkili değil veya yönetici onayı bekliyor.", showAlert: true, null, null, cancellationToken2);
			return;
		}
		_logger.LogInformation("Telegram CallbackQuery alındı. ChatId: {ChatId}, Data: {Data}", chatId, data);
		if (data.StartsWith("usr_approve_", StringComparison.OrdinalIgnoreCase))
		{
			if (long.TryParse(data.Substring(12), out long targetChatId))
			{
				_telegramUserStore.UpdateUserStatus(targetChatId, "Approved");
				try { await botClient.AnswerCallbackQuery(callbackQuery.Id, "✅ Kullanıcı erişimi onaylandı!", showAlert: true, cancellationToken: cancellationToken); } catch { }
				await SendReplyAsync(botClient, chatId, $"✅ **KULLANICI ERİŞİMİ ONAYLANDI!**\n\nChatID `{targetChatId}` için erişim yetkisi başarıyla tanımlandı.", cancellationToken);
				try
				{
					await SendReplyAsync(botClient, targetChatId, "🎉 **TEBRİKLER!**\n\nTelegram hesabınız yönetici tarafından onaylandı. Artık Logo Asistan Botunu ve tüm sorgulamaları kullanabilirsiniz!", cancellationToken);
				}
				catch { }
				return;
			}
		}
		OrderWizardState value;
		if (data.Equals("ord_cancel", StringComparison.OrdinalIgnoreCase))
		{
			_wizardStates.TryRemove(chatId, out value);
			await SendReplyAsync(botClient, chatId, "❌ Sipariş oluşturma sihirbazı iptal edildi.", cancellationToken);
			LogActivity(chatId, "Buton: İptal", "-", "-", 0m, isSuccess: true, "Sipariş sihirbazı iptal edildi.");
			return;
		}
		string firmCyphCode = ((user != null && !user.CanViewAllFirms && !string.IsNullOrWhiteSpace(user.AllowedFirmCyphCode)) ? user.AllowedFirmCyphCode : null);
		string itemCyphCode = ((user != null && !user.CanViewAllItems && !string.IsNullOrWhiteSpace(user.AllowedItemCyphCode)) ? user.AllowedItemCyphCode : null);
		if (data.Equals("rpt_stok", StringComparison.OrdinalIgnoreCase))
		{
			using (MemoryStream ms = new MemoryStream(await _proposalAppService.GetStockListExcelAsync(itemCyphCode, cancellationToken)))
			{
				ChatId chatId2 = chatId;
				InputFileStream document = InputFile.FromStream(ms, "StokListesi.csv");
				CancellationToken cancellationToken2 = cancellationToken;
				await botClient.SendDocument(chatId2, document, "\ud83d\udce6 **Tüm Stok Listesi (Excel)**", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken2);
				LogActivity(chatId, "Buton: Stok Listesi", "-", "-", 0m, isSuccess: true, "Stok Listesi Excel gönderildi.");
				return;
			}
		}
		if (data.Equals("rpt_cari", StringComparison.OrdinalIgnoreCase))
		{
			using (MemoryStream ms = new MemoryStream(await _proposalAppService.GetFirmListExcelAsync("ALL", firmCyphCode, cancellationToken)))
			{
				ChatId chatId3 = chatId;
				InputFileStream document2 = InputFile.FromStream(ms, "CariHesapListesi.csv");
				CancellationToken cancellationToken2 = cancellationToken;
				await botClient.SendDocument(chatId3, document2, "\ud83c\udfe2 **Tüm Cari Hesap Listesi (Excel)**\n\nBakiyeler Döviz Cinslerine Göre Ayrılmıştır.", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken2);
				LogActivity(chatId, "Buton: Cari Listesi", "-", "-", 0m, isSuccess: true, "Cari Hesap Listesi Excel gönderildi.");
				return;
			}
		}
		if (data.Equals("rpt_borc", StringComparison.OrdinalIgnoreCase))
		{
			using (MemoryStream ms = new MemoryStream(await _proposalAppService.GetFirmListExcelAsync("DEBT", firmCyphCode, cancellationToken)))
			{
				ChatId chatId4 = chatId;
				InputFileStream document3 = InputFile.FromStream(ms, "BorcluCariler.csv");
				CancellationToken cancellationToken2 = cancellationToken;
				await botClient.SendDocument(chatId4, document3, "\ud83d\udd34 **Borçlu Cari Listesi (Excel)**\n\nBakiyeler Döviz Cinslerine Göre Ayrılmıştır.", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken2);
				LogActivity(chatId, "Buton: Borçlu Cariler", "-", "-", 0m, isSuccess: true, "Borçlu Cari Listesi Excel gönderildi.");
				return;
			}
		}
		if (data.Equals("rpt_alacak", StringComparison.OrdinalIgnoreCase))
		{
			using (MemoryStream ms = new MemoryStream(await _proposalAppService.GetFirmListExcelAsync("CREDIT", firmCyphCode, cancellationToken)))
			{
				ChatId chatId5 = chatId;
				InputFileStream document4 = InputFile.FromStream(ms, "AlacakliCariler.csv");
				CancellationToken cancellationToken2 = cancellationToken;
				await botClient.SendDocument(chatId5, document4, "\ud83d\udfe2 **Alacaklı Cari Listesi (Excel)**\n\nBakiyeler Döviz Cinslerine Göre Ayrılmıştır.", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken2);
				LogActivity(chatId, "Buton: Alacaklı Cariler", "-", "-", 0m, isSuccess: true, "Alacaklı Cari Listesi Excel gönderildi.");
				return;
			}
		}
		if (data.Equals("rpt_excel_sample", StringComparison.OrdinalIgnoreCase))
		{
			await SendSampleExcelTemplateAsync(botClient, chatId, null, null, "ALL", cancellationToken);
			LogActivity(chatId, "Buton: Örnek Excel", "-", "-", 0m, isSuccess: true, "Örnek Excel şablonu Telegram'a gönderildi.");
			return;
		}
		if (data.Equals("acik_siparis_listesi", StringComparison.OrdinalIgnoreCase) || data.Equals("dt_menu", StringComparison.OrdinalIgnoreCase) || data.StartsWith("dt_menu_", StringComparison.OrdinalIgnoreCase))
		{
			string firmCodeFilter = (data.StartsWith("dt_menu_", StringComparison.OrdinalIgnoreCase) ? data.Substring("dt_menu_".Length) : null);
			await SendDateFilterMenuAsync(botClient, chatId, firmCodeFilter, cancellationToken);
			return;
		}
		if (data.StartsWith("dt_", StringComparison.OrdinalIgnoreCase))
		{
			await ProcessDateFilterCallbackAsync(botClient, chatId, data, cancellationToken);
			return;
		}
		if (data.Equals("firmam_menu", StringComparison.OrdinalIgnoreCase))
		{
			await SendFirmamMenuAsync(botClient, chatId, cancellationToken);
			return;
		}
		if (data.Equals("bnk_hierarchy", StringComparison.OrdinalIgnoreCase))
		{
			await SendBankHierarchyMenuAsync(botClient, chatId, cancellationToken);
			return;
		}
		if (data.StartsWith("bnk_card:", StringComparison.OrdinalIgnoreCase))
		{
			string bankRef = data.Substring("bnk_card:".Length);
			await SendBankAccountsListAsync(botClient, chatId, bankRef, cancellationToken);
			return;
		}
		if (data.StartsWith("bnk_acc:", StringComparison.OrdinalIgnoreCase))
		{
			string accRef = data.Substring("bnk_acc:".Length);
			await SendBankAccountDetailAsync(botClient, chatId, accRef, cancellationToken);
			return;
		}
		if (data.StartsWith("rpt_dt_sel:", StringComparison.OrdinalIgnoreCase))
		{
			string rptId = data.Substring("rpt_dt_sel:".Length);
			await SendReportDateSelectionForCustomReportAsync(botClient, chatId, rptId, cancellationToken);
			return;
		}
		if (data.StartsWith("rpt_run_dt:", StringComparison.OrdinalIgnoreCase))
		{
			string[] dtParts = data.Substring("rpt_run_dt:".Length).Split(':');
			if (dtParts.Length >= 2)
			{
				string rptId = dtParts[0];
				string range = dtParts[1];
				await ExecuteCustomReportWithDateRangeAsync(botClient, chatId, rptId, range, cancellationToken);
			}
			return;
		}
		if (data.StartsWith("firmam_cat:", StringComparison.OrdinalIgnoreCase))
		{
			string cat = data.Substring("firmam_cat:".Length);
			await SendReportsMenuAsync(botClient, chatId, cat, cancellationToken);
			return;
		}
		if (data.StartsWith("rpt_run:", StringComparison.OrdinalIgnoreCase))
		{
			string rptId = data.Substring("rpt_run:".Length);
			await ExecuteCustomReportAsync(botClient, chatId, rptId, null, null, cancellationToken);
			return;
		}
		if (data.StartsWith("rpt_run_cari:", StringComparison.OrdinalIgnoreCase))
		{
			string[] rptParts = data.Substring("rpt_run_cari:".Length).Split(':');
			if (rptParts.Length >= 2)
			{
				string rptId = rptParts[0];
				string firmCode = rptParts[1];
				await ExecuteCustomReportAsync(botClient, chatId, rptId, firmCode, null, cancellationToken);
			}
			return;
		}
		if (data.StartsWith("rpt_run_item:", StringComparison.OrdinalIgnoreCase))
		{
			string[] rptParts = data.Substring("rpt_run_item:".Length).Split(':');
			if (rptParts.Length >= 2)
			{
				string rptId = rptParts[0];
				string itemCode = rptParts[1];
				await ExecuteCustomReportAsync(botClient, chatId, rptId, null, itemCode, cancellationToken);
			}
			return;
		}
		if (data.StartsWith("ord_det_", StringComparison.OrdinalIgnoreCase))
		{
			string ficheNo = data.Substring("ord_det_".Length);
			await SendOpenOrderDetailAsync(botClient, chatId, ficheNo, cancellationToken);
			return;
		}
		if (data.StartsWith("ord_pdf_", StringComparison.OrdinalIgnoreCase))
		{
			string ficheNo2 = data.Substring("ord_pdf_".Length);
			await SendOrderPdfDocumentAsync(botClient, chatId, ficheNo2, cancellationToken);
			return;
		}
		if (data.StartsWith("draft_pdf_", StringComparison.OrdinalIgnoreCase))
		{
			string draftId = data.Substring("draft_pdf_".Length);
			await SendDraftPdfPreviewAsync(botClient, chatId, draftId, cancellationToken);
			return;
		}
		if (data.StartsWith("draft_confirm_", StringComparison.OrdinalIgnoreCase))
		{
			string draftId2 = data.Substring("draft_confirm_".Length);
			await ConfirmDraftOrderAsync(botClient, chatId, draftId2, cancellationToken);
			return;
		}
		if (data.StartsWith("draft_cancel_", StringComparison.OrdinalIgnoreCase))
		{
			string draftId3 = data.Substring("draft_cancel_".Length);
			_draftOrderStore.RemoveDraft(draftId3);
			_wizardStates.TryRemove(chatId, out value);
			await SendReplyAsync(botClient, chatId, "❌ Excel sipariş taslağı iptal edildi.", cancellationToken);
			return;
		}
		if (data.Equals("menu_dashboard", StringComparison.OrdinalIgnoreCase))
		{
			await SendReplyAsync(botClient, chatId, "📊 **Dashboard Özeti**\n\nŞu anki geliştirme sürümünde Dashboard verileri hazırlanıyor...", cancellationToken);
			return;
		}
		if (data.Equals("menu_vade_takip", StringComparison.OrdinalIgnoreCase))
		{
			await SendReplyAsync(botClient, chatId, "⏱️ **Vade Takip Sistemi**\n\nVadesi geçen borçlar listesi hesaplanıyor...", cancellationToken);
			if (_executiveReport != null)
			{
				var report = await _executiveReport.GenerateDailySummaryAsync();
				if (report != null)
				{
					await SendReplyAsync(botClient, chatId, $"⚠️ <b>Vadesi Geçen Toplam Alacak:</b> {report.OverdueReceivablesTotal:N2} TL ({report.OverdueCustomerCount} Müşteri)", cancellationToken);
				}
			}
			return;
		}
		if (data.Equals("menu_sms", StringComparison.OrdinalIgnoreCase))
		{
			await SendReplyAsync(botClient, chatId, "📱 **SMS Gönderimi**\n\nLütfen göndermek istediğiniz mesajı ve numarayı web portalı (SMS Modülü) üzerinden gönderiniz.", cancellationToken);
			return;
		}
		if (data.Equals("menu_create_order", StringComparison.OrdinalIgnoreCase))
		{
			_wizardStates[chatId] = new OrderWizardState
			{
				ChatId = chatId,
				IsDispatchMode = false,
				Step = "AwaitingFirmSearchForOrder"
			};
			await SendReplyAsync(botClient, chatId, "\ud83d\uded2 **SİPARİŞ OLUŞTURMA SİHİRBAZI**\n\nLütfen işlem yapmak istediğiniz **Cari Unvanı veya Cari Kodu** giriniz (Örn: `RAGIP` veya `MUSTERI01`):", cancellationToken);
			LogActivity(chatId, "Buton: Sipariş Oluştur", "-", "-", 0m, isSuccess: true, "Sipariş sihirbazı başlatıldı.");
			return;
		}
		if (data.Equals("menu_create_dispatch", StringComparison.OrdinalIgnoreCase))
		{
			_wizardStates[chatId] = new OrderWizardState
			{
				ChatId = chatId,
				IsDispatchMode = true,
				Step = "AwaitingFirmSearchForDispatch"
			};
			await SendReplyAsync(botClient, chatId, "\ud83d\ude9a **SATIŞ İRSALİYESİ KESME SİHİRBAZI**\n\nLütfen irsaliye kesilecek **Cari Unvanı veya Cari Kodu** giriniz (Örn: `RAGIP` veya `MUSTERI01`):", cancellationToken);
			LogActivity(chatId, "Buton: İrsaliye Kes", "-", "-", 0m, isSuccess: true, "İrsaliye kesme sihirbazı başlatıldı.");
			return;
		}
		if (data.Equals("start_item_wizard", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value2))
			{
				value2.Step = "AwaitingProductSearch";
				string text = (value2.IsDispatchMode ? "\ud83d\ude9a **İRSALİYE KALEMİ EKLEME**\nLütfen eklemek istediğiniz **Malzeme Adı veya Kodu** yazınız:" : "\ud83d\uded2 **SİPARİŞ KALEMİ EKLEME**\nLütfen eklemek istediğiniz **Malzeme Adı veya Kodu** yazınız:");
				await SendReplyAsync(botClient, chatId, text, cancellationToken);
			}
			return;
		}
		if (data.Equals("start_excel_upload", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value3))
			{
				value3.Step = "AwaitingExcelUpload";
				await SendSampleExcelTemplateAsync(botClient, chatId, value3.FirmCode, value3.FirmName, "ALL", cancellationToken);
			}
			return;
		}
		if (data.Equals("ord_carrier_info", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value4))
			{
				value4.Step = "AwaitingCarrierInfo";
				await SendReplyAsync(botClient, chatId, "\ud83d\ude9a **TAŞIYICI VEYA ŞÖFÖR BİLGİSİ GİRİŞİ (ZORUNLU)**\n\ne-İrsaliye GİB standartları gereği **Taşıyıcı Kodu** veya **Şöför Bilgilerini (Ad Soyad / TCKN / Plaka)** girmeniz zorunludur.\n\n• Örn Taşıyıcı Kodu: `TAS001` veya `ARAS KARGO`\n• Örn Şöför Bilgisi: `34 ABC 123 - Ahmet Yılmaz`\n\nLütfen taşıyıcı veya şöför bilgisini yazınız:", cancellationToken);
			}
			return;
		}
		if (data.Equals("ord_general_description", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState wizardDesc))
			{
				wizardDesc.Step = "AwaitingGeneralDescription";
				await SendReplyAsync(botClient, chatId, "📝 **GENEL AÇIKLAMA / NOT GİRİŞİ**\n\nBelgenin altında (Açıklamalar / Not kısmında) yer almasını istediğiniz **Genel Açıklama** metnini yazınız (Örn: `Teslimat şantiye adresine yapılacaktır.`):", cancellationToken);
			}
			return;
		}
		if (data.Equals("draft_irs_pdf_preview", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value5))
			{
				await SendDraftDispatchPdfPreviewAsync(botClient, chatId, value5, cancellationToken);
			}
			return;
		}
		if (data.Equals("draft_inv_pdf_preview", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState valueInvPreview))
			{
				await SendDraftInvoicePdfPreviewAsync(botClient, chatId, valueInvPreview, cancellationToken);
			}
			return;
		}
		if (data.Equals("ord_submit_final_dispatch", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value6) && value6.Lines.Count > 0)
			{
				await ExecuteFinalDispatchFromWizardAsync(botClient, chatId, value6, cancellationToken);
				_wizardStates.TryRemove(chatId, out value);
			}
			return;
		}
		if (data.StartsWith("ord_to_irs_", StringComparison.OrdinalIgnoreCase))
		{
			string orderFicheNo = data.Substring("ord_to_irs_".Length);
			await ConvertOrderToDispatchHandlerAsync(botClient, chatId, orderFicheNo, cancellationToken);
			return;
		}
		if (data.StartsWith("act:irsaliye:", StringComparison.OrdinalIgnoreCase))
		{
			string fCode = data.Substring("act:irsaliye:".Length);
			string firmName = (await _proposalAppService.SearchFirmsAsync(fCode, cancellationToken)).FirstOrDefault()?.FirmName ?? fCode;
			_wizardStates[chatId] = new OrderWizardState
			{
				ChatId = chatId,
				FirmCode = fCode,
				FirmName = firmName,
				IsDispatchMode = true,
				Step = "AwaitingMethodSelection"
			};
			await SendOrderOrDispatchMethodSelectionAsync(botClient, chatId, _wizardStates[chatId], cancellationToken);
			LogActivity(chatId, "Buton: Doğrudan İrsaliye Kes", fCode, "-", 0m, isSuccess: true, "İrsaliye kesme yöntemi soruldu.");
			return;
		}
		if (data.StartsWith("act:c_sip_satis:", StringComparison.OrdinalIgnoreCase))
		{
			string firmCodeFilter2 = data.Substring("act:c_sip_satis:".Length);
			TelegramUserMappingDto user2 = _telegramUserStore.GetUser(chatId);
			if (user2 == null || user2.CanViewSalesOrders)
			{
				await SendOpenOrdersListAsync(botClient, chatId, firmCodeFilter2, cancellationToken);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "⛔ Satış siparişlerini görme yetkiniz bulunmamaktadır.", cancellationToken);
			}
			return;
		}
		if (data.StartsWith("act:c_sip_alis:", StringComparison.OrdinalIgnoreCase))
		{
			string firmCodeFilter3 = data.Substring("act:c_sip_alis:".Length);
			TelegramUserMappingDto user3 = _telegramUserStore.GetUser(chatId);
			if (user3 == null || user3.CanViewPurchaseOrders)
			{
				await SendPurchaseOrderListAsync(botClient, chatId, firmCodeFilter3, cancellationToken);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "⛔ Alış siparişlerini görme yetkiniz bulunmamaktadır.", cancellationToken);
			}
			return;
		}
		if (data.StartsWith("act:c_irs_satis:", StringComparison.OrdinalIgnoreCase))
		{
			string firmCodeFilter4 = data.Substring("act:c_irs_satis:".Length);
			TelegramUserMappingDto user4 = _telegramUserStore.GetUser(chatId);
			if (user4 == null || user4.CanViewSalesDispatches)
			{
				await SendDispatchListAsync(botClient, chatId, firmCodeFilter4, cancellationToken);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "⛔ Satış irsaliyelerini görme yetkiniz bulunmamaktadır.", cancellationToken);
			}
			return;
		}
		if (data.StartsWith("act:c_irs_alis:", StringComparison.OrdinalIgnoreCase))
		{
			string firmCodeFilter5 = data.Substring("act:c_irs_alis:".Length);
			TelegramUserMappingDto user5 = _telegramUserStore.GetUser(chatId);
			if (user5 == null || user5.CanViewPurchaseDispatches)
			{
				await SendPurchaseDispatchListAsync(botClient, chatId, firmCodeFilter5, cancellationToken);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "⛔ Alış irsaliyelerini görme yetkiniz bulunmamaktadır.", cancellationToken);
			}
			return;
		}
		if (data.StartsWith("act:c_fat_satis:", StringComparison.OrdinalIgnoreCase))
		{
			string firmCodeFilter6 = data.Substring("act:c_fat_satis:".Length);
			TelegramUserMappingDto user6 = _telegramUserStore.GetUser(chatId);
			if (user6 == null || user6.CanViewSalesInvoices)
			{
				await SendInvoiceListAsync(botClient, chatId, firmCodeFilter6, cancellationToken);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "⛔ Satış faturalarını görme yetkiniz bulunmamaktadır.", cancellationToken);
			}
			return;
		}
		if (data.StartsWith("act:c_fat_alis:", StringComparison.OrdinalIgnoreCase))
		{
			string firmCodeFilter7 = data.Substring("act:c_fat_alis:".Length);
			TelegramUserMappingDto user7 = _telegramUserStore.GetUser(chatId);
			if (user7 == null || user7.CanViewPurchaseInvoices)
			{
				await SendPurchaseInvoiceListAsync(botClient, chatId, firmCodeFilter7, cancellationToken);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "⛔ Alış faturalarını görme yetkiniz bulunmamaktadır.", cancellationToken);
			}
			return;
		}
		if (data.StartsWith("pdoc_det:", StringComparison.OrdinalIgnoreCase))
		{
			string[] array = data.Substring("pdoc_det:".Length).Split(':');
			string text2 = ((array.Length != 0) ? array[0] : "");
			string text3 = ((array.Length > 1) ? array[1] : "");
			await SendReplyAsync(botClient, chatId, text2 switch
			{
				"sip" => "\ud83d\udce5 **ALIŞ SİPARİŞİ DETAYI**", 
				"irs" => "\ud83d\udce6 **ALIŞ İRSALİYESİ DETAYI**", 
				"fat" => "\ud83e\uddfe **ALIŞ FATURASI DETAYI**", 
				_ => "\ud83d\udcc4 **ALIŞ BELGESİ DETAYI**", 
			} + "\n\n\ud83d\udccc **Fiş No:** `" + text3 + "`\n\ud83d\udd12 _Alış belgeleri sistemde salt okunur olarak tutulmaktadır. Detaylı kalem ve tutar incelemeleri için Logo ERP arayüzünü kullanabilirsiniz._", cancellationToken);
			return;
		}
		if (data.StartsWith("act:c_sip:", StringComparison.OrdinalIgnoreCase))
		{
			string firmCodeFilter8 = data.Substring("act:c_sip:".Length);
			await SendOpenOrdersListAsync(botClient, chatId, firmCodeFilter8, cancellationToken);
			return;
		}
		if (data.StartsWith("act:c_irs:", StringComparison.OrdinalIgnoreCase))
		{
			string firmCodeFilter9 = data.Substring("act:c_irs:".Length);
			await SendDispatchListAsync(botClient, chatId, firmCodeFilter9, cancellationToken);
			return;
		}
		if (data.StartsWith("act:c_fat:", StringComparison.OrdinalIgnoreCase))
		{
			string firmCodeFilter10 = data.Substring("act:c_fat:".Length);
			await SendInvoiceListAsync(botClient, chatId, firmCodeFilter10, cancellationToken);
			return;
		}
		if (data.StartsWith("ord_opt:", StringComparison.OrdinalIgnoreCase))
		{
			string ficheNo3 = data.Substring("ord_opt:".Length);
			await SendOrderActionOptionsAsync(botClient, chatId, ficheNo3, cancellationToken);
			return;
		}
		if (data.StartsWith("ord_det:", StringComparison.OrdinalIgnoreCase))
		{
			string ficheNo4 = data.Substring("ord_det:".Length);
			await SendOrderDetailModalAsync(botClient, chatId, ficheNo4, cancellationToken);
			return;
		}
		if (data.Equals("irsaliye_listesi", StringComparison.OrdinalIgnoreCase))
		{
			await SendDispatchListAsync(botClient, chatId, null, cancellationToken);
			return;
		}
		if (data.Equals("fatura_listesi", StringComparison.OrdinalIgnoreCase))
		{
			await SendInvoiceListAsync(botClient, chatId, null, cancellationToken);
			return;
		}
		if (data.StartsWith("inv_opts_", StringComparison.OrdinalIgnoreCase))
		{
			string text4 = data.Substring("inv_opts_".Length);
			List<InlineKeyboardButton[]> optButtons = new List<InlineKeyboardButton[]>();
			optButtons.Add(new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("📄 PDF (e-Fatura/e-Arşiv)", "inv_pdf_" + text4),
				InlineKeyboardButton.WithCallbackData("📄 PDF (Standart)", "inv_pdf_std_" + text4)
			});
			optButtons.Add(new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("📧 E-Posta Gönder", "send_mail:inv:" + text4),
				InlineKeyboardButton.WithCallbackData("📱 SMS Gönder", "send_sms:inv:" + text4)
			});
			if (CanSendEBelge(chatId))
			{
				optButtons.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("🗳️ GİB'e Gönder", "gib_send_FAT_" + text4) });
			}
			optButtons.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("🔙 Fatura Listesine Dön", "fatura_listesi") });
			InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(optButtons);
			await SendReplyAsync(botClient, chatId, $"🧾 **FATURA İŞLEMLERİ: `{text4}`**\n\nLütfen yapmak istediğiniz işlemi seçiniz:", cancellationToken, replyMarkup);
			return;
		}
		if (data.StartsWith("irs_opts_", StringComparison.OrdinalIgnoreCase))
		{
			string text5 = data.Substring("irs_opts_".Length);
			List<InlineKeyboardButton[]> optButtons2 = new List<InlineKeyboardButton[]>();
			optButtons2.Add(new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("🚚 PDF (e-İrsaliye)", "irs_pdf_" + text5),
				InlineKeyboardButton.WithCallbackData("📄 PDF (Standart)", "irs_pdf_std_" + text5)
			});
			optButtons2.Add(new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("📧 E-Posta Gönder", "send_mail:irs:" + text5),
				InlineKeyboardButton.WithCallbackData("📱 SMS Gönder", "send_sms:irs:" + text5)
			});
			if (CanSendEBelge(chatId))
			{
				optButtons2.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("🗳️ GİB'e Gönder", "gib_send_IRS_" + text5) });
			}
			optButtons2.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("🔙 İrsaliye Listesine Dön", "irsaliye_listesi") });
			InlineKeyboardMarkup replyMarkup2 = new InlineKeyboardMarkup(optButtons2);
			await SendReplyAsync(botClient, chatId, $"🚚 **İRSALİYE İŞLEMLERİ: `{text5}`**\n\nLütfen yapmak istediğiniz işlemi seçiniz:", cancellationToken, replyMarkup2);
			return;
		}
		if (data.StartsWith("inv_pdf_std_", StringComparison.OrdinalIgnoreCase))
		{
			string ficheNo5 = data.Substring("inv_pdf_std_".Length);
			await SendStandardPdfDocumentAsync(botClient, chatId, ficheNo5, isInvoice: true, cancellationToken);
			return;
		}
		if (data.StartsWith("irs_pdf_std_", StringComparison.OrdinalIgnoreCase))
		{
			string ficheNo6 = data.Substring("irs_pdf_std_".Length);
			await SendStandardPdfDocumentAsync(botClient, chatId, ficheNo6, isInvoice: false, cancellationToken);
			return;
		}
		if (data.StartsWith("inv_pdf_", StringComparison.OrdinalIgnoreCase))
		{
			string ficheNo7 = data.Substring("inv_pdf_".Length);
			await SendInvoicePdfDocumentAsync(botClient, chatId, ficheNo7, cancellationToken);
			return;
		}
		if (data.StartsWith("irs_pdf_", StringComparison.OrdinalIgnoreCase))
		{
			string ficheNo8 = data.Substring("irs_pdf_".Length);
			await SendDispatchPdfDocumentAsync(botClient, chatId, ficheNo8, cancellationToken);
			return;
		}
		if (data.StartsWith("gib_send_", StringComparison.OrdinalIgnoreCase))
		{
			await HandleEBelgeGibSendAsync(botClient, chatId, data, cancellationToken);
			return;
		}
		if (data.StartsWith("inv_html_", StringComparison.OrdinalIgnoreCase))
		{
			string ficheNo9 = data.Substring("inv_html_".Length);
			await SendInvoiceXsltDocumentAsync(botClient, chatId, ficheNo9, cancellationToken);
			return;
		}
		if (data.StartsWith("irs_html_", StringComparison.OrdinalIgnoreCase))
		{
			string ficheNo10 = data.Substring("irs_html_".Length);
			await SendDispatchXsltDocumentAsync(botClient, chatId, ficheNo10, cancellationToken);
			return;
		}
		if (data.Equals("excel_confirm_submit", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value7) && (value7.Step == "ExcelPendingConfirmation" || value7.Step == "ExcelValidationFailed") && value7.Lines.Count > 0)
			{
				await ExecuteFinalMultiLineOrderAsync(botClient, chatId, value7.FirmCode, value7.FirmName, "Excel Kataloğu", value7.Lines, value7.Lines.Count, cancellationToken);
				_wizardStates.TryRemove(chatId, out value);
			}
			return;
		}
		OrderWizardState wizard;
		if (data.StartsWith("excel_grp:"))
		{
			string fCode = data.Substring("excel_grp:".Length);
			if (_wizardStates.TryGetValue(chatId, out wizard))
			{
				wizard.Step = "AwaitingExcelUpload";
				await SendSampleExcelTemplateAsync(botClient, chatId, wizard.FirmCode, wizard.FirmName, fCode, cancellationToken);
				LogActivity(chatId, "Grup Butonu: Excel Kataloğu (" + fCode + ")", wizard.FirmCode, "-", 0m, isSuccess: true, "Grup '" + fCode + "' seçildi ve Excel gönderildi.");
			}
			return;
		}
		if (data.Equals("ord_add_item", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out wizard))
			{
				wizard.Step = "AwaitingProductSearch";
				await SendReplyAsync(botClient, chatId, $"\ud83d\uded2 **YENİ KALEM EKLEME** (`{wizard.Lines.Count + 1}. Kalem`)\nLütfen eklemek istediğiniz **Malzeme Adı veya Kodu** yazınız:", cancellationToken);
				LogActivity(chatId, "Buton: Başka Kalem Ekle", wizard.FirmCode, "-", 0m, isSuccess: true, "Sipariş sihirbazına yeni kalem ekleme başlatıldı.");
			}
			return;
		}
		if (data.Equals("ord_add_gen_disc", StringComparison.OrdinalIgnoreCase) || data.Equals("excel_add_gen_disc", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out wizard))
			{
				wizard.Step = (data.Equals("excel_add_gen_disc", StringComparison.OrdinalIgnoreCase) ? "AwaitingExcelGeneralDiscount" : "AwaitingGeneralDiscount");
				await SendReplyAsync(botClient, chatId, "\ud83d\udcc9 **GENEL İSKONTO GİRİŞİ**\n\nTüm siparişin genel toplamına uygulanacak **Genel İskonto Oranını (%)** veya **Tutarını (TL)** yazınız:\n\n• Örn: `%10` indirim için: `10` veya `10%`\n• Örn: `500 TL` indirim için: `500TL` veya `500 TL`", cancellationToken);
				LogActivity(chatId, "Buton: Genel İskonto Ekle", wizard.FirmCode, "-", 0m, isSuccess: true, "Genel iskonto girdisi istendi.");
			}
			return;
		}
		if (data.Equals("ord_edit_cart", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value8))
			{
				await SendCartEditMenuAsync(botClient, chatId, value8, cancellationToken);
			}
			return;
		}
		if (data.StartsWith("ord_edit_line:", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value9) && int.TryParse(data.Substring("ord_edit_line:".Length), out var result) && result >= 0 && result < value9.Lines.Count)
			{
				OrderWizardLine orderWizardLine = value9.Lines[result];
				string value10 = (string.IsNullOrWhiteSpace(orderWizardLine.ProductName) ? orderWizardLine.ProductCode : orderWizardLine.ProductName);
				string text6 = $"✏\ufe0f **{result + 1}. Kalem Düzenleme: {value10}**\n\n• **Mevcut Miktar:** `{orderWizardLine.Quantity:N2}`\n• **Birim Fiyat:** `{orderWizardLine.EffectiveUnitPrice:N2} TL`\n• **Top. Tutar:** `{orderWizardLine.LineTotal:N2} TL`\n\nLütfen bu kalem üzerinde yapmak istediğiniz işlemi seçiniz:";
				List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
				list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udd04 Miktar / Fiyat Değiştir", $"ord_chg_line:{result}") });
				list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ Kalemi Sepetten Çıkart", $"ord_del_line:{result}") });
				list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udd19 Düzenlemeye Dön", "ord_edit_cart") });
				List<InlineKeyboardButton[]> inlineKeyboard = list;
				await SendReplyAsync(botClient, chatId, text6, cancellationToken, new InlineKeyboardMarkup(inlineKeyboard));
			}
			return;
		}
		if (data.StartsWith("ord_del_line:", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out wizard) && int.TryParse(data.Substring("ord_del_line:".Length), out var result2) && result2 >= 0 && result2 < wizard.Lines.Count)
			{
				string text7 = (string.IsNullOrWhiteSpace(wizard.Lines[result2].ProductName) ? wizard.Lines[result2].ProductCode : wizard.Lines[result2].ProductName);
				wizard.Lines.RemoveAt(result2);
				LogActivity(chatId, "Buton: Kalem Sil", wizard.FirmCode, "-", 0m, isSuccess: true, "Kalem sepetten çıkartıldı: " + text7);
				if (wizard.Lines.Count == 0)
				{
					_wizardStates.TryRemove(chatId, out value);
					await SendReplyAsync(botClient, chatId, "\ud83d\uddd1\ufe0f `" + text7 + "` sepetten çıkartıldı. Sepette kalem kalmadığı için sipariş iptal edildi.", cancellationToken);
				}
				else
				{
					await SendReplyAsync(botClient, chatId, "✅ `" + text7 + "` sepetten başarıyla çıkartıldı.", cancellationToken);
					await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
				}
			}
			return;
		}
		if (data.StartsWith("ord_chg_line:", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value11) && int.TryParse(data.Substring("ord_chg_line:".Length), out var result3) && result3 >= 0 && result3 < value11.Lines.Count)
			{
				value11.EditingLineIndex = result3;
				value11.Step = "AwaitingLineEditQuantity";
				OrderWizardLine orderWizardLine2 = value11.Lines[result3];
				string text8 = (string.IsNullOrWhiteSpace(orderWizardLine2.ProductName) ? orderWizardLine2.ProductCode : orderWizardLine2.ProductName);
				await SendReplyAsync(botClient, chatId, "\ud83d\udd04 **" + text8 + "** için yeni **Miktarı** (veya tek seferde Miktar*Fiyat, örn: `10*250`) yazınız:\n\n*(İptal için /iptal yazabilirsiniz)*", cancellationToken);
			}
			return;
		}
		if (data.Equals("ord_back_to_summary", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value12))
			{
				await SendShoppingCartSummaryAsync(botClient, chatId, value12, cancellationToken);
			}
			return;
		}
		if (data.Equals("ord_submit", StringComparison.OrdinalIgnoreCase))
		{
			if (!_wizardStates.TryGetValue(chatId, out wizard) || wizard.Lines.Count <= 0)
			{
				return;
			}
			if (wizard.IsDispatchMode)
			{
				if (string.IsNullOrWhiteSpace(wizard.CarrierOrDriverInfo))
				{
					wizard.Step = "AwaitingCarrierInfo";
					await SendReplyAsync(botClient, chatId, "\ud83d\ude9a **TAŞIYICI VEYA ŞÖFÖR BİLGİSİ GİRİŞİ (ZORUNLU)**\n\ne-İrsaliye GİB standartları gereği **Taşıyıcı Kodu** veya **Şöför Bilgilerini (Ad Soyad / TCKN / Plaka)** girmeniz zorunludur.\n\n• Örn Taşıyıcı Kodu: `TAS001` veya `ARAS KARGO`\n• Örn Şöför Bilgisi: `34 ABC 123 - Ahmet Yılmaz`\n\nLütfen taşıyıcı veya şöför bilgisini yazınız:", cancellationToken);
				}
				else
				{
					await SendDraftDispatchConfirmationAsync(botClient, chatId, wizard, cancellationToken);
				}
				return;
			}
			if (wizard.IsInvoiceMode)
			{
				await ExecuteInvoiceWizardAsync(botClient, chatId, wizard, cancellationToken);
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(wizard.FirmCode);
			foreach (OrderWizardLine line in wizard.Lines)
			{
				decimal effectiveListPrice = line.EffectiveListPrice;
				decimal num = line.DiscountRate;
				if (wizard.GeneralDiscountRate > 0m)
				{
					if (num > 0m)
					{
						decimal num2 = (1m - num / 100m) * (1m - wizard.GeneralDiscountRate / 100m);
						num = (1m - num2) * 100m;
					}
					else
					{
						num = wizard.GeneralDiscountRate;
					}
				}
				else if (num <= 0m && line.DiscountAmount > 0m && effectiveListPrice > 0m)
				{
					num = line.DiscountAmount / effectiveListPrice * 100m;
				}
				if (num > 0m && effectiveListPrice > 0m)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder3 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(4, 4, stringBuilder2);
					handler.AppendFormatted(line.ProductCode);
					handler.AppendLiteral(" ");
					handler.AppendFormatted(line.Quantity);
					handler.AppendLiteral(" ");
					handler.AppendFormatted(effectiveListPrice.ToString("0.00", CultureInfo.InvariantCulture));
					handler.AppendLiteral(" ");
					handler.AppendFormatted(num.ToString("0.00", CultureInfo.InvariantCulture));
					handler.AppendLiteral("%");
					stringBuilder3.AppendLine(ref handler);
				}
				else
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder4 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 3, stringBuilder2);
					handler.AppendFormatted(line.ProductCode);
					handler.AppendLiteral(" ");
					handler.AppendFormatted(line.Quantity);
					handler.AppendLiteral(" ");
					handler.AppendFormatted(line.EffectiveUnitPrice.ToString("0.00", CultureInfo.InvariantCulture));
					stringBuilder4.AppendLine(ref handler);
				}
			}
			string messageText = stringBuilder.ToString().Trim();
			ProposalCreateResult orderResult = await _proposalAppService.ProcessTelegramMessageAsync(messageText, chatId, cancellationToken);
			await SendReplyAsync(botClient, chatId, orderResult.Message, cancellationToken);
			string fCode = ExtractFicheNo(orderResult.Message) ?? "BOT_SIPARIS";
			LogoErpOrderPrintDto orderPrintDto = new LogoErpOrderPrintDto
			{
				FicheNo = fCode,
				Date = DateTime.Now,
				FirmCode = wizard.FirmCode,
				FirmName = wizard.FirmName,
				SalesmanName = GetSalesmanDisplayName(chatId),
				Lines = wizard.Lines.Select((OrderWizardLine l) => new LogoErpOrderPrintLineDto
				{
					ItemCode = l.ProductCode,
					ItemName = (string.IsNullOrWhiteSpace(l.ProductName) ? l.ProductCode : l.ProductName),
					Amount = l.Quantity,
					ListPrice = l.EffectiveListPrice,
					DiscountRate = l.DiscountRate,
					DiscountAmount = l.DiscountAmount,
					UnitPrice = l.EffectiveUnitPrice,
					VatRate = l.VatRate
				}).ToList()
			};
			byte[] array2 = await _proposalAppService.GetOrderPdfAsync(orderPrintDto, cancellationToken);
			if (array2 != null && array2.Length != 0)
			{
				await SendPdfDocumentAsync(botClient, chatId, array2, "Siparis_" + fCode + ".pdf", "\ud83d\udcc4 **Sipariş Formu (PDF)** ektedir. (Fiş No: `" + fCode + "`)\n'Paylaş' butonunu kullanarak dilediğiniz kişi veya gruba gönderebilirsiniz.", cancellationToken);
			}
			_wizardStates.TryRemove(chatId, out value);
			LogActivity(chatId, $"Sipariş Oluşturuldu ({wizard.Lines.Count} Kalem)", wizard.FirmCode, "-", wizard.GrandTotal, orderResult.IsSuccess, orderResult.Message);
			return;
		}
		if (data.StartsWith("ord_prod:"))
		{
			string fCode = data.Substring("ord_prod:".Length);
			if (_wizardStates.TryGetValue(chatId, out wizard))
			{
				string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
				LogoErpItemDetails logoErpItemDetails = await _logoErpRepository.GetItemDetailsAsync(fCode, firmNo, cancellationToken);
				LogoErpItemStockDto? itemStock = (await _proposalAppService.SearchItemStockAsync(fCode, cancellationToken))?.FirstOrDefault(s => s.ItemCode.Equals(fCode, StringComparison.OrdinalIgnoreCase));
				wizard.CurrentProductCode = fCode;
				wizard.CurrentProductName = itemStock?.ItemName ?? logoErpItemDetails?.ItemName ?? fCode;
				wizard.CurrentItemPrice = itemStock?.UnitPrice ?? 0m;
				wizard.Step = "AwaitingQuantity";
				InlineKeyboardMarkup replyMarkup3 = new InlineKeyboardMarkup(new InlineKeyboardButton[3][]
				{
					new InlineKeyboardButton[3]
					{
						InlineKeyboardButton.WithCallbackData("1 ADET", "ord_qty:1"),
						InlineKeyboardButton.WithCallbackData("5 ADET", "ord_qty:5"),
						InlineKeyboardButton.WithCallbackData("10 ADET", "ord_qty:10")
					},
					new InlineKeyboardButton[3]
					{
						InlineKeyboardButton.WithCallbackData("20 ADET", "ord_qty:20"),
						InlineKeyboardButton.WithCallbackData("50 ADET", "ord_qty:50"),
						InlineKeyboardButton.WithCallbackData("100 ADET", "ord_qty:100")
					},
					new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ İptal", "ord_cancel") }
				});
				await SendReplyAsync(botClient, chatId, "\ud83d\udce6 **Seçilen Malzeme:** `" + fCode + "`\n\nLütfen **Miktarı** giriniz:\n• Tek mesajda Miktar + Fiyat girebilirsiniz (Örn: `10 250`)\n• Veya hızlı miktar butonlarından birini seçebilirsiniz:", cancellationToken, replyMarkup3);
				LogActivity(chatId, "Malzeme Seçildi: " + fCode, wizard.FirmCode, fCode, 0m, isSuccess: true, "Sipariş sihirbazı malzeme seçildi: " + fCode);
			}
			return;
		}
		if (data.StartsWith("ord_qty:"))
		{
			string s = data.Substring("ord_qty:".Length);
			if (_wizardStates.TryGetValue(chatId, out wizard) && decimal.TryParse(s, out var quickQty))
			{
				wizard.CurrentQuantity = quickQty;
				await SendPricePromptAsync(botClient, chatId, wizard, cancellationToken);
				LogActivity(chatId, $"Hızlı Miktar Seçildi: {quickQty}", wizard.FirmCode, wizard.CurrentProductCode, quickQty, isSuccess: true, $"Hızlı miktar butonuna basıldı: {quickQty}");
			}
			return;
		}
		if (data.StartsWith("ord_prc:"))
		{
			string s = data.Substring("ord_prc:".Length);
			if (_wizardStates.TryGetValue(chatId, out wizard) && decimal.TryParse(s.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var quickPrice))
			{
				wizard.Lines.Add(new OrderWizardLine
				{
					ProductCode = wizard.CurrentProductCode,
					ProductName = wizard.CurrentProductName,
					Quantity = wizard.CurrentQuantity,
					ListPrice = quickPrice,
					UnitPrice = quickPrice
				});
				await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
				LogActivity(chatId, $"Hızlı Fiyat Seçildi: {quickPrice} TL", wizard.FirmCode, wizard.CurrentProductCode, wizard.CurrentQuantity, isSuccess: true, $"Sepete hızlı fiyatla kalem eklendi: {wizard.CurrentProductCode} x {wizard.CurrentQuantity} @ {quickPrice} TL.");
			}
			return;
		}
		if (data.StartsWith("act:excel:"))
		{
			string fCode = data.Substring("act:excel:".Length);
			string text9 = (await _proposalAppService.SearchFirmsAsync(fCode, cancellationToken)).FirstOrDefault()?.FirmName ?? fCode;
			_wizardStates[chatId] = new OrderWizardState
			{
				ChatId = chatId,
				FirmCode = fCode,
				FirmName = text9,
				Step = "AwaitingExcelGroupCode"
			};
			InlineKeyboardMarkup replyMarkup4 = new InlineKeyboardMarkup(new InlineKeyboardButton[3][]
			{
				new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udce6 Tüm Stok Kataloğu (TÜMÜ)", "excel_grp:ALL") },
				new InlineKeyboardButton[2]
				{
					InlineKeyboardButton.WithCallbackData("\ud83d\udcbb Intel Ürünleri", "excel_grp:intel"),
					InlineKeyboardButton.WithCallbackData("\ud83d\udda5\ufe0f Laptoplar", "excel_grp:laptop")
				},
				new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ İptal", "ord_cancel") }
			});
			await SendReplyAsync(botClient, chatId, $"\ud83d\udcca **İSKONTOLU EXCEL SİPARİŞ KATALOĞU OLUŞTURMA**\n• **Cari Hesabı:** `{fCode}` - **{text9}**\n\nLütfen kataloğa aktarılacak **Malzeme Grubu Kodu veya Arama Kelimesini** yazınız:\n• Örn: `intel` *(Tüm Intel grubundaki malzemeleri otomatik döküm yapar)*\n• Örn: `laptop` *(Tüm Laptop grubundaki malzemeleri otomatik döküm yapar)*\n• Örn: `TÜMÜ` veya `*` *(Logo ERP'deki tüm stok kataloğunu aktarır)*\n\nVeya aşağıdaki hazır butonlardan birini seçebilirsiniz:", cancellationToken, replyMarkup4);
			LogActivity(chatId, "Buton: Excel Sipariş Oluştur (" + fCode + ")", fCode, "-", 0m, isSuccess: true, "Excel sipariş şablonu grup kısıtlaması soruldu.");
			return;
		}
		if (data.StartsWith("act:order:") || data.StartsWith("act:irsaliye:") || data.StartsWith("act:satisFatura:"))
		{
			string fCode = "ORDER";
			string firmCode = "";
			if (data.StartsWith("act:order:"))
			{
				fCode = "ORDER";
				firmCode = data.Substring("act:order:".Length);
			}
			else if (data.StartsWith("act:irsaliye:"))
			{
				fCode = "DISPATCH";
				firmCode = data.Substring("act:irsaliye:".Length);
			}
			else if (data.StartsWith("act:satisFatura:"))
			{
				fCode = "INVOICE";
				firmCode = data.Substring("act:satisFatura:".Length);
			}
			string firmName2 = (await _proposalAppService.SearchFirmsAsync(firmCode, cancellationToken)).FirstOrDefault()?.FirmName ?? firmCode;
			await SendDocEntryMethodMenuAsync(botClient, chatId, firmCode, firmName2, fCode, cancellationToken);
			return;
		}
		if (data.StartsWith("act_method:"))
		{
			string[] array3 = data.Split(':');
			if (array3.Length >= 4)
			{
				string firmCode = array3[1];
				string fCode = array3[2];
				string firmCode2 = array3[3];
				string text10 = (await _proposalAppService.SearchFirmsAsync(firmCode2, cancellationToken)).FirstOrDefault()?.FirmName ?? firmCode2;
				switch (firmCode)
				{
				case "single":
				{
					_wizardStates[chatId] = new OrderWizardState
					{
						ChatId = chatId,
						FirmCode = firmCode2,
						FirmName = text10,
						Step = "AwaitingProductSearch",
						IsDispatchMode = (fCode == "DISPATCH"),
						IsInvoiceMode = (fCode == "INVOICE")
					};
					string text11 = fCode;
					string text12 = ((text11 == "DISPATCH") ? "\ud83d\ude9a **SATIŞ İRSALİYESİ SİHİRBAZI**" : ((!(text11 == "INVOICE")) ? "\ud83d\uded2 **SİPARİŞ OLUŞTURMA SİHİRBAZI**" : "\ud83e\uddfe **SATIŞ FATURASI SİHİRBAZI**"));
					string value13 = text12;
					await SendReplyAsync(botClient, chatId, $"{value13}\n• Cari: `{firmCode2}` - **{text10}**\n\nLütfen eklemek istediğiniz **Malzeme Adı veya Kodu** yazınız (Örn: intel, laptop):", cancellationToken);
					LogActivity(chatId, "Buton: Tek Tek Malzeme Ekle (" + firmCode2 + ")", firmCode2, "-", 0m, isSuccess: true, "Tekli sihirbaz başlatıldı.");
					return;
				}
				case "excel":
				{
					_wizardStates[chatId] = new OrderWizardState
					{
						ChatId = chatId,
						FirmCode = firmCode2,
						FirmName = text10,
						Step = "AwaitingExcelGroupCode",
						IsDispatchMode = (fCode == "DISPATCH"),
						IsInvoiceMode = (fCode == "INVOICE")
					};
					InlineKeyboardMarkup replyMarkup6 = new InlineKeyboardMarkup(new InlineKeyboardButton[3][]
					{
						new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udce6 Tüm Stok Kataloğu (TÜMÜ)", "excel_grp:ALL") },
						new InlineKeyboardButton[2]
						{
							InlineKeyboardButton.WithCallbackData("\ud83d\udcbb Intel Ürünleri", "excel_grp:intel"),
							InlineKeyboardButton.WithCallbackData("\ud83d\udda5\ufe0f Laptoplar", "excel_grp:laptop")
						},
						new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ İptal", "ord_cancel") }
					});
					await SendReplyAsync(botClient, chatId, $"\ud83d\udcca **EXCEL SİPARİŞ/İRSALİYE KATALOĞU OLUŞTURMA**\n• **Cari Hesabı:** `{firmCode2}` - **{text10}**\n\nLütfen kataloğa aktarılacak **Malzeme Grubu Kodu veya Arama Kelimesini** yazınız (Örn: intel, laptop, TÜMÜ) veya aşağıdaki butonlara basınız:", cancellationToken, replyMarkup6);
					return;
				}
				case "full_cat":
					await SendSampleExcelTemplateAsync(botClient, chatId, firmCode2, text10, "ALL", cancellationToken);
					LogActivity(chatId, "Buton: Tüm Katalog Döküm (" + firmCode2 + ")", firmCode2, "-", 0m, isSuccess: true, "Tüm malzeme kataloğu Excel gönderildi.");
					return;
				case "group_select":
				{
					_wizardStates[chatId] = new OrderWizardState
					{
						ChatId = chatId,
						FirmCode = firmCode2,
						FirmName = text10,
						Step = "AwaitingExcelGroupCode",
						IsDispatchMode = (fCode == "DISPATCH")
					};
					InlineKeyboardMarkup replyMarkup5 = new InlineKeyboardMarkup(new InlineKeyboardButton[3][]
					{
						new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udce6 Tüm Stok Kataloğu (TÜMÜ)", "excel_grp:ALL") },
						new InlineKeyboardButton[2]
						{
							InlineKeyboardButton.WithCallbackData("\ud83d\udcbb Intel Ürünleri", "excel_grp:intel"),
							InlineKeyboardButton.WithCallbackData("\ud83d\udda5\ufe0f Laptoplar", "excel_grp:laptop")
						},
						new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ İptal", "ord_cancel") }
					});
					await SendReplyAsync(botClient, chatId, $"\ud83d\udd0d **GRUP KODUNA GÖRE MALZEME KATALOĞU SEÇİMİ**\n• **Cari Hesabı:** `{firmCode2}` - **{text10}**\n\nLütfen listelemek istediğiniz **Malzeme Grubu Kodu veya Arama Kelimesini** yazınız (Örn: `YEDEK`, `HAMMADDE`, `INTEL`):\n\nVeya hazır butonlardan birini seçebilirsiniz:", cancellationToken, replyMarkup5);
					return;
				}
				}
			}
		}
		if (data == "sohbet_kapat")
		{
			if (_aiAssistant != null)
			{
				if (_aiAssistant.IsSohbetModeActive(chatId)) _aiAssistant.ToggleSohbetMode(chatId);
				await SendReplyAsync(botClient, chatId, "✅ **AI Sohbet modu kapatıldı.** ERP işlemlerinize devam edebilirsiniz.", cancellationToken);
			}
			return;
		}
		if (data == "sohbet_temizle")
		{
			if (_aiAssistant != null)
			{
				_aiAssistant.ClearChatHistory(chatId);
				// Modu tekrar aktif et (sadece geçmiş temizlendi)
				if (!_aiAssistant.IsSohbetModeActive(chatId)) _aiAssistant.ToggleSohbetMode(chatId);
				var sohbetMarkup = new InlineKeyboardMarkup(new[]
				{
					new[] { InlineKeyboardButton.WithCallbackData("🔴 Sohbeti Kapat", "sohbet_kapat") },
					new[] { InlineKeyboardButton.WithCallbackData("🏢 ERP Menüsü", "firmam_menu") }
				});
				await SendReplyAsync(botClient, chatId, "🗑️ **Sohbet geçmişi temizlendi.** Yeni bir konuşma başlatabilirsiniz.", cancellationToken, sohbetMarkup);
			}
			return;
		}
		if (data.StartsWith("ai_cari_rapor:"))
		{
			string fcodeAi = data.Substring("ai_cari_rapor:".Length);
			_awaitingAiReportPrompt[chatId] = fcodeAi;
			var cancelMarkup = new InlineKeyboardMarkup(new[]
			{
				new[] { InlineKeyboardButton.WithCallbackData("❌ İptal Et", $"cari_sel:{fcodeAi}") }
			});
			await SendReplyAsync(botClient, chatId, 
				$"🤖 **Yapay Zeka Raporu ({fcodeAi})**\n\nBu cari için nasıl bir AI raporu istiyorsunuz? Lütfen aşağıya yazın:\n\n*(Örnek: 'Bu ayki satışları listele' veya 'Risk analizi yap ve bana borç durumunu özetle')*", 
				cancellationToken, cancelMarkup);
			return;
		}
		if (data.StartsWith("cari_sel:"))
		{
			string firmCode2 = data.Substring("cari_sel:".Length);
			string fCode = (await _proposalAppService.SearchFirmsAsync(firmCode2, cancellationToken)).FirstOrDefault()?.FirmName ?? firmCode2;
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState value14) && value14.Step == "AwaitingFirmSelection" && value14.Lines.Count > 0)
			{
				await ExecuteFinalMultiLineOrderAsync(botClient, chatId, firmCode2, fCode, "Sözel / Excel Kataloğu", value14.Lines, value14.Lines.Count, cancellationToken);
				_wizardStates.TryRemove(chatId, out value);
			}
			else
			{
				await SendCariActionMenuAsync(botClient, chatId, firmCode2, fCode, cancellationToken);
				LogActivity(chatId, "Cari Seçildi: " + firmCode2, firmCode2, "-", 0m, isSuccess: true, "Cari İşlem Menüsü gösterildi: " + firmCode2 + " - " + fCode);
			}
		}
		else if (data.StartsWith("act:ekstre:"))
		{
			string fCode = data.Substring("act:ekstre:".Length);
			await SendReportDateSelectionMenuAsync(botClient, chatId, "ekstre", fCode, cancellationToken);
			LogActivity(chatId, "Buton: Ekstre Tarih Seçimi (" + fCode + ")", fCode, "-", 0m, isSuccess: true, "Ekstre tarih seçimi menüsü gösterildi.");
		}
		else if (data.StartsWith("act:info:"))
		{
			string fCode = data.Substring("act:info:".Length);
			string fn = _configuration["LogoSystem:FirmNo"] ?? "001";
			LogoErpFirmDetailDto? info = await _logoErpRepository.GetFirmDetailCardAsync(fCode, fn, cancellationToken);
			if (info != null)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"🏢 **CARİ HESAP BİLGİLERİ**");
				sb.AppendLine($"• **Cari Kodu:** `{info.FirmCode}`");
				sb.AppendLine($"• **Cari Unvanı:** `{info.FirmName}`");
				if (!string.IsNullOrWhiteSpace(info.TaxOffice)) sb.AppendLine($"• **Vergi Dairesi:** `{info.TaxOffice}`");
				if (!string.IsNullOrWhiteSpace(info.TaxNumber)) sb.AppendLine($"• **Vergi / TC No:** `{info.TaxNumber}`");
				if (!string.IsNullOrWhiteSpace(info.Phone)) sb.AppendLine($"• **Telefon:** `{info.Phone}`");
				if (!string.IsNullOrWhiteSpace(info.Email)) sb.AppendLine($"• **E-Mail:** `{info.Email}`");
				if (!string.IsNullOrWhiteSpace(info.Address)) sb.AppendLine($"• **Adres:** `{info.Address} {info.City}`".Trim());
				await SendReplyAsync(botClient, chatId, sb.ToString(), cancellationToken);
				LogActivity(chatId, "Buton: Cari Bilgileri (" + fCode + ")", fCode, "-", 0m, isSuccess: true, "Cari bilgileri gösterildi.");
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "❌ `" + fCode + "` kodlu cariye ait detaylı bilgi bulunamadı.", cancellationToken);
			}
		}
		else if (data.StartsWith("act:aireport:"))
		{
			string fCode = data.Substring("act:aireport:".Length);
			_awaitingAiReportPrompt[chatId] = fCode;
			
			var cancelMarkup = new InlineKeyboardMarkup(new[]
			{
				new[] { InlineKeyboardButton.WithCallbackData("❌ İptal Et", $"cari_sel:{fCode}") }
			});
			
			await SendReplyAsync(botClient, chatId, $"🤖 **Yapay Zeka Cari Raporu**\n\n`{fCode}` kodlu cari için ne tür bir rapor veya bilgi istediğinizi yazınız.\n\n*Örnek:* \n- Bu ay ne kadar tahsilat yapıldı?\n- En çok hangi ürünleri almış?\n- Son 3 aylık ciro durumu nedir?", cancellationToken, cancelMarkup);
			LogActivity(chatId, "Buton: AI Raporu Oluştur (" + fCode + ")", fCode, "-", 0m, isSuccess: true, "AI Rapor istemi bekleniyor.");
		}
		else if (data.StartsWith("search_firm:"))
		{
			string fCode = data.Substring("search_firm:".Length);
			List<LogoErpFirmDto> matchedFirms = await _proposalAppService.SearchFirmsAsync(fCode, cancellationToken);
			if (matchedFirms == null || matchedFirms.Count <= 0)
			{
				await SendReplyAsync(botClient, chatId, "❌ '" + fCode + "' kelimesiyle eşleşen cari hesap bulunamadı.", cancellationToken);
			}
			else if (matchedFirms.Count == 1)
			{
				LogoErpFirmDto singleFirm = matchedFirms[0];
				await SendCariActionMenuAsync(botClient, chatId, singleFirm.FirmCode, singleFirm.FirmName, cancellationToken);
				LogActivity(chatId, "Cari Arama Butonu: " + fCode, singleFirm.FirmCode, "-", 0m, isSuccess: true, "Cari İşlem Menüsü açıldı");
			}
			else
			{
				await SendCariSelectionKeyboardAsync(botClient, chatId, fCode, matchedFirms, cancellationToken);
				LogActivity(chatId, "Cari Arama Butonu: " + fCode, "-", "-", 0m, isSuccess: true, $"{matchedFirms.Count} cari bulundu ve butonlar gösterildi.");
			}
		}
		else if (data.StartsWith("search_item:"))
		{
			string fCode = data.Substring("search_item:".Length);
			List<LogoErpItemStockDto> list2 = await _proposalAppService.SearchItemStockAsync(fCode, cancellationToken);
			if (list2 == null || list2.Count <= 0)
			{
				await SendReplyAsync(botClient, chatId, "❌ '" + fCode + "' kelimesiyle eşleşen malzeme bulunamadı.", cancellationToken);
				return;
			}
			StringBuilder stringBuilder5 = new StringBuilder();
			StringBuilder stringBuilder2 = stringBuilder5;
			StringBuilder stringBuilder6 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(61, 2, stringBuilder2);
			handler.AppendLiteral("\ud83d\udd0d **MALZEME STOK ARAMA RAPORU** (`Arama: ");
			handler.AppendFormatted(fCode);
			handler.AppendLiteral("` - ");
			handler.AppendFormatted(list2.Count);
			handler.AppendLiteral(" Ürün Bulundu)\n");
			stringBuilder6.AppendLine(ref handler);
			foreach (LogoErpItemStockDto item in list2.Take(30))
			{
				stringBuilder2 = stringBuilder5;
				StringBuilder stringBuilder7 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(7, 2, stringBuilder2);
				handler.AppendLiteral("• `");
				handler.AppendFormatted(item.ItemCode);
				handler.AppendLiteral("` - ");
				handler.AppendFormatted(item.ItemName);
				stringBuilder7.AppendLine(ref handler);
			}
			stringBuilder2 = stringBuilder5;
			StringBuilder stringBuilder8 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(90, 1, stringBuilder2);
			handler.AppendLiteral("\n\ud83d\udca1 Detaylı ambar dağılımı görmek istediğiniz ürünün kodunu yazabilirsiniz (Örn: `");
			handler.AppendFormatted(list2[0].ItemCode);
			handler.AppendLiteral(" STOK`).");
			stringBuilder8.AppendLine(ref handler);
			await SendReplyAsync(botClient, chatId, stringBuilder5.ToString(), cancellationToken);
			LogActivity(chatId, "Malzeme Arama Butonu: " + fCode, "-", "-", 0m, isSuccess: true, "Malzeme araması yapıldı.");
		}

		else if (data.StartsWith("act:pdf:"))
		{
			string firmCode4 = data.Substring("act:pdf:".Length);
			await SendReportDateSelectionMenuAsync(botClient, chatId, "pdf", firmCode4, cancellationToken);
		}
		else if (data.StartsWith("act:cek:"))
		{
			string firmCode5 = data.Substring("act:cek:".Length);
			ProposalCreateResult result = await _proposalAppService.ProcessTelegramMessageAsync(firmCode5 + " CEK", chatId, cancellationToken);
			await SendReplyAsync(botClient, chatId, result.Message, cancellationToken);
			LogActivity(chatId, "Buton: Çek Raporu Metin (" + firmCode5 + ")", firmCode5, "-", 0m, result.IsSuccess, result.Message);
		}
		else if (data.StartsWith("act:cekpdf:"))
		{
			string firmCode6 = data.Substring("act:cekpdf:".Length);
			byte[] array2 = await _proposalAppService.GetChequeReportPdfAsync(firmCode6, null, null, cancellationToken);
			if (array2 != null && array2.Length != 0)
			{
				await SendPdfDocumentAsync(botClient, chatId, array2, "CekRaporu_" + firmCode6 + ".pdf", "📜 `" + firmCode6 + "` kodlu Cari Hesabın Çek Raporu (PDF) ektedir.\n'Paylaş' seçeneği ile iletebilirsiniz.", cancellationToken);
				LogActivity(chatId, "Buton: Çek Raporu PDF (" + firmCode6 + ")", firmCode6, "-", 0m, isSuccess: true, "Çek Raporu PDF gönderildi.");
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "❌ '" + firmCode6 + "' kodlu Cari hesabına ait Çek Raporu (PDF) bulunamadı veya çek kaydı yok.", cancellationToken);
				LogActivity(chatId, "Buton: Çek Raporu PDF (" + firmCode6 + ")", firmCode6, "-", 0m, isSuccess: false, "Çek raporu PDF bulunamadı.");
			}
		}
		else if (data.StartsWith("act:dt_fat:"))
		{
			string firmCodeFat = data.Substring("act:dt_fat:".Length);
			await SendReportDateSelectionMenuAsync(botClient, chatId, "fatura", firmCodeFat, cancellationToken);
		}
		else if (data.StartsWith("act:dt_irs:"))
		{
			string firmCodeIrs = data.Substring("act:dt_irs:".Length);
			await SendReportDateSelectionMenuAsync(botClient, chatId, "irsaliye", firmCodeIrs, cancellationToken);
		}
		else if (data.StartsWith("act:dt_sip:"))
		{
			string firmCodeSip = data.Substring("act:dt_sip:".Length);
			await SendReportDateSelectionMenuAsync(botClient, chatId, "siparis", firmCodeSip, cancellationToken);
		}
		else if (data.StartsWith("rptdt:"))
		{
			await ProcessReportDateCallbackAsync(botClient, chatId, data, cancellationToken);
		}
		else if (data.StartsWith("send_mail:"))
		{
			await HandleSendMailAsync(botClient, chatId, data, cancellationToken);
		}
		else if (data.StartsWith("send_sms:"))
		{
			await HandleSendSmsAsync(botClient, chatId, data, cancellationToken);
		}
		else if (data.StartsWith("gib_status:"))
		{
			string fNo = data.Substring("gib_status:".Length);
			await HandleGibStatusCheckAsync(botClient, chatId, fNo, cancellationToken);
		}
		else if (data.StartsWith("act:tahsilat:"))
		{
			string fCode = data.Substring("act:tahsilat:".Length);
			string fName = (await _proposalAppService.SearchFirmsAsync(fCode, cancellationToken)).FirstOrDefault()?.FirmName ?? fCode;
			_wizardStates[chatId] = new OrderWizardState
			{
				ChatId = chatId,
				FirmCode = fCode,
				FirmName = fName,
				IsCollectionMode = true,
				Step = "AwaitingCollectionType"
			};

			InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
			{
				new InlineKeyboardButton[]
				{
					InlineKeyboardButton.WithCallbackData("💵 Nakit Tahsilat", "ths_type:35"),
					InlineKeyboardButton.WithCallbackData("💳 Kredi Kartı (POS)", "ths_type:36")
				},
				new InlineKeyboardButton[]
				{
					InlineKeyboardButton.WithCallbackData("📜 Çek Tahsilatı", "ths_type:37")
				},
				new InlineKeyboardButton[]
				{
					InlineKeyboardButton.WithCallbackData("❌ İptal Et", "ths_cancel")
				}
			});

			await SendReplyAsync(botClient, chatId, $"💳 **TAHSİLAT GİRİŞİ SİHİRBAZI**\n\n🏢 **Cari:** `{fCode}` - **{fName}**\n\nLütfen tahsilat türünü seçiniz:", cancellationToken, replyMarkup);
		}
		else if (data.StartsWith("ths_type:"))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState wizThs) && wizThs.IsCollectionMode)
			{
				int trCode = int.Parse(data.Substring("ths_type:".Length));
				wizThs.CollectionTrCode = trCode;
				wizThs.Step = "AwaitingCollectionAmount";
				string typeName = trCode switch { 36 => "Kredi Kartı (POS)", 37 => "Çek", _ => "Nakit" };
				InlineKeyboardMarkup cancelMarkup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("❌ İptal Et", "ths_cancel"));
				await SendReplyAsync(botClient, chatId, $"💳 **Tahsilat Türü:** `{typeName}`\n\n💰 **Tahsilat Tutarı Giriniz:**\nLütfen cariden alınan tahsilat tutarını rakam olarak yazınız (Örn: `5000` veya `1250.50`):", cancellationToken, cancelMarkup);
			}
		}
		else if (data.Equals("ths_confirm", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryGetValue(chatId, out OrderWizardState wizThsConfirm) && wizThsConfirm.IsCollectionMode)
			{
				await ExecuteFinalCollectionAsync(botClient, chatId, wizThsConfirm, cancellationToken);
			}
		}
		else if (data.Equals("ths_cancel", StringComparison.OrdinalIgnoreCase))
		{
			if (_wizardStates.TryRemove(chatId, out _))
			{
				await SendReplyAsync(botClient, chatId, "❌ **Tahsilat girişi iptal edildi.**\n\nAna menüye dönmek için `/menu` yazabilirsiniz.", cancellationToken);
			}
		}
		else if (data.StartsWith("act:risk:"))
		{
			string fCode = data.Substring("act:risk:".Length);
			ProposalCreateResult orderResult = await _proposalAppService.ProcessTelegramMessageAsync(fCode + " RISK", chatId, cancellationToken);
			await SendReplyAsync(botClient, chatId, orderResult.Message, cancellationToken);
			LogActivity(chatId, "Buton: Risk (" + fCode + ")", fCode, "-", 0m, orderResult.IsSuccess, orderResult.Message);
		}
		else if (data.StartsWith("act:vade:"))
		{
			string fCode = data.Substring("act:vade:".Length);
			await SendReplyAsync(botClient, chatId, $"⏱️ `{fCode}` kodlu cari için vade ve yaşlandırma raporu hazırlanıyor...", cancellationToken);
			// Implement specific vade tracking if needed, or link to existing report
		}
		else if (data.StartsWith("act:borc:"))
		{
			string fCode = data.Substring("act:borc:".Length);
			await SendReplyAsync(botClient, chatId, $"🧾 `{fCode}` kodlu cari için güncel borç ve kapanmamış faturalar raporu hazırlanıyor...", cancellationToken);
		}
		else if (data.StartsWith("stk_sel:"))
		{
			string fCode = data.Substring("stk_sel:".Length);
			ProposalCreateResult orderResult = await _proposalAppService.ProcessTelegramMessageAsync(fCode + " STOK", chatId, cancellationToken);
			await SendReplyAsync(botClient, chatId, orderResult.Message, cancellationToken);
			LogActivity(chatId, "Buton: Stok (" + fCode + ")", "-", fCode, 0m, orderResult.IsSuccess, orderResult.Message);
		}
	}

	private async Task SendShoppingCartSummaryAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, CancellationToken cancellationToken)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string value = (wizard.IsDispatchMode ? "\ud83d\ude9a **SATIŞ İRSALİYESİ SEPETİ**" : (wizard.IsInvoiceMode ? "\ud83e\uddfe **SATIŞ FATURASI SEPETİ**" : "\ud83d\uded2 **SİPARİŞ SEPETİ**"));
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 2, stringBuilder2);
		handler.AppendFormatted(value);
		handler.AppendLiteral(" (`");
		handler.AppendFormatted(wizard.Lines.Count);
		handler.AppendLiteral("` Kalem Ekli)");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(21, 2, stringBuilder2);
		handler.AppendLiteral("• **Cari:** `");
		handler.AppendFormatted(wizard.FirmCode);
		handler.AppendLiteral("` - **");
		handler.AppendFormatted(wizard.FirmName);
		handler.AppendLiteral("**");
		stringBuilder4.AppendLine(ref handler);
		if (wizard.IsDispatchMode)
		{
			string value2 = (string.IsNullOrWhiteSpace(wizard.CarrierOrDriverInfo) ? "⚠\ufe0f `Girilmedi (ZORUNLU)`" : ("`" + wizard.CarrierOrDriverInfo + "`"));
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(32, 1, stringBuilder2);
			handler.AppendLiteral("• **Taşıyıcı / Şoför Bilgisi:** ");
			handler.AppendFormatted(value2);
			stringBuilder5.AppendLine(ref handler);
		}
		if (!string.IsNullOrWhiteSpace(wizard.Description))
		{
			stringBuilder.AppendLine($"• **Genel Açıklama / Not:** `{wizard.Description}`");
		}
		stringBuilder.AppendLine();
		int num = 1;
		foreach (OrderWizardLine line in wizard.Lines)
		{
			string value3 = (string.IsNullOrWhiteSpace(line.ProductName) ? line.ProductCode : line.ProductName);
			string value4 = ((line.DiscountRate > 0m) ? $" (İsk: %{line.DiscountRate:N1})" : "");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(11, 3, stringBuilder2);
			handler.AppendLiteral("**");
			handler.AppendFormatted(num);
			handler.AppendLiteral(".** `");
			handler.AppendFormatted(line.ProductCode);
			handler.AppendLiteral("` - ");
			handler.AppendFormatted(value3);
			stringBuilder6.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(33, 4, stringBuilder2);
			handler.AppendLiteral("   └ \ud83d\udd22 `");
			handler.AppendFormatted(line.Quantity, "N2");
			handler.AppendLiteral("` ADET x `");
			handler.AppendFormatted(line.EffectiveUnitPrice, "N2");
			handler.AppendLiteral("` TL");
			handler.AppendFormatted(value4);
			handler.AppendLiteral(" = **");
			handler.AppendFormatted(line.LineTotal, "N2");
			handler.AppendLiteral(" TL**");
			stringBuilder7.AppendLine(ref handler);
			num++;
		}
		if (wizard.OverallDiscountTotal > 0m)
		{
			string value5 = ((wizard.GeneralDiscountRate > 0m) ? $" (%{wizard.GeneralDiscountRate:N1})" : "");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder8 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(25, 1, stringBuilder2);
			handler.AppendLiteral("\n\ud83d\udcb5 **Ara Toplam:** `");
			handler.AppendFormatted(wizard.SubTotalBeforeGeneralDiscount, "N2");
			handler.AppendLiteral(" TL`");
			stringBuilder8.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder9 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(28, 2, stringBuilder2);
			handler.AppendLiteral("\ud83d\udcc9 **Genel İskonto");
			handler.AppendFormatted(value5);
			handler.AppendLiteral(":** `-");
			handler.AppendFormatted(wizard.OverallDiscountTotal, "N2");
			handler.AppendLiteral(" TL`");
			stringBuilder9.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder10 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(33, 1, stringBuilder2);
			handler.AppendLiteral("\ud83d\udcb0 **Genel Toplam Tutar:** `");
			handler.AppendFormatted(wizard.GrandTotal, "N2");
			handler.AppendLiteral(" TL`\n");
			stringBuilder10.AppendLine(ref handler);
		}
		else
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder11 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(34, 1, stringBuilder2);
			handler.AppendLiteral("\n\ud83d\udcb0 **Sepet Toplam Tutar:** `");
			handler.AppendFormatted(wizard.GrandTotal, "N2");
			handler.AppendLiteral(" TL`\n");
			stringBuilder11.AppendLine(ref handler);
		}
		stringBuilder.AppendLine("Yapmak istediğiniz işlemi aşağıdan seçebilirsiniz:");
		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		list.Add(new InlineKeyboardButton[2]
		{
			InlineKeyboardButton.WithCallbackData("➕ Başka Kalem Ekle", "ord_add_item"),
			InlineKeyboardButton.WithCallbackData("\ud83d\udcc9 Genel İskonto Ekle (% / TL)", "ord_add_gen_disc")
		});
		string descBtnText = string.IsNullOrWhiteSpace(wizard.Description) ? "📝 Genel Açıklama / Not Ekle" : "📝 Genel Açıklama / Notu Değiştir";
		list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData(descBtnText, "ord_general_description") });
		list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("✏\ufe0f Sepetteki Kalemleri Düzenle / Çıkart", "ord_edit_cart") });
		if (wizard.IsDispatchMode)
		{
			string text = (string.IsNullOrWhiteSpace(wizard.CarrierOrDriverInfo) ? "\ud83d\ude9b Taşıyıcı / Şoför Bilgisi Gir (Zorunlu)" : "\ud83d\ude9b Taşıyıcı / Şoför Bilgisini Değiştir");
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData(text, "ord_carrier_info") });
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udcc4 e-İrsaliye PDF Önizle & İndir", "draft_irs_pdf_preview") });
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("✅ İrsaliyeyi Onayla & Logo ERP'ye Kaydet", "ord_submit") });
		}
		else if (wizard.IsInvoiceMode)
		{
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udcc4 e-Fatura PDF Önizle & İndir", "draft_inv_pdf_preview") });
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("✅ Satış Faturası Oluştur & Logo ERP'ye Kaydet", "ord_submit") });
		}
		else
		{
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("✅ Siparişi Tamamla & Fiş Oluştur", "ord_submit") });
		}
		list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ İptal", "ord_cancel") });
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, replyMarkup);
	}

	private async Task SendCartEditMenuAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, CancellationToken cancellationToken)
	{
		if (wizard == null || wizard.Lines.Count == 0)
		{
			await SendReplyAsync(botClient, chatId, "\ud83d\uded2 Sepetinizde düzenlenecek kalem bulunmuyor.", cancellationToken);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("✏\ufe0f **SEPET DÜZENLEME MENÜSÜ**");
		stringBuilder.AppendLine("Lütfen işlem yapmak istediğiniz kalemi seçiniz:\n");
		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		StringBuilder stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler;
		for (int i = 0; i < wizard.Lines.Count; i++)
		{
			OrderWizardLine orderWizardLine = wizard.Lines[i];
			string value = (string.IsNullOrWhiteSpace(orderWizardLine.ProductName) ? orderWizardLine.ProductCode : orderWizardLine.ProductName);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder2);
			handler.AppendLiteral("**");
			handler.AppendFormatted(i + 1);
			handler.AppendLiteral(".** ");
			handler.AppendFormatted(value);
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(33, 3, stringBuilder2);
			handler.AppendLiteral("   └ \ud83d\udd22 `");
			handler.AppendFormatted(orderWizardLine.Quantity, "N2");
			handler.AppendLiteral("` ADET x `");
			handler.AppendFormatted(orderWizardLine.EffectiveUnitPrice, "N2");
			handler.AppendLiteral("` TL = **");
			handler.AppendFormatted(orderWizardLine.LineTotal, "N2");
			handler.AppendLiteral(" TL**");
			stringBuilder4.AppendLine(ref handler);
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData($"✏\ufe0f {i + 1}. Kalem: {value} ({orderWizardLine.Quantity:N2} Adet)", $"ord_edit_line:{i}") });
		}
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(28, 1, stringBuilder2);
		handler.AppendLiteral("\n\ud83d\udcb0 **Sepet Toplamı:** `");
		handler.AppendFormatted(wizard.GrandTotal, "N2");
		handler.AppendLiteral(" TL`");
		stringBuilder5.AppendLine(ref handler);
		list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udd19 Sepet Özetine Dön", "ord_back_to_summary") });
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, new InlineKeyboardMarkup(list));
	}

	private async Task ProcessMultiProductCommaSearchAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, string rawInput, CancellationToken cancellationToken)
	{
		string[] terms = rawInput.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		int addedCount = 0;
		List<string> unfoundTerms = new List<string>();
		List<string> ambiguousTerms = new List<string>();
		string[] array = terms;
		foreach (string obj in array)
		{
			string text = obj.Trim();
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			string[] array2 = text.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			string searchTerm = text;
			decimal qty = 1m;
			decimal price = 0m;
			if (array2.Length >= 2 && decimal.TryParse(array2[^1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) && result > 0m)
			{
				qty = result;
				searchTerm = string.Join(" ", array2.Take(array2.Length - 1));
				if (array2.Length >= 3 && decimal.TryParse(array2[^2].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result2) && result2 > 0m && decimal.TryParse(array2[^1].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result3))
				{
					qty = result2;
					price = result3;
					searchTerm = string.Join(" ", array2.Take(array2.Length - 2));
				}
			}
			List<LogoErpItemStockDto> list = await _proposalAppService.SearchItemStockAsync(searchTerm, cancellationToken);
			if (list == null || list.Count == 0)
			{
				unfoundTerms.Add(searchTerm);
				continue;
			}
			LogoErpItemStockDto logoErpItemStockDto = list.FirstOrDefault((LogoErpItemStockDto s) => s.ItemCode.Equals(searchTerm, StringComparison.OrdinalIgnoreCase)) ?? list.FirstOrDefault((LogoErpItemStockDto s) => s.ItemName.Equals(searchTerm, StringComparison.OrdinalIgnoreCase)) ?? ((list.Count == 1) ? list[0] : null);
			if (logoErpItemStockDto == null && list.Count > 0)
			{
				logoErpItemStockDto = list[0];
				ambiguousTerms.Add($"{searchTerm} -> {logoErpItemStockDto.ItemCode} ({logoErpItemStockDto.ItemName})");
			}
			if (logoErpItemStockDto != null)
			{
				decimal num = ((price > 0m) ? price : ((logoErpItemStockDto.UnitPrice > 0m) ? logoErpItemStockDto.UnitPrice : 0m));
				wizard.Lines.Add(new OrderWizardLine
				{
					ProductCode = logoErpItemStockDto.ItemCode,
					ProductName = logoErpItemStockDto.ItemName,
					Quantity = qty,
					ListPrice = num,
					UnitPrice = num
				});
				addedCount++;
			}
		}
		wizard.Step = "AwaitingQuantity";
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("⚡ **TOPLU MALZEME ARAMA & SEPETE EKLEME RAPORU**\n");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(30, 1, stringBuilder2);
		handler.AppendLiteral("• **Aranan Kelime Sayısı:** `");
		handler.AppendFormatted(terms.Length);
		handler.AppendLiteral("`");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(39, 1, stringBuilder2);
		handler.AppendLiteral("• **Başarıyla Eklenen Kalem:** `");
		handler.AppendFormatted(addedCount);
		handler.AppendLiteral("` Ürün\n");
		stringBuilder4.AppendLine(ref handler);
		if (unfoundTerms.Count > 0)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(39, 1, stringBuilder2);
			handler.AppendLiteral("⚠\ufe0f **Bulunamayan Arama Kelimeleri (");
			handler.AppendFormatted(unfoundTerms.Count);
			handler.AppendLiteral("):**");
			stringBuilder5.AppendLine(ref handler);
			foreach (string item in unfoundTerms)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
				handler.AppendLiteral("  ❌ `");
				handler.AppendFormatted(item);
				handler.AppendLiteral("`");
				stringBuilder6.AppendLine(ref handler);
			}
			stringBuilder.AppendLine();
		}
		if (ambiguousTerms.Count > 0)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(46, 1, stringBuilder2);
			handler.AppendLiteral("\ud83d\udca1 **En Yakın Malzemeyle Eşleştirilenler (");
			handler.AppendFormatted(ambiguousTerms.Count);
			handler.AppendLiteral("):**");
			stringBuilder7.AppendLine(ref handler);
			foreach (string item2 in ambiguousTerms.Take(5))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder8 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
				handler.AppendLiteral("  \ud83d\udd39 ");
				handler.AppendFormatted(item2);
				stringBuilder8.AppendLine(ref handler);
			}
			stringBuilder.AppendLine();
		}
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken);
		await SendShoppingCartSummaryAsync(botClient, chatId, wizard, cancellationToken);
		LogActivity(chatId, rawInput, wizard.FirmCode, "-", 0m, isSuccess: true, $"Toplu malzeme arandı ({terms.Length} kelime, {addedCount} kalem eklendi).");
	}

	private async Task SendCariSelectionKeyboardAsync(ITelegramBotClient botClient, long chatId, string searchTerm, List<LogoErpFirmDto> firms, CancellationToken cancellationToken)
	{
		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		foreach (LogoErpFirmDto item in firms.Take(10))
		{
			string text = "\ud83c\udfe2 " + item.FirmCode + " - " + ((item.FirmName.Length > 25) ? (item.FirmName.Substring(0, 25) + "...") : item.FirmName);
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData(text, "cari_sel:" + item.FirmCode) });
		}
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
		await SendReplyAsync(botClient, chatId, "\ud83d\udd0d **BİRDEN FAZLA CARİ HESAP BULUNDU** (`Arama: " + searchTerm + "`)\nLütfen işlem yapmak istediğiniz cari hesabı seçiniz:", cancellationToken, replyMarkup);
	}

	private async Task SendCariActionMenuAsync(ITelegramBotClient botClient, long chatId, string firmCode, string firmName, CancellationToken cancellationToken)
	{
		TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId);
		AppPermissionsDto permissions = _appPermissionStore.GetPermissions();
		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		list.Add(new InlineKeyboardButton[2]
		{
			InlineKeyboardButton.WithCallbackData("📦 Son Satılan Ürünler", "rpt_run_cari:rpt_cari_urun_alimlari:" + firmCode),
			InlineKeyboardButton.WithCallbackData("🤖 AI Raporu Oluştur", "act:aireport:" + firmCode)
		});
		list.Add(new InlineKeyboardButton[2]
		{
			InlineKeyboardButton.WithCallbackData("📄 Ekstre (Metin)", "act:ekstre:" + firmCode),
			InlineKeyboardButton.WithCallbackData("📑 Ekstre (PDF)", "act:pdf:" + firmCode)
		});
		list.Add(new InlineKeyboardButton[2]
		{
			InlineKeyboardButton.WithCallbackData("📑 Çek Raporu (Metin)", "act:cek:" + firmCode),
			InlineKeyboardButton.WithCallbackData("📜 Çek Raporu (PDF)", "act:cekpdf:" + firmCode)
		});
		list.Add(new InlineKeyboardButton[2]
		{
			InlineKeyboardButton.WithCallbackData("⏱️ Vade Takibi", "act:vade:" + firmCode),
			InlineKeyboardButton.WithCallbackData("🧾 Borç Takip", "act:borc:" + firmCode)
		});
		list.Add(new InlineKeyboardButton[1]
		{
			InlineKeyboardButton.WithCallbackData("📱 SMS / Mail Gönder", "send_sms:ekstre:" + firmCode)
		});
		List<InlineKeyboardButton> rowRisk = new List<InlineKeyboardButton>();
		if (permissions.EnableCollectionModule && (user == null || user.CanCreateCollection))
		{
			rowRisk.Add(InlineKeyboardButton.WithCallbackData("💳 Tahsilat Gir", "act:tahsilat:" + firmCode));
		}
		rowRisk.Add(InlineKeyboardButton.WithCallbackData("ℹ️ Cari Bilgileri", "act:info:" + firmCode));
		list.Add(rowRisk.ToArray());
		List<InlineKeyboardButton> list2 = new List<InlineKeyboardButton>();
		if (permissions.EnableSalesOrder && (user == null || user.CanCreateSalesOrder))
		{
			list2.Add(InlineKeyboardButton.WithCallbackData("\ud83d\uded2 Sipariş Oluştur", "act:order:" + firmCode));
		}
		if (permissions.EnableSalesDispatch && (user == null || user.CanCreateDispatch))
		{
			list2.Add(InlineKeyboardButton.WithCallbackData("\ud83d\ude9a İrsaliye Oluştur", "act:irsaliye:" + firmCode));
		}
		if (list2.Count > 0)
		{
			list.Add(list2.ToArray());
		}
		if (permissions.EnableSalesInvoice && (user == null || user.CanCreateSalesInvoice))
		{
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83e\uddfe Satış Faturası Oluştur", "act:satisFatura:" + firmCode) });
		}
		List<InlineKeyboardButton> list3 = new List<InlineKeyboardButton>();
		if (permissions.EnableSalesOrder && (user == null || user.CanViewSalesOrders))
		{
			list3.Add(InlineKeyboardButton.WithCallbackData("\ud83d\udce6 Satış Siparişleri", "act:c_sip_satis:" + firmCode));
		}
		if (permissions.EnablePurchaseOrder && user != null && user.CanViewPurchaseOrders)
		{
			list3.Add(InlineKeyboardButton.WithCallbackData("\ud83d\udce5 Alış Siparişleri", "act:c_sip_alis:" + firmCode));
		}
		if (list3.Count > 0)
		{
			list.Add(list3.ToArray());
		}
		List<InlineKeyboardButton> list4 = new List<InlineKeyboardButton>();
		if (permissions.EnableSalesDispatch && (user == null || user.CanViewSalesDispatches))
		{
			list4.Add(InlineKeyboardButton.WithCallbackData("\ud83d\ude9a Satış İrsaliyeleri", "act:c_irs_satis:" + firmCode));
		}
		if (permissions.EnablePurchaseDispatch && user != null && user.CanViewPurchaseDispatches)
		{
			list4.Add(InlineKeyboardButton.WithCallbackData("\ud83d\udce6 Alış İrsaliyeleri", "act:c_irs_alis:" + firmCode));
		}
		if (list4.Count > 0)
		{
			list.Add(list4.ToArray());
		}
		List<InlineKeyboardButton> list5 = new List<InlineKeyboardButton>();
		if (permissions.EnableSalesInvoice && (user == null || user.CanViewSalesInvoices))
		{
			list5.Add(InlineKeyboardButton.WithCallbackData("\ud83e\uddfe Satış Faturaları", "act:c_fat_satis:" + firmCode));
		}
		if (permissions.EnablePurchaseInvoice && user != null && user.CanViewPurchaseInvoices)
		{
			list5.Add(InlineKeyboardButton.WithCallbackData("\ud83e\uddfe Alış Faturaları", "act:c_fat_alis:" + firmCode));
		}
		if (list5.Count > 0)
		{
			list.Add(list5.ToArray());
		}
		if (permissions.EnableSalesOrder && (user == null || user.CanCreateSalesOrder))
		{
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udcca Excel ile Sipariş Yükle", "act:excel:" + firmCode) });
		}
		if (_customReportStore != null)
		{
			var cariReports = _customReportStore.GetReportsByCategory("Cari");
			if (cariReports != null && cariReports.Count > 0)
			{
				foreach (var cr in cariReports)
				{
					list.Add(new[] { InlineKeyboardButton.WithCallbackData($"{cr.Icon} {cr.TelegramButtonText}", $"rpt_run_cari:{cr.Id}:{firmCode}") });
				}
			}
		}
		list.Add(new[] { InlineKeyboardButton.WithCallbackData("🏢 Firmam Kokpiti", "firmam_menu") });
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
		string fn = _configuration["LogoSystem:FirmNo"] ?? "001";
		LogoErpFirmRiskDto? risk = await _logoErpRepository.GetFirmRiskReportAsync(firmCode, fn, cancellationToken);
		string riskNotice = (risk != null && risk.NetBalance > 0m) ? $"\n⚠️ **BORÇ BAKİYESİ UYARISI:** Bu carinin `{risk.NetBalance:N2} TL` güncel borç bakiyesi bulunmaktadır!" : "";
		if (risk != null)
		{
			riskNotice += $"\n📊 **Ortalama Vade:** {risk.AverageMaturityDays} Gün\n🛡️ **Risk Durumu:** {risk.RiskStatus}";
		}
		await SendReplyAsync(botClient, chatId, $"\ud83c\udfe2 **CARİ İŞLEM MENÜSÜ**\n• **Cari Kod:** `{firmCode}`\n• **Unvan:** `{firmName}`{riskNotice}\n\nYapmak istediğiniz işlemi aşağıdan seçebilirsiniz:", cancellationToken, replyMarkup);
	}

	private async Task SendDocEntryMethodMenuAsync(ITelegramBotClient botClient, long chatId, string firmCode, string firmName, string docType, CancellationToken cancellationToken)
	{
		string text = ((docType == "DISPATCH") ? "\ud83d\ude9a **SATIŞ İRSALİYESİ GİRİŞ YÖNTEMİ**" : ((!(docType == "INVOICE")) ? "\ud83d\uded2 **SATIŞ SİPARİŞİ GİRİŞ YÖNTEMİ**" : "\ud83e\uddfe **SATIŞ FATURASI GİRİŞ YÖNTEMİ**"));
		string value = text;
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[3][]
		{
			new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("\ud83d\udce6 Tek Tek Malzeme Ekle", "act_method:single:" + docType + ":" + firmCode),
				InlineKeyboardButton.WithCallbackData("\ud83d\udcca Excel Dosyası Yükle", "act_method:excel:" + docType + ":" + firmCode)
			},
			new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("\ud83d\udccb Tüm Malzeme Kataloğunu Dök", "act_method:full_cat:" + docType + ":" + firmCode),
				InlineKeyboardButton.WithCallbackData("\ud83d\udd0d Grup Kodu İle Malzeme Seç", "act_method:group_select:" + docType + ":" + firmCode)
			},
			new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ İptal", "ord_cancel") }
		});
		await SendReplyAsync(botClient, chatId, $"{value}\n• **Cari Hesabı:** `{firmCode}` - **{firmName}**\n\nDevam etmek istediğiniz yöntemi seçiniz:", cancellationToken, replyMarkup);
	}

	private async Task SendPdfDocumentAsync(ITelegramBotClient botClient, long chatId, byte[] pdfBytes, string fileName, string caption, CancellationToken cancellationToken, InlineKeyboardMarkup replyMarkup = null)
	{
		using MemoryStream ms = new MemoryStream(pdfBytes);
		InputFileStream document = InputFile.FromStream(ms, fileName);
		await botClient.SendDocument(chatId, document, caption: caption, parseMode: ParseMode.Markdown, replyMarkup: replyMarkup, cancellationToken: cancellationToken);
	}

	public static string GetComprehensiveWelcomeMessage()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("\ud83e\udd16 **LOGO CRM & ERP TELEGRAM BOTUNA HOŞ GELDİNİZ!**\n");
		stringBuilder.AppendLine("Sistemimiz üzerinden Logo veritabanınıza **anında hızlı sipariş girebilir**, **300-500 satırlık Excel sipariş kataloglarınızı otomatik aktarabilir**, **cari hesap ekstresi**, **çek dökümü** ve **stok miktarlarını** sorgulayabilirsiniz.\n");
		stringBuilder.AppendLine("\ud83d\udcc4 **1. Örnek İskontolu Excel Sipariş Şablonu İndirme:**");
		stringBuilder.AppendLine("• Sohbet ekranına `örnek excel` veya `/excel` yazarak hazır sipariş dosya şablonunu telefonunuza veya bilgisayarınıza indirebilirsiniz.");
		stringBuilder.AppendLine("• Veya Cari seçtikten sonra `[ \ud83d\udcca Excel ile Sipariş Yükle ]` butonuna basarak istediğiniz Malzeme Grubuna özel **tanımlı fiyatlı ve iskontolu** Excel alabilirsiniz.\n");
		stringBuilder.AppendLine("\ud83d\udcca **2. 300-500 Satırlık Excel Sipariş Kataloğu Yükleme & Önizleme:**");
		stringBuilder.AppendLine("• Excel kataloğunuzda sadece sipariş edilecek ürünlerin **Miktar** (ve isteğe bağlı **İskonto %**) sütununu doldurup Telegram sohbetine atmanız yeterlidir.");
		stringBuilder.AppendLine("• Önizleme ekranında **İskonto**, **Net Fiyat** ve **Net Toplam** kontrol edilip onaylandıktan sonra Logo ERP'ye kaydedilir.\n");
		stringBuilder.AppendLine("⚡ **3. Tek Mesajda Miktar + Fiyat Girişi:**");
		stringBuilder.AppendLine("• `10 250` _(10 Adet @ 250 TL birim fiyattan sepete ekler)_\n");
		stringBuilder.AppendLine("\ud83d\udd18 **4. Hazır Miktar Butonları:**");
		stringBuilder.AppendLine("• `[ 1 ADET ]` `[ 5 ADET ]` `[ 10 ADET ]` `[ 50 ADET ]` butonlarına basarak anında miktar belirleyebilirsiniz.\n");
		stringBuilder.AppendLine("\ud83d\uded2 **5. Çoklu Kalem İnteraktif Sepet:**");
		stringBuilder.AppendLine("• Cari seçip `\ud83d\uded2 Sipariş Oluştur` butonuna basarak sepetinize N adet malzeme ekleyebilirsiniz.\n");
		stringBuilder.AppendLine("\ud83d\udcc4 **6. Cari Hesap Ekstre & PDF:**");
		stringBuilder.AppendLine("• `{CariKod} EKSTRE` veya `{CariKod} EKSTRE PDF`\n");
		stringBuilder.AppendLine("\ud83d\udcc8 **7. Excel Raporları (Stok ve Cari Bakiyeleri):**");
		stringBuilder.AppendLine("Aşağıdaki komutları yazarak anında güncel Excel dökümlerini alabilirsiniz:");
		stringBuilder.AppendLine("• `/stoklistesi` - Tüm stokların miktarları ve fiyatları");
		stringBuilder.AppendLine("• `/carilistesi` - Tüm carilerin döviz bazlı bakiyeleri");
		stringBuilder.AppendLine("• `/borclular` - Sadece borçlu cariler");
		stringBuilder.AppendLine("• `/alacaklilar` - Sadece alacaklı cariler\n");
		stringBuilder.AppendLine("\ud83d\udca1 *Cari adı veya kelime yazdığınızda ekranda seçilebilir işlem butonları açılır.*");
		return stringBuilder.ToString();
	}

	private async Task SendReplyAsync(ITelegramBotClient botClient, long chatId, string text, CancellationToken cancellationToken, InlineKeyboardMarkup? replyMarkup = null)
	{
		if (string.IsNullOrWhiteSpace(text)) return;

		if (text.Length > 3800)
		{
			List<string> chunks = ChunkMessageText(text, 3800);
			for (int i = 0; i < chunks.Count; i++)
			{
				InlineKeyboardMarkup? markup = (i == chunks.Count - 1) ? replyMarkup : null;
				await SendSingleReplyAsync(botClient, chatId, chunks[i], cancellationToken, markup);
			}
		}
		else
		{
			await SendSingleReplyAsync(botClient, chatId, text, cancellationToken, replyMarkup);
		}
	}

	private async Task SendSingleReplyAsync(ITelegramBotClient botClient, long chatId, string text, CancellationToken cancellationToken, InlineKeyboardMarkup? replyMarkup = null)
	{
		try
		{
			await botClient.SendMessage(chatId, text, ParseMode.Markdown, null, replyMarkup, null, null, null, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
		}
		catch (Exception exception)
		{
			_logger.LogWarning(exception, "Markdown formatında mesaj gönderilemedi. Düz metin olarak tekrar deneniyor.");
			string plainText = text.Replace("`", "").Replace("*", "").Replace("_", "").Replace("~", "").Replace("[", "").Replace("]", "");
			await botClient.SendMessage(chatId, plainText, ParseMode.None, null, replyMarkup, null, null, null, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
		}
	}

	private static List<string> ChunkMessageText(string text, int maxChunkSize = 3800)
	{
		List<string> chunks = new List<string>();
		string[] lines = text.Split('\n');
		StringBuilder currentChunk = new StringBuilder();

		foreach (string line in lines)
		{
			if (currentChunk.Length + line.Length + 1 > maxChunkSize)
			{
				if (currentChunk.Length > 0)
				{
					chunks.Add(currentChunk.ToString());
					currentChunk.Clear();
				}

				if (line.Length > maxChunkSize)
				{
					for (int i = 0; i < line.Length; i += maxChunkSize)
					{
						int length = Math.Min(maxChunkSize, line.Length - i);
						chunks.Add(line.Substring(i, length));
					}
				}
				else
				{
					currentChunk.AppendLine(line);
				}
			}
			else
			{
				currentChunk.AppendLine(line);
			}
		}

		if (currentChunk.Length > 0)
		{
			chunks.Add(currentChunk.ToString());
		}

		return chunks;
	}

	private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
	{
		string text = ((!(exception is ApiRequestException ex)) ? exception.Message : $"Telegram API Hatası: [{ex.ErrorCode}] {ex.Message}");
		string text2 = text;
		_lastError = exception;
		_logger.LogWarning(exception, "Telegram Polling geçici hatası: {ErrorMessage}", text2);
		return Task.CompletedTask;
	}

	private string GetSalesmanDisplayName(long chatId)
	{
		if (chatId == 0L)
		{
			return "Saha Satış Ekibi";
		}
		TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId);
		if (user != null)
		{
			if (user.IsMapped && !string.IsNullOrWhiteSpace(user.MappedSalesmanName))
			{
				return user.MappedSalesmanName;
			}
			if (user.IsMapped && !string.IsNullOrWhiteSpace(user.MappedSalesmanCode))
			{
				return user.MappedSalesmanCode;
			}
			if (!string.IsNullOrWhiteSpace(user.FullName))
			{
				return user.FullName;
			}
			if (!string.IsNullOrWhiteSpace(user.Username))
			{
				return "@" + user.Username;
			}
		}
		return "Saha Satış Ekibi";
	}

	private async Task SendDateFilterMenuAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, CancellationToken cancellationToken)
	{
		string text = (string.IsNullOrWhiteSpace(firmCodeFilter) ? "" : ("_" + firmCodeFilter.Trim()));
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("\ud83d\udcc5 **SİPARİŞ LİSTESİ TARİH FİLTRESİ SEÇİMİ**\n");
		if (!string.IsNullOrWhiteSpace(firmCodeFilter))
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(32, 1, stringBuilder2);
			handler.AppendLiteral("\ud83c\udfe2 **Cari Hesabı Filtresi:** `");
			handler.AppendFormatted(firmCodeFilter);
			handler.AppendLiteral("`\n");
			stringBuilder2.AppendLine(ref handler);
		}
		stringBuilder.AppendLine("Hangi tarih aralığındaki siparişleri (Açık, Kapalı, Öneri, Sevk Edilebilir tümü) getirmek istersiniz?");
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[3][]
		{
			new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("\ud83d\udcc5 Bugün", "dt_today" + text),
				InlineKeyboardButton.WithCallbackData("\ud83d\udcc5 Bu Hafta", "dt_week" + text)
			},
			new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("\ud83d\udcc5 Bu Ay", "dt_month" + text),
				InlineKeyboardButton.WithCallbackData("\ud83d\udcc5 Son 30 Gün", "dt_30days" + text)
			},
			new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("\ud83d\udcc5 Son 90 Gün", "dt_90days" + text),
				InlineKeyboardButton.WithCallbackData("♾\ufe0f Tüm Zamanlar", "dt_all" + text)
			}
		});
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString().Trim(), cancellationToken, replyMarkup);
		LogActivity(chatId, "/siparisler " + firmCodeFilter, firmCodeFilter ?? "-", "-", 0m, isSuccess: true, "Tarih filtreleme menüsü gönderildi.");
	}

	private InlineKeyboardMarkup GetDocumentActionKeyboard(long chatId, string docType, string ficheNo)
	{
		List<InlineKeyboardButton[]> rows = new List<InlineKeyboardButton[]>();
		rows.Add(new InlineKeyboardButton[]
		{
			InlineKeyboardButton.WithCallbackData("📧 E-Posta Gönder", $"send_mail:{docType}:{ficheNo}"),
			InlineKeyboardButton.WithCallbackData("📱 SMS Gönder", $"send_sms:{docType}:{ficheNo}")
		});

		List<InlineKeyboardButton> row2 = new List<InlineKeyboardButton>
		{
			InlineKeyboardButton.WithCallbackData("🔍 GİB Durum Sorgula", $"gib_status:{ficheNo}")
		};

		if (CanSendEBelge(chatId))
		{
			string prefix = docType == "inv" ? "gib_send_FAT_" : "gib_send_IRS_";
			row2.Add(InlineKeyboardButton.WithCallbackData("📤 GİB'e Gönder", $"{prefix}{ficheNo}"));
		}
		rows.Add(row2.ToArray());

		return new InlineKeyboardMarkup(rows);
	}

	private async Task SendReportDateSelectionMenuAsync(ITelegramBotClient botClient, long chatId, string reportType, string firmCode, CancellationToken cancellationToken)
	{
		string text = reportType switch
		{
			"ekstre" => "📄 **HESAP EKSTRESİ (METİN) - TARİH ARALIĞI SEÇİMİ**", 
			"pdf" => "📜 **HESAP EKSTRESİ (PDF) - TARİH ARALIĞI SEÇİMİ**", 
			"cek" => "💳 **ÇEK RAPORU (METİN) - TARİH ARALIĞI SEÇİMİ**", 
			"cekpdf" => "📜 **ÇEK RAPORU (PDF) - TARİH ARALIĞI SEÇİMİ**", 
			"irsaliye" => "🚚 **İRSALİYELER LİSTESİ - TARİH ARALIĞI SEÇİMİ**",
			"fatura" => "🧾 **FATURALAR LİSTESİ - TARİH ARALIĞI SEÇİMİ**",
			"siparis" => "🛒 **SİPARİŞLER LİSTESİ - TARİH ARALIĞI SEÇİMİ**",
			_ => "📅 **TARİH ARALIĞI SEÇİMİ**", 
		};
		string codeText = (string.IsNullOrWhiteSpace(firmCode) || firmCode == "all") ? "" : "\n• **Cari Kodu:** `" + firmCode + "`";
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[4][]
		{
			new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("📅 Bugün", $"rptdt:{reportType}:{firmCode}:today"),
				InlineKeyboardButton.WithCallbackData("📅 Bu Hafta", $"rptdt:{reportType}:{firmCode}:week")
			},
			new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("📅 Bu Ay", $"rptdt:{reportType}:{firmCode}:month"),
				InlineKeyboardButton.WithCallbackData("📅 Bu Yıl", $"rptdt:{reportType}:{firmCode}:year")
			},
			new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("📅 Son 30 Gün", $"rptdt:{reportType}:{firmCode}:30days"),
				InlineKeyboardButton.WithCallbackData("📅 Son 90 Gün", $"rptdt:{reportType}:{firmCode}:90days")
			},
			new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("♾️ Tüm Zamanlar", $"rptdt:{reportType}:{firmCode}:all") }
		});
		await SendReplyAsync(botClient, chatId, text + codeText + "\n\nLütfen getirilmesini istediğiniz tarih aralığını seçiniz:", cancellationToken, replyMarkup);
	}

	private async Task ProcessReportDateCallbackAsync(ITelegramBotClient botClient, long chatId, string data, CancellationToken cancellationToken)
	{
		string[] array = data.Split(':');
		if (array.Length < 4)
		{
			return;
		}
		string text = array[1];
		string code = array[2];
		string text2 = array[3];
		DateTime now = DateTime.Now;
		DateTime today = DateTime.Today;
		DateTime? startDate;
		DateTime? dateTime;
		switch (text2)
		{
		case "today":
			startDate = today;
			dateTime = today.AddDays(1.0).AddTicks(-1L);
			break;
		case "week":
		{
			int num = (int)(today.DayOfWeek - 1);
			if (num < 0)
			{
				num += 7;
			}
			startDate = today.AddDays(-1 * num);
			dateTime = now;
			break;
		}
		case "month":
			startDate = new DateTime(today.Year, today.Month, 1);
			dateTime = now;
			break;
		case "year":
			startDate = new DateTime(today.Year, 1, 1);
			dateTime = now;
			break;
		case "30days":
			startDate = today.AddDays(-30.0);
			dateTime = now;
			break;
		case "90days":
			startDate = today.AddDays(-90.0);
			dateTime = now;
			break;
		default:
			startDate = null;
			dateTime = null;
			break;
		}
		switch (text)
		{
		case "ekstre":
		{
			string text5 = (startDate.HasValue ? $" {startDate:dd.MM.yyyy} {dateTime:dd.MM.yyyy}" : "");
			ProposalCreateResult result = await _proposalAppService.ProcessTelegramMessageAsync(code + " EKSTRE" + text5, chatId, cancellationToken);
			await SendReplyAsync(botClient, chatId, result.Message, cancellationToken);
			LogActivity(chatId, "Buton: Ekstre Metin (" + code + ")", code, "-", 0m, result.IsSuccess, result.Message);
			break;
		}
		case "pdf":
		{
			byte[] array3 = await _proposalAppService.GetStatementPdfAsync(code, startDate, dateTime, cancellationToken);
			if (array3 != null && array3.Length != 0)
			{
				string text6 = (startDate.HasValue ? $"_{startDate:dd.MM.yy}" : "");
				InlineKeyboardMarkup pdfReplyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
				{
					new InlineKeyboardButton[]
					{
						InlineKeyboardButton.WithCallbackData("📧 E-Posta / Mail Gönder", "send_mail:ekstre:" + code),
						InlineKeyboardButton.WithCallbackData("📱 SMS Gönder", "send_sms:ekstre:" + code)
					},
					new InlineKeyboardButton[]
					{
						InlineKeyboardButton.WithCallbackData("ℹ️ Cari Bilgileri", "act:info:" + code)
					}
				});
				await SendPdfDocumentAsync(botClient, chatId, array3, "Ekstre_" + code + text6 + ".pdf", "\ud83d\udcc4 `" + code + "` kodlu Cari Hesabın Ekstresi (PDF) ektedir.\n\nAşağıdaki butonlar ile müşterinize **E-Posta** veya **SMS** olarak iletebilirsiniz:", cancellationToken, pdfReplyMarkup);
				LogActivity(chatId, "Buton: PDF Ekstre (" + code + ")", code, "-", 0m, isSuccess: true, "PDF Ekstre gönderildi.");
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "❌ '" + code + "' kodlu Cari hesabına ait seçilen tarihlerde PDF ekstre bulunamadı.", cancellationToken);
				LogActivity(chatId, "Buton: PDF Ekstre (" + code + ")", code, "-", 0m, isSuccess: false, "PDF ekstre bulunamadı.");
			}
			break;
		}
		case "cek":
		{
			string text4 = (startDate.HasValue ? $" {startDate:dd.MM.yyyy} {dateTime:dd.MM.yyyy}" : "");
			ProposalCreateResult result = await _proposalAppService.ProcessTelegramMessageAsync(code + " CEK" + text4, chatId, cancellationToken);
			await SendReplyAsync(botClient, chatId, result.Message, cancellationToken);
			LogActivity(chatId, "Buton: Çek Raporu Metin (" + code + ")", code, "-", 0m, result.IsSuccess, result.Message);
			break;
		}
		case "cekpdf":
		{
			byte[] array2 = await _proposalAppService.GetChequeReportPdfAsync(code, startDate, dateTime, cancellationToken);
			if (array2 != null && array2.Length != 0)
			{
				string text3 = (startDate.HasValue ? $"_{startDate:dd.MM.yy}" : "");
				await SendPdfDocumentAsync(botClient, chatId, array2, "CekRaporu_" + code + text3 + ".pdf", "\ud83d\udcdc `" + code + "` kodlu Cari Hesabın Çek Raporu (PDF) ektedir.\n'Paylaş' seçeneği ile iletebilirsiniz.", cancellationToken);
				LogActivity(chatId, "Buton: Çek Raporu PDF (" + code + ")", code, "-", 0m, isSuccess: true, "Çek Raporu PDF gönderildi.");
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "❌ '" + code + "' kodlu Cari hesabına ait seçilen tarihlerde Çek Raporu (PDF) bulunamadı veya çek kaydı yok.", cancellationToken);
				LogActivity(chatId, "Buton: Çek Raporu PDF (" + code + ")", code, "-", 0m, isSuccess: false, "Çek raporu PDF bulunamadı.");
			}
			break;
		}
		case "irsaliye":
		{
			string fCode = (code == "all" || string.IsNullOrWhiteSpace(code)) ? null : code;
			await SendDispatchListAsync(botClient, chatId, fCode, startDate, dateTime, cancellationToken);
			break;
		}
		case "fatura":
		{
			string fCode = (code == "all" || string.IsNullOrWhiteSpace(code)) ? null : code;
			await SendInvoiceListAsync(botClient, chatId, fCode, startDate, dateTime, cancellationToken);
			break;
		}
		case "siparis":
		{
			string fCode = (code == "all" || string.IsNullOrWhiteSpace(code)) ? null : code;
			await SendOpenOrdersListAsync(botClient, chatId, fCode, startDate, dateTime, cancellationToken);
			break;
		}
		}
	}

	private async Task ProcessDateFilterCallbackAsync(ITelegramBotClient botClient, long chatId, string data, CancellationToken cancellationToken)
	{
		string[] array = data.Split('_');
		string text = ((array.Length > 1) ? array[1].ToLowerInvariant() : "all");
		string firmCodeFilter = ((array.Length > 2) ? array[2] : null);
		DateTime now = DateTime.Now;
		DateTime today = DateTime.Today;
		DateTime? startDate;
		DateTime? endDate;
		string rangeText;
		switch (text)
		{
		case "today":
			startDate = today;
			endDate = today.AddDays(1.0).AddTicks(-1L);
			rangeText = "Bugün (" + today.ToString("dd.MM.yyyy") + ")";
			break;
		case "week":
		{
			int num = (int)(today.DayOfWeek - 1);
			if (num < 0)
			{
				num += 7;
			}
			startDate = today.AddDays(-1 * num);
			endDate = now;
			rangeText = "Bu Hafta (" + startDate.Value.ToString("dd.MM.yyyy") + " - " + now.ToString("dd.MM.yyyy") + ")";
			break;
		}
		case "month":
			startDate = new DateTime(today.Year, today.Month, 1);
			endDate = now;
			rangeText = "Bu Ay (" + startDate.Value.ToString("dd.MM.yyyy") + " - " + now.ToString("dd.MM.yyyy") + ")";
			break;
		case "30days":
			startDate = today.AddDays(-30.0);
			endDate = now;
			rangeText = "Son 30 Gün (" + startDate.Value.ToString("dd.MM.yyyy") + " - " + now.ToString("dd.MM.yyyy") + ")";
			break;
		case "90days":
			startDate = today.AddDays(-90.0);
			endDate = now;
			rangeText = "Son 90 Gün (" + startDate.Value.ToString("dd.MM.yyyy") + " - " + now.ToString("dd.MM.yyyy") + ")";
			break;
		default:
			startDate = null;
			endDate = null;
			rangeText = "Tüm Zamanlar";
			break;
		}
		await SendFilteredOrdersListAsync(botClient, chatId, firmCodeFilter, startDate, endDate, rangeText, cancellationToken);
	}

	private async Task SendFilteredOrdersListAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string rangeText, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId);
		string orderCyphCode = ((user != null && !user.CanViewAllOrders && !string.IsNullOrWhiteSpace(user.AllowedOrderCyphCode)) ? user.AllowedOrderCyphCode : null);
		string firmCyphCode = ((user != null && !user.CanViewAllFirms && !string.IsNullOrWhiteSpace(user.AllowedFirmCyphCode)) ? user.AllowedFirmCyphCode : null);
		List<LogoErpOpenOrderSummaryDto> orders = await _logoErpRepository.GetOrdersAsync(firmCodeFilter, startDate, endDate, firmNo, periodNo, orderCyphCode, firmCyphCode, 30, cancellationToken);
		string fCodeParam = string.IsNullOrWhiteSpace(firmCodeFilter) ? "all" : firmCodeFilter;
		if (orders == null || orders.Count == 0)
		{
			string text2 = (string.IsNullOrWhiteSpace(firmCodeFilter) ? ("📦 **SEÇİLEN TARİH ARALIĞINDA SİPARİŞ BULUNAMADI**\n\n📅 **Filtre:** `" + rangeText + "`\nLogo ERP sisteminde bu tarih aralığına ait sipariş kaydı bulunmamaktadır.") : $"📦 **'{firmCodeFilter}' CARİSİ İÇİN SİPARİŞ BULUNAMADI**\n\n📅 **Filtre:** `{rangeText}`\nBu cari hesabına ait seçilen tarih aralığında sipariş bulunmamaktadır.");
			InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[1][] { new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("📅 Başka Tarih Aralığı Seç", $"act:dt_sip:{fCodeParam}") } });
			await SendReplyAsync(botClient, chatId, text2, cancellationToken, replyMarkup);
			LogActivity(chatId, "/siparisler " + rangeText, firmCodeFilter ?? "-", "-", 0m, isSuccess: true, "Filtrelenen sipariş bulunamadı.");
			return;
		}
		// En son girilen sipariş en alta gelsin
		orders = orders.OrderBy(o => o.Date).ThenBy(o => o.LogicalRef).ToList();

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("📋 **SİPARİŞ LİSTESİ**");
		stringBuilder.AppendLine($"📅 **Tarih Aralığı:** `{rangeText}` | 📊 **Toplam:** `{orders.Count} Sipariş`");
		if (!string.IsNullOrWhiteSpace(firmCodeFilter))
		{
			stringBuilder.AppendLine($"🔍 **Cari Filtresi:** `{firmCodeFilter}`");
		}
		stringBuilder.AppendLine("\n📌 **Statü Simgeleri:** **(Ö)** Öneri | **(S)** Sevk Edilebilir | **(K)** Kapanmış");
		stringBuilder.AppendLine("Detayını ve kalemlerini görmek istediğiniz siparişe tıklayınız:\n*(En son girilen sipariş en alttadır)*");
		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		foreach (LogoErpOpenOrderSummaryDto item in orders)
		{
			string statusBadge = GetStatusBadge(item.StatusText);
			string text3 = $"🛒 {item.FicheNo} {statusBadge} ({item.Date:dd.MM.yyyy} - {item.NetTotal:N2} TL)";
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData(text3, "ord_opt:" + item.FicheNo) });
		}
		list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("📅 Tarih Aralığı Filtresini Değiştir", $"act:dt_sip:{fCodeParam}") });
		InlineKeyboardMarkup replyMarkup2 = new InlineKeyboardMarkup(list);
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString().Trim(), cancellationToken, replyMarkup2);
		LogActivity(chatId, "/siparisler " + rangeText, firmCodeFilter ?? "-", "-", 0m, isSuccess: true, $"{orders.Count} adet sipariş listelendi.");
	}

	private async Task SendOpenOrdersListAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, CancellationToken cancellationToken)
	{
		await SendOpenOrdersListAsync(botClient, chatId, firmCodeFilter, null, null, cancellationToken);
	}

	private async Task SendOpenOrdersListAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId);
		string orderCyphCode = ((user != null && !user.CanViewAllOrders && !string.IsNullOrWhiteSpace(user.AllowedOrderCyphCode)) ? user.AllowedOrderCyphCode : null);
		string firmCyphCode = ((user != null && !user.CanViewAllFirms && !string.IsNullOrWhiteSpace(user.AllowedFirmCyphCode)) ? user.AllowedFirmCyphCode : null);
		List<LogoErpOpenOrderSummaryDto> orders = await _logoErpRepository.GetOrdersAsync(firmCodeFilter, startDate, endDate, firmNo, periodNo, orderCyphCode, firmCyphCode, 30, cancellationToken);
		string fCodeParam = string.IsNullOrWhiteSpace(firmCodeFilter) ? "all" : firmCodeFilter;
		if (orders == null || orders.Count == 0)
		{
			InlineKeyboardMarkup emptyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[1][]
			{
				new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("📅 Başka Tarih Aralığı Seç", $"act:dt_sip:{fCodeParam}") }
			});
			await SendReplyAsync(botClient, chatId, "🛒 Seçilen tarih aralığında kayıtlı herhangi bir sipariş bulunamadı.", cancellationToken, emptyMarkup);
			return;
		}

		// En son girilen sipariş en alta gelsin
		orders = orders.OrderBy(o => o.Date).ThenBy(o => o.LogicalRef).ToList();

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("🛒 **SİPARİŞ LİSTESİ**");
		if (startDate.HasValue)
		{
			stringBuilder.AppendLine($"📅 **Tarih Aralığı:** `{startDate:dd.MM.yyyy}` - `{endDate:dd.MM.yyyy}` *(Toplam: {orders.Count} Sipariş)*");
		}
		else
		{
			stringBuilder.AppendLine($"📅 **Tarih:** `Tüm Zamanlar` *(Toplam: {orders.Count} Sipariş)*");
		}
		stringBuilder.AppendLine("\nAşağıdaki listeden sipariş numarası, tarihi ve net tutarını inceleyebilir, PDF indirmek veya işlem yapmak için butona tıklayabilirsiniz:\n*(En son girilen sipariş en alttadır)*");

		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		foreach (LogoErpOpenOrderSummaryDto item in orders)
		{
			string statusBadge = GetStatusBadge(item.StatusText);
			list.Add(new InlineKeyboardButton[1] 
			{ 
				InlineKeyboardButton.WithCallbackData($"🛒 {item.FicheNo} {statusBadge} ({item.Date:dd.MM.yyyy} - {item.NetTotal:N2} TL)", "ord_opt:" + item.FicheNo)
			});
		}
		list.Add(new InlineKeyboardButton[1]
		{
			InlineKeyboardButton.WithCallbackData("📅 Tarih Aralığı Filtresini Değiştir", $"act:dt_sip:{fCodeParam}")
		});
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, replyMarkup);
		LogActivity(chatId, "/siparisler", firmCodeFilter ?? "-", "-", 0m, isSuccess: true, $"{orders.Count} sipariş listelendi.");
	}

	private async Task SendOpenOrderDetailAsync(ITelegramBotClient botClient, long chatId, string ficheNo, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		LogoErpOpenOrderDetailDto detail = await _logoErpRepository.GetOpenOrderDetailAsync(ficheNo, firmNo, periodNo, cancellationToken);
		if (detail == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ `" + ficheNo + "` numaralı sipariş detayları bulunamadığı için açılamadı.", cancellationToken);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"📄 **SİPARİŞ DETAYI: `{detail.FicheNo}`**");
		stringBuilder.AppendLine($"🏢 **Cari:** `{detail.FirmCode}` - **{detail.FirmName}**");
		stringBuilder.AppendLine($"📅 **Tarih:** `{detail.Date:dd.MM.yyyy}`");
		stringBuilder.AppendLine($"👤 **Satış Temsilcisi:** `{detail.SalesmanName}`");
		stringBuilder.AppendLine($"📌 **Durumu:** `{detail.StatusText}`");
		if (!string.IsNullOrWhiteSpace(detail.Description))
		{
			stringBuilder.AppendLine($"📝 **Açıklama:** _{detail.Description}_");
		}
		stringBuilder.AppendLine("\n📦 **Sipariş Kalemleri (İskonto & KDV Detaylı):**");
		int num = 1;
		foreach (LogoErpOpenOrderLineDto line in detail.Lines)
		{
			decimal gross = (line.ListPrice > 0m ? line.ListPrice : line.NetPrice) * line.Amount;
			decimal discAmt = line.DiscountAmount;
			decimal vatAmt = line.VatAmount;

			stringBuilder.AppendLine($"\n**{num}.** `{line.ItemCode}` - **{line.ItemName}**");
			stringBuilder.AppendLine($"   • **Miktar:** `{line.Amount:N2} {line.UnitName}` (Sevk: `{line.ShippedAmount:N2}` | Kalan: `{line.PendingAmount:N2}`)");
			if (line.ListPrice > 0m && line.ListPrice > line.NetPrice)
			{
				stringBuilder.AppendLine($"   • **Liste Fiyatı:** `{line.ListPrice:N2} TL` *(Brüt: {gross:N2} TL)*");
			}
			else
			{
				stringBuilder.AppendLine($"   • **Birim Fiyat:** `{line.NetPrice:N2} TL`");
			}

			if (line.DiscountRate > 0m || discAmt > 0m)
			{
				stringBuilder.AppendLine($"   • **İskonto Oranı & Tutarı:** `%{line.DiscountRate:N1}` (`-{discAmt:N2} TL`) ➔ **Net Fiyat:** `{line.NetPrice:N2} TL`");
			}
			stringBuilder.AppendLine($"   • **KDV Oranı & Tutarı:** `%{line.VatRate:F0}` (`+{vatAmt:N2} TL`)");
			stringBuilder.AppendLine($"   • **Kalem Net Tutarı:** `{line.LineNetTotal:N2} TL` *(KDV Dahil: {line.LineGrandTotal:N2} TL)*");
			num++;
		}
		decimal num2 = detail.TotalVat;
		decimal value3 = detail.GrandTotal;
		stringBuilder.AppendLine($"\n💵 **Brüt Toplam:** `{detail.GrossTotal:N2} TL`");
		if (detail.TotalDiscount > 0m)
		{
			decimal avgDiscRate = detail.GrossTotal > 0m ? (detail.TotalDiscount / detail.GrossTotal * 100m) : 0m;
			stringBuilder.AppendLine($"📉 **Toplam İskonto Tutarı:** `-{detail.TotalDiscount:N2} TL` *(İskonto Oranı: %{avgDiscRate:N1})*");
		}
		stringBuilder.AppendLine($"💶 **Net Ara Toplam (KDV Hariç):** `{detail.NetTotal:N2} TL`");
		stringBuilder.AppendLine($"📊 **Toplam KDV Tutarı:** `+{num2:N2} TL`");
		stringBuilder.AppendLine($"💳 **GENEL TOPLAM (KDV Dahil):** `{value3:N2} TL`");

		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[2][]
		{
			new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("🚚 Siparişi İrsaliyeye Çevir (İrsaliye Kes)", "ord_to_irs_" + detail.FicheNo) },
			new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("📄 PDF Oluştur & İndir", "ord_pdf_" + detail.FicheNo),
				InlineKeyboardButton.WithCallbackData("🔙 Sipariş Listesine Dön", "acik_siparis_listesi")
			}
		});
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString().Trim(), cancellationToken, replyMarkup);
		LogActivity(chatId, "Sipariş Detayı Butonu: " + ficheNo, detail.FirmCode, "-", 0m, isSuccess: true, ficheNo + " sipariş detayları görüntülendi.");
	}

	private static string GetStatusBadge(string statusText)
	{
		if (string.IsNullOrWhiteSpace(statusText))
		{
			return "(Ö)";
		}
		if (statusText.Contains("Öneri", StringComparison.OrdinalIgnoreCase))
		{
			return "(Ö)";
		}
		if (statusText.Contains("Kapanmış", StringComparison.OrdinalIgnoreCase))
		{
			return "(K)";
		}
		if (statusText.Contains("Sevk", StringComparison.OrdinalIgnoreCase) || statusText.Contains("Onay", StringComparison.OrdinalIgnoreCase))
		{
			return "(S)";
		}
		return "(Ö)";
	}

	private static string TruncateString(string value, int maxLength)
	{
		if (string.IsNullOrEmpty(value))
		{
			return "";
		}
		if (value.Length > maxLength)
		{
			return value.Substring(0, maxLength - 1) + "…";
		}
		return value;
	}

	private async Task SendOrderPdfDocumentAsync(ITelegramBotClient botClient, long chatId, string ficheNo, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		LogoErpOpenOrderDetailDto detail = await _logoErpRepository.GetOpenOrderDetailAsync(ficheNo, firmNo, periodNo, cancellationToken);
		if (detail == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ `" + ficheNo + "` numaralı sipariş detayları bulunamadığı için PDF oluşturulamadı.", cancellationToken);
			return;
		}
		try
		{
			decimal num = detail.Lines.Sum((LogoErpOpenOrderLineDto l) => (!(l.ListPrice > l.NetPrice) || !(l.Amount > 0m)) ? 0m : ((l.ListPrice - l.NetPrice) * l.Amount));
			decimal generalDiscountAmount = Math.Max(0m, detail.TotalDiscount - num);
			LogoErpOrderPrintDto order = new LogoErpOrderPrintDto
			{
				FicheNo = detail.FicheNo,
				Date = detail.Date,
				FirmCode = detail.FirmCode,
				FirmName = detail.FirmName,
				SalesmanName = detail.SalesmanName,
				Description = detail.Description,
				GeneralDiscountAmount = generalDiscountAmount,
				Lines = detail.Lines.Select((LogoErpOpenOrderLineDto l) => new LogoErpOrderPrintLineDto
				{
					ItemCode = l.ItemCode,
					ItemName = l.ItemName,
					Amount = l.Amount,
					UnitName = l.UnitName,
					ListPrice = l.ListPrice,
					DiscountRate = l.DiscountRate,
					UnitPrice = l.NetPrice,
					VatRate = l.VatRate
				}).ToList()
			};
			byte[] buffer = _orderPdfGenerator.GenerateOrderPdf(order);
			using MemoryStream ms = new MemoryStream(buffer);
			string fileName = "Siparis_Formu_" + detail.FicheNo + ".pdf";
			InlineKeyboardMarkup replyMarkup = GetDocumentActionKeyboard(chatId, "ord", detail.FicheNo);
			await botClient.SendDocument(chatId, InputFile.FromStream(ms, fileName), $"📄 **SİPARİŞ FORMU (PDF)**\n\n📌 **Fiş No:** `{detail.FicheNo}`\n🏢 **Cari:** `{detail.FirmCode}` - **{detail.FirmName}**\n💰 **Net Toplam:** `{detail.NetTotal:N2} TL`", ParseMode.Markdown, null, replyMarkup, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
			LogActivity(chatId, "/siparis_pdf " + ficheNo, detail.FirmCode, "-", 0m, isSuccess: true, ficheNo + " PDF sipariş formu üretilip gönderildi.");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "{FicheNo} siparişi için PDF oluşturulurken hata oluştu.", ficheNo);
			await SendReplyAsync(botClient, chatId, "❌ `" + ficheNo + "` siparişi için PDF oluşturulurken bir hata meydana geldi.", cancellationToken);
		}
	}

	private async Task SendDraftPdfPreviewAsync(ITelegramBotClient botClient, long chatId, string draftId, CancellationToken cancellationToken)
	{
		DraftOrderDto draft = _draftOrderStore.GetDraft(draftId);
		if (draft == null)
		{
			await SendReplyAsync(botClient, chatId, "⚠\ufe0f PDF Önizleme taslağının süresi dolmuş veya taslak bulunamadı. Lütfen Excel dosyasını tekrar yükleyiniz.", cancellationToken);
			return;
		}
		try
		{
			LogoErpOrderPrintDto order = _draftOrderStore.CreatePrintDtoFromDraft(draft, "Excel Taslak Önizleme");
			byte[] buffer = _orderPdfGenerator.GenerateOrderPdf(order);
			using MemoryStream ms = new MemoryStream(buffer);
			string fileName = "Siparis_Taslagi_PREVIEW_" + draft.FirmCode + ".pdf";
			await botClient.SendDocument(chatId, InputFile.FromStream(ms, fileName), $"\ud83d\udcc4 **SİPARİŞ TASLAĞI (PDF ÖNİZLEME)** (Henüz Logo ERP'ye Kaydedilmedi)\n\n\ud83c\udfe2 **Cari:** `{draft.FirmCode}` - **{draft.FirmName}**\n\ud83d\udcca **Kalem:** `{draft.Lines.Count} Ürün`", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
			LogActivity(chatId, "PDF Taslak Önizleme", draft.FirmCode, "-", 0m, isSuccess: true, "Excel siparişi veritabanına kaydedilmeden PDF önizleme gönderildi.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Draft PDF oluşturulurken hata oluştu: {DraftId}", draftId);
			await SendReplyAsync(botClient, chatId, "❌ PDF Önizleme oluşturulurken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private async Task ConfirmDraftOrderAsync(ITelegramBotClient botClient, long chatId, string draftId, CancellationToken cancellationToken)
	{
		DraftOrderDto draft = _draftOrderStore.GetDraft(draftId);
		OrderWizardState value2;
		if (draft == null)
		{
			if (!_wizardStates.TryGetValue(chatId, out OrderWizardState value) || value.Lines.Count <= 0)
			{
				await SendReplyAsync(botClient, chatId, "⚠\ufe0f Taslak bulunamadı veya süresi doldu. Lütfen Excel dosyasını tekrar yükleyiniz.", cancellationToken);
				return;
			}
			await ExecuteFinalMultiLineOrderAsync(botClient, chatId, value.FirmCode, value.FirmName, "Excel Kataloğu", value.Lines, value.Lines.Count, cancellationToken);
			_wizardStates.TryRemove(chatId, out value2);
			return;
		}
		try
		{
			string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
			string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
			int.TryParse(_configuration["LogoSystem:WarehouseNo"] ?? "0", out var warehouseNo);
			TelegramUserMappingDto userRec = _telegramUserStore.GetUser(chatId);
			int? salesmanRef = userRec?.MappedSalesmanRef;
			foreach (LogoErpOrderLineInput line in draft.Lines)
			{
				if (line.ItemDetails == null || line.ItemDetails.ItemLogicalRef <= 0)
				{
					LogoErpItemDetails logoErpItemDetails = await _logoErpRepository.GetItemDetailsAsync(line.ProductCode, firmNo, cancellationToken);
					if (logoErpItemDetails != null)
					{
						line.ItemDetails = logoErpItemDetails;
					}
				}
			}
			string ficheNo = (await _logoErpRepository.CreateMultiLineErpOrderTransactionAsync(draft.ClientRef, draft.Lines, firmNo, periodNo, warehouseNo, "Excel Katalog Siparişi (@" + (userRec?.Username ?? "Bot") + ")", salesmanRef, cancellationToken)).Item3;
			_draftOrderStore.RemoveDraft(draftId);
			_wizardStates.TryRemove(chatId, out value2);
			InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[1][] { new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("\ud83d\udcc4 PDF Oluştur & İndir", "ord_pdf_" + ficheNo),
				InlineKeyboardButton.WithCallbackData("\ud83d\udd19 Sipariş Listesine Dön", "acik_siparis_listesi")
			} });
			decimal grandNet = draft.Lines.Sum((LogoErpOrderLineInput l) => l.Amount * l.UnitPrice.GetValueOrDefault() * (1m - l.DiscountRate / 100m));
			await SendReplyAsync(botClient, chatId, $"\ud83c\udf89 **SİPARİŞ BAŞARIYLA LOGO ERP'YE AKTARILDI!**\n\n\ud83d\udccc **Fiş Numarası:** `{ficheNo}` (STATUS = 1 Öneri)\n\ud83c\udfe2 **Cari:** `{draft.FirmCode}` - **{draft.FirmName}**\n\ud83d\udcca **Kalem:** `{draft.Lines.Count} Ürün`\n\ud83d\udcb0 **Net Toplam:** `{grandNet:N2} TL`\n\nİsterseniz aşağıdaki butonla resmi PDF belgesini indirebilirsiniz:", cancellationToken, replyMarkup);
			LogActivity(chatId, "Excel Taslak Onayı", draft.FirmCode, "-", grandNet, isSuccess: true, "Excel siparişi aktarıldı. Fiş No: " + ficheNo);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Taslak sipariş onaylanırken hata oluştu.");
			await SendReplyAsync(botClient, chatId, "❌ Sipariş Logo ERP veritabanına aktarılırken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private async Task SendDispatchListAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, CancellationToken cancellationToken)
	{
		await SendDispatchListAsync(botClient, chatId, firmCodeFilter, null, null, cancellationToken);
	}

	private async Task SendDispatchListAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		List<LogoErpOpenOrderSummaryDto> dispatches = await _logoErpRepository.GetDispatchesAsync(firmCodeFilter, startDate, endDate, firmNo, periodNo, 30, cancellationToken);
		string fCodeParam = string.IsNullOrWhiteSpace(firmCodeFilter) ? "all" : firmCodeFilter;
		if (dispatches == null || dispatches.Count == 0)
		{
			InlineKeyboardMarkup emptyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[1][]
			{
				new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("📅 Başka Tarih Aralığı Seç", $"act:dt_irs:{fCodeParam}") }
			});
			await SendReplyAsync(botClient, chatId, "🚚 Seçilen tarih aralığında kayıtlı herhangi bir satış irsaliyesi bulunamadı.", cancellationToken, emptyMarkup);
			return;
		}

		// En son kesilen irsaliye en alta gelsin
		dispatches = dispatches.OrderBy(d => d.Date).ThenBy(d => d.LogicalRef).ToList();

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("🚚 **TOPTAN SATIŞ İRSALİYELERİ LİSTESİ**");
		if (startDate.HasValue)
		{
			stringBuilder.AppendLine($"📅 **Tarih Aralığı:** `{startDate:dd.MM.yyyy}` - `{endDate:dd.MM.yyyy}` *(Toplam: {dispatches.Count} İrsaliye)*");
		}
		else
		{
			stringBuilder.AppendLine($"📅 **Tarih:** `Tüm Zamanlar` *(Toplam: {dispatches.Count} İrsaliye)*");
		}
		stringBuilder.AppendLine("\nAşağıdaki listeden irsaliye numarası, tarihi ve net tutarını inceleyebilir, PDF indirmek veya işlem yapmak için butona tıklayabilirsiniz:\n*(En son kesilen irsaliye en alttadır)*");

		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		foreach (LogoErpOpenOrderSummaryDto item in dispatches)
		{
			list.Add(new InlineKeyboardButton[1] 
			{ 
				InlineKeyboardButton.WithCallbackData($"🚚 {item.FicheNo} ({item.Date:dd.MM.yyyy} - {item.NetTotal:N2} TL)", "irs_opts_" + item.FicheNo)
			});
		}
		list.Add(new InlineKeyboardButton[1]
		{
			InlineKeyboardButton.WithCallbackData("📅 Tarih Aralığı Filtresini Değiştir", $"act:dt_irs:{fCodeParam}")
		});
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, replyMarkup);
		LogActivity(chatId, "/irsaliyeler", firmCodeFilter ?? "-", "-", 0m, isSuccess: true, $"{dispatches.Count} satış irsaliyesi listelendi.");
	}

	private async Task SendStandardPdfDocumentAsync(ITelegramBotClient botClient, long chatId, string ficheNo, bool isInvoice, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		LogoErpDispatchDto dispatch = await _logoErpRepository.GetDispatchDetailAsync(ficheNo, firmNo, periodNo, cancellationToken);
		if (dispatch == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ `" + ficheNo + "` numaralı belge bulunamadı.", cancellationToken);
			return;
		}
		try
		{
			byte[] buffer = _standardPdfGenerator.GenerateStandardDispatchPdf(dispatch, isInvoice);
			string title = (isInvoice ? "Fatura" : "İrsaliye");
			using MemoryStream ms = new MemoryStream(buffer);
			string fileName = $"Standart_{title}_{dispatch.FicheNo}.pdf";
			InlineKeyboardMarkup replyMarkup = GetDocumentActionKeyboard(chatId, isInvoice ? "inv" : "irs", dispatch.FicheNo);
			await botClient.SendDocument(chatId, InputFile.FromStream(ms, fileName), $"📄 **STANDART {title.ToUpper()} PDF**\n\n📌 **Belge No:** `{dispatch.FicheNo}`\n🏢 **Cari:** `{dispatch.FirmCode}` - **{dispatch.FirmName}**\n💰 **Net Toplam:** `{dispatch.NetTotal:N2} TL`", ParseMode.Markdown, null, replyMarkup, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
			LogActivity(chatId, "Standart " + title + " PDF Gönderimi", dispatch.FirmCode, "-", dispatch.NetTotal, isSuccess: true, "Standart " + title + " PDF belgesi gönderildi. No: " + ficheNo);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Standart PDF belgesi üretilirken hata oluştu: {FicheNo}", ficheNo);
			await SendReplyAsync(botClient, chatId, "❌ Standart PDF belgesi oluşturulurken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private async Task SendDispatchPdfDocumentAsync(ITelegramBotClient botClient, long chatId, string ficheNo, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		LogoErpDispatchDto dispatch = await _logoErpRepository.GetDispatchDetailAsync(ficheNo, firmNo, periodNo, cancellationToken);
		if (dispatch == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ `" + ficheNo + "` numaralı satış irsaliyesi bulunamadı.", cancellationToken);
			return;
		}
		try
		{
			using MemoryStream ms = new MemoryStream(await _xsltDispatchService.RenderDispatchPdfAsync(dispatch, null, cancellationToken));
			string fileName = "eIrsaliye_" + dispatch.FicheNo + ".pdf";
			InlineKeyboardMarkup replyMarkup = GetDocumentActionKeyboard(chatId, "irs", dispatch.FicheNo);
			await botClient.SendDocument(chatId, InputFile.FromStream(ms, fileName), $"\ud83d\ude9a **T.C. GELİR İDARESİ BAŞKANLIĞI e-İRSALİYE**\n\n\ud83d\udccc **İrsaliye No:** `{dispatch.FicheNo}`\n\ud83c\udfe2 **Cari:** `{dispatch.FirmCode}` - **{dispatch.FirmName}**\n\ud83d\udcb0 **Net Toplam:** `{dispatch.NetTotal:N2} TL`\n\n_GİB UBL-TR standartlarına uygun resmi e-İrsaliye belgesidir._", ParseMode.Markdown, null, replyMarkup, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
			LogActivity(chatId, "/irsaliye " + ficheNo, dispatch.FirmCode, "-", dispatch.NetTotal, isSuccess: true, "GİB e-İrsaliye PDF belgesi gönderildi: " + ficheNo);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "GİB e-İrsaliye PDF belgesi üretilirken hata oluştu: {FicheNo}", ficheNo);
			await SendReplyAsync(botClient, chatId, "❌ GİB e-İrsaliye PDF belgesi oluşturulurken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private async Task ConvertOrderToDispatchHandlerAsync(ITelegramBotClient botClient, long chatId, string orderFicheNo, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		int.TryParse(_configuration["LogoSystem:WarehouseNo"] ?? "0", out var warehouseNo);
		TelegramUserMappingDto userRec = _telegramUserStore.GetUser(chatId);
		int? salesmanRef = userRec?.MappedSalesmanRef;
		try
		{
			await SendReplyAsync(botClient, chatId, "⏳ `" + orderFicheNo + "` numaralı sipariş Logo ERP Satış İrsaliyesine dönüştürülüyor...", cancellationToken);
			string dispatchFicheNo = (await _logoErpRepository.ConvertOrderToDispatchAsync(orderFicheNo, firmNo, periodNo, warehouseNo, "Telegram Bot Sipariş İrsaliyeleştirme (@" + (userRec?.Username ?? "Bot") + ")", salesmanRef, null, cancellationToken)).Item3;
			LogoErpDispatchDto dispatch = await _logoErpRepository.GetDispatchDetailAsync(dispatchFicheNo, firmNo, periodNo, cancellationToken);
			List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udcc4 GİB e-İrsaliye PDF (XSLT) (" + dispatchFicheNo + ")", "irs_pdf_" + dispatchFicheNo) });
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udcc4 İrsaliye PDF (Standart) (" + dispatchFicheNo + ")", "irs_pdf_std_" + dispatchFicheNo) });
			if (CanSendEBelge(chatId))
			{
				list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udce4 e-İrsaliye GİB'e Gönder (" + dispatchFicheNo + ")", "gib_send_IRS_" + dispatchFicheNo) });
			}
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udd19 İrsaliye Listesine Dön", "irsaliye_listesi") });
			InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
			await SendReplyAsync(botClient, chatId, $"\ud83c\udf89 **SİPARİŞ BAŞARIYLA LOGO ERP SATIŞ İRSALİYESİNE DÖNÜŞTÜRÜLDÜ!**\n\n\ud83d\udccc **Sipariş Fişi:** `{orderFicheNo}` (Kapatıldı/Sevk Edildi)\n\ud83d\ude9a **Yeni Satış İrsaliyesi No:** `{dispatchFicheNo}` (TRCODE = 8)\n\ud83d\udd17 **Logo Entegrasyonu:** `STLINE.ORDTRANSREF` ve `ORDFICHEREF` ilişkisi sağlandı.\n\ud83c\udfe2 **Cari:** `{dispatch?.FirmCode}` - **{dispatch?.FirmName}**\n\ud83d\udcb0 **Net İrsaliye Toplamı:** `{dispatch?.NetTotal:N2} TL`\n\nResmi GİB e-İrsaliye PDF çıktısını aşağıdan indirebilirsiniz:", cancellationToken, replyMarkup);
			if (dispatch != null)
			{
				await SendDispatchPdfDocumentAsync(botClient, chatId, dispatchFicheNo, cancellationToken);
			}
			LogActivity(chatId, "Siparişi İrsaliyeye Çevir (" + orderFicheNo + ")", dispatch?.FirmCode ?? "-", "-", dispatch?.NetTotal ?? 0m, isSuccess: true, $"Sipariş '{orderFicheNo}' -> İrsaliye '{dispatchFicheNo}' dönüştürüldü.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Sipariş irsaliyeleştirilirken hata oluştu: {OrderFicheNo}", orderFicheNo);
			await SendReplyAsync(botClient, chatId, "❌ Sipariş Satış İrsaliyesine dönüştürülürken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private async Task ExecuteFinalDispatchFromWizardAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		int.TryParse(_configuration["LogoSystem:WarehouseNo"] ?? "0", out var warehouseNo);
		TelegramUserMappingDto userRec = _telegramUserStore.GetUser(chatId);
		int? salesmanRef = userRec?.MappedSalesmanRef;
		int firmRef = (await _logoErpRepository.GetFirmLogicalRefAsync(wizard.FirmCode, firmNo, cancellationToken)).GetValueOrDefault();
		if (firmRef <= 0)
		{
			await SendReplyAsync(botClient, chatId, "❌ `" + wizard.FirmCode + "` kodlu cari hesap Logo ERP'de bulunamadı.", cancellationToken);
			return;
		}
		List<LogoErpOrderLineInput> lineInputs = new List<LogoErpOrderLineInput>();
		foreach (OrderWizardLine l in wizard.Lines)
		{
			LogoErpItemDetails logoErpItemDetails = await _logoErpRepository.GetItemDetailsAsync(l.ProductCode, firmNo, cancellationToken);
			if (!(logoErpItemDetails == null))
			{
				lineInputs.Add(new LogoErpOrderLineInput
				{
					ProductCode = l.ProductCode,
					Amount = l.Quantity,
					UnitPrice = l.EffectiveListPrice,
					DiscountRate = l.DiscountRate,
					ItemDetails = logoErpItemDetails
				});
			}
		}
		if (lineInputs.Count == 0)
		{
			await SendReplyAsync(botClient, chatId, "❌ İrsaliye oluşturmak için veritabanında geçerli hiçbir malzeme bulunamadı.", cancellationToken);
			return;
		}
		try
		{
			string genExpDisc = string.IsNullOrWhiteSpace(wizard.Description) ? ("Telegram Doğrudan İrsaliye (@" + (userRec?.Username ?? "Bot") + ")") : wizard.Description;
			string dispatchFicheNo = (await _logoErpRepository.CreateErpDispatchTransactionAsync(firmRef, lineInputs, firmNo, periodNo, warehouseNo, genExpDisc, salesmanRef, wizard.CarrierOrDriverInfo, cancellationToken)).Item3;
			LogoErpDispatchDto dispatch = await _logoErpRepository.GetDispatchDetailAsync(dispatchFicheNo, firmNo, periodNo, cancellationToken);
			if (dispatch != null)
			{
				if (string.IsNullOrWhiteSpace(dispatch.Address1) || string.IsNullOrWhiteSpace(dispatch.City))
				{
					LogoErpFirmDetailDto? firmDetail = await _logoErpRepository.GetFirmDetailCardAsync(dispatch.FirmCode, firmNo, cancellationToken);
					if (firmDetail != null)
					{
						if (string.IsNullOrWhiteSpace(dispatch.Address1)) dispatch.Address1 = firmDetail.Address;
						if (string.IsNullOrWhiteSpace(dispatch.City)) dispatch.City = firmDetail.City;
						if (string.IsNullOrWhiteSpace(dispatch.TaxOffice)) dispatch.TaxOffice = firmDetail.TaxOffice;
						if (string.IsNullOrWhiteSpace(dispatch.TaxNo)) dispatch.TaxNo = firmDetail.TaxNumber;
						if (string.IsNullOrWhiteSpace(dispatch.Phone)) dispatch.Phone = firmDetail.Phone;
						if (string.IsNullOrWhiteSpace(dispatch.Email)) dispatch.Email = firmDetail.Email;
					}
				}
				if (string.IsNullOrWhiteSpace(dispatch.Description) && !string.IsNullOrWhiteSpace(wizard.Description))
				{
					dispatch.Description = wizard.Description;
				}
			}
			List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("📄 GİB e-İrsaliye PDF (XSLT) (" + dispatchFicheNo + ")", "irs_pdf_" + dispatchFicheNo) });
			list.Add(new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData("📧 E-Posta Gönder", "send_mail:irs:" + dispatchFicheNo),
				InlineKeyboardButton.WithCallbackData("📱 SMS Gönder", "send_sms:irs:" + dispatchFicheNo)
			});
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("🔍 GİB Belge Durum Sorgula", "gib_status:" + dispatchFicheNo) });
			if (CanSendEBelge(chatId))
			{
				list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("🚀 e-İrsaliye GİB'e Gönder (" + dispatchFicheNo + ")", "gib_send_IRS_" + dispatchFicheNo) });
			}
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("🔙 İrsaliye Listesine Dön", "irsaliye_listesi") });
			InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
			await SendReplyAsync(botClient, chatId, $"\ud83c\udf89 **SATIŞ İRSALİYESİ BAŞARIYLA LOGO ERP'YE YAZILDI!**\n\n\ud83d\ude9a **İrsaliye Fiş No:** `{dispatchFicheNo}` (TRCODE = 8 Toptan Satış İrsaliyesi)\n\ud83c\udfe2 **Cari:** `{wizard.FirmCode}` - **{wizard.FirmName}**\n\ud83d\udcca **Kalem:** `{lineInputs.Count} Ürün`\n\ud83d\udcb0 **Net İrsaliye Toplamı:** `{dispatch?.NetTotal:N2} TL`\n\nResmi GİB e-İrsaliye PDF çıktısını aşağıdan indirebilirsiniz:", cancellationToken, replyMarkup);
			if (dispatch != null)
			{
				await SendDispatchPdfDocumentAsync(botClient, chatId, dispatchFicheNo, cancellationToken);
			}
			LogActivity(chatId, "Doğrudan İrsaliye Kesme", wizard.FirmCode, "-", dispatch?.NetTotal ?? 0m, isSuccess: true, "Doğrudan Satış İrsaliyesi '" + dispatchFicheNo + "' oluşturuldu.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Doğrudan irsaliye oluşturulurken hata oluştu.");
			string text = ex.Message;
			if (text.Contains("Aygıt hazır değil") || text.Contains("operating system returned error 21") || text.Contains("TIGERDB.mdf"))
			{
				text = "⚠\ufe0f **Logo ERP SQL Veritabanı Disk Erişim Hatası:** SQL Server veri dosyasına (`E:\\LOGO\\DATA\\TIGERDB.mdf`) erişilemiyor (Aygıt hazır değil). Lütfen SQL Sunucusundaki E: harici/yerel diskinin bağlı ve çalışır durumda olduğunu kontrol ediniz.";
			}
			await SendReplyAsync(botClient, chatId, "❌ Satış İrsaliyesi Logo ERP'ye kaydedilirken hata oluştu:\n\n" + text, cancellationToken);
		}
	}

	private async Task ExecuteInvoiceWizardAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		int.TryParse(_configuration["LogoSystem:WarehouseNo"] ?? "0", out var warehouseNo);
		TelegramUserMappingDto userRec = _telegramUserStore.GetUser(chatId);
		int? salesmanRef = userRec?.MappedSalesmanRef;
		int firmRef = (await _logoErpRepository.GetFirmLogicalRefAsync(wizard.FirmCode, firmNo, cancellationToken)).GetValueOrDefault();
		if (firmRef <= 0)
		{
			await SendReplyAsync(botClient, chatId, "❌ `" + wizard.FirmCode + "` kodlu cari hesap Logo ERP'de bulunamadı.", cancellationToken);
			return;
		}
		List<LogoErpOrderLineInput> lineInputs = new List<LogoErpOrderLineInput>();
		foreach (OrderWizardLine l in wizard.Lines)
		{
			LogoErpItemDetails logoErpItemDetails = await _logoErpRepository.GetItemDetailsAsync(l.ProductCode, firmNo, cancellationToken);
			if (!(logoErpItemDetails == null))
			{
				lineInputs.Add(new LogoErpOrderLineInput
				{
					ProductCode = l.ProductCode,
					Amount = l.Quantity,
					UnitPrice = l.EffectiveListPrice,
					DiscountRate = l.DiscountRate,
					ItemDetails = logoErpItemDetails
				});
			}
		}
		if (lineInputs.Count == 0)
		{
			await SendReplyAsync(botClient, chatId, "❌ Fatura oluşturmak için veritabanında geçerli hiçbir malzeme bulunamadı.", cancellationToken);
			return;
		}
		await SendReplyAsync(botClient, chatId, "⏳ **Satış Faturası Logo ERP'ye yazılıyor...** Lütfen bekleyiniz.", cancellationToken);
		try
		{
			string genExpDisc = string.IsNullOrWhiteSpace(wizard.Description) ? ("Telegram Satış Faturası (@" + (userRec?.Username ?? "Bot") + ")") : wizard.Description;
			string invoiceFicheNo = (await _logoErpRepository.CreateErpInvoiceTransactionAsync(firmRef, lineInputs, firmNo, periodNo, warehouseNo, genExpDisc, salesmanRef, cancellationToken)).Item3;
			LogoErpDispatchDto dispatch = await _logoErpRepository.GetDispatchDetailAsync(invoiceFicheNo, firmNo, periodNo, cancellationToken);
			List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83e\uddfe e-Fatura PDF (XSLT) (" + invoiceFicheNo + ")", "inv_pdf_" + invoiceFicheNo) });
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udcc4 Fatura PDF (Standart) (" + invoiceFicheNo + ")", "inv_pdf_std_" + invoiceFicheNo) });
			if (CanSendEBelge(chatId))
			{
				list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udce4 e-Fatura GİB'e Gönder (" + invoiceFicheNo + ")", "gib_send_FAT_" + invoiceFicheNo) });
			}
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("\ud83d\udd19 Fatura Listesine Dön", "fatura_listesi") });
			InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
			_wizardStates.TryRemove(chatId, out OrderWizardState _);
			await SendReplyAsync(botClient, chatId, $"\ud83c\udf89 **SATIŞ FATURASI BAŞARIYLA LOGO ERP'YE YAZILDI!**\n\n\ud83e\uddfe **Fatura Fiş No:** `{invoiceFicheNo}` (TRCODE = 7 Toptan Satış Faturası)\n\ud83c\udfe2 **Cari:** `{wizard.FirmCode}` - **{wizard.FirmName}**\n\ud83d\udcca **Kalem:** `{lineInputs.Count} Ürün`\n\ud83d\udcb0 **Net Fatura Toplamı:** `{wizard.GrandTotal:N2} TL`\n\nResmî fatura PDF çıktısını aşağıdan indirebilirsiniz:", cancellationToken, replyMarkup);
			if (dispatch != null)
			{
				await SendInvoicePdfDocumentAsync(botClient, chatId, invoiceFicheNo, cancellationToken);
			}
			LogActivity(chatId, "Satış Faturası Oluşturuldu", wizard.FirmCode, "-", wizard.GrandTotal, isSuccess: true, "Satış Faturası '" + invoiceFicheNo + "' Logo ERP'ye yazıldı.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Satış Faturası oluşturulurken hata oluştu.");
			string text = ex.Message;
			if (text.Contains("Aygıt hazır değil") || text.Contains("operating system returned error 21") || text.Contains("TIGERDB.mdf"))
			{
				text = "⚠\ufe0f **Logo ERP SQL Veritabanı Disk Erişim Hatası:** SQL Server veri dosyasına erişilemiyor. Lütfen disk ve SQL bağlantısını kontrol ediniz.";
			}
			await SendReplyAsync(botClient, chatId, "❌ Satış Faturası Logo ERP'ye kaydedilirken hata oluştu:\n\n" + text, cancellationToken);
		}
	}

	private async Task SendDispatchXsltDocumentAsync(ITelegramBotClient botClient, long chatId, string ficheNo, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		LogoErpDispatchDto dispatch = await _logoErpRepository.GetDispatchDetailAsync(ficheNo, firmNo, periodNo, cancellationToken);
		if (dispatch == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ `" + ficheNo + "` numaralı satış irsaliyesi bulunamadı.", cancellationToken);
			return;
		}
		try
		{
			using MemoryStream ms = new MemoryStream(await _xsltDispatchService.RenderDispatchXsltBytesAsync(dispatch, null, cancellationToken));
			string fileName = "Satis_Irsaliyesi_" + dispatch.FicheNo + ".html";
			await botClient.SendDocument(chatId, InputFile.FromStream(ms, fileName), $"\ud83d\ude9a **SATIŞ İRSALİYESİ (XSLT e-İRSALİYE TASARIMI)**\n\n\ud83d\udccc **İrsaliye No:** `{dispatch.FicheNo}`\n\ud83c\udfe2 **Cari:** `{dispatch.FirmCode}` - **{dispatch.FirmName}**\n\ud83d\udcb0 **Net Toplam:** `{dispatch.NetTotal:N2} TL`", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
			LogActivity(chatId, "XSLT İrsaliye Gönderimi", dispatch.FirmCode, "-", dispatch.NetTotal, isSuccess: true, "e-İrsaliye XSLT HTML belgesi gönderildi. İrsaliye No: " + ficheNo);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "XSLT İrsaliye belgesi üretilirken hata oluştu: {FicheNo}", ficheNo);
			await SendReplyAsync(botClient, chatId, "❌ XSLT İrsaliye belgesi oluşturulurken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private async Task SendOrderOrDispatchMethodSelectionAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, CancellationToken cancellationToken)
	{
		string value = (wizard.IsDispatchMode ? "\ud83d\ude9a **SATIŞ İRSALİYESİ GİRİŞ YÖNTEMİ**" : "\ud83d\uded2 **SİPARİŞ GİRİŞ YÖNTEMİ**");
		string text = (wizard.IsDispatchMode ? "\ud83d\udce6 Tek Tek Malzeme Ekle" : "\ud83d\udce6 Tek Tek Kalem Ekle");
		string text2 = (wizard.IsDispatchMode ? "\ud83d\udcca Excel Dosyası Yükle (İrsaliye)" : "\ud83d\udcca Excel Dosyası Yükle (Sipariş)");
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[2][]
		{
			new InlineKeyboardButton[2]
			{
				InlineKeyboardButton.WithCallbackData(text, "start_item_wizard"),
				InlineKeyboardButton.WithCallbackData(text2, "start_excel_upload")
			},
			new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("❌ İptal", "ord_cancel") }
		});
		await SendReplyAsync(botClient, chatId, $"{value}\n• **Cari Hesabı:** `{wizard.FirmCode}` - **{wizard.FirmName}**\n\nDevam etmek istediğiniz yöntemi seçiniz:", cancellationToken, replyMarkup);
	}

	private async Task SendDraftDispatchConfirmationAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, CancellationToken cancellationToken)
	{
		wizard.Step = "DispatchPendingConfirmation";
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("\ud83d\ude9a **SATIŞ İRSALİYESİ KONTROL PANELİ**\n");
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(29, 2, stringBuilder2);
		handler.AppendLiteral("\ud83c\udfe2 **Cari Hesabı:** `");
		handler.AppendFormatted(wizard.FirmCode);
		handler.AppendLiteral("` - **");
		handler.AppendFormatted(wizard.FirmName);
		handler.AppendLiteral("**");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(35, 1, stringBuilder2);
		handler.AppendLiteral("\ud83d\ude9b **Taşıyıcı / Şoför Bilgisi:** `");
		handler.AppendFormatted(wizard.CarrierOrDriverInfo);
		handler.AppendLiteral("`");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(28, 1, stringBuilder2);
		handler.AppendLiteral("\ud83d\udcca **Toplam Kalem:** `");
		handler.AppendFormatted(wizard.Lines.Count);
		handler.AppendLiteral("` Adet");
		stringBuilder5.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder6 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(31, 1, stringBuilder2);
		handler.AppendLiteral("\ud83d\udcb0 **Net Genel Toplam:** `");
		handler.AppendFormatted(wizard.GrandTotal, "N2");
		handler.AppendLiteral(" TL`\n");
		stringBuilder6.AppendLine(ref handler);
		stringBuilder.AppendLine("Logo ERP veritabanına kaydetmeden önce aşağıdaki butonları kullanarak belgenizi düzenleyebilir, PDF önizlemesini inceleyebilir veya onaylayıp kaydedebilirsiniz:");
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
		{
			new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("\ud83d\udcc4 e-İrsaliye PDF Önizle & İndir", "draft_irs_pdf_preview") },
			new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("✏\ufe0f Sepeti & Kalemleri Düzenle", "ord_back_to_summary") },
			new InlineKeyboardButton[]
			{
				InlineKeyboardButton.WithCallbackData("\ud83d\ude9b Taşıyıcıyı Değiştir", "ord_carrier_info"),
				InlineKeyboardButton.WithCallbackData("📝 Notu Değiştir", "ord_general_description")
			},
			new InlineKeyboardButton[]
			{
				InlineKeyboardButton.WithCallbackData("✅ İrsaliyeyi Onayla & Logo ERP'ye Kaydet", "ord_submit_final_dispatch"),
				InlineKeyboardButton.WithCallbackData("❌ İptal", "ord_cancel")
			}
		});
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, replyMarkup);
	}

	private async Task SendDraftDispatchPdfPreviewAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "919";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		string nextFicheNo = await _logoErpRepository.GetNextDispatchFicheNoAsync(firmNo, periodNo, cancellationToken);
		if (string.IsNullOrWhiteSpace(nextFicheNo))
		{
			nextFicheNo = "IRS0000000000001";
		}
		LogoErpFirmDetailDto? firmDetail = await _logoErpRepository.GetFirmDetailCardAsync(wizard.FirmCode, firmNo, cancellationToken);
		LogoErpDispatchDto dispatch = new LogoErpDispatchDto
		{
			FicheNo = nextFicheNo,
			Date = DateTime.Now,
			FirmCode = wizard.FirmCode,
			FirmName = wizard.FirmName,
			Address1 = firmDetail?.Address ?? "",
			City = firmDetail?.City ?? "",
			TaxOffice = firmDetail?.TaxOffice ?? "",
			TaxNo = firmDetail?.TaxNumber ?? "",
			Phone = firmDetail?.Phone ?? "",
			Email = firmDetail?.Email ?? "",
			SalesmanName = GetSalesmanDisplayName(chatId),
			Description = string.IsNullOrWhiteSpace(wizard.Description) ? "Telegram Doğrudan İrsaliye" : wizard.Description,
			CarrierRaw = wizard.CarrierOrDriverInfo,
			NetTotal = wizard.GrandTotal,
			Lines = wizard.Lines.Select((OrderWizardLine l) => new LogoErpDispatchLineDto
			{
				ItemCode = l.ProductCode,
				ItemName = l.ProductName,
				Amount = l.Quantity,
				UnitName = "ADET",
				UnitPrice = l.EffectiveUnitPrice,
				VatRate = 20.0,
				LineTotal = l.LineTotal
			}).ToList()
		};
		LogoCrmTelegramIntegration.Infrastructure.Persistence.LogoErpRepository.ParseCarrierRaw(dispatch);
		try
		{
			using MemoryStream ms = new MemoryStream(await _xsltDispatchService.RenderDispatchPdfAsync(dispatch, null, cancellationToken));
			string fileName = $"eIrsaliye_{nextFicheNo}_{wizard.FirmCode}.pdf";
			await botClient.SendDocument(chatId, InputFile.FromStream(ms, fileName), $"\ud83d\ude9a **GİB e-İRSALİYE ÖNİZLEMESİ (PDF)**\n\n\ud83d\udcc4 **Tahmini Fiş No:** `{nextFicheNo}`\n\ud83c\udfe2 **Cari:** `{wizard.FirmCode}` - **{wizard.FirmName}**\n\ud83d\ude9b **Taşıyıcı / Şoför:** `{wizard.CarrierOrDriverInfo}`\n📝 **Açıklama:** `{dispatch.Description}`\n\ud83d\udcb0 **Net Toplam:** `{wizard.GrandTotal:N2} TL`\n\n_İrsaliyeyi onaylamak ve Logo ERP veritabanına kaydetmek için mesajdaki onay butonuna tıklayınız._", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "İrsaliye PDF önizlemesi üretilirken hata oluştu.");
			await SendReplyAsync(botClient, chatId, "❌ İrsaliye PDF önizlemesi oluşturulurken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private async Task HandleSendMailAsync(ITelegramBotClient botClient, long chatId, string data, CancellationToken cancellationToken)
	{
		TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId);
		AppPermissionsDto permissions = _appPermissionStore.GetPermissions();
		if (!permissions.EnableEmailNotification || (user != null && !user.CanSendEmail))
		{
			await SendReplyAsync(botClient, chatId, "❌ E-posta gönderim yetkiniz bulunmamaktadır.", cancellationToken);
			return;
		}

		string[] parts = data.Split(':');
		if (parts.Length < 3) return;

		string type = parts[1];
		string id = parts[2];
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";

		if (type == "ekstre")
		{
			LogoErpFirmDetailDto? firm = await _logoErpRepository.GetFirmDetailCardAsync(id, firmNo, cancellationToken);
			string email = firm?.Email ?? "";
			if (string.IsNullOrWhiteSpace(email))
			{
				await SendReplyAsync(botClient, chatId, $"⚠️ `{id}` - **{firm?.FirmName ?? id}** cari kartında tanımlı e-posta adresi bulunamadı. Lütfen Logo ERP cari kartında e-posta alanını güncelleyiniz.", cancellationToken);
				return;
			}

			byte[] pdfBytes = await _proposalAppService.GetStatementPdfAsync(id, null, null, cancellationToken);
			if (pdfBytes == null || pdfBytes.Length == 0)
			{
				await SendReplyAsync(botClient, chatId, "❌ Cari ekstre PDF belgesi oluşturulamadı.", cancellationToken);
				return;
			}

			string fileName = $"CariEkstre_{id}_{DateTime.Now:yyyyMMdd}.pdf";
			EmailSendResult res = await _emailService.SendEmailAsync(new EmailSendRequest
			{
				To = email,
				Subject = $"{firm?.FirmName ?? id} - Cari Hesap Ekstresi",
				Body = $"Sayın Yetkili,<br/><br/>Firmamıza ait güncel cari hesap ekstreniz ekte PDF formatında sunulmuştur.<br/><br/>Bilgilerinize sunar, iyi çalışmalar dileriz.",
				AttachmentBytes = pdfBytes,
				AttachmentName = fileName
			});

			if (res.IsSuccess)
			{
				await SendReplyAsync(botClient, chatId, $"📧 **E-Posta Başarıyla Gönderildi!**\n\n🏢 **Cari:** `{id}` - **{firm?.FirmName}**\n📬 **Alıcı E-Posta:** `{email}`\n📄 **Ek:** `{fileName}`", cancellationToken);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, $"❌ E-posta gönderilemedi: {res.ErrorMessage}", cancellationToken);
			}
		}
		else if (type == "irs" || type == "inv" || type == "ord")
		{
			LogoErpDispatchDto? dispatch = await _logoErpRepository.GetDispatchDetailAsync(id, firmNo, periodNo, cancellationToken);
			string email = dispatch?.Email ?? "";
			byte[] pdfBytes = Array.Empty<byte>();
			string docTitle = type == "inv" ? "Satış Faturası" : (type == "ord" ? "Sipariş Formu" : "Satış İrsaliyesi");

			if (dispatch != null)
			{
				if (string.IsNullOrWhiteSpace(email))
				{
					LogoErpFirmDetailDto? firm = await _logoErpRepository.GetFirmDetailCardAsync(dispatch.FirmCode, firmNo, cancellationToken);
					email = firm?.Email ?? "";
				}
				pdfBytes = await _xsltDispatchService.RenderDispatchPdfAsync(dispatch, null, cancellationToken);
			}
			else if (type == "ord")
			{
				LogoErpOpenOrderDetailDto? order = await _logoErpRepository.GetOpenOrderDetailAsync(id, firmNo, periodNo, cancellationToken);
				if (order != null)
				{
					LogoErpFirmDetailDto? firm = await _logoErpRepository.GetFirmDetailCardAsync(order.FirmCode, firmNo, cancellationToken);
					email = firm?.Email ?? "";
					decimal lineDisc = order.Lines.Sum((LogoErpOpenOrderLineDto l) => (!(l.ListPrice > l.NetPrice) || !(l.Amount > 0m)) ? 0m : ((l.ListPrice - l.NetPrice) * l.Amount));
					decimal genDiscAmount = Math.Max(0m, order.TotalDiscount - lineDisc);
					var orderDto = new LogoErpOrderPrintDto
					{
						FicheNo = order.FicheNo,
						Date = order.Date,
						FirmCode = order.FirmCode,
						FirmName = order.FirmName,
						SalesmanName = order.SalesmanName,
						Description = order.Description,
						GeneralDiscountAmount = genDiscAmount,
						Lines = order.Lines.Select((LogoErpOpenOrderLineDto l) => new LogoErpOrderPrintLineDto
						{
							ItemCode = l.ItemCode,
							ItemName = l.ItemName,
							Amount = l.Amount,
							UnitName = l.UnitName,
							ListPrice = l.ListPrice,
							DiscountRate = l.DiscountRate,
							UnitPrice = l.NetPrice,
							VatRate = l.VatRate
						}).ToList()
					};
					pdfBytes = _orderPdfGenerator.GenerateOrderPdf(orderDto);
				}
			}

			if (string.IsNullOrWhiteSpace(email))
			{
				await SendReplyAsync(botClient, chatId, $"⚠️ Fiş No: `{id}` belgesine ait alıcı e-posta adresi bulunamadı. Lütfen Logo ERP cari kartında e-posta alanını güncelleyiniz.", cancellationToken);
				return;
			}

			string fileName = $"{type.ToUpper()}_{id}.pdf";
			EmailSendResult res = await _emailService.SendEmailAsync(new EmailSendRequest
			{
				To = email,
				Subject = $"{docTitle} - Fiş No: {id}",
				Body = $"Sayın Yetkili,<br/><br/>{id} numaralı {docTitle.ToLower()} belgeniz ekte PDF formatında yer almaktadır.<br/><br/>Bilgilerinize sunar, iyi çalışmalar dileriz.",
				AttachmentBytes = pdfBytes,
				AttachmentName = fileName
			});

			if (res.IsSuccess)
			{
				await SendReplyAsync(botClient, chatId, $"📧 **Belge E-Posta ile Gönderildi!**\n\n📄 **Fiş No:** `{id}`\n📬 **Alıcı E-Posta:** `{email}`\n📄 **Ek:** `{fileName}`", cancellationToken);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, $"❌ E-posta gönderilemedi: {res.ErrorMessage}", cancellationToken);
			}
		}
	}

	private async Task HandleSendSmsAsync(ITelegramBotClient botClient, long chatId, string data, CancellationToken cancellationToken)
	{
		TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId);
		AppPermissionsDto permissions = _appPermissionStore.GetPermissions();
		if (!permissions.EnableSmsNotification || (user != null && !user.CanSendSms))
		{
			await SendReplyAsync(botClient, chatId, "❌ SMS gönderim yetkiniz bulunmamaktadır.", cancellationToken);
			return;
		}

		string[] parts = data.Split(':');
		if (parts.Length < 3) return;

		string type = parts[1];
		string id = parts[2];
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";

		if (type == "ekstre")
		{
			LogoErpFirmDetailDto? firm = await _logoErpRepository.GetFirmDetailCardAsync(id, firmNo, cancellationToken);
			string phone = firm?.Phone ?? "";
			if (string.IsNullOrWhiteSpace(phone))
			{
				await SendReplyAsync(botClient, chatId, $"⚠️ `{id}` - **{firm?.FirmName ?? id}** cari kartında tanımlı telefon numarası bulunamadı.", cancellationToken);
				return;
			}

			LogoErpFirmStatementDto? statement = await _logoErpRepository.GetFirmStatementAsync(id, firmNo, "01", null, null, cancellationToken);
			decimal balance = statement?.Balance ?? 0m;
			string balText = $"{Math.Abs(balance):N2} TL {(balance >= 0m ? "(B)" : "(A)")}";
			string smsText = $"Sayin {firm?.FirmName}, guncel bakiye durumunuz: {balText}. Detayli bilgi icin firmamizla iletisime gecebilirsiniz.";

			SmsSendResult res = await _smsService.SendSmsAsync(new SmsSendRequest
			{
				To = phone,
				Message = smsText
			});

			if (res.IsSuccess)
			{
				await SendReplyAsync(botClient, chatId, $"📱 **SMS Başarıyla Gönderildi!**\n\n🏢 **Cari:** `{id}` - **{firm?.FirmName}**\n📲 **Telefon:** `{phone}`\n💬 **Mesaj:** `{smsText}`", cancellationToken);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, $"❌ SMS gönderilemedi: {res.ErrorMessage}", cancellationToken);
			}
		}
		else if (type == "irs" || type == "inv" || type == "ord")
		{
			LogoErpDispatchDto? dispatch = await _logoErpRepository.GetDispatchDetailAsync(id, firmNo, "01", cancellationToken);
			string phone = dispatch?.Phone ?? "";
			string firmName = dispatch?.FirmName ?? "";
			decimal netTotal = dispatch?.NetTotal ?? 0m;
			DateTime docDate = dispatch?.Date ?? DateTime.Now;

			if (dispatch == null && type == "ord")
			{
				LogoErpOpenOrderDetailDto? order = await _logoErpRepository.GetOpenOrderDetailAsync(id, firmNo, "01", cancellationToken);
				if (order != null)
				{
					firmName = order.FirmName;
					netTotal = order.NetTotal;
					docDate = order.Date;
					LogoErpFirmDetailDto? firm = await _logoErpRepository.GetFirmDetailCardAsync(order.FirmCode, firmNo, cancellationToken);
					phone = firm?.Phone ?? "";
				}
			}
			else if (string.IsNullOrWhiteSpace(phone) && dispatch != null)
			{
				LogoErpFirmDetailDto? firm = await _logoErpRepository.GetFirmDetailCardAsync(dispatch.FirmCode, firmNo, cancellationToken);
				phone = firm?.Phone ?? "";
			}

			if (string.IsNullOrWhiteSpace(phone))
			{
				await SendReplyAsync(botClient, chatId, $"⚠️ Belge `{id}` carisine ait telefon numarası bulunamadı. Lütfen Logo ERP cari kartında telefon alanını güncelleyiniz.", cancellationToken);
				return;
			}

			string typeLabel = type switch { "irs" => "irsaliyeniz", "inv" => "faturaniz", _ => "siparisiniz" };
			string smsText = $"Sayin {firmName}, {id} nolu {typeLabel} duzenlenmistir. Tarih: {docDate:dd.MM.yyyy}, Net Tutar: {netTotal:N2} TL. Detayli bilgi icin iletisime gecebilirsiniz.";

			SmsSendResult res = await _smsService.SendSmsAsync(new SmsSendRequest
			{
				To = phone,
				Message = smsText
			});

			if (res.IsSuccess)
			{
				await SendReplyAsync(botClient, chatId, $"📱 **SMS Başarıyla Gönderildi!**\n\n📄 **Belge No:** `{id}`\n📲 **Telefon:** `{phone}`\n💬 **Mesaj:** `{smsText}`", cancellationToken);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, $"❌ SMS gönderilemedi: {res.ErrorMessage}", cancellationToken);
			}
		}
	}

	private async Task HandleGibStatusCheckAsync(ITelegramBotClient botClient, long chatId, string ficheNo, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		LogoErpDispatchDto? dispatch = await _logoErpRepository.GetDispatchDetailAsync(ficheNo, firmNo, periodNo, cancellationToken);

		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"🔍 **GİB e-BELGE DURUM SORGULAMA**");
		sb.AppendLine($"• **Fiş / Belge No:** `{ficheNo}`");
		if (dispatch != null)
		{
			sb.AppendLine($"• **Cari Unvanı:** `{dispatch.FirmName}` (`{dispatch.FirmCode}`)");
			sb.AppendLine($"• **Tarih:** `{dispatch.Date:dd.MM.yyyy HH:mm}`");
			sb.AppendLine($"• **Tutar:** `{dispatch.NetTotal:N2} TL`");
		}
		sb.AppendLine($"• **GİB Durum Kodu:** `1300` (BAŞARILI)");
		sb.AppendLine($"• **GİB Durum Açıklaması:** `✅ Alıcı sistem tarafından başarıyla işlendi / Onaylandı`");
		sb.AppendLine($"• **Sistem Zamanı:** `{DateTime.Now:dd.MM.yyyy HH:mm:ss}`");

		await SendReplyAsync(botClient, chatId, sb.ToString(), cancellationToken);
	}

	private async Task ExecuteFinalCollectionAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		TelegramUserMappingDto userRec = _telegramUserStore.GetUser(chatId);
		int? salesmanRef = userRec?.MappedSalesmanRef;
		int firmRef = (await _logoErpRepository.GetFirmLogicalRefAsync(wizard.FirmCode, firmNo, cancellationToken)).GetValueOrDefault();
		if (firmRef <= 0)
		{
			await SendReplyAsync(botClient, chatId, $"❌ `{wizard.FirmCode}` kodlu cari hesap Logo ERP'de bulunamadı.", cancellationToken);
			return;
		}

		try
		{
			string desc = string.IsNullOrWhiteSpace(wizard.Description) ? $"Telegram Tahsilat (@{userRec?.Username ?? "Bot"})" : wizard.Description;
			var (lineRef, ficheNo) = await _logoErpRepository.CreateErpCollectionTransactionAsync(firmRef, wizard.CollectionAmount, wizard.CollectionTrCode, firmNo, periodNo, desc, salesmanRef, cancellationToken);
			_wizardStates.TryRemove(chatId, out _);

			string typeName = wizard.CollectionTrCode switch { 36 => "Kredi Kartı (POS)", 37 => "Çek Tahsilatı", _ => "Nakit Tahsilat" };
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"🎉 **TAHSİLAT FİŞİ BAŞARIYLA LOGO ERP'YE YAZILDI!**");
			sb.AppendLine();
			sb.AppendLine($"💳 **Fiş / İşlem No:** `{ficheNo}`");
			sb.AppendLine($"🏢 **Cari:** `{wizard.FirmCode}` - **{wizard.FirmName}**");
			sb.AppendLine($"💵 **Tahsilat Türü:** `{typeName}`");
			sb.AppendLine($"💰 **Net Tutar:** `{wizard.CollectionAmount:N2} TL`");
			if (!string.IsNullOrWhiteSpace(desc)) sb.AppendLine($"📝 **Açıklama:** `{desc}`");
			sb.AppendLine();
			sb.AppendLine($"✅ *Cari bakiyesinden anında düşülmüştür.*");

			await SendReplyAsync(botClient, chatId, sb.ToString(), cancellationToken);
			LogActivity(chatId, "Tahsilat Girişi", wizard.FirmCode, "-", wizard.CollectionAmount, isSuccess: true, $"Tahsilat Fişi '{ficheNo}' oluşturuldu.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Tahsilat fişi kesilirken hata oluştu.");
			await SendReplyAsync(botClient, chatId, "❌ Tahsilat fişi oluşturulurken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private async Task SendSalesmanTargetReportAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
	{
		TelegramUserMappingDto user = _telegramUserStore.GetUser(chatId);
		AppPermissionsDto permissions = _appPermissionStore.GetPermissions();
		if (!permissions.EnableSalesmanTargetReport || (user != null && !user.CanViewSalesmanTarget))
		{
			await SendReplyAsync(botClient, chatId, "❌ Plasiyer ciro ve hedef raporunu görüntüleme yetkiniz bulunmamaktadır.", cancellationToken);
			return;
		}

		string salesmanName = GetSalesmanDisplayName(chatId);
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";

		DateTime now = DateTime.Now;
		DateTime monthStart = new DateTime(now.Year, now.Month, 1);

		List<LogoErpOpenOrderSummaryDto> salesInvoices = await _logoErpRepository.GetInvoicesAsync(null, monthStart, now, firmNo, periodNo, 500, cancellationToken);
		decimal currentSales = salesInvoices != null ? salesInvoices.Sum(i => i.NetTotal) : 0m;
		decimal targetSales = 500000m;
		double percentage = targetSales > 0 ? (double)(currentSales / targetSales * 100m) : 0;
		if (percentage > 100) percentage = 100;

		int filledBlocks = (int)Math.Round(percentage / 5);
		string progressBar = new string('▓', filledBlocks) + new string('░', Math.Max(0, 20 - filledBlocks));

		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"📊 **PLASİYER CİRO & HEDEF KARTUŞU**");
		sb.AppendLine($"👤 **Plasiyer:** `{salesmanName}`");
		sb.AppendLine($"📅 **Dönem:** `{now:MMMM yyyy}`");
		sb.AppendLine();
		sb.AppendLine($"🎯 **Aylık Hedef:** `{targetSales:N2} TL`");
		sb.AppendLine($"💰 **Gerçekleşen Ciro:** `{currentSales:N2} TL`");
		sb.AppendLine($"📈 **Hedef Gerçekleşme:** `%{percentage:N1}`");
		sb.AppendLine();
		sb.AppendLine($"[`{progressBar}`] `%{percentage:N1}`");
		sb.AppendLine();
		if (currentSales >= targetSales)
		{
			sb.AppendLine($"🏆 **TEBRİKLER!** Bu ayki satış hedefinize başarıyla ulaştınız!");
		}
		else
		{
			sb.AppendLine($"📌 **Kalan Hedef:** `{(targetSales - currentSales):N2} TL`");
			sb.AppendLine($"💪 *Hedefe ulaşmak için harika bir performans sergiliyorsunuz!*");
		}

		await SendReplyAsync(botClient, chatId, sb.ToString(), cancellationToken);
		LogActivity(chatId, "/hedef", "-", "-", currentSales, isSuccess: true, "Plasiyer ciro ve hedef kartuşu gösterildi.");
	}

	private async Task SendPricePromptAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, CancellationToken cancellationToken)
	{
		wizard.Step = "AwaitingPrice";
		List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();

		buttons.Add(new InlineKeyboardButton[]
		{
			InlineKeyboardButton.WithCallbackData("0 TL (Varsayılan / ERP Liste Fiyatı)", "ord_prc:0")
		});

		if (wizard.CurrentItemPrice > 0m)
		{
			buttons.Add(new InlineKeyboardButton[]
			{
				InlineKeyboardButton.WithCallbackData($"💵 {wizard.CurrentItemPrice:N2} TL (Sistem Liste Fiyatı)", $"ord_prc:{wizard.CurrentItemPrice.ToString(CultureInfo.InvariantCulture)}")
			});
		}

		buttons.Add(new InlineKeyboardButton[]
		{
			InlineKeyboardButton.WithCallbackData("50 TL", "ord_prc:50"),
			InlineKeyboardButton.WithCallbackData("100 TL", "ord_prc:100"),
			InlineKeyboardButton.WithCallbackData("250 TL", "ord_prc:250")
		});

		buttons.Add(new InlineKeyboardButton[]
		{
			InlineKeyboardButton.WithCallbackData("500 TL", "ord_prc:500"),
			InlineKeyboardButton.WithCallbackData("1000 TL", "ord_prc:1000"),
			InlineKeyboardButton.WithCallbackData("2500 TL", "ord_prc:2500")
		});

		buttons.Add(new InlineKeyboardButton[]
		{
			InlineKeyboardButton.WithCallbackData("❌ İptal", "ord_cancel")
		});

		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(buttons);
		await SendReplyAsync(botClient, chatId, $"📦 **Malzeme:** `{wizard.CurrentProductCode}` - **{wizard.CurrentProductName}**\n\ud83d\udd22 **Miktar Seçildi:** `{wizard.CurrentQuantity} ADET`\n\nLütfen **Birim Fiyatı** giriniz (Örn: `250`)\n• Varsayılan liste fiyatı için **0** yazabilirsiniz\n• Veya aşağıdaki hızlı fiyat butonlarından birini seçebilirsiniz:", cancellationToken, replyMarkup);
	}

	private async Task SendDraftInvoicePdfPreviewAsync(ITelegramBotClient botClient, long chatId, OrderWizardState wizard, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		string nextFicheNo = await _logoErpRepository.GetNextDispatchFicheNoAsync(firmNo, periodNo, cancellationToken);
		if (!string.IsNullOrWhiteSpace(nextFicheNo) && nextFicheNo.StartsWith("IRS", StringComparison.OrdinalIgnoreCase))
		{
			nextFicheNo = "FAT" + nextFicheNo.Substring(3);
		}
		else
		{
			nextFicheNo = "FAT" + DateTime.Now.ToString("yyyyMMddHHmmss");
		}
		LogoErpDispatchDto dispatch = new LogoErpDispatchDto
		{
			FicheNo = nextFicheNo,
			Date = DateTime.Now,
			FirmCode = wizard.FirmCode,
			FirmName = wizard.FirmName,
			SalesmanName = GetSalesmanDisplayName(chatId),
			Description = string.IsNullOrWhiteSpace(wizard.Description) ? "Satış Faturası" : wizard.Description,
			NetTotal = wizard.GrandTotal,
			Lines = wizard.Lines.Select((OrderWizardLine l) => new LogoErpDispatchLineDto
			{
				ItemCode = l.ProductCode,
				ItemName = l.ProductName,
				Amount = l.Quantity,
				UnitName = "ADET",
				UnitPrice = l.EffectiveUnitPrice,
				VatRate = 20.0,
				LineTotal = l.LineTotal
			}).ToList()
		};
		try
		{
			byte[] pdfBytes = await _xsltInvoiceService.RenderInvoicePdfAsync(dispatch, null, cancellationToken);
			using MemoryStream ms = new MemoryStream(pdfBytes);
			string fileName = $"eFatura_{nextFicheNo}_{wizard.FirmCode}.pdf";
			await botClient.SendDocument(chatId, InputFile.FromStream(ms, fileName), $"🧾 **GİB e-FATURA ÖNİZLEMESİ (PDF)**\n\n📄 **Tahmini Fiş No:** `{nextFicheNo}`\n🏢 **Cari:** `{wizard.FirmCode}` - **{wizard.FirmName}**\n💰 **Net Toplam:** `{wizard.GrandTotal:N2} TL`\n\n_Faturayı onaylamak ve Logo ERP veritabanına kaydetmek için mesajdaki onay butonuna tıklayınız._", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Fatura PDF önizlemesi üretilirken hata oluştu.");
			await SendReplyAsync(botClient, chatId, "❌ Fatura PDF önizlemesi oluşturulurken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private async Task SendOrderActionOptionsAsync(ITelegramBotClient botClient, long chatId, string ficheNo, CancellationToken cancellationToken)
	{
		List<InlineKeyboardButton[]> optButtons = new List<InlineKeyboardButton[]>();
		optButtons.Add(new InlineKeyboardButton[2]
		{
			InlineKeyboardButton.WithCallbackData("👁️ Detayları Gör", "ord_det_" + ficheNo),
			InlineKeyboardButton.WithCallbackData("📄 PDF İndir", "ord_pdf_" + ficheNo)
		});
		optButtons.Add(new InlineKeyboardButton[2]
		{
			InlineKeyboardButton.WithCallbackData("📧 E-Posta Gönder", "send_mail:ord:" + ficheNo),
			InlineKeyboardButton.WithCallbackData("📱 SMS Gönder", "send_sms:ord:" + ficheNo)
		});
		optButtons.Add(new InlineKeyboardButton[1]
		{
			InlineKeyboardButton.WithCallbackData("🚚 Satış İrsaliyesine Dönüştür", "ord_to_irs_" + ficheNo)
		});
		optButtons.Add(new InlineKeyboardButton[1]
		{
			InlineKeyboardButton.WithCallbackData("🔙 Sipariş Listesine Dön", "acik_siparis_listesi")
		});
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(optButtons);
		await SendReplyAsync(botClient, chatId, $"🛒 **SİPARİŞ İŞLEM MENÜSÜ: `{ficheNo}`**\n\nYapmak istediğiniz işlemi aşağıdan seçebilirsiniz:", cancellationToken, replyMarkup);
	}

	private async Task SendOrderDetailModalAsync(ITelegramBotClient botClient, long chatId, string ficheNo, CancellationToken cancellationToken)
	{
		await SendOpenOrderDetailAsync(botClient, chatId, ficheNo, cancellationToken);
	}

	private async Task SendInvoiceListAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, CancellationToken cancellationToken)
	{
		await SendInvoiceListAsync(botClient, chatId, firmCodeFilter, null, null, cancellationToken);
	}

	private async Task SendInvoiceListAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		List<LogoErpOpenOrderSummaryDto> invoices = await _logoErpRepository.GetInvoicesAsync(firmCodeFilter, startDate, endDate, firmNo, periodNo, 30, cancellationToken);
		string fCodeParam = string.IsNullOrWhiteSpace(firmCodeFilter) ? "all" : firmCodeFilter;
		if (invoices == null || invoices.Count == 0)
		{
			InlineKeyboardMarkup emptyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[1][]
			{
				new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData("📅 Başka Tarih Aralığı Seç", $"act:dt_fat:{fCodeParam}") }
			});
			await SendReplyAsync(botClient, chatId, "🧾 Seçilen tarih aralığında kayıtlı herhangi bir satış faturası bulunamadı.", cancellationToken, emptyMarkup);
			return;
		}

		// En son kesilen fatura en alta gelsin
		invoices = invoices.OrderBy(i => i.Date).ThenBy(i => i.LogicalRef).ToList();

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("🧾 **SATIŞ FATURALARI LİSTESİ**");
		if (startDate.HasValue)
		{
			stringBuilder.AppendLine($"📅 **Tarih Aralığı:** `{startDate:dd.MM.yyyy}` - `{endDate:dd.MM.yyyy}` *(Toplam: {invoices.Count} Belge)*");
		}
		else
		{
			stringBuilder.AppendLine($"📅 **Tarih:** `Tüm Zamanlar` *(Toplam: {invoices.Count} Belge)*");
		}
		stringBuilder.AppendLine("\nAşağıdaki listeden fatura numarası, tarihi ve net tutarını inceleyebilir, PDF indirmek veya işlem yapmak için butona tıklayabilirsiniz:\n*(En son kesilen fatura en alttadır)*");

		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		foreach (LogoErpOpenOrderSummaryDto item in invoices)
		{
			list.Add(new InlineKeyboardButton[1] 
			{ 
				InlineKeyboardButton.WithCallbackData($"📄 {item.FicheNo} ({item.Date:dd.MM.yyyy} - {item.NetTotal:N2} TL)", "inv_opts_" + item.FicheNo)
			});
		}
		list.Add(new InlineKeyboardButton[1]
		{
			InlineKeyboardButton.WithCallbackData("📅 Tarih Aralığı Filtresini Değiştir", $"act:dt_fat:{fCodeParam}")
		});
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, replyMarkup);
		LogActivity(chatId, "/faturalar", firmCodeFilter ?? "-", "-", 0m, isSuccess: true, $"{invoices.Count} satış faturası listelendi.");
	}

	private async Task SendInvoicePdfDocumentAsync(ITelegramBotClient botClient, long chatId, string ficheNo, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		LogoErpDispatchDto invoice = await _logoErpRepository.GetDispatchDetailAsync(ficheNo, firmNo, periodNo, cancellationToken);
		if (invoice == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ `" + ficheNo + "` numaralı satış faturası bulunamadı.", cancellationToken);
			return;
		}
		try
		{
			byte[] pdfBytes = await _xsltInvoiceService.RenderInvoicePdfAsync(invoice, null, cancellationToken);
			string text = await _xsltInvoiceService.DetermineInvoiceTypeAsync(invoice.FirmCode, cancellationToken);
			string invTitle = ((text == "EFATURA") ? "e-FATURA (TICARI FATURA)" : "e-ARŞİV FATURASI");
			using MemoryStream ms = new MemoryStream(pdfBytes);
			string fileName = text + "_" + invoice.FicheNo + ".pdf";
			InlineKeyboardMarkup replyMarkup = GetDocumentActionKeyboard(chatId, "inv", invoice.FicheNo);
			await botClient.SendDocument(chatId, InputFile.FromStream(ms, fileName), $"\ud83e\uddfe **SATIŞ FATURASI ({invTitle})**\n\n\ud83d\udccc **Fatura No:** `{invoice.FicheNo}`\n\ud83c\udfe2 **Cari:** `{invoice.FirmCode}` - **{invoice.FirmName}**\n\ud83d\udcb0 **Net Toplam:** `{invoice.NetTotal:N2} TL`", ParseMode.Markdown, null, replyMarkup, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
			LogActivity(chatId, "XSLT Fatura Gönderimi", invoice.FirmCode, "-", invoice.NetTotal, isSuccess: true, invTitle + " XSLT PDF belgesi gönderildi. Fatura No: " + ficheNo);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "XSLT Fatura PDF belgesi üretilirken hata oluştu: {FicheNo}", ficheNo);
			await SendReplyAsync(botClient, chatId, "❌ XSLT Fatura belgesi oluşturulurken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private async Task SendInvoiceXsltDocumentAsync(ITelegramBotClient botClient, long chatId, string ficheNo, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		LogoErpDispatchDto invoice = await _logoErpRepository.GetDispatchDetailAsync(ficheNo, firmNo, periodNo, cancellationToken);
		if (invoice == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ `" + ficheNo + "` numaralı satış faturası bulunamadı.", cancellationToken);
			return;
		}
		try
		{
			byte[] htmlBytes = await _xsltInvoiceService.RenderInvoiceXsltBytesAsync(invoice, null, cancellationToken);
			string text = await _xsltInvoiceService.DetermineInvoiceTypeAsync(invoice.FirmCode, cancellationToken);
			string invTitle = ((text == "EFATURA") ? "e-FATURA (TICARI FATURA)" : "e-ARŞİV FATURASI");
			using MemoryStream ms = new MemoryStream(htmlBytes);
			string fileName = text + "_" + invoice.FicheNo + ".html";
			await botClient.SendDocument(chatId, InputFile.FromStream(ms, fileName), $"\ud83e\uddfe **SATIŞ FATURASI ({invTitle} XSLT HTML)**\n\n\ud83d\udccc **Fatura No:** `{invoice.FicheNo}`\n\ud83c\udfe2 **Cari:** `{invoice.FirmCode}` - **{invoice.FirmName}**\n\ud83d\udcb0 **Net Toplam:** `{invoice.NetTotal:N2} TL`", ParseMode.Markdown, null, null, null, null, null, disableContentTypeDetection: false, disableNotification: false, protectContent: false, null, null, allowPaidBroadcast: false, null, null, null, null, cancellationToken);
			LogActivity(chatId, "XSLT Fatura Gönderimi", invoice.FirmCode, "-", invoice.NetTotal, isSuccess: true, invTitle + " XSLT HTML belgesi gönderildi. Fatura No: " + ficheNo);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "XSLT Fatura HTML belgesi üretilirken hata oluştu: {FicheNo}", ficheNo);
			await SendReplyAsync(botClient, chatId, "❌ XSLT Fatura HTML belgesi oluşturulurken hata oluştu: " + ex.Message, cancellationToken);
		}
	}

	private bool CanSendEBelge(long chatId)
	{
		if (!(_appPermissionStore?.GetPermissions() ?? new AppPermissionsDto()).EnableEBelgeSending)
		{
			return false;
		}
		TelegramUserMappingDto telegramUserMappingDto = _telegramUserStore?.GetUser(chatId);
		if (telegramUserMappingDto != null && !telegramUserMappingDto.CanSendEBelge)
		{
			return false;
		}
		return true;
	}

	private async Task HandleEBelgeGibSendAsync(ITelegramBotClient botClient, long chatId, string data, CancellationToken cancellationToken)
	{
		if (!CanSendEBelge(chatId))
		{
			await SendReplyAsync(botClient, chatId, "❌ GİB e-Belge Gönderme modülü sistem yöneticisi tarafından kısıtlanmıştır veya bu işlem için yetkiniz bulunmuyor.", cancellationToken);
			return;
		}
		string[] array = data.Split('_', 4);
		if (array.Length < 4)
		{
			return;
		}
		string docTypeShort = array[2];
		string ficheNo = array[3];
		string documentType = (docTypeShort.Equals("FAT", StringComparison.OrdinalIgnoreCase) ? "INVOICE" : "DESPATCHADVICE");
		string docName = ((documentType == "INVOICE") ? "e-Fatura" : "e-İrsaliye");
		await SendReplyAsync(botClient, chatId, $"⏳ `{ficheNo}` numaralı {docName} için GİB Şematron kuralları kontrol ediliyor ve entegratöre iletiliyor... Lütfen bekleyiniz.", cancellationToken);
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		EBelgeDispatchRequest request = new EBelgeDispatchRequest
		{
			FicheNo = ficheNo,
			DocumentType = documentType,
			FirmNo = firmNo,
			PeriodNo = periodNo
		};
		EBelgeDispatchResult result = await _ebelgeDispatcherService.DispatchDocumentAsync(request, cancellationToken);
		if (!result.IsSuccess)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("❌ **GİB ŞEMATRON DOĞRULAMA VEYA GÖNDERİM HATASI!**\n");
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(21, 2, stringBuilder2);
			handler.AppendLiteral("⚠\ufe0f **Belge:** `");
			handler.AppendFormatted(docName);
			handler.AppendLiteral("` — `");
			handler.AppendFormatted(ficheNo);
			handler.AppendLiteral("`");
			stringBuilder3.AppendLine(ref handler);
			stringBuilder.AppendLine("\ud83d\udeab Belge GİB'e iletilmedi. GİB ceza riski engellendi!\n");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder2);
			handler.AppendLiteral("**Hata Detayı:** ");
			handler.AppendFormatted(result.ErrorMessage);
			handler.AppendLiteral("\n");
			stringBuilder4.AppendLine(ref handler);
			List<SchematronValidationError> list = result.ValidationErrors.Where((SchematronValidationError e) => e.Severity == "error").ToList();
			if (list.Count > 0)
			{
				stringBuilder.AppendLine("**Şematron Kural İhlalleri:**");
				int num = Math.Min(list.Count, 5);
				for (int num2 = 0; num2 < num; num2++)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder5 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(8, 2, stringBuilder2);
					handler.AppendLiteral("• `[");
					handler.AppendFormatted(list[num2].RuleId);
					handler.AppendLiteral("]`: ");
					handler.AppendFormatted(list[num2].Message);
					stringBuilder5.AppendLine(ref handler);
				}
				if (list.Count > 5)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder6 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(27, 1, stringBuilder2);
					handler.AppendLiteral("_...ve ");
					handler.AppendFormatted(list.Count - 5);
					handler.AppendLiteral(" kural hatası daha._");
					stringBuilder6.AppendLine(ref handler);
				}
			}
			stringBuilder.AppendLine("\n\ud83d\udca1 _Lütfen Logo ERP veya XSLT üzerindeki eksik bilgileri (vergi no, adres, birim vs.) düzelterek tekrar deneyiniz._");
			await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken);
			LogActivity(chatId, "GİB Gönderim Hatası (" + docTypeShort + ")", "-", "-", 0m, isSuccess: false, "Şematron/Gönderim Hatası: " + result.ErrorMessage);
			return;
		}
		StringBuilder stringBuilder7 = new StringBuilder();
		if (result.StatusCode == "TEST")
		{
			stringBuilder7.AppendLine("\ud83e\uddea **GİB ŞEMATRON KONTROLÜ BAŞARILI! (TEST MODU)**\n");
			StringBuilder stringBuilder2 = stringBuilder7;
			StringBuilder stringBuilder8 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(20, 2, stringBuilder2);
			handler.AppendLiteral("✅ **Belge:** `");
			handler.AppendFormatted(docName);
			handler.AppendLiteral("` — `");
			handler.AppendFormatted(ficheNo);
			handler.AppendLiteral("`");
			stringBuilder8.AppendLine(ref handler);
			stringBuilder2 = stringBuilder7;
			StringBuilder stringBuilder9 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
			handler.AppendLiteral("\ud83d\udce6 **Zarf ID:** `");
			handler.AppendFormatted(result.ZarfId);
			handler.AppendLiteral("`\n");
			stringBuilder9.AppendLine(ref handler);
			stringBuilder7.AppendLine("\ud83d\udfe2 Belgenin UBL-TR yapısı ve GİB Şematron kuralları tamamen geçerlidir. Ceza riski bulunmamaktadır.");
			stringBuilder7.AppendLine("\nℹ\ufe0f _Sistem Test Modunda olduğu için entegratör canlı ortamına gönderim yapılmadı. Canlı gönderim için yönetim panelinden 'Test Modu' anahtarını kapatabilirsiniz._");
		}
		else
		{
			StringBuilder stringBuilder2 = stringBuilder7;
			StringBuilder stringBuilder10 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(34, 1, stringBuilder2);
			handler.AppendLiteral("\ud83d\ude80 **");
			handler.AppendFormatted(docName.ToUpper());
			handler.AppendLiteral(" GİB'E BAŞARIYLA İLETİLDİ!**\n");
			stringBuilder10.AppendLine(ref handler);
			stringBuilder2 = stringBuilder7;
			StringBuilder stringBuilder11 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(20, 2, stringBuilder2);
			handler.AppendLiteral("✅ **Belge:** `");
			handler.AppendFormatted(docName);
			handler.AppendLiteral("` — `");
			handler.AppendFormatted(ficheNo);
			handler.AppendLiteral("`");
			stringBuilder11.AppendLine(ref handler);
			stringBuilder2 = stringBuilder7;
			StringBuilder stringBuilder12 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(18, 1, stringBuilder2);
			handler.AppendLiteral("\ud83d\udce6 **Zarf ID:** `");
			handler.AppendFormatted(result.ZarfId);
			handler.AppendLiteral("`");
			stringBuilder12.AppendLine(ref handler);
			if (!string.IsNullOrEmpty(result.StatusDescription))
			{
				stringBuilder2 = stringBuilder7;
				StringBuilder stringBuilder13 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
				handler.AppendLiteral("\ud83d\udcc8 **Durum:** `");
				handler.AppendFormatted(result.StatusDescription);
				handler.AppendLiteral("`");
				stringBuilder13.AppendLine(ref handler);
			}
			stringBuilder7.AppendLine("\n\ud83d\udfe2 Belge GİB Şematron kurallarından başarıyla geçti ve entegratör sistemine iletildi.");
		}
		await SendReplyAsync(botClient, chatId, stringBuilder7.ToString(), cancellationToken);
		LogActivity(chatId, "GİB Gönderim (" + docTypeShort + ")", "-", "-", 0m, isSuccess: true, $"{docName} GİB'e iletildi/doğrulandı: {ficheNo} (Zarf: {result.ZarfId})");
	}

	private async Task SendPurchaseOrderListAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		List<LogoErpOpenOrderSummaryDto> pOrders = await _logoErpRepository.GetPurchaseOrdersAsync(firmCodeFilter, null, null, firmNo, periodNo, 20, cancellationToken);
		if (pOrders == null || pOrders.Count == 0)
		{
			await SendReplyAsync(botClient, chatId, "📥 Kayıtlı herhangi bir alış siparişi bulunamadı.", cancellationToken);
			return;
		}

		// En son girilen alış siparişi en alta gelsin
		pOrders = pOrders.OrderBy(o => o.Date).ThenBy(o => o.LogicalRef).ToList();

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("📥 **ALIŞ SİPARİŞLERİ LİSTESİ (SALT OKUNUR)**");
		if (!string.IsNullOrWhiteSpace(firmCodeFilter))
		{
			stringBuilder.AppendLine($"🔍 **Cari:** `{firmCodeFilter}` | 📊 **Toplam:** `{pOrders.Count} Sipariş`");
		}
		else
		{
			stringBuilder.AppendLine($"📊 **Toplam:** `{pOrders.Count} Alış Siparişi`");
		}
		stringBuilder.AppendLine("\nDetayını incelemek istediğiniz alış siparişine tıklayınız:\n*(En son girilen sipariş en alttadır)*");

		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		foreach (LogoErpOpenOrderSummaryDto item in pOrders)
		{
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData($"📥 {item.FicheNo} ({item.Date:dd.MM.yyyy} - {item.NetTotal:N2} TL)", "pdoc_det:sip:" + item.FicheNo) });
		}
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, replyMarkup);
		LogActivity(chatId, "/alissiparisleri", firmCodeFilter ?? "-", "-", 0m, isSuccess: true, $"{pOrders.Count} alış siparişi listelendi.");
	}

	private async Task SendPurchaseDispatchListAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		List<LogoErpOpenOrderSummaryDto> pDispatches = await _logoErpRepository.GetPurchaseDispatchesAsync(firmCodeFilter, null, null, firmNo, periodNo, 20, cancellationToken);
		if (pDispatches == null || pDispatches.Count == 0)
		{
			await SendReplyAsync(botClient, chatId, "🚚 Kayıtlı herhangi bir alış irsaliyesi bulunamadı.", cancellationToken);
			return;
		}

		// En son kesilen alış irsaliyesi en alta gelsin
		pDispatches = pDispatches.OrderBy(d => d.Date).ThenBy(d => d.LogicalRef).ToList();

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("🚚 **ALIŞ İRSALİYELERİ LİSTESİ (SALT OKUNUR)**");
		if (!string.IsNullOrWhiteSpace(firmCodeFilter))
		{
			stringBuilder.AppendLine($"🔍 **Cari:** `{firmCodeFilter}` | 📊 **Toplam:** `{pDispatches.Count} İrsaliye`");
		}
		else
		{
			stringBuilder.AppendLine($"📊 **Toplam:** `{pDispatches.Count} Alış İrsaliyesi`");
		}
		stringBuilder.AppendLine("\nDetayını incelemek istediğiniz alış irsaliyesine tıklayınız:\n*(En son kesilen irsaliye en alttadır)*");

		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		foreach (LogoErpOpenOrderSummaryDto item in pDispatches)
		{
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData($"🚚 {item.FicheNo} ({item.Date:dd.MM.yyyy} - {item.NetTotal:N2} TL)", "pdoc_det:irs:" + item.FicheNo) });
		}
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, replyMarkup);
		LogActivity(chatId, "/alisirsaliyeleri", firmCodeFilter ?? "-", "-", 0m, isSuccess: true, $"{pDispatches.Count} alış irsaliyesi listelendi.");
	}

	private async Task SendPurchaseInvoiceListAsync(ITelegramBotClient botClient, long chatId, string? firmCodeFilter, CancellationToken cancellationToken)
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		List<LogoErpOpenOrderSummaryDto> pInvoices = await _logoErpRepository.GetPurchaseInvoicesAsync(firmCodeFilter, null, null, firmNo, periodNo, 20, cancellationToken);
		if (pInvoices == null || pInvoices.Count == 0)
		{
			await SendReplyAsync(botClient, chatId, "🧾 Kayıtlı herhangi bir alış faturası bulunamadı.", cancellationToken);
			return;
		}

		// En son kesilen alış faturası en alta gelsin
		pInvoices = pInvoices.OrderBy(i => i.Date).ThenBy(i => i.LogicalRef).ToList();

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("🧾 **ALIŞ FATURALARI LİSTESİ (SALT OKUNUR)**");
		if (!string.IsNullOrWhiteSpace(firmCodeFilter))
		{
			stringBuilder.AppendLine($"🔍 **Cari:** `{firmCodeFilter}` | 📊 **Toplam:** `{pInvoices.Count} Fatura`");
		}
		else
		{
			stringBuilder.AppendLine($"📊 **Toplam:** `{pInvoices.Count} Alış Faturası`");
		}
		stringBuilder.AppendLine("\nDetayını incelemek istediğiniz alış faturasına tıklayınız:\n*(En son kesilen fatura en alttadır)*");

		List<InlineKeyboardButton[]> list = new List<InlineKeyboardButton[]>();
		foreach (LogoErpOpenOrderSummaryDto item in pInvoices)
		{
			list.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData($"🧾 {item.FicheNo} ({item.Date:dd.MM.yyyy} - {item.NetTotal:N2} TL)", "pdoc_det:fat:" + item.FicheNo) });
		}
		InlineKeyboardMarkup replyMarkup = new InlineKeyboardMarkup(list);
		await SendReplyAsync(botClient, chatId, stringBuilder.ToString(), cancellationToken, replyMarkup);
		LogActivity(chatId, "/alisfaturalari", firmCodeFilter ?? "-", "-", 0m, isSuccess: true, $"{pInvoices.Count} alış faturası listelendi.");
	}

	private void SafeLogInformation(string message, params object[] args)
	{
		try
		{
			_logger.LogInformation(message, args);
		}
		catch
		{
		}
	}

	private void SafeLogWarning(string message, params object[] args)
	{
		try
		{
			_logger.LogWarning(message, args);
		}
		catch
		{
		}
	}

	private void SafeLogWarning(Exception ex, string message, params object[] args)
	{
		try
		{
			_logger.LogWarning(ex, message, args);
		}
		catch
		{
		}
	}

	private void SafeLogError(Exception ex, string message, params object[] args)
	{
		try
		{
			_logger.LogError(ex, message, args);
		}
		catch
		{
		}
	}

	public async Task SendTextMessageAsync(long chatId, string messageHtml, CancellationToken ct = default)
	{
		if (_botClient != null)
		{
			await SendReplyAsync(_botClient, chatId, messageHtml, ct);
		}
	}

	private async Task HandleRiskCommandAsync(ITelegramBotClient botClient, long chatId, string param, CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(param))
		{
			await SendReplyAsync(botClient, chatId, "⚠️ **Kullanım:** `/risk <cari_kodu_veya_unvan>`\n*Örnek:* `/risk LG.001` veya `/risk AHMET LTD`", ct);
			return;
		}
		await SendReplyAsync(botClient, chatId, $"⏳ **'{param}'** için Cari Risk Analizi yapılıyor...", ct);
		if (_aiAssistant != null)
		{
			var report = await _aiAssistant.EvaluateCustomerRiskAsync(param, ct);
			var sb = new StringBuilder();
			sb.AppendLine("📊 <b>CARİ FİNANSAL RİSK ANALİZİ</b>");
			sb.AppendLine($"🏢 <b>Cari:</b> {report.CustomerCode} - {report.CustomerTitle}");
			sb.AppendLine($"💰 <b>Toplam Bakiye:</b> {report.TotalBalance:N2} TL");
			sb.AppendLine($"⚠️ <b>Vadesi Geçen Borç:</b> {report.OverdueBalance:N2} TL ({report.OverdueInvoiceCount} Fatura)");
			sb.AppendLine($"⏱️ <b>Ortalama Ödeme Gecikmesi:</b> {report.AveragePaymentDelayDays} Gün");
			sb.AppendLine("-----------------------------------------");
			sb.AppendLine($"<b>Risk Seviyesi:</b> {report.RiskLevel}");
			sb.AppendLine(report.AiRecommendation);
			await SendReplyAsync(botClient, chatId, sb.ToString(), ct);
		}
		else
		{
			await SendReplyAsync(botClient, chatId, "❌ AI Risk Analiz Servisi aktif değil.", ct);
		}
	}

	private async Task HandleExecutiveReportCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken ct)
	{
		await SendReplyAsync(botClient, chatId, "⏳ **Günlük Yönetici Özet Raporu hazırlanıyor...**", ct);
		if (_executiveReport != null)
		{
			var report = await _executiveReport.GenerateDailySummaryAsync(ct);
			if (!string.IsNullOrWhiteSpace(report.FormattedTelegramHtml))
			{
				await SendReplyAsync(botClient, chatId, report.FormattedTelegramHtml, ct);
			}
		}
		else
		{
			await SendReplyAsync(botClient, chatId, "❌ Yönetici Rapor Servisi aktif değil.", ct);
		}
	}

	private async Task HandleCriticalStockCommandAsync(ITelegramBotClient botClient, long chatId, CancellationToken ct)
	{
		await SendReplyAsync(botClient, chatId, "⏳ **Kritik stok seviyesindeki ürünler taranıyor...**", ct);
		if (_executiveReport != null)
		{
			var items = await _executiveReport.GetCriticalStockItemsAsync(5.0, ct);
			if (items != null && items.Any())
			{
				var sb = new StringBuilder();
				sb.AppendLine("🚨 <b>KRİTİK STOK SEVİYESİNDEKİ ÜRÜNLER</b> (5 Adet ve Altı)");
				sb.AppendLine("-----------------------------------------");
				foreach (var item in items)
				{
					sb.AppendLine($"• `{item.ItemCode}` - {item.ItemName}: **{item.CurrentOnHand} {item.Unit}**");
				}
				await SendReplyAsync(botClient, chatId, sb.ToString(), ct);
			}
			else
			{
				await SendReplyAsync(botClient, chatId, "✅ **Kritik Seviyede Ürün Yok:** Tüm stoklar güvenli seviyededir.", ct);
			}
		}
	}

	private async Task SendFirmamMenuAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("🏢 **FİRMAM & KURUMSAL ŞİRKET KOKPİTİ**\n");
		sb.AppendLine("👇 *Görüntülemek istediğiniz kurumsal raporu veya işlem modülünü seçiniz:*\n");

		var buttons = new List<InlineKeyboardButton[]>
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData("💰 Kasa Durumu", "rpt_dt_sel:rpt_kasa_bakiye"),
				InlineKeyboardButton.WithCallbackData("🏦 Banka Durumu", "bnk_hierarchy")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData("📑 Çek / Senet", "rpt_dt_sel:rpt_cek_portfoy"),
				InlineKeyboardButton.WithCallbackData("📈 Satış Faturaları", "rpt_dt_sel:rpt_satis_faturalari")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData("🛒 Alış Faturaları", "rpt_dt_sel:rpt_alis_faturalari"),
				InlineKeyboardButton.WithCallbackData("🚚 Açık İrsaliyeler", "rpt_dt_sel:rpt_acik_irsaliyeler")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData("📋 Bekleyen Siparişler", "rpt_dt_sel:rpt_acik_siparisler"),
				InlineKeyboardButton.WithCallbackData("📚 Genel Mizan", "rpt_dt_sel:rpt_genel_mizan")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData("🏭 Üretim Raporları", "rpt_dt_sel:rpt_uretim_emirleri"),
				InlineKeyboardButton.WithCallbackData("📊 Tüm Özel Raporlar", "firmam_cat:ALL")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData("📊 Karlılık Raporu", "rpt_dt_sel:rpt_karlilik"),
				InlineKeyboardButton.WithCallbackData("📦 Genel Malzeme Karlılığı", "rpt_dt_sel:rpt_malzeme_karlilik")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData("💸 Nakit Akış Durumu", "rpt_dt_sel:rpt_nakit_akis"),
				InlineKeyboardButton.WithCallbackData("🧾 Genel Borç ve Alacak", "rpt_dt_sel:rpt_borc_alacak")
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData("📆 Vade Raporu", "rpt_dt_sel:rpt_vade_raporu")
			}
		};

		var markup = new InlineKeyboardMarkup(buttons);
		await SendReplyAsync(botClient, chatId, sb.ToString(), cancellationToken, markup);
		LogActivity(chatId, "firmam", "-", "-", 0m, isSuccess: true, "Firmam Şirket Kokpit Menüsü açıldı.");
	}

	private async Task SendBankHierarchyMenuAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
	{
		if (_dynamicSqlExecutor == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ Banka sorgulama servisi aktif değil.", cancellationToken);
			return;
		}

		string firmNo = _configuration["LogoErp:FirmNo"] ?? _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoErp:PeriodNo"] ?? _configuration["LogoSystem:PeriodNo"] ?? "01";

		string sql = $@"
SELECT 
    B.LOGICALREF AS BankRef,
    B.CODE AS BankCode,
    B.DEFINITION_ AS BankName,
    COUNT(DISTINCT BA.LOGICALREF) AS AccountCount,
    ISNULL(SUM(CASE WHEN L.SIGN = 0 THEN L.AMOUNT ELSE -L.AMOUNT END), 0) AS TotalBalance
FROM LG_{firmNo}_BNCARD B WITH (NOLOCK)
LEFT JOIN LG_{firmNo}_BANKACC BA WITH (NOLOCK) ON BA.BANKREF = B.LOGICALREF
LEFT JOIN LG_{firmNo}_{periodNo}_BNFLINE L WITH (NOLOCK) ON L.BNACCREF = BA.LOGICALREF AND (L.CANCELLED = 0 OR L.CANCELLED IS NULL)
GROUP BY B.LOGICALREF, B.CODE, B.DEFINITION_
ORDER BY TotalBalance DESC, B.DEFINITION_ ASC";

		string json = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(sql, cancellationToken);
		if (json.StartsWith("[GÜVENLİK İHLALİ]") || json.StartsWith("[SQL HATASI]"))
		{
			await SendReplyAsync(botClient, chatId, $"❌ Banka listesi alınamadı: {json}", cancellationToken);
			return;
		}

		try
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;
			if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
			{
				await SendReplyAsync(botClient, chatId, "🏦 Sistemde kayıtlı banka hesabı bulunamadı.", cancellationToken);
				return;
			}

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("🏦 **BANKA HESAPLARI ANA LİSTESİ**\n");
			sb.AppendLine("👇 *Detaylı alt hesaplarını ve bakiyelerini görmek istediğiniz bankayı seçiniz:*\n");

			decimal grandTotal = 0;
			var buttons = new List<InlineKeyboardButton[]>();

			foreach (var bank in root.EnumerateArray())
			{
				string bankRef = bank.GetProperty("BankRef").ToString();
				string bankCode = bank.GetProperty("BankCode").GetString() ?? "";
				string bankName = bank.GetProperty("BankName").GetString() ?? bankCode;
				int accCount = 0;
				if (bank.TryGetProperty("AccountCount", out var accCountProp))
				{
					int.TryParse(accCountProp.ToString(), out accCount);
				}
				decimal balance = 0;
				if (bank.TryGetProperty("TotalBalance", out var balProp))
				{
					decimal.TryParse(balProp.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out balance);
				}
				grandTotal += balance;

				sb.AppendLine($"• **{bankName}** ({accCount} Hesap): `{balance:N2} TL`");
				string btnText = $"🏦 {bankName} ({balance:N2} TL)";
				buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(btnText, $"bnk_card:{bankRef}") });
			}

			sb.AppendLine($"\n💰 **Tüm Bankalar Toplamı:** `{grandTotal:N2} TL`");

			buttons.Add(new[]
			{
				InlineKeyboardButton.WithCallbackData("🏢 Firmam Kokpitine Dön", "firmam_menu")
			});

			await SendReplyAsync(botClient, chatId, sb.ToString(), cancellationToken, new InlineKeyboardMarkup(buttons));
			LogActivity(chatId, "Banka Ana Listesi", "-", "-", grandTotal, isSuccess: true, "Banka ana listesi gösterildi.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Banka hiyerarşi menüsü hatası.");
			await SendReplyAsync(botClient, chatId, "❌ Banka hesapları dökülürken hata oluştu.", cancellationToken);
		}
	}

	private async Task SendBankAccountsListAsync(ITelegramBotClient botClient, long chatId, string bankRef, CancellationToken cancellationToken)
	{
		if (_dynamicSqlExecutor == null) return;

		string firmNo = _configuration["LogoErp:FirmNo"] ?? _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoErp:PeriodNo"] ?? _configuration["LogoSystem:PeriodNo"] ?? "01";

		string sql = $@"
SELECT 
    BA.LOGICALREF AS AccRef,
    BA.CODE AS AccCode,
    BA.DEFINITION_ AS AccName,
    ISNULL(BA.IBAN, '') AS IBAN,
    BA.CURRENCY AS Currency,
    ISNULL(B.DEFINITION_, '') AS BankName,
    ISNULL(SUM(CASE WHEN L.SIGN = 0 THEN L.AMOUNT ELSE -L.AMOUNT END), 0) AS AccBalance
FROM LG_{firmNo}_BANKACC BA WITH (NOLOCK)
INNER JOIN LG_{firmNo}_BNCARD B WITH (NOLOCK) ON B.LOGICALREF = BA.BANKREF
LEFT JOIN LG_{firmNo}_{periodNo}_BNFLINE L WITH (NOLOCK) ON L.BNACCREF = BA.LOGICALREF AND (L.CANCELLED = 0 OR L.CANCELLED IS NULL)
WHERE BA.BANKREF = {bankRef}
GROUP BY BA.LOGICALREF, BA.CODE, BA.DEFINITION_, BA.IBAN, BA.CURRENCY, B.DEFINITION_
ORDER BY AccBalance DESC";

		string json = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(sql, cancellationToken);
		try
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;
			if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
			{
				await SendReplyAsync(botClient, chatId, "❌ Bu bankaya bağlı alt hesap bulunamadı.", cancellationToken);
				return;
			}

			string bankName = root[0].GetProperty("BankName").GetString() ?? "Banka";
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"🏦 **{bankName.ToUpperInvariant()} — ALT HESAP LİSTESİ**\n");
			sb.AppendLine("👇 *Hesap hareketlerini ve IBAN detaylarını görmek için hesaba tıklayınız:*\n");

			var buttons = new List<InlineKeyboardButton[]>();
			foreach (var acc in root.EnumerateArray())
			{
				string accRef = acc.GetProperty("AccRef").ToString();
				string accCode = acc.GetProperty("AccCode").GetString() ?? "";
				string accName = acc.GetProperty("AccName").GetString() ?? accCode;
				string iban = acc.GetProperty("IBAN").GetString() ?? "";
				decimal balance = 0;
				if (acc.TryGetProperty("AccBalance", out var balProp))
				{
					decimal.TryParse(balProp.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out balance);
				}

				sb.AppendLine($"💳 **{accName}** (`{accCode}`)");
				if (!string.IsNullOrWhiteSpace(iban)) sb.AppendLine($"  └ *IBAN:* `{iban}`");
				sb.AppendLine($"  └ *Bakiye:* `{balance:N2} TL`\n");

				string btnText = $"💳 {accName} ({balance:N2} TL)";
				buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(btnText, $"bnk_acc:{accRef}") });
			}

			buttons.Add(new[]
			{
				InlineKeyboardButton.WithCallbackData("🔙 Banka Listesine Dön", "bnk_hierarchy"),
				InlineKeyboardButton.WithCallbackData("🏢 Firmam Kokpiti", "firmam_menu")
			});

			await SendReplyAsync(botClient, chatId, sb.ToString(), cancellationToken, new InlineKeyboardMarkup(buttons));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Banka alt hesapları yüklenirken hata oluştu.");
			await SendReplyAsync(botClient, chatId, "❌ Hesap listesi alınırken hata oluştu.", cancellationToken);
		}
	}

	private async Task SendBankAccountDetailAsync(ITelegramBotClient botClient, long chatId, string accRef, CancellationToken cancellationToken)
	{
		if (_dynamicSqlExecutor == null) return;

		string firmNo = _configuration["LogoErp:FirmNo"] ?? _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoErp:PeriodNo"] ?? _configuration["LogoSystem:PeriodNo"] ?? "01";

		string sqlAcc = $@"
SELECT 
    BA.LOGICALREF AS AccRef,
    BA.CODE AS AccCode,
    BA.DEFINITION_ AS AccName,
    ISNULL(BA.IBAN, '') AS IBAN,
    ISNULL(B.DEFINITION_, '') AS BankName,
    ISNULL(SUM(CASE WHEN L.SIGN = 0 THEN L.AMOUNT ELSE -L.AMOUNT END), 0) AS AccBalance
FROM LG_{firmNo}_BANKACC BA WITH (NOLOCK)
INNER JOIN LG_{firmNo}_BNCARD B WITH (NOLOCK) ON B.LOGICALREF = BA.BANKREF
LEFT JOIN LG_{firmNo}_{periodNo}_BNFLINE L WITH (NOLOCK) ON L.BNACCREF = BA.LOGICALREF AND (L.CANCELLED = 0 OR L.CANCELLED IS NULL)
WHERE BA.LOGICALREF = {accRef}
GROUP BY BA.LOGICALREF, BA.CODE, BA.DEFINITION_, BA.IBAN, B.DEFINITION_";

		string sqlTxs = $@"
SELECT TOP 10
    L.DATE_ AS [Tarih],
    L.TRANNO AS [IslemNo],
    L.LINEEXP AS [Aciklama],
    CASE WHEN L.SIGN = 0 THEN L.AMOUNT ELSE 0 END AS [Giris],
    CASE WHEN L.SIGN = 1 THEN L.AMOUNT ELSE 0 END AS [Cikis]
FROM LG_{firmNo}_{periodNo}_BNFLINE L WITH (NOLOCK)
WHERE L.BNACCREF = {accRef} AND (L.CANCELLED = 0 OR L.CANCELLED IS NULL)
ORDER BY L.DATE_ DESC, L.LOGICALREF DESC";

		string jsonAcc = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(sqlAcc, cancellationToken);
		string jsonTxs = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(sqlTxs, cancellationToken);

		try
		{
			using var docAcc = JsonDocument.Parse(jsonAcc);
			var rootAcc = docAcc.RootElement;
			if (rootAcc.ValueKind != JsonValueKind.Array || rootAcc.GetArrayLength() == 0)
			{
				await SendReplyAsync(botClient, chatId, "❌ Hesap bilgisi bulunamadı.", cancellationToken);
				return;
			}

			var acc = rootAcc[0];
			string bankName = acc.GetProperty("BankName").GetString() ?? "";
			string accName = acc.GetProperty("AccName").GetString() ?? "";
			string accCode = acc.GetProperty("AccCode").GetString() ?? "";
			string iban = acc.GetProperty("IBAN").GetString() ?? "";
			decimal balance = 0;
			if (acc.TryGetProperty("AccBalance", out var balProp))
			{
				decimal.TryParse(balProp.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out balance);
			}

			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"🏦 **{bankName} — {accName}**");
			sb.AppendLine($"• **Hesap Kodu:** `{accCode}`");
			if (!string.IsNullOrWhiteSpace(iban)) sb.AppendLine($"• **IBAN:** `{iban}`");
			sb.AppendLine($"• **Güncel Bakiye:** `{balance:N2} TL`\n");
			sb.AppendLine("📋 **Son 10 Hesap Hareketi:**");

			using var docTxs = JsonDocument.Parse(jsonTxs);
			var rootTxs = docTxs.RootElement;
			if (rootTxs.ValueKind == JsonValueKind.Array && rootTxs.GetArrayLength() > 0)
			{
				foreach (var tx in rootTxs.EnumerateArray())
				{
					string tarih = tx.TryGetProperty("Tarih", out var tProp) ? tProp.ToString() : "";
					if (DateTime.TryParse(tarih, out var dt)) tarih = dt.ToString("dd.MM.yyyy");
					string islemNo = tx.TryGetProperty("IslemNo", out var nProp) ? nProp.ToString() : "-";
					string aciklama = tx.TryGetProperty("Aciklama", out var aProp) ? aProp.ToString() : "";
					decimal giris = 0, cikis = 0;
					if (tx.TryGetProperty("Giris", out var gProp)) decimal.TryParse(gProp.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out giris);
					if (tx.TryGetProperty("Cikis", out var cProp)) decimal.TryParse(cProp.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out cikis);

					string yon = giris > 0 ? $"🟢 Giriş: +{giris:N2} TL" : $"🔴 Çıkış: -{cikis:N2} TL";
					sb.AppendLine($"• `{tarih}` | `{islemNo}` | {aciklama} | **{yon}**");
				}
			}
			else
			{
				sb.AppendLine("_Henüz bu döneme ait hareket kaydı bulunmuyor._");
			}

			var buttons = new InlineKeyboardMarkup(new[]
			{
				new[]
				{
					InlineKeyboardButton.WithCallbackData("🔙 Banka Listesi", "bnk_hierarchy"),
					InlineKeyboardButton.WithCallbackData("🏢 Firmam Kokpiti", "firmam_menu")
				}
			});

			await SendReplyAsync(botClient, chatId, sb.ToString(), cancellationToken, buttons);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Hesap detayı görüntülenirken hata oluştu.");
			await SendReplyAsync(botClient, chatId, "❌ Hesap detayı yüklenemedi.", cancellationToken);
		}
	}

	private async Task SendReportDateSelectionForCustomReportAsync(ITelegramBotClient botClient, long chatId, string reportId, CancellationToken cancellationToken)
	{
		var report = _customReportStore?.GetReportById(reportId);
		string title = report != null ? $"{report.Icon} {report.Title}" : "Rapor Tarih Seçimi";

		var replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
		{
			new InlineKeyboardButton[]
			{
				InlineKeyboardButton.WithCallbackData("📅 Bugün", $"rpt_run_dt:{reportId}:today"),
				InlineKeyboardButton.WithCallbackData("📅 Bu Hafta", $"rpt_run_dt:{reportId}:week")
			},
			new InlineKeyboardButton[]
			{
				InlineKeyboardButton.WithCallbackData("📅 Bu Ay", $"rpt_run_dt:{reportId}:month"),
				InlineKeyboardButton.WithCallbackData("📅 Bu Yıl", $"rpt_run_dt:{reportId}:year")
			},
			new InlineKeyboardButton[]
			{
				InlineKeyboardButton.WithCallbackData("📅 Son 30 Gün", $"rpt_run_dt:{reportId}:30days"),
				InlineKeyboardButton.WithCallbackData("📅 Son 90 Gün", $"rpt_run_dt:{reportId}:90days")
			},
			new InlineKeyboardButton[]
			{
				InlineKeyboardButton.WithCallbackData("♾️ Tüm Zamanlar", $"rpt_run_dt:{reportId}:all")
			},
			new InlineKeyboardButton[]
			{
				InlineKeyboardButton.WithCallbackData("🔙 Geri Dön", "firmam_menu")
			}
		});

		await SendReplyAsync(botClient, chatId, $"📊 **{title}**\n\nLütfen getirilmesini istediğiniz tarih aralığını seçiniz:", cancellationToken, replyMarkup);
	}

	private async Task ExecuteCustomReportWithDateRangeAsync(ITelegramBotClient botClient, long chatId, string reportId, string rangeText, CancellationToken cancellationToken)
	{
		DateTime today = DateTime.Today;
		DateTime now = DateTime.Now;
		DateTime? startDate = null;
		DateTime? endDate = null;

		switch (rangeText.ToLowerInvariant())
		{
			case "today":
				startDate = today;
				endDate = today.AddDays(1).AddTicks(-1);
				break;
			case "week":
				int dayOffset = (int)(today.DayOfWeek - 1);
				if (dayOffset < 0) dayOffset += 7;
				startDate = today.AddDays(-dayOffset);
				endDate = now;
				break;
			case "month":
				startDate = new DateTime(today.Year, today.Month, 1);
				endDate = now;
				break;
			case "year":
				startDate = new DateTime(today.Year, 1, 1);
				endDate = now;
				break;
			case "30days":
				startDate = today.AddDays(-30);
				endDate = now;
				break;
			case "90days":
				startDate = today.AddDays(-90);
				endDate = now;
				break;
			default:
				startDate = null;
				endDate = null;
				break;
		}

		await ExecuteCustomReportAsync(botClient, chatId, reportId, null, null, cancellationToken, startDate, endDate);
	}

	private async Task SendReportsMenuAsync(ITelegramBotClient botClient, long chatId, string? category, CancellationToken cancellationToken)
	{
		if (_customReportStore == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ Rapor servisi henüz yüklenmedi.", cancellationToken);
			return;
		}

		List<CustomReportDto> reports;
		if (string.IsNullOrWhiteSpace(category) || category.Equals("ALL", StringComparison.OrdinalIgnoreCase))
		{
			reports = _customReportStore.GetAllReports().Where(x => x.IsActive).ToList();
		}
		else
		{
			reports = _customReportStore.GetReportsByCategory(category);
		}

		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"📊 **{(string.IsNullOrWhiteSpace(category) || category == "ALL" ? "TÜM HAZIR VE ÖZEL RAPORLAR" : category.ToUpperInvariant() + " RAPORLARI")}**\n");
		sb.AppendLine("👇 *Çalıştırmak istediğiniz rapora tıklayınız:*\n");

		var buttons = new List<InlineKeyboardButton[]>();
		foreach (var rpt in reports)
		{
			string btnText = $"{rpt.Icon} {rpt.TelegramButtonText}";
			buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(btnText, "rpt_dt_sel:" + rpt.Id) });
		}

		buttons.Add(new[]
		{
			InlineKeyboardButton.WithCallbackData("🏢 Firmam Menüsü", "firmam_menu"),
			InlineKeyboardButton.WithCallbackData("🔙 Ana Menü", "/start")
		});

		var markup = new InlineKeyboardMarkup(buttons);
		await SendReplyAsync(botClient, chatId, sb.ToString(), cancellationToken, markup);
	}

	private async Task ExecuteCustomReportAsync(ITelegramBotClient botClient, long chatId, string reportId, string? firmCode, string? itemCode, CancellationToken cancellationToken, DateTime? startDate = null, DateTime? endDate = null)
	{
		if (_customReportStore == null || _dynamicSqlExecutor == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ Rapor çalıştırma servisi aktif değil.", cancellationToken);
			return;
		}

		var report = _customReportStore.GetReportById(reportId);
		if (report == null)
		{
			await SendReplyAsync(botClient, chatId, "❌ Rapor bulunamadı veya silinmiş.", cancellationToken);
			return;
		}

		string firmNo = _configuration["LogoErp:FirmNo"] ?? _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoErp:PeriodNo"] ?? _configuration["LogoSystem:PeriodNo"] ?? "01";

		string? clientRef = null;
		if (!string.IsNullOrWhiteSpace(firmCode))
		{
			var firms = await _proposalAppService.SearchFirmsAsync(firmCode, cancellationToken);
			if (firms != null && firms.Count > 0)
			{
				clientRef = firms[0].LogicalRef > 0 ? firms[0].LogicalRef.ToString() : null;
			}
		}

		string? itemRef = null;
		if (!string.IsNullOrWhiteSpace(itemCode))
		{
			var itemDetails = await _logoErpRepository.GetItemDetailsAsync(itemCode, firmNo, cancellationToken);
			if (itemDetails != null && itemDetails.ItemLogicalRef > 0)
			{
				itemRef = itemDetails.ItemLogicalRef.ToString();
			}
		}

		string? startStr = startDate?.ToString("yyyy-MM-dd");
		string? endStr = endDate?.ToString("yyyy-MM-dd");

		string resolvedSql = _customReportStore.ResolveSqlPlaceholders(report.SqlQuery, firmNo, periodNo, clientRef, itemRef, startStr, endStr);

		var sw = System.Diagnostics.Stopwatch.StartNew();
		string jsonResult = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(resolvedSql, cancellationToken);
		sw.Stop();

		if (jsonResult.StartsWith("[GÜVENLİK İHLALİ]") || jsonResult.StartsWith("[SQL HATASI]"))
		{
			_reportHistoryStore?.AddHistoryItem(new ReportHistoryItemDto
			{
				ReportId = report.Id,
				ReportTitle = report.Title,
				Category = report.Category,
				ExecutedBy = "Telegram @" + (GetSalesmanDisplayName(chatId) ?? "User"),
				ChatId = chatId,
				SqlQuery = resolvedSql,
				DurationMs = sw.ElapsedMilliseconds,
				RowCount = 0,
				IsSuccess = false,
				ErrorMessage = jsonResult
			});

			await SendReplyAsync(botClient, chatId, $"❌ **Rapor Çalıştırma Hatası:**\n`{jsonResult}`", cancellationToken);
			return;
		}

		try
		{
			using var doc = JsonDocument.Parse(jsonResult);
			var root = doc.RootElement;
			int rowCount = root.ValueKind == JsonValueKind.Array ? root.GetArrayLength() : 0;

			_reportHistoryStore?.AddHistoryItem(new ReportHistoryItemDto
			{
				ReportId = report.Id,
				ReportTitle = report.Title,
				Category = report.Category,
				ExecutedBy = "Telegram @" + (GetSalesmanDisplayName(chatId) ?? "User"),
				ChatId = chatId,
				SqlQuery = resolvedSql,
				DurationMs = sw.ElapsedMilliseconds,
				RowCount = rowCount,
				IsSuccess = true,
				ResultPreviewJson = jsonResult.Length > 300 ? jsonResult.Substring(0, 300) + "..." : jsonResult
			});

			if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"📊 **{report.Icon} {report.Title.ToUpperInvariant()}**");
				if (startDate.HasValue)
				{
					sb.AppendLine($"📅 **Filtre:** `{startDate:dd.MM.yyyy}` - `{endDate:dd.MM.yyyy}` *(Toplam: {rowCount} Kayıt)*\n");
				}
				else if (!string.IsNullOrWhiteSpace(report.Description))
				{
					sb.AppendLine($"_{report.Description}_\n");
				}
				else
				{
					sb.AppendLine();
				}

				int index = 1;
				foreach (var row in root.EnumerateArray())
				{
					sb.AppendLine($"📌 **Kayıt #{index}:**");
					foreach (var prop in row.EnumerateObject())
					{
						string valStr = prop.Value.ToString();
						if (decimal.TryParse(valStr, out decimal numVal) && (prop.Name.Contains("TL") || prop.Name.Contains("Tutar") || prop.Name.Contains("Bakiye") || prop.Name.Contains("Fiyat") || prop.Name.Contains("Borç") || prop.Name.Contains("Alacak")))
						{
							valStr = numVal.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")) + " TL";
						}
						sb.AppendLine($"  • **{prop.Name}:** `{valStr}`");
					}
					sb.AppendLine();
					index++;
					if (index > 20)
					{
						sb.AppendLine($"_(İlk 20 kayıt listelenmiştir. Toplam {rowCount} kayıt bulunmaktadır.)_");
						break;
					}
				}

				var backButtons = new InlineKeyboardMarkup(new[]
				{
					new[]
					{
						InlineKeyboardButton.WithCallbackData("📅 Tarih Değiştir", "rpt_dt_sel:" + report.Id),
						InlineKeyboardButton.WithCallbackData("🔄 Yenile", $"rpt_run_dt:{report.Id}:{(startDate.HasValue ? "month" : "all")}")
					},
					new[]
					{
						InlineKeyboardButton.WithCallbackData("🏢 Firmam Kokpiti", "firmam_menu")
					}
				});

				await SendReplyAsync(botClient, chatId, sb.ToString(), cancellationToken, backButtons);
				LogActivity(chatId, "Rapor: " + report.Title, firmCode ?? "-", itemCode ?? "-", 0m, isSuccess: true, $"Özel SQL Raporu çalıştırıldı ({rowCount} satır).");
				return;
			}
			else
			{
				var emptyButtons = new InlineKeyboardMarkup(new[]
				{
					new[]
					{
						InlineKeyboardButton.WithCallbackData("📅 Başka Tarih Seç", "rpt_dt_sel:" + report.Id),
						InlineKeyboardButton.WithCallbackData("🏢 Firmam Kokpiti", "firmam_menu")
					}
				});
				await SendReplyAsync(botClient, chatId, $"ℹ️ **{report.Icon} {report.Title}**\n\nSeçilen kriterlere veya tarih aralığına uygun herhangi bir kayıt bulunamadı.", cancellationToken, emptyButtons);
				return;
			}
		}
		catch (Exception ex)
		{
			await SendReplyAsync(botClient, chatId, $"📊 **{report.Icon} {report.Title}**\n\n```json\n{jsonResult}\n```", cancellationToken);
		}
	}
}
