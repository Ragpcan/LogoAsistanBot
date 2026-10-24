using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace LogoCrmTelegramIntegration.Application.Services
{
	/// <summary>
	/// SMS gönderimi için servis arayüzü.
	/// </summary>
	public interface ISmsService
	{
		/// <summary>
		/// Belirtilen numaraya SMS gönderir.
		/// </summary>
		Task<SmsSendResult> SendSmsAsync(SmsSendRequest request);
	}

	/// <summary>SMS gönderim isteği modeli.</summary>
	public class SmsSendRequest
	{
		/// <summary>Alıcı GSM numarası (ör: 05XXXXXXXXX veya 5XXXXXXXXX).</summary>
		public string To { get; set; } = string.Empty;

		/// <summary>Gönderilecek mesaj metni.</summary>
		public string Message { get; set; } = string.Empty;

		/// <summary>NetGSM kullanıcı kodu.</summary>
		public string UserCode { get; set; } = string.Empty;

		/// <summary>NetGSM şifresi.</summary>
		public string Password { get; set; } = string.Empty;

		/// <summary>SMS başlığı (gönderen adı, ör: FirmaAdı).</summary>
		public string MsgHeader { get; set; } = string.Empty;
	}

	/// <summary>SMS gönderim sonucu.</summary>
	public class SmsSendResult
	{
		public bool IsSuccess { get; set; }
		public string? ErrorMessage { get; set; }
		public string? ProviderResponse { get; set; }

		public static SmsSendResult Success(string? response = null) =>
			new() { IsSuccess = true, ProviderResponse = response };
		public static SmsSendResult Fail(string msg, string? response = null) =>
			new() { IsSuccess = false, ErrorMessage = msg, ProviderResponse = response };
	}

	/// <summary>
	/// NetGSM API üzerinden SMS gönderir.
	/// API Dokümantasyonu: https://www.netgsm.com.tr/dokuman/
	/// Hata kodları:
	///   00 = Başarılı
	///   20 = Mesaj metni hatalı
	///   30 = Geçersiz kullanıcı adı/şifre
	///   40 = Mesaj başlığı tanımlı değil
	///   70 = Hatalı sorgulama (eksik parametre)
	/// </summary>
	public class NetGsmSmsService : ISmsService
	{
		private readonly HttpClient _httpClient;
		private const string NetGsmApiUrl = "https://api.netgsm.com.tr/sms/send/get";

		public NetGsmSmsService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<SmsSendResult> SendSmsAsync(SmsSendRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.To))
				return SmsSendResult.Fail("Alıcı GSM numarası boş olamaz.");
			if (string.IsNullOrWhiteSpace(request.Message))
				return SmsSendResult.Fail("Mesaj metni boş olamaz.");
			if (string.IsNullOrWhiteSpace(request.UserCode))
				return SmsSendResult.Fail("NetGSM kullanıcı kodu tanımlanmamış.");

			// Türkçe karakter desteği için ISO-8859-9 yerine URL encoding kullan
			string encodedMessage = HttpUtility.UrlEncode(request.Message, System.Text.Encoding.GetEncoding("ISO-8859-9"));

			// Numarayı normalleştir (başındaki 0'ı kaldır veya +90'ı temizle)
			string gsmNo = request.To.TrimStart('+').TrimStart();
			if (gsmNo.StartsWith("90") && gsmNo.Length == 12) gsmNo = gsmNo.Substring(2);
			if (gsmNo.StartsWith("0") && gsmNo.Length == 11) gsmNo = gsmNo.Substring(1);

			string url = $"{NetGsmApiUrl}?usercode={request.UserCode}&password={request.Password}" +
			             $"&gsmno={gsmNo}&message={encodedMessage}" +
			             $"&msgheader={HttpUtility.UrlEncode(request.MsgHeader)}" +
			             $"&dil=TR";

			try
			{
				var response = await _httpClient.GetAsync(url);
				string responseText = await response.Content.ReadAsStringAsync();
				responseText = responseText.Trim();

				// NetGSM cevap kodu yorumlama
				if (responseText.StartsWith("00") || responseText == "0" || response.IsSuccessStatusCode)
				{
					return SmsSendResult.Success(responseText);
				}

				string hataAciklamasi = responseText switch
				{
					"20" => "Mesaj metni hatalı veya boş.",
					"30" => "Geçersiz kullanıcı adı veya şifre.",
					"40" => "Mesaj başlığı (header) tanımlı değil.",
					"70" => "Hatalı sorgulama, eksik parametre var.",
					"80" => "Kullanıcı hesabı engellendi.",
					_ => $"NetGSM hata kodu: {responseText}"
				};

				return SmsSendResult.Fail(hataAciklamasi, responseText);
			}
			catch (Exception ex)
			{
				return SmsSendResult.Fail($"SMS gönderilemedi: {ex.Message}");
			}
		}
	}
}
