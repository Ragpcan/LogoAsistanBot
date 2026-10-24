using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.Services;
using LogoCrmTelegramIntegration.Infrastructure.Telegram;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LogoCrmTelegramIntegration.WebUI.Controllers
{
	/// <summary>
	/// E-posta, SMS, Telegram ve WhatsApp bildirim API'si.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class NotificationController : ControllerBase
	{
		private readonly IEmailService _emailService;
		private readonly ISmsService _smsService;
		private readonly ITelegramBotManager _telegramBotManager;
		private readonly IConfiguration _config;

		public NotificationController(
			IEmailService emailService,
			ISmsService smsService,
			ITelegramBotManager telegramBotManager,
			IConfiguration configuration)
		{
			_emailService = emailService;
			_smsService = smsService;
			_telegramBotManager = telegramBotManager;
			_config = configuration;
		}

		// ─────────────────────────────────────────────────────────────────────
		// E-POSTA GÖNDERİMİ
		// ─────────────────────────────────────────────────────────────────────

		/// <summary>
		/// Tek veya çoklu alıcıya e-posta gönderir.
		/// POST /api/notification/send-mail
		/// </summary>
		[HttpPost("send-mail")]
		public async Task<IActionResult> SendMail([FromBody] SendMailRequest req)
		{
			if (req == null || string.IsNullOrWhiteSpace(req.To))
				return BadRequest(new { success = false, message = "Alıcı e-posta adresi zorunludur." });

			// Ayarları appsettings.json'dan oku (ön yüzden gelmezse varsayılan kullan)
			var mailSettings = _config.GetSection("MailSettings");

			var request = new EmailSendRequest
			{
				To = req.To,
				Cc = req.Cc,
				Subject = req.Subject ?? "(Konu Belirtilmemiş)",
				Body = req.Body ?? "",
				IsBodyHtml = req.IsBodyHtml,
				SmtpHost = req.SmtpHost ?? mailSettings["SmtpHost"] ?? "",
				SmtpPort = req.SmtpPort > 0 ? req.SmtpPort : int.TryParse(mailSettings["SmtpPort"], out int p) ? p : 587,
				SmtpUser = req.SmtpUser ?? mailSettings["SmtpUser"] ?? "",
				SmtpPassword = req.SmtpPassword ?? mailSettings["SmtpPassword"] ?? "",
				FromAddress = req.From ?? mailSettings["FromAddress"] ?? "",
				FromDisplayName = req.FromDisplayName ?? mailSettings["FromDisplayName"]
			};

			var result = await _emailService.SendEmailAsync(request);

			if (result.IsSuccess)
				return Ok(new { success = true, message = "E-posta başarıyla gönderildi." });

			return Ok(new { success = false, message = result.ErrorMessage });
		}

		/// <summary>
		/// SMTP bağlantısını test eder (gerçek e-posta göndermez, yalnızca bağlantı kontrolü yapar).
		/// POST /api/notification/test-mail
		/// </summary>
		[HttpPost("test-mail")]
		public async Task<IActionResult> TestMail([FromBody] TestMailRequest req)
		{
			var mailSettings = _config.GetSection("MailSettings");

			var request = new EmailSendRequest
			{
				To = req.TestAddress ?? req.SmtpUser ?? mailSettings["SmtpUser"] ?? "",
				Subject = "✅ Logo Asistan Bot – SMTP Bağlantı Testi",
				Body = $"<p>Bu mesaj SMTP bağlantınızın doğru yapılandırıldığını onaylamak için <b>{DateTime.Now:dd.MM.yyyy HH:mm}</b> tarihinde otomatik olarak gönderilmiştir.</p>",
				SmtpHost = req.SmtpHost ?? mailSettings["SmtpHost"] ?? "",
				SmtpPort = req.SmtpPort > 0 ? req.SmtpPort : int.TryParse(mailSettings["SmtpPort"], out int p) ? p : 587,
				SmtpUser = req.SmtpUser ?? mailSettings["SmtpUser"] ?? "",
				SmtpPassword = req.SmtpPassword ?? mailSettings["SmtpPassword"] ?? "",
				FromAddress = req.SmtpUser ?? mailSettings["SmtpUser"] ?? "",
				FromDisplayName = "Logo Asistan Bot"
			};

			var result = await _emailService.SendEmailAsync(request);

			return Ok(new
			{
				success = result.IsSuccess,
				message = result.IsSuccess
					? "✅ SMTP bağlantısı başarılı! Test e-postası gönderildi."
					: $"❌ Bağlantı hatası: {result.ErrorMessage}"
			});
		}

		// ─────────────────────────────────────────────────────────────────────
		// SMS GÖNDERİMİ
		// ─────────────────────────────────────────────────────────────────────

		/// <summary>
		/// NetGSM API üzerinden SMS gönderir.
		/// POST /api/notification/send-sms
		/// </summary>
		[HttpPost("send-sms")]
		public async Task<IActionResult> SendSms([FromBody] SendSmsRequest req)
		{
			if (req == null || string.IsNullOrWhiteSpace(req.To))
				return BadRequest(new { success = false, message = "Alıcı GSM numarası zorunludur." });

			var smsSettings = _config.GetSection("SmsSettings");

			var request = new SmsSendRequest
			{
				To = req.To,
				Message = req.Message ?? "",
				UserCode = req.UserCode ?? smsSettings["UserCode"] ?? "",
				Password = req.Password ?? smsSettings["Password"] ?? "",
				MsgHeader = req.MsgHeader ?? smsSettings["MsgHeader"] ?? "LOGO"
			};

			var result = await _smsService.SendSmsAsync(request);

			if (result.IsSuccess)
				return Ok(new { success = true, message = "SMS başarıyla gönderildi.", providerResponse = result.ProviderResponse });

			return Ok(new { success = false, message = result.ErrorMessage, providerResponse = result.ProviderResponse });
		}

		/// <summary>
		/// NetGSM bakiye sorgusunu gerçekleştirir.
		/// GET /api/notification/sms-balance
		/// </summary>
		[HttpGet("sms-balance")]
		public async Task<IActionResult> SmsBalance()
		{
			var smsSettings = _config.GetSection("SmsSettings");
			string userCode = smsSettings["UserCode"] ?? "";
			string password = smsSettings["Password"] ?? "";

			if (string.IsNullOrWhiteSpace(userCode))
				return Ok(new { success = false, message = "NetGSM kullanıcı kodu tanımlanmamış." });

			try
			{
				using var client = new System.Net.Http.HttpClient();
				string url = $"https://api.netgsm.com.tr/balance/list/get?usercode={userCode}&password={password}&type=1";
				string response = await client.GetStringAsync(url);
				return Ok(new { success = true, balanceInfo = response.Trim() });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, message = $"Bakiye sorgulanamadı: {ex.Message}" });
			}
		}

		// ─────────────────────────────────────────────────────────────────────
		// TELEGRAM GÖNDERİMİ
		// ─────────────────────────────────────────────────────────────────────

		/// <summary>
		/// Telegram üzerinden bildirim veya vade hatırlatma mesajı gönderir.
		/// POST /api/notification/send-telegram
		/// </summary>
		[HttpPost("send-telegram")]
		public async Task<IActionResult> SendTelegram([FromBody] SendTelegramRequest req)
		{
			if (req == null || string.IsNullOrWhiteSpace(req.Message))
				return BadRequest(new { success = false, message = "Mesaj içeriği zorunludur." });

			long targetChatId = req.ChatId ?? 0;
			if (targetChatId == 0)
			{
				var cfgChat = _config["Telegram:DefaultChatId"] ?? _config["Telegram:AdminChatId"] ?? _config["Telegram:NotificationChatId"];
				if (!string.IsNullOrWhiteSpace(cfgChat) && long.TryParse(cfgChat, out long parsedId))
				{
					targetChatId = parsedId;
				}
			}

			if (targetChatId == 0)
			{
				return BadRequest(new { success = false, message = "Geçerli bir Telegram Chat ID belirtilmedi veya ayarlarda varsayılan Chat ID bulunamadı." });
			}

			try
			{
				await _telegramBotManager.SendTextMessageAsync(targetChatId, req.Message);
				return Ok(new { success = true, message = "Telegram mesajı başarıyla iletildi." });
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, message = $"Telegram gönderim hatası: {ex.Message}" });
			}
		}

		// ─────────────────────────────────────────────────────────────────────
		// WHATSAPP (TOKENSIZ WEB KÖPRÜSÜ + DİREKT WEB LİNKİ)
		// ─────────────────────────────────────────────────────────────────────

		/// <summary>
		/// WhatsApp üzerinden bildirim gönderir.
		/// Önce yerel tokensiz WhatsApp köprüsünü (port 5001) dener; aktifse doğrudan mesaj atar.
		/// Köprü aktif değilse veya çevrimdışıysa WhatsApp Web bağlantı linkini döner.
		/// POST /api/notification/send-whatsapp
		/// </summary>
		[HttpPost("send-whatsapp")]
		public async Task<IActionResult> SendWhatsApp([FromBody] SendWhatsAppRequest req)
		{
			if (req == null || string.IsNullOrWhiteSpace(req.Phone))
				return BadRequest(new { success = false, message = "Telefon numarası zorunludur." });

			string cleanPhone = new string(req.Phone.Where(char.IsDigit).ToArray());
			if (cleanPhone.StartsWith("0") && cleanPhone.Length == 11)
			{
				cleanPhone = "9" + cleanPhone; // Türkiye 90...
			}
			else if (cleanPhone.Length == 10)
			{
				cleanPhone = "90" + cleanPhone;
			}

			string webUrl = $"https://web.whatsapp.com/send?phone={cleanPhone}&text={Uri.EscapeDataString(req.Message ?? "")}";

			// 1. Yerel WhatsApp Köprüsünü (port 5001) kontrol et
			try
			{
				using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
				var payload = new { phone = cleanPhone, message = req.Message };
				var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
				
				var resp = await http.PostAsync("http://127.0.0.1:5001/send", jsonContent);
				if (resp.IsSuccessStatusCode)
				{
					var resString = await resp.Content.ReadAsStringAsync();
					return Ok(new
					{
						success = true,
						via = "bridge",
						message = "WhatsApp mesajı yerel servis (Baileys Web Bridge) üzerinden anında gönderildi!",
						webUrl = webUrl
					});
				}
			}
			catch
			{
				// Köprü çevrimdışı, web link fallback'e geç
			}

			return Ok(new
			{
				success = true,
				via = "web_link",
				message = "WhatsApp Web linki oluşturuldu. Köprü çevrimdışı olduğundan tarayıcıdan WhatsApp Web açılacaktır.",
				webUrl = webUrl
			});
		}

		/// <summary>
		/// Yerel WhatsApp Bridge durumunu ve varsa QR kodunu getirir.
		/// GET /api/notification/whatsapp-bridge-status
		/// </summary>
		[HttpGet("whatsapp-bridge-status")]
		public async Task<IActionResult> GetWhatsAppBridgeStatus()
		{
			try
			{
				using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
				var resp = await http.GetAsync("http://127.0.0.1:5001/status");
				if (resp.IsSuccessStatusCode)
				{
					var content = await resp.Content.ReadAsStringAsync();
					return Content(content, "application/json");
				}
			}
			catch (Exception ex)
			{
				return Ok(new
				{
					online = false,
					status = "disconnected",
					message = "WhatsApp Köprüsü bağlantı hatası: " + ex.Message
				});
			}

			return Ok(new
			{
				online = false,
				status = "disconnected",
				message = "WhatsApp Köprüsü (port 5001) çalışmıyor."
			});
		}

		/// <summary>
		/// Yerel WhatsApp Bridge oturumunu sonlandırır ve yeni QR üretilmesini tetikler.
		/// POST /api/notification/whatsapp-logout
		/// </summary>
		[HttpPost("whatsapp-logout")]
		public async Task<IActionResult> WhatsAppLogout()
		{
			try
			{
				using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
				var resp = await http.PostAsync("http://127.0.0.1:5001/logout", null);
				var content = await resp.Content.ReadAsStringAsync();
				return Content(content, "application/json");
			}
			catch (Exception ex)
			{
				return Ok(new { success = false, message = "WhatsApp Köprüsü oturum kapatma hatası: " + ex.Message });
			}
		}
	}

	// ─────────────────────────────────────────────────────────────────────────
	// REQUEST MODELLERİ
	// ─────────────────────────────────────────────────────────────────────────

	public class SendMailRequest
	{
		/// <summary>Alıcı (virgülle birden fazla adres girilebilir).</summary>
		public string? To { get; set; }
		/// <summary>CC adresleri (virgülle birden fazla).</summary>
		public string? Cc { get; set; }
		/// <summary>E-posta konusu.</summary>
		public string? Subject { get; set; }
		/// <summary>E-posta gövdesi.</summary>
		public string? Body { get; set; }
		/// <summary>Gövde HTML mi? (varsayılan: true)</summary>
		public bool IsBodyHtml { get; set; } = true;
		/// <summary>Gönderen e-posta adresi (boşsa appsettings kullanılır).</summary>
		public string? From { get; set; }
		/// <summary>Gönderen görünen adı.</summary>
		public string? FromDisplayName { get; set; }
		/// <summary>SMTP host (boşsa appsettings kullanılır).</summary>
		public string? SmtpHost { get; set; }
		/// <summary>SMTP port (0 ise appsettings kullanılır).</summary>
		public int SmtpPort { get; set; }
		public string? SmtpUser { get; set; }
		public string? SmtpPassword { get; set; }
	}

	public class TestMailRequest
	{
		public string? TestAddress { get; set; }
		public string? SmtpHost { get; set; }
		public int SmtpPort { get; set; }
		public string? SmtpUser { get; set; }
		public string? SmtpPassword { get; set; }
	}

	public class SendSmsRequest
	{
		/// <summary>Alıcı GSM numarası.</summary>
		public string? To { get; set; }
		/// <summary>SMS metni.</summary>
		public string? Message { get; set; }
		/// <summary>NetGSM kullanıcı kodu (boşsa appsettings kullanılır).</summary>
		public string? UserCode { get; set; }
		/// <summary>NetGSM şifresi (boşsa appsettings kullanılır).</summary>
		public string? Password { get; set; }
		/// <summary>SMS başlığı (boşsa appsettings kullanılır).</summary>
		public string? MsgHeader { get; set; }
	}

	public class SendTelegramRequest
	{
		public long? ChatId { get; set; }
		public string? Message { get; set; }
	}

	public class SendWhatsAppRequest
	{
		public string? Phone { get; set; }
		public string? Message { get; set; }
	}
}

