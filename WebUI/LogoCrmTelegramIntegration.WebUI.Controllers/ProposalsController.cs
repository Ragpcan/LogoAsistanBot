using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LogoCrmTelegramIntegration.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProposalsController : ControllerBase
{
	private readonly IProposalLogStore _logStore;

	private readonly IProposalRepository _proposalRepository;

	private readonly ILogoErpRepository _logoErpRepository;

	private readonly IProposalAppService _proposalAppService;

	private readonly IXsltDispatchService _xsltDispatchService;

	private readonly IXsltInvoiceService _xsltInvoiceService;

	private readonly IConfiguration _configuration;

	public ProposalsController(IProposalLogStore logStore, IProposalRepository proposalRepository, ILogoErpRepository logoErpRepository, IProposalAppService proposalAppService, IXsltDispatchService xsltDispatchService, IXsltInvoiceService xsltInvoiceService, IConfiguration configuration)
	{
		_logStore = logStore;
		_proposalRepository = proposalRepository;
		_logoErpRepository = logoErpRepository;
		_proposalAppService = proposalAppService;
		_xsltDispatchService = xsltDispatchService;
		_xsltInvoiceService = xsltInvoiceService;
		_configuration = configuration;
	}

	[HttpGet("logs")]
	public IActionResult GetLogs([FromQuery] int count = 50)
	{
		IReadOnlyList<ProposalLogItem> logs = _logStore.GetLogs(count);
		return Ok(logs);
	}

	[HttpPost("clear-logs")]
	public IActionResult ClearLogs()
	{
		_logStore.ClearLogs();
		return Ok(new
		{
			isSuccess = true,
			message = "Günlük kayıtları temizlendi."
		});
	}

	[HttpGet("preview-dispatch-xslt")]
	public async Task<IActionResult> PreviewDispatchXslt([FromQuery] string ficheNo, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		try
		{
			LogoErpDispatchDto logoErpDispatchDto = await _logoErpRepository.GetDispatchDetailAsync(ficheNo, firmNo, periodNo, ct);
			if (logoErpDispatchDto == null)
			{
				logoErpDispatchDto = new LogoErpDispatchDto
				{
					FicheNo = ficheNo,
					Date = DateTime.Now,
					FirmCode = "RAGIP",
					FirmName = "RAGIP MÜŞTERİ HESABI A.Ş.",
					Description = "Logo Web ERP e-İrsaliye XSLT Tasarım Testi",
					GrossTotal = 15000m,
					TotalVat = 3000m,
					NetTotal = 18000m,
					Lines = new List<LogoErpDispatchLineDto>
					{
						new LogoErpDispatchLineDto
						{
							ItemCode = "A.001.01",
							ItemName = "Endüstriyel Hammadde",
							Amount = 20m,
							UnitName = "ADET",
							UnitPrice = 150m,
							LineTotal = 3000m,
							VatRate = 20.0
						},
						new LogoErpDispatchLineDto
						{
							ItemCode = "B.002.05",
							ItemName = "Yedek Parça Rulman",
							Amount = 20m,
							UnitName = "ADET",
							UnitPrice = 600m,
							LineTotal = 12000m,
							VatRate = 20.0
						}
					}
				};
			}
			return Content(await _xsltDispatchService.RenderDispatchXsltHtmlAsync(logoErpDispatchDto, null, ct), "text/html; charset=utf-8");
		}
		catch (Exception ex)
		{
			return Content("<div style='color:red; padding:1rem;'>XSLT e-İrsaliye Şablonu İşlenirken Hata Oluştu: " + ex.Message + "</div>", "text/html; charset=utf-8");
		}
	}

	[HttpGet("preview-invoice-xslt")]
	public async Task<IActionResult> PreviewInvoiceXslt([FromQuery] string ficheNo, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		try
		{
			LogoErpDispatchDto logoErpDispatchDto = await _logoErpRepository.GetDispatchDetailAsync(ficheNo, firmNo, periodNo, ct);
			if (logoErpDispatchDto == null)
			{
				logoErpDispatchDto = new LogoErpDispatchDto
				{
					FicheNo = ficheNo,
					Date = DateTime.Now,
					FirmCode = "MUSTERI01",
					FirmName = "MÜŞTERİ01 SAN. TİC. LTD. ŞTİ.",
					Description = "Logo Web ERP e-Fatura XSLT Tasarım Testi",
					GrossTotal = 12450m,
					TotalVat = 2490m,
					NetTotal = 14940m,
					Lines = new List<LogoErpDispatchLineDto>
					{
						new LogoErpDispatchLineDto
						{
							ItemCode = "A.001.01",
							ItemName = "Endüstriyel Hammadde",
							Amount = 15m,
							UnitName = "ADET",
							UnitPrice = 150m,
							LineTotal = 2250m,
							VatRate = 20.0
						},
						new LogoErpDispatchLineDto
						{
							ItemCode = "B.002.05",
							ItemName = "Yedek Parça Rulman",
							Amount = 17m,
							UnitName = "ADET",
							UnitPrice = 600m,
							LineTotal = 10200m,
							VatRate = 20.0
						}
					}
				};
			}
			return Content(await _xsltInvoiceService.RenderInvoiceXsltHtmlAsync(logoErpDispatchDto, null, ct), "text/html; charset=utf-8");
		}
		catch (Exception ex)
		{
			return Content("<div style='color:red; padding:1rem;'>XSLT e-Fatura Şablonu İşlenirken Hata Oluştu: " + ex.Message + "</div>", "text/html; charset=utf-8");
		}
	}

	[HttpGet("company-info")]
	public async Task<IActionResult> GetCompanyInfo(CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		return Ok((await _logoErpRepository.GetCompanyInfoAsync(firmNo, ct)) ?? new LogoCompanyInfoDto
		{
			Name = (_configuration["LogoSystem:CompanyName"] ?? "LOGO ERP ŞİRKETİ"),
			Title = (_configuration["LogoSystem:CompanyTitle"] ?? "LOGO ERP ŞİRKETİ A.Ş."),
			TaxOffice = "Kadıköy V.D.",
			TaxNr = "1234567890",
			Address = "İstanbul, Türkiye"
		});
	}

	[HttpGet("document-detail")]
	public async Task<IActionResult> GetDocumentDetail([FromQuery] string ficheNo, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		LogoErpDispatchDto logoErpDispatchDto = await _logoErpRepository.GetDispatchDetailAsync(ficheNo, firmNo, periodNo, ct);
		if (logoErpDispatchDto == null)
		{
			return NotFound(new
			{
				message = "'" + ficheNo + "' fiş numaralı belge bulunamadı."
			});
		}
		return Ok(logoErpDispatchDto);
	}

	[HttpGet("firms")]
	public async Task<IActionResult> GetFirms([FromQuery] string? listType = "ALL", [FromQuery] string? searchTerm = null, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		try
		{
			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				return Ok(await _logoErpRepository.SearchFirmsAsync(searchTerm, firmNo, ct));
			}
			return Ok(await _logoErpRepository.GetFirmListAsync(listType ?? "ALL", firmNo, periodNo, ct));
		}
		catch
		{
			List<LogoErpFirmBalanceDto> value = new List<LogoErpFirmBalanceDto>
			{
				new LogoErpFirmBalanceDto
				{
					FirmCode = "RAGIP",
					FirmName = "RAGIP MÜŞTERİ HESABI A.Ş.",
					BalanceTL = 14500.00m,
					NetBalanceLocal = 14500.00m
				},
				new LogoErpFirmBalanceDto
				{
					FirmCode = "MUSTERI01",
					FirmName = "MÜŞTERİ01 SAN. TİC. LTD. ŞTİ.",
					BalanceTL = -3250.00m,
					NetBalanceLocal = -3250.00m
				},
				new LogoErpFirmBalanceDto
				{
					FirmCode = "TEKNOLOJI01",
					FirmName = "TEKNOLOJİ YAZILIM LTD. ŞTİ.",
					BalanceTL = 0m,
					NetBalanceLocal = 0m
				}
			};
			return Ok(value);
		}
	}

	[HttpGet("sales-orders")]
	public async Task<IActionResult> GetSalesOrders([FromQuery] string? firmCode = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int limit = 50, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		try
		{
			return Ok(await _logoErpRepository.GetOrdersAsync(firmCode, startDate, endDate, firmNo, periodNo, limit, ct));
		}
		catch
		{
			return Ok(new List<LogoErpOpenOrderSummaryDto>());
		}
	}

	[HttpGet("sales-dispatches")]
	public async Task<IActionResult> GetSalesDispatches([FromQuery] string? firmCode = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int limit = 50, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		try
		{
			return Ok(await _logoErpRepository.GetDispatchesAsync(firmCode, startDate, endDate, firmNo, periodNo, limit, ct));
		}
		catch
		{
			return Ok(new List<LogoErpOpenOrderSummaryDto>());
		}
	}

	[HttpGet("sales-invoices")]
	public async Task<IActionResult> GetSalesInvoices([FromQuery] string? firmCode = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int limit = 50, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		try
		{
			return Ok(await _logoErpRepository.GetInvoicesAsync(firmCode, startDate, endDate, firmNo, periodNo, limit, ct));
		}
		catch
		{
			return Ok(new List<LogoErpOpenOrderSummaryDto>());
		}
	}

	[HttpGet("purchase-orders")]
	public async Task<IActionResult> GetPurchaseOrders([FromQuery] string? firmCode = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int limit = 50, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		try
		{
			return Ok(await _logoErpRepository.GetPurchaseOrdersAsync(firmCode, startDate, endDate, firmNo, periodNo, limit, ct));
		}
		catch
		{
			return Ok(new List<LogoErpOpenOrderSummaryDto>());
		}
	}

	[HttpGet("purchase-dispatches")]
	public async Task<IActionResult> GetPurchaseDispatches([FromQuery] string? firmCode = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int limit = 50, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		try
		{
			return Ok(await _logoErpRepository.GetPurchaseDispatchesAsync(firmCode, startDate, endDate, firmNo, periodNo, limit, ct));
		}
		catch
		{
			return Ok(new List<LogoErpOpenOrderSummaryDto>());
		}
	}

	[HttpGet("purchase-invoices")]
	public async Task<IActionResult> GetPurchaseInvoices([FromQuery] string? firmCode = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int limit = 50, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		try
		{
			return Ok(await _logoErpRepository.GetPurchaseInvoicesAsync(firmCode, startDate, endDate, firmNo, periodNo, limit, ct));
		}
		catch
		{
			return Ok(new List<LogoErpOpenOrderSummaryDto>());
		}
	}

	[HttpGet("stocks")]
	public async Task<IActionResult> GetStocks([FromQuery] string? searchTerm = null, [FromQuery] int warehouseNo = 0, CancellationToken ct = default(CancellationToken))
	{
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		try
		{
			return Ok(await _logoErpRepository.SearchItemStockAsync(searchTerm ?? "", firmNo, periodNo, warehouseNo, ct));
		}
		catch
		{
			List<LogoErpItemStockDto> value = new List<LogoErpItemStockDto>
			{
				new LogoErpItemStockDto
				{
					ItemCode = "A.001.01",
					ItemName = "Endüstriyel Hammadde",
					GroupCode = "HAMMADDE",
					TotalStock = 250m,
					UnitName = "ADET",
					UnitPrice = 150.00m
				},
				new LogoErpItemStockDto
				{
					ItemCode = "B.002.05",
					ItemName = "Yedek Parça Rulman",
					GroupCode = "YEDEK PARÇA",
					TotalStock = 15m,
					UnitName = "ADET",
					UnitPrice = 420.00m
				}
			};
			return Ok(value);
		}
	}

	[HttpPost("lookup")]
	public async Task<IActionResult> LookupCrmData([FromBody] LookupRequest request)
	{
		string text = _configuration["LogoSystem:TargetSystem"] ?? "CRM";
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		if (text.Equals("ERP", StringComparison.OrdinalIgnoreCase))
		{
			int? clientRef = null;
			LogoErpItemDetails itemDetails = null;
			if (!string.IsNullOrWhiteSpace(request.FirmCode))
			{
				clientRef = await _logoErpRepository.GetFirmLogicalRefAsync(request.FirmCode, firmNo);
			}
			if (!string.IsNullOrWhiteSpace(request.ProductCode))
			{
				itemDetails = await _logoErpRepository.GetItemDetailsAsync(request.ProductCode, firmNo);
			}
			return Ok(new
			{
				targetSystem = "ERP (Tiger / Go3)",
				firmCode = request.FirmCode,
				firmFound = clientRef.HasValue,
				firmOid = (clientRef?.ToString() ?? "BULUNAMADI"),
				productCode = request.ProductCode,
				productFound = (itemDetails != null),
				productOid = ((itemDetails != null) ? $"LOGICALREF: {itemDetails.ItemLogicalRef}, UNITREF: {itemDetails.UnitRef}, KDV: %{itemDetails.Vat}" : "BULUNAMADI")
			});
		}
		Guid? firmOid = null;
		Guid? productOid = null;
		if (!string.IsNullOrWhiteSpace(request.FirmCode))
		{
			firmOid = await _proposalRepository.GetFirmOidByCodeAsync(request.FirmCode);
		}
		if (!string.IsNullOrWhiteSpace(request.ProductCode))
		{
			productOid = await _proposalRepository.GetProductOidByCodeAsync(request.ProductCode);
		}
		return Ok(new
		{
			targetSystem = "CRM",
			firmCode = request.FirmCode,
			firmFound = firmOid.HasValue,
			firmOid = (firmOid?.ToString() ?? "BULUNAMADI"),
			productCode = request.ProductCode,
			productFound = productOid.HasValue,
			productOid = (productOid?.ToString() ?? "BULUNAMADI")
		});
	}

	[HttpPost("create-test-proposal")]
	public async Task<IActionResult> CreateTestProposal([FromBody] CreateTestProposalRequest request)
	{
		return Ok(await _proposalAppService.ProcessTelegramMessageAsync(request.MessageText));
	}

	[HttpPost("send-gib")]
	public async Task<IActionResult> SendDocumentToGib([FromBody] SendGibRequest request, [FromServices] IEBelgeDispatcherService dispatcherService, CancellationToken ct = default(CancellationToken))
	{
		if (request == null || string.IsNullOrWhiteSpace(request.FicheNo))
		{
			return BadRequest(new
			{
				isSuccess = false,
				errorMessage = "Fiş numarası belirtilmedi."
			});
		}
		string firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
		string periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
		EBelgeDispatchRequest request2 = new EBelgeDispatchRequest
		{
			FicheNo = request.FicheNo,
			DocumentType = (string.Equals(request.DocumentType, "INVOICE", StringComparison.OrdinalIgnoreCase) ? "INVOICE" : "DESPATCHADVICE"),
			FirmNo = firmNo,
			PeriodNo = periodNo
		};
		return Ok(await dispatcherService.DispatchDocumentAsync(request2, ct));
	}
}
