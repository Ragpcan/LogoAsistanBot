using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LogoCrmTelegramIntegration.Application.Services
{
	/// <summary>
	/// E-posta gönderimi için servis arayüzü.
	/// </summary>
	public interface IEmailService
	{
		/// <summary>
		/// Tek veya çoklu alıcıya HTML destekli e-posta gönderir.
		/// </summary>
		Task<EmailSendResult> SendEmailAsync(EmailSendRequest request);
	}

	/// <summary>E-posta gönderim isteği modeli.</summary>
	public class EmailSendRequest
	{
		/// <summary>Alıcı e-posta adresleri (virgülle ayrılmış veya tek adres).</summary>
		public string To { get; set; } = string.Empty;

		/// <summary>CC adresleri (isteğe bağlı, virgülle ayrılmış).</summary>
		public string? Cc { get; set; }

		/// <summary>Konu.</summary>
		public string Subject { get; set; } = string.Empty;

		/// <summary>E-posta gövdesi (HTML destekli).</summary>
		public string Body { get; set; } = string.Empty;

		/// <summary>SMTP sunucu adresi (ör: smtp.gmail.com).</summary>
		public string SmtpHost { get; set; } = string.Empty;

		/// <summary>SMTP port (587 = STARTTLS, 465 = SSL).</summary>
		public int SmtpPort { get; set; } = 587;

		/// <summary>SMTP kullanıcı adı.</summary>
		public string SmtpUser { get; set; } = string.Empty;

		/// <summary>SMTP şifresi.</summary>
		public string SmtpPassword { get; set; } = string.Empty;

		/// <summary>Gönderen e-posta adresi.</summary>
		public string FromAddress { get; set; } = string.Empty;

		/// <summary>Gönderen görünen adı (ör: Firma Adı).</summary>
		public string? FromDisplayName { get; set; }

		/// <summary>Ek dosya bayt dizisi (isteğe bağlı).</summary>
		public byte[]? AttachmentBytes { get; set; }

		/// <summary>Ek dosya adı (ör: Ekstre.pdf).</summary>
		public string? AttachmentName { get; set; }

		/// <summary>Gövdenin HTML olup olmadığı (varsayılan: true).</summary>
		public bool IsBodyHtml { get; set; } = true;
	}

	/// <summary>E-posta gönderim sonucu.</summary>
	public class EmailSendResult
	{
		public bool IsSuccess { get; set; }
		public string? ErrorMessage { get; set; }

		public static EmailSendResult Success() => new() { IsSuccess = true };
		public static EmailSendResult Fail(string msg) => new() { IsSuccess = false, ErrorMessage = msg };
	}

	/// <summary>
	/// SMTP protokolü ile profesyonel e-posta gönderimi sağlar.
	/// Hem Gmail (587/STARTTLS) hem de Office 365 / özel SMTP sunucuları desteklenir.
	/// </summary>
	public class SmtpEmailService : IEmailService
	{
		public async Task<EmailSendResult> SendEmailAsync(EmailSendRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.To))
				return EmailSendResult.Fail("Alıcı e-posta adresi boş olamaz.");
			if (string.IsNullOrWhiteSpace(request.SmtpHost))
				return EmailSendResult.Fail("SMTP sunucu adresi tanımlanmamış.");
			if (string.IsNullOrWhiteSpace(request.SmtpUser))
				return EmailSendResult.Fail("SMTP kullanıcı adı tanımlanmamış.");

			try
			{
				using var client = new SmtpClient(request.SmtpHost, request.SmtpPort)
				{
					Credentials = new NetworkCredential(request.SmtpUser, request.SmtpPassword),
					EnableSsl = request.SmtpPort == 465 || request.SmtpPort == 587,
					Timeout = 30_000 // 30 saniye zaman aşımı
				};

				var fromAddress = string.IsNullOrWhiteSpace(request.FromDisplayName)
					? new MailAddress(request.FromAddress)
					: new MailAddress(request.FromAddress, request.FromDisplayName, Encoding.UTF8);

				using var mail = new MailMessage
				{
					From = fromAddress,
					Subject = request.Subject,
					Body = request.Body,
					IsBodyHtml = request.IsBodyHtml,
					SubjectEncoding = Encoding.UTF8,
					BodyEncoding = Encoding.UTF8
				};

				// Birden fazla alıcı desteği
				foreach (var addr in request.To.Split(',', ';', StringSplitOptions.RemoveEmptyEntries))
				{
					var a = addr.Trim();
					if (!string.IsNullOrEmpty(a)) mail.To.Add(a);
				}

				// CC desteği
				if (!string.IsNullOrWhiteSpace(request.Cc))
				{
					foreach (var addr in request.Cc.Split(',', ';', StringSplitOptions.RemoveEmptyEntries))
					{
						var a = addr.Trim();
						if (!string.IsNullOrEmpty(a)) mail.CC.Add(a);
					}
				}

				// Ek dosya
				if (request.AttachmentBytes != null && !string.IsNullOrWhiteSpace(request.AttachmentName))
				{
					var stream = new MemoryStream(request.AttachmentBytes);
					mail.Attachments.Add(new Attachment(stream, request.AttachmentName));
				}

				await client.SendMailAsync(mail);
				return EmailSendResult.Success();
			}
			catch (Exception ex)
			{
				return EmailSendResult.Fail($"E-posta gönderilemedi: {ex.Message}");
			}
		}
	}
}
