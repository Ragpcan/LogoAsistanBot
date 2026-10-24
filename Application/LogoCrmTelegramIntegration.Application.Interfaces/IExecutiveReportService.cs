using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public class DailyExecutiveSummaryDto
{
    public decimal TodaySalesTotal { get; set; }
    public int TodaySalesFicheCount { get; set; }
    public decimal TodayCollectionTotal { get; set; }
    public int PendingOrderCount { get; set; }
    public decimal PendingOrderTotal { get; set; }
    public decimal OverdueReceivablesTotal { get; set; }
    public int OverdueCustomerCount { get; set; }
    public List<CriticalStockItemDto> CriticalStockItems { get; set; } = new();
    public string FormattedTelegramHtml { get; set; } = string.Empty;
}

public class CriticalStockItemDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public double CurrentOnHand { get; set; }
    public string Unit { get; set; } = "ADET";
}

public interface IExecutiveReportService
{
    Task<DailyExecutiveSummaryDto> GenerateDailySummaryAsync(CancellationToken ct = default);
    Task<List<CriticalStockItemDto>> GetCriticalStockItemsAsync(double minThreshold = 5.0, CancellationToken ct = default);
}
