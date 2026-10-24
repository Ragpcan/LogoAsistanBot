using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class EBelgeWebServiceClient : IEBelgeWebServiceClient
{
	private readonly IConfiguration _config;

	private readonly ILogger<EBelgeWebServiceClient> _logger;

	private readonly HttpClient _httpClient;

	private string? _cachedSessionId;

	private DateTime _sessionExpiresAt = DateTime.MinValue;

	public EBelgeWebServiceClient(IConfiguration config, ILogger<EBelgeWebServiceClient> logger, IHttpClientFactory httpClientFactory)
	{
		_config = config;
		_logger = logger;
		_httpClient = httpClientFactory.CreateClient("EBelgeWs");
	}

	private EBelgeWebServiceSettings ReadSettings()
	{
		EBelgeWebServiceSettings eBelgeWebServiceSettings = new EBelgeWebServiceSettings();
		_config.GetSection("EBelgeWebService").Bind(eBelgeWebServiceSettings);
		return eBelgeWebServiceSettings;
	}

	public async Task<string> LoginAsync(CancellationToken ct = default(CancellationToken))
	{
		if (_cachedSessionId != null && DateTime.UtcNow < _sessionExpiresAt)
		{
			_logger.LogDebug("e-Belge session cache'ten kullanılıyor. Geçerlilik: {Exp}", _sessionExpiresAt);
			return _cachedSessionId;
		}
		EBelgeWebServiceSettings settings = ReadSettings();
		if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
		{
			throw new InvalidOperationException("EBelgeWebService:ServiceUrl appsettings.json'da tanımlı değil.");
		}
		_logger.LogInformation("e-Belge servisine login isteği gönderiliyor. URL: {Url}", settings.ServiceUrl);
		await Task.Delay(50, ct);
		string text = $"STUB-SESSION-{Guid.NewGuid():N}";
		int num = ((settings.SessionTimeoutMinutes > 0) ? settings.SessionTimeoutMinutes : 30);
		_cachedSessionId = text;
		_sessionExpiresAt = DateTime.UtcNow.AddMinutes(num);
		_logger.LogWarning("e-Belge Login STUB modu aktif — gerçek servis bağlantısı henüz yapılandırılmamış. SessionId: {SessionId}", text);
		return text;
	}

	public async Task<string> SendDocumentAsync(string sessionId, string base64ZippedXml, string documentType, CancellationToken ct = default(CancellationToken))
	{
		if (ReadSettings().TestMode)
		{
			_logger.LogWarning("e-Belge TEST MODU aktif — belge GİB'e gönderilmeyecek. DocType: {DocType}", documentType);
			await Task.Delay(100, ct);
			return $"STUB-ZARF-{Guid.NewGuid():N}";
		}
		throw new NotImplementedException("Gerçek e-Belge gönderim servisi henüz yapılandırılmamış. appsettings.json'da EBelgeWebService:TestMode = true olarak bırakın veya entegratör API'sini uygulayın.");
	}

	public async Task<(string StatusCode, string StatusDescription)> GetDocumentStatusAsync(string sessionId, string zarfId, CancellationToken ct = default(CancellationToken))
	{
		_logger.LogInformation("Belge durumu sorgulanıyor. ZarfId: {ZarfId}", zarfId);
		await Task.Delay(50, ct);
		return (StatusCode: "1300", StatusDescription: "Belge başarıyla işlendi (TEST MODU)");
	}

	public async Task<bool> PingAsync(CancellationToken ct = default(CancellationToken))
	{
		EBelgeWebServiceSettings settings = ReadSettings();
		if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
		{
			_logger.LogWarning("e-Belge Ping: ServiceUrl tanımlı değil.");
			return false;
		}
		try
		{
			using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			cts.CancelAfter(TimeSpan.FromSeconds(8L));
			HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(settings.ServiceUrl, cts.Token);
			_logger.LogInformation("e-Belge Ping sonucu: {StatusCode} — {Url}", (int)httpResponseMessage.StatusCode, settings.ServiceUrl);
			return httpResponseMessage.StatusCode < HttpStatusCode.InternalServerError;
		}
		catch (Exception exception)
		{
			_logger.LogWarning(exception, "e-Belge Ping başarısız. URL: {Url}", settings.ServiceUrl);
			return false;
		}
	}
}
