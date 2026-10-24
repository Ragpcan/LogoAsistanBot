using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class EBelgeDispatcherService : IEBelgeDispatcherService
{
	private readonly IConfiguration _config;

	private readonly ILogger<EBelgeDispatcherService> _logger;

	private readonly ILogoErpRepository _logoErpRepo;

	private readonly IGibSchematronValidator _schematronValidator;

	private readonly IEBelgeWebServiceClient _wsClient;

	private readonly IXsltDispatchService _xsltDispatchService;

	private readonly IXsltInvoiceService _xsltInvoiceService;

	public EBelgeDispatcherService(IConfiguration config, ILogger<EBelgeDispatcherService> logger, ILogoErpRepository logoErpRepo, IGibSchematronValidator schematronValidator, IEBelgeWebServiceClient wsClient, IXsltDispatchService xsltDispatchService, IXsltInvoiceService xsltInvoiceService)
	{
		_config = config;
		_logger = logger;
		_logoErpRepo = logoErpRepo;
		_schematronValidator = schematronValidator;
		_wsClient = wsClient;
		_xsltDispatchService = xsltDispatchService;
		_xsltInvoiceService = xsltInvoiceService;
	}

	public async Task<EBelgeDispatchResult> DispatchDocumentAsync(EBelgeDispatchRequest request, CancellationToken ct = default(CancellationToken))
	{
		_logger.LogInformation("e-Belge gönderim pipeline başlatıldı. FicheNo: {FicheNo}, DocType: {DocType}", request.FicheNo, request.DocumentType);
		EBelgeDispatchResult result = new EBelgeDispatchResult();
		try
		{
			string ublXml;
			try
			{
				ublXml = await BuildUblXmlAsync(request, ct);
				_logger.LogInformation("UBL-TR XML üretildi. FicheNo: {FicheNo}, Boyut: {Size} karakter", request.FicheNo, ublXml.Length);
			}
			catch (Exception ex)
			{
				result.IsSuccess = false;
				result.ErrorMessage = "UBL-TR XML üretilemedi: " + ex.Message;
				_logger.LogError(ex, "UBL-TR XML üretim hatası.");
				return result;
			}
			if (_schematronValidator.IsSchematronAvailable(request.DocumentType))
			{
				SchematronResult schematronResult = await _schematronValidator.ValidateAsync(ublXml, request.DocumentType, ct);
				result.ValidationErrors = schematronResult.Errors;
				if (!schematronResult.IsValid)
				{
					string text = string.Join("; ", from e in schematronResult.Errors
						where e.Severity == "error"
						select "[" + e.RuleId + "] " + e.Message);
					_logger.LogWarning("GİB Şematron doğrulaması başarısız. FicheNo: {FicheNo}. Hatalar: {Errors}", request.FicheNo, text);
					result.IsSuccess = false;
					result.ErrorMessage = $"GİB Şematron doğrulaması başarısız. {schematronResult.Errors.Count((SchematronValidationError e) => e.Severity == "error")} kural ihlali.";
					return result;
				}
				_logger.LogInformation("GİB Şematron doğrulaması başarılı. FicheNo: {FicheNo}", request.FicheNo);
			}
			else
			{
				_logger.LogWarning("Şematron dosyası bulunamadı, doğrulama atlandı. DocType: {DocType}", request.DocumentType);
			}
			string base64Payload = ZipAndBase64(ublXml, request.FicheNo + ".xml");
			_logger.LogDebug("ZIP+Base64 payload hazırlandı. Boyut: {Size} karakter", base64Payload.Length);
			if (_config.GetValue("EBelgeWebService:TestMode", defaultValue: true))
			{
				_logger.LogWarning("TEST MODU aktif — belge GİB'e gönderilmedi. FicheNo: {FicheNo}", request.FicheNo);
				result.IsSuccess = true;
				result.ZarfId = $"TEST-{Guid.NewGuid():N}";
				result.StatusCode = "TEST";
				result.StatusDescription = "Test modunda başarılı doğrulama tamamlandı. Gerçek gönderim yapılmadı.";
				return result;
			}
			string sessionId;
			try
			{
				sessionId = await _wsClient.LoginAsync(ct);
				_logger.LogInformation("e-Belge servisine başarıyla giriş yapıldı.");
			}
			catch (Exception ex2)
			{
				result.IsSuccess = false;
				result.ErrorMessage = "e-Belge servisine giriş yapılamadı: " + ex2.Message;
				_logger.LogError(ex2, "e-Belge Login hatası.");
				return result;
			}
			try
			{
				string text2 = (result.ZarfId = await _wsClient.SendDocumentAsync(sessionId, base64Payload, request.DocumentType, ct));
				result.IsSuccess = true;
				result.StatusCode = "SENT";
				result.StatusDescription = "Belge başarıyla gönderildi.";
				_logger.LogInformation("Belge başarıyla gönderildi. FicheNo: {FicheNo}, ZarfId: {ZarfId}", request.FicheNo, text2);
			}
			catch (Exception ex3)
			{
				result.IsSuccess = false;
				result.ErrorMessage = "Belge gönderimi başarısız: " + ex3.Message;
				_logger.LogError(ex3, "e-Belge gönderim hatası.");
				return result;
			}
		}
		catch (Exception ex4)
		{
			result.IsSuccess = false;
			result.ErrorMessage = "Pipeline beklenmeyen hata: " + ex4.Message;
			_logger.LogError(ex4, "e-Belge pipeline beklenmeyen hata. FicheNo: {FicheNo}", request.FicheNo);
		}
		return result;
	}

	public async Task<EBelgeDispatchResult> PollDocumentStatusAsync(string zarfId, string documentType, CancellationToken ct = default(CancellationToken))
	{
		_ = 1;
		try
		{
			string sessionId = await _wsClient.LoginAsync(ct);
			var (text, text2) = await _wsClient.GetDocumentStatusAsync(sessionId, zarfId, ct);
			_logger.LogInformation("Belge durum sorgusu tamamlandı. ZarfId: {ZarfId}, Status: {Code} - {Desc}", zarfId, text, text2);
			return new EBelgeDispatchResult
			{
				IsSuccess = true,
				ZarfId = zarfId,
				StatusCode = text,
				StatusDescription = text2
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Belge durum sorgusu başarısız. ZarfId: {ZarfId}", zarfId);
			return new EBelgeDispatchResult
			{
				IsSuccess = false,
				ZarfId = zarfId,
				ErrorMessage = ex.Message
			};
		}
	}

	private async Task<string> BuildUblXmlAsync(EBelgeDispatchRequest request, CancellationToken ct)
	{
		string firmNo = request.FirmNo ?? _config["LogoSystem:FirmNo"] ?? "001";
		string periodNo = request.PeriodNo ?? _config["LogoSystem:PeriodNo"] ?? "01";
		if (request.DocumentType.Equals("DESPATCHADVICE", StringComparison.OrdinalIgnoreCase))
		{
			LogoErpDispatchDto dispatch = (await _logoErpRepo.GetDispatchDetailAsync(request.FicheNo, firmNo, periodNo, ct)) ?? throw new InvalidOperationException("İrsaliye bulunamadı: " + request.FicheNo);
			return await _xsltDispatchService.BuildUblTrXmlPublicAsync(dispatch, null, ct);
		}
		if (request.DocumentType.Equals("INVOICE", StringComparison.OrdinalIgnoreCase) || request.DocumentType.Equals("EARSIV", StringComparison.OrdinalIgnoreCase))
		{
			LogoErpDispatchDto dispatch2 = (await _logoErpRepo.GetDispatchDetailAsync(request.FicheNo, firmNo, periodNo, ct)) ?? throw new InvalidOperationException("Fatura bulunamadı: " + request.FicheNo);
			return await _xsltDispatchService.BuildUblTrXmlPublicAsync(dispatch2, null, ct);
		}
		throw new ArgumentException("Bilinmeyen belge türü: " + request.DocumentType);
	}

	private static string ZipAndBase64(string xmlContent, string fileNameInZip)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
		{
			using Stream stream = zipArchive.CreateEntry(fileNameInZip, CompressionLevel.Optimal).Open();
			using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
			streamWriter.Write(xmlContent);
		}
		memoryStream.Position = 0L;
		return Convert.ToBase64String(memoryStream.ToArray());
	}
}
