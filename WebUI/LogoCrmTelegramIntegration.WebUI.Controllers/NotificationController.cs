using System;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LogoCrmTelegramIntegration.WebUI.Controllers
{
	/// <summary>
	/// E-posta ve SMS gönderim API'si.
	/// Ön yüzden tetiklenen bildirim isteklerini karşılar.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class NotificationController : ControllerBase
	{
		private readonly IEmailService _emailService;
		private readonly ISmsService _smsService;
		private readonly IConfiguration _config;

		public NotificationController(
			IEmailService emailService,
			ISmsService smsService,
			IConfiguration configuration)
		{
			_emailService = emailService;
			_smsService = smsService;
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
}
