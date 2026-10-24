using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface ILogoErpRepository
{
	Task<int?> GetFirmLogicalRefAsync(string firmCode, string firmNo, CancellationToken ct = default(CancellationToken));

	Task<LogoErpFirmDetailDto?> GetFirmDetailCardAsync(string firmCode, string firmNo, CancellationToken ct = default(CancellationToken));

	Task<LogoErpItemDetails?> GetItemDetailsAsync(string productCode, string firmNo, CancellationToken ct = default(CancellationToken));

	Task<(int ficheRef, int lineRef, string ficheNo)> CreateErpOrderTransactionAsync(int clientRef, LogoErpItemDetails itemDetails, decimal amount, string firmNo, string periodNo, int warehouseNo, string description = "Telegram'dan Hızlı Sipariş", int? salesmanRef = null, decimal? unitPrice = null, CancellationToken ct = default(CancellationToken));

	Task<(int ficheRef, List<int> lineRefs, string ficheNo)> CreateMultiLineErpOrderTransactionAsync(int clientRef, List<LogoErpOrderLineInput> lineInputs, string firmNo, string periodNo, int warehouseNo, string description = "Telegram'dan Hızlı Sipariş", int? salesmanRef = null, CancellationToken ct = default(CancellationToken));

	Task<LogoErpFirmRiskDto?> GetFirmRiskReportAsync(string firmCode, string firmNo, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpChequeItemDto>> GetFirmChequeReportAsync(string firmCode, string firmNo, string periodNo, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default(CancellationToken));

	Task<LogoErpFirmStatementDto?> GetFirmStatementAsync(string firmCode, string firmNo, string periodNo, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default(CancellationToken));

	Task<LogoErpItemStockDto?> GetItemStockAsync(string productCode, string firmNo, string periodNo, int warehouseNo, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpGroupStockItemDto>> GetGroupStockSummaryAsync(string groupCode, string firmNo, string periodNo, int warehouseNo, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpSalesmanDto>> GetSalesmenAsync(string firmNo, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpFirmDto>> SearchFirmsAsync(string searchTerm, string firmNo, CancellationToken ct);

	Task<List<LogoErpFirmDto>> SearchFirmsAsync(string searchTerm, string firmNo, string? firmCyphCode = null, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpFirmBalanceDto>> GetFirmListAsync(string listType, string firmNo, string periodNo, CancellationToken ct);

	Task<List<LogoErpFirmBalanceDto>> GetFirmListAsync(string listType, string firmNo, string periodNo, string? firmCyphCode = null, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpItemStockDto>> SearchItemStockAsync(string searchTerm, string firmNo, string periodNo, int warehouseNo, CancellationToken ct);

	Task<List<LogoErpItemStockDto>> SearchItemStockAsync(string searchTerm, string firmNo, string periodNo, int warehouseNo, string? itemCyphCode = null, CancellationToken ct = default(CancellationToken));

	Task<List<(string Code, LogoErpItemDetails Details)>> SearchItemDetailsAsync(string searchTerm, string firmNo, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpOpenOrderSummaryDto>> GetOrdersAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit, CancellationToken ct);

	Task<List<LogoErpOpenOrderSummaryDto>> GetOrdersAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, string? orderCyphCode = null, string? firmCyphCode = null, int limit = 30, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpOpenOrderSummaryDto>> GetOpenOrdersAsync(string? firmCodeFilter, string firmNo, string periodNo, int limit, CancellationToken ct);

	Task<List<LogoErpOpenOrderSummaryDto>> GetOpenOrdersAsync(string? firmCodeFilter, string firmNo, string periodNo, string? orderCyphCode = null, string? firmCyphCode = null, int limit = 20, CancellationToken ct = default(CancellationToken));

	Task<LogoErpOpenOrderDetailDto?> GetOpenOrderDetailAsync(string ficheNo, string firmNo, string periodNo, CancellationToken ct = default(CancellationToken));

	Task<(int ficheRef, List<int> lineRefs, string ficheNo)> CreateErpDispatchTransactionAsync(int clientRef, List<LogoErpOrderLineInput> lineInputs, string firmNo, string periodNo, int warehouseNo, string description = "Telegram Bot (@LogoCrmBot) - Satış İrsaliyesi", int? salesmanRef = null, string? carrierOrDriverInfo = null, CancellationToken ct = default(CancellationToken));

	Task<LogoErpDispatchDto?> GetDispatchDetailAsync(string ficheNo, string firmNo, string periodNo, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpOpenOrderSummaryDto>> GetDispatchesAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit = 30, CancellationToken ct = default(CancellationToken));

	Task<(int ficheRef, List<int> lineRefs, string dispatchFicheNo)> ConvertOrderToDispatchAsync(string orderFicheNo, string firmNo, string periodNo, int warehouseNo, string description = "Siparişten Oluşturulan Satış İrsaliyesi", int? salesmanRef = null, string? carrierOrDriverInfo = null, CancellationToken ct = default(CancellationToken));

	Task<LogoCompanyInfoDto?> GetCompanyInfoAsync(string firmNo, CancellationToken ct = default(CancellationToken));

	Task<string> GetNextDispatchFicheNoAsync(string firmNo, string periodNo, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpOpenOrderSummaryDto>> GetInvoicesAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit = 30, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpOpenOrderSummaryDto>> GetPurchaseOrdersAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit = 30, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpOpenOrderSummaryDto>> GetPurchaseDispatchesAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit = 30, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpOpenOrderSummaryDto>> GetPurchaseInvoicesAsync(string? firmCodeFilter, DateTime? startDate, DateTime? endDate, string firmNo, string periodNo, int limit = 30, CancellationToken ct = default(CancellationToken));

	Task<string> GetCustomerInvoiceTypeAsync(string firmCode, string firmNo, CancellationToken ct = default(CancellationToken));

	Task<(int ficheRef, List<int> lineRefs, string ficheNo)> CreateErpInvoiceTransactionAsync(int clientRef, List<LogoErpOrderLineInput> lineInputs, string firmNo, string periodNo, int warehouseNo, string description = "Telegram Bot (@LogoCrmBot) - Satış Faturası", int? salesmanRef = null, CancellationToken ct = default(CancellationToken));

	Task<(int lineRef, string ficheNo)> CreateErpCollectionTransactionAsync(int clientRef, decimal amount, int trCode, string firmNo, string periodNo, string description = "Telegram Bot (@LogoCrmBot) - Tahsilat Fişi", int? salesmanRef = null, CancellationToken ct = default(CancellationToken));
}
