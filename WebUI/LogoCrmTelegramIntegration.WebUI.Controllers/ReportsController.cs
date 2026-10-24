using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using LogoCrmTelegramIntegration.Domain.Entities;
using LogoCrmTelegramIntegration.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ICustomReportStore _customReportStore;
    private readonly IDynamicSqlExecutor _dynamicSqlExecutor;
    private readonly IReportHistoryStore _reportHistoryStore;
    private readonly IInvoicePaymentStore _invoicePaymentStore;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        ICustomReportStore customReportStore,
        IDynamicSqlExecutor dynamicSqlExecutor,
        IReportHistoryStore reportHistoryStore,
        IInvoicePaymentStore invoicePaymentStore,
        IConfiguration configuration,
        ILogger<ReportsController> logger)
    {
        _customReportStore = customReportStore;
        _dynamicSqlExecutor = dynamicSqlExecutor;
        _reportHistoryStore = reportHistoryStore;
        _invoicePaymentStore = invoicePaymentStore;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAllReports()
    {
        var reports = _customReportStore.GetAllReports();
        return Ok(reports);
    }

    [HttpGet("{id}")]
    public IActionResult GetReportById(string id)
    {
        var report = _customReportStore.GetReportById(id);
        if (report == null) return NotFound(new { isSuccess = false, message = "Rapor bulunamadı." });
        return Ok(report);
    }

    [HttpPost]
    public IActionResult SaveOrUpdateReport([FromBody] CustomReportDto report)
    {
        if (report == null)
        {
            return BadRequest(new { isSuccess = false, message = "Geçersiz rapor verisi." });
        }

        if (!_customReportStore.SaveOrUpdateReport(report, out string errorMessage))
        {
            return BadRequest(new { isSuccess = false, message = errorMessage });
        }

        _logger.LogInformation("Özel Rapor kaydedildi: {Title} ({Id})", report.Title, report.Id);
        return Ok(new { isSuccess = true, message = $"'{report.Title}' raporu başarıyla kaydedildi.", report });
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteReport(string id)
    {
        if (!_customReportStore.DeleteReport(id, out string errorMessage))
        {
            return BadRequest(new { isSuccess = false, message = errorMessage });
        }

        _logger.LogInformation("Özel Rapor silindi: {Id}", id);
        return Ok(new { isSuccess = true, message = "Rapor başarıyla silindi." });
    }

    [HttpGet("history")]
    public IActionResult GetReportHistory([FromQuery] int take = 100)
    {
        var history = _reportHistoryStore.GetHistory(take);
        return Ok(history);
    }

    [HttpDelete("history")]
    public IActionResult ClearReportHistory()
    {
        _reportHistoryStore.ClearHistory();
        return Ok(new { isSuccess = true, message = "Rapor çalıştırma geçmişi temizlendi." });
    }

    [HttpPost("test-sql")]
    public async Task<IActionResult> TestSqlQuery([FromBody] TestSqlRequest request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.SqlQuery))
        {
            return BadRequest(new { isSuccess = false, message = "SQL sorgusu boş olamaz." });
        }

        string firmNo = request.FirmNo ?? _configuration["LogoSystem:FirmNo"] ?? "001";
        string periodNo = request.PeriodNo ?? _configuration["LogoSystem:PeriodNo"] ?? "01";

        string resolvedSql = _customReportStore.ResolveSqlPlaceholders(
            request.SqlQuery,
            firmNo,
            periodNo,
            request.ClientRef,
            request.ItemRef,
            request.StartDate,
            request.EndDate);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            string jsonResult = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(resolvedSql, ct);
            sw.Stop();
            bool isError = jsonResult.StartsWith("[GÜVENLİK İHLALİ]") || jsonResult.StartsWith("[SQL HATASI]");

            int rowCount = 0;
            var columns = new List<string>();
            var rows = new List<List<object?>>();
            var dataList = new List<Dictionary<string, object?>>();

            if (!isError && jsonResult.StartsWith("["))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(jsonResult);
                    rowCount = doc.RootElement.GetArrayLength();
                    if (rowCount > 0)
                    {
                        var firstElem = doc.RootElement[0];
                        foreach (var prop in firstElem.EnumerateObject())
                        {
                            columns.Add(prop.Name);
                        }

                        foreach (var rowElem in doc.RootElement.EnumerateArray())
                        {
                            var row = new List<object?>();
                            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                            foreach (var col in columns)
                            {
                                if (rowElem.TryGetProperty(col, out var val))
                                {
                                    object? v = val.ValueKind switch
                                    {
                                        System.Text.Json.JsonValueKind.Number => val.TryGetInt64(out var l) ? (object)l : (val.TryGetDouble(out var d) ? (object)d : val.GetRawText()),
                                        System.Text.Json.JsonValueKind.String => val.GetString(),
                                        System.Text.Json.JsonValueKind.True => true,
                                        System.Text.Json.JsonValueKind.False => false,
                                        System.Text.Json.JsonValueKind.Null => null,
                                        _ => val.ToString()
                                    };
                                    row.Add(v);
                                    dict[col] = v;
                                }
                                else
                                {
                                    row.Add(null);
                                    dict[col] = null;
                                }
                            }
                            rows.Add(row);
                            dataList.Add(dict);
                        }
                    }
                }
                catch { }
            }

            _reportHistoryStore.AddHistoryItem(new ReportHistoryItemDto
            {
                ReportTitle = "Web Canlı SQL Testi",
                Category = "SQL Test",
                ExecutedBy = "Web Admin",
                SqlQuery = resolvedSql,
                DurationMs = sw.ElapsedMilliseconds,
                RowCount = rowCount,
                IsSuccess = !isError,
                ErrorMessage = isError ? jsonResult : null,
                ResultPreviewJson = jsonResult.Length > 500 ? jsonResult.Substring(0, 500) + "..." : jsonResult
            });

            return Ok(new
            {
                isSuccess = !isError,
                resolvedSql = resolvedSql,
                resultJson = jsonResult,
                durationMs = sw.ElapsedMilliseconds,
                rowCount = rowCount,
                columns = columns,
                rows = rows,
                data = dataList,
                message = isError ? jsonResult : "SQL sorgusu başarıyla çalıştırıldı."
            });
        }
        catch (Exception ex)
        {
            sw.Stop();
            _reportHistoryStore.AddHistoryItem(new ReportHistoryItemDto
            {
                ReportTitle = "Web Canlı SQL Testi",
                Category = "SQL Test",
                ExecutedBy = "Web Admin",
                SqlQuery = resolvedSql,
                DurationMs = sw.ElapsedMilliseconds,
                RowCount = 0,
                IsSuccess = false,
                ErrorMessage = ex.Message
            });

            return Ok(new
            {
                isSuccess = false,
                resolvedSql = resolvedSql,
                durationMs = sw.ElapsedMilliseconds,
                message = $"SQL Hatası: {ex.Message}"
            });
        }
    }

    [HttpGet("invoice-due-dates")]
    public async Task<IActionResult> GetInvoiceDueDates(
        [FromQuery] string? firmNo,
        [FromQuery] string? periodNo,
        [FromQuery] string? type,
        [FromQuery] string? paymentStatus,
        [FromQuery] string? maturityStatus,
        [FromQuery] string? search,
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        [FromQuery] string? cariCode,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        CancellationToken ct = default)
    {
        try
        {
            var firm = (string.IsNullOrWhiteSpace(firmNo) ? _configuration["LogoSystem:FirmNo"] ?? "001" : firmNo).Trim().PadLeft(3, '0');
            var period = (string.IsNullOrWhiteSpace(periodNo) ? _configuration["LogoSystem:PeriodNo"] ?? "01" : periodNo).Trim().PadLeft(2, '0');
            var connStr = _configuration.GetConnectionString("LogoCrmConnection");

            if (string.IsNullOrWhiteSpace(connStr))
                return BadRequest(new { isSuccess = false, message = "SQL bağlantı dizesi bulunamadı." });

            string sql = $@"
SELECT 
    F.LOGICALREF AS LogicalRef,
    F.FICHENO AS FicheNo,
    ISNULL(F.DOCODE, '') AS DoCode,
    F.TRCODE AS TrCode,
    CASE 
        WHEN F.TRCODE IN (7, 8, 9, 35) THEN 'SATIŞ'
        WHEN F.TRCODE IN (1, 2, 3, 4, 5, 6) THEN 'ALIŞ'
        ELSE 'DİĞER'
    END AS InvoiceType,
    F.DATE_ AS InvoiceDate,
    ISNULL(P.DATE_, F.DATE_) AS DueDate,
    DATEDIFF(day, GETDATE(), ISNULL(P.DATE_, F.DATE_)) AS DaysRemaining,
    F.NETTOTAL AS NetTotal,
    ISNULL(F.GROSSTOTAL, F.NETTOTAL) AS GrossTotal,
    ISNULL(F.TOTALVAT, 0) AS TotalVat,
    ISNULL(C.LOGICALREF, 0) AS ClientRef,
    ISNULL(C.CODE, '') AS CariCode,
    ISNULL(C.DEFINITION_, '') AS CariName,
    ISNULL(C.TELNRS1, '') AS Phone,
    ISNULL(C.EMAILADDR, '') AS Email,
    ISNULL(PP.CODE, '') AS PayPlanCode,
    ISNULL(PP.DEFINITION_, '') AS PayPlanName,
    ISNULL(P.PAID, 0) AS LogoPaid,
    CASE WHEN ISNULL(P.PAID, 0) >= F.NETTOTAL AND F.NETTOTAL > 0 THEN 1 ELSE 0 END AS LogoClosed
FROM LG_{firm}_{period}_INVOICE F WITH(NOLOCK)
LEFT JOIN LG_{firm}_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
LEFT JOIN LG_{firm}_{period}_PAYTRANS P WITH(NOLOCK) ON P.FICHEREF = F.LOGICALREF AND P.MODULENR = 4
LEFT JOIN LG_{firm}_PAYPLANS PP WITH(NOLOCK) ON F.PAYDEFREF = PP.LOGICALREF
WHERE F.CANCELLED = 0
ORDER BY ISNULL(P.DATE_, F.DATE_) ASC, F.DATE_ DESC";

            using var conn = new SqlConnection(connStr);
            var rawList = (await conn.QueryAsync(new CommandDefinition(sql, cancellationToken: ct))).ToList();

            var manualPayments = _invoicePaymentStore.GetAll();

            var items = new List<object>();
            decimal totalAmount = 0;
            decimal totalPaid = 0;
            decimal totalUnpaid = 0;
            decimal totalOverdue = 0;
            int totalOverdueCount = 0;
            int todayDueCount = 0;
            decimal todayDueAmount = 0;

            foreach (var r in rawList)
            {
                int logicalRef = (int)r.LogicalRef;
                string ficheNo = (string)r.FicheNo ?? "";
                string docode = (string)r.DoCode ?? "";
                int trcode = (int)r.TrCode;
                string invType = (string)r.InvoiceType ?? "";
                DateTime invDate = (DateTime)r.InvoiceDate;
                DateTime dueDate = (DateTime)r.DueDate;
                int daysRemaining = (int)r.DaysRemaining;
                decimal netTotal = (decimal)r.NetTotal;
                decimal grossTotal = (decimal)r.GrossTotal;
                decimal totalVat = (decimal)r.TotalVat;
                int clientRef = (int)r.ClientRef;
                string cCode = (string)r.CariCode ?? "";
                string cName = (string)r.CariName ?? "";
                string phone = (string)r.Phone ?? "";
                string email = (string)r.Email ?? "";
                string payPlanCode = (string)r.PayPlanCode ?? "";
                string payPlanName = (string)r.PayPlanName ?? "";
                decimal logoPaid = (decimal)r.LogoPaid;
                int logoClosed = (int)r.LogoClosed;

                string invoiceKey = $"{firm}_{logicalRef}";

                string status = "Odenmedi";
                decimal paidAmount = 0;
                DateTime? paidDate = null;
                string? note = null;
                string? updatedBy = null;
                DateTime? updatedAt = null;

                if (manualPayments.TryGetValue(invoiceKey, out var mp))
                {
                    status = mp.Status;
                    paidAmount = mp.PaidAmount > 0 ? mp.PaidAmount : (status == "Odendi" ? netTotal : 0);
                    paidDate = mp.PaidDate;
                    note = mp.Note;
                    updatedBy = mp.UpdatedBy;
                    updatedAt = mp.UpdatedAt;
                }
                else if (logoClosed == 1 || logoPaid >= netTotal && netTotal > 0)
                {
                    status = "Odendi";
                    paidAmount = logoPaid > 0 ? logoPaid : netTotal;
                    paidDate = dueDate;
                }
                else if (logoPaid > 0)
                {
                    status = "Kismi";
                    paidAmount = logoPaid;
                }

                string matClass = daysRemaining < 0 ? "Overdue" : (daysRemaining == 0 ? "Today" : "Upcoming");

                if (!string.IsNullOrWhiteSpace(type) && type != "all")
                {
                    if (type == "sales" && invType != "SATIŞ") continue;
                    if (type == "purchase" && invType != "ALIŞ") continue;
                }

                if (!string.IsNullOrWhiteSpace(paymentStatus) && paymentStatus != "all")
                {
                    if (string.Equals(paymentStatus, "odendi", StringComparison.OrdinalIgnoreCase) && status != "Odendi") continue;
                    if (string.Equals(paymentStatus, "odenmedi", StringComparison.OrdinalIgnoreCase) && status != "Odenmedi") continue;
                    if (string.Equals(paymentStatus, "kismi", StringComparison.OrdinalIgnoreCase) && status != "Kismi") continue;
                }

                if (!string.IsNullOrWhiteSpace(maturityStatus) && maturityStatus != "all")
                {
                    if (maturityStatus == "overdue" && daysRemaining >= 0) continue;
                    if (maturityStatus == "today" && daysRemaining != 0) continue;
                    if (maturityStatus == "days_0_15" && (daysRemaining < 0 || daysRemaining > 15)) continue;
                    if (maturityStatus == "days_16_30" && (daysRemaining < 16 || daysRemaining > 30)) continue;
                    if (maturityStatus == "days_30_plus" && daysRemaining <= 30) continue;
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string s = search.Trim().ToLowerInvariant();
                    if (!ficheNo.ToLowerInvariant().Contains(s) &&
                        !docode.ToLowerInvariant().Contains(s) &&
                        !cCode.ToLowerInvariant().Contains(s) &&
                        !cName.ToLowerInvariant().Contains(s))
                    {
                        continue;
                    }
                }

                if (!string.IsNullOrWhiteSpace(cariCode) && !string.Equals(cCode, cariCode.Trim(), StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrWhiteSpace(startDate) && DateTime.TryParse(startDate, out var sd) && invDate.Date < sd.Date)
                    continue;

                if (!string.IsNullOrWhiteSpace(endDate) && DateTime.TryParse(endDate, out var ed) && invDate.Date > ed.Date)
                    continue;

                if (minAmount.HasValue && netTotal < minAmount.Value)
                    continue;

                if (maxAmount.HasValue && netTotal > maxAmount.Value)
                    continue;

                totalAmount += netTotal;
                if (status == "Odendi")
                {
                    totalPaid += netTotal;
                }
                else
                {
                    totalUnpaid += (netTotal - paidAmount);
                    if (daysRemaining < 0)
                    {
                        totalOverdue += (netTotal - paidAmount);
                        totalOverdueCount++;
                    }
                    else if (daysRemaining == 0)
                    {
                        todayDueCount++;
                        todayDueAmount += (netTotal - paidAmount);
                    }
                }

                items.Add(new
                {
                    invoiceKey,
                    logicalRef,
                    ficheNo,
                    docode,
                    trcode,
                    invoiceType = invType,
                    invoiceDate = invDate.ToString("yyyy-MM-dd"),
                    invoiceDateFormatted = invDate.ToString("dd.MM.yyyy"),
                    dueDate = dueDate.ToString("yyyy-MM-dd"),
                    dueDateFormatted = dueDate.ToString("dd.MM.yyyy"),
                    daysRemaining,
                    maturityStatus = matClass,
                    netTotal,
                    grossTotal,
                    totalVat,
                    clientRef,
                    cariCode = cCode,
                    cariName = cName,
                    phone,
                    email,
                    payPlanCode,
                    payPlanName,
                    paymentStatus = status,
                    paidAmount,
                    remainingAmount = Math.Max(0, netTotal - paidAmount),
                    paidDate = paidDate?.ToString("dd.MM.yyyy"),
                    note,
                    updatedBy,
                    updatedAt = updatedAt?.ToString("dd.MM.yyyy HH:mm")
                });
            }

            return Ok(new
            {
                isSuccess = true,
                summary = new
                {
                    totalCount = items.Count,
                    totalAmount,
                    totalPaid,
                    totalUnpaid,
                    totalOverdue,
                    totalOverdueCount,
                    todayDueCount,
                    todayDueAmount
                },
                items
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatura vade takibi verileri alınırken hata oluştu.");
            return Ok(new { isSuccess = false, message = "Vade verileri alınırken hata oluştu: " + ex.Message });
        }
    }

    [HttpPost("invoice-payment-status")]
    public IActionResult UpdateInvoicePaymentStatus([FromBody] UpdateInvoicePaymentRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.InvoiceKey))
            return BadRequest(new { isSuccess = false, message = "Geçersiz fatura anahtarı." });

        _invoicePaymentStore.SetPaymentStatus(
            req.InvoiceKey,
            req.FicheNo ?? "",
            req.Status ?? "Odenmedi",
            req.PaidAmount,
            req.Note,
            req.UpdatedBy ?? "admin",
            req.PaidDate
        );

        return Ok(new
        {
            isSuccess = true,
            message = $"Fatura ({req.FicheNo}) ödeme durumu '{req.Status}' olarak güncellendi."
        });
    }

    [HttpGet("cari-transactions")]
    public async Task<IActionResult> GetCariTransactions(
        [FromQuery] string? cariCode,
        [FromQuery] string? firmNo,
        [FromQuery] string? periodNo,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cariCode))
            return BadRequest(new { isSuccess = false, message = "Cari kodu belirtilmelidir." });

        try
        {
            var firm = (string.IsNullOrWhiteSpace(firmNo) ? _configuration["LogoSystem:FirmNo"] ?? "001" : firmNo).Trim().PadLeft(3, '0');
            var period = (string.IsNullOrWhiteSpace(periodNo) ? _configuration["LogoSystem:PeriodNo"] ?? "01" : periodNo).Trim().PadLeft(2, '0');
            var connStr = _configuration.GetConnectionString("LogoCrmConnection");

            string sql = $@"
SELECT 
    F.LOGICALREF AS LogicalRef,
    F.FICHENO AS FicheNo,
    ISNULL(F.DOCODE, '') AS DoCode,
    F.TRCODE AS TrCode,
    CASE 
        WHEN F.TRCODE IN (7, 8, 9, 35) THEN 'SATIŞ'
        WHEN F.TRCODE IN (1, 2, 3, 4, 5, 6) THEN 'ALIŞ'
        ELSE 'DİĞER'
    END AS InvoiceType,
    F.DATE_ AS InvoiceDate,
    ISNULL(P.DATE_, F.DATE_) AS DueDate,
    DATEDIFF(day, GETDATE(), ISNULL(P.DATE_, F.DATE_)) AS DaysRemaining,
    F.NETTOTAL AS NetTotal,
    ISNULL(P.PAID, 0) AS LogoPaid,
    CASE WHEN ISNULL(P.PAID, 0) >= F.NETTOTAL AND F.NETTOTAL > 0 THEN 1 ELSE 0 END AS LogoClosed
FROM LG_{firm}_{period}_INVOICE F WITH(NOLOCK)
INNER JOIN LG_{firm}_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
LEFT JOIN LG_{firm}_{period}_PAYTRANS P WITH(NOLOCK) ON P.FICHEREF = F.LOGICALREF AND P.MODULENR = 4
WHERE F.CANCELLED = 0 AND C.CODE = @CariCode
ORDER BY F.DATE_ DESC, ISNULL(P.DATE_, F.DATE_) DESC";

            using var conn = new SqlConnection(connStr);
            var rawList = (await conn.QueryAsync(new CommandDefinition(sql, new { CariCode = cariCode.Trim() }, cancellationToken: ct))).ToList();

            var manualPayments = _invoicePaymentStore.GetAll();

            var transactions = new List<object>();
            decimal totalDebt = 0;
            decimal totalPaid = 0;
            decimal totalUnpaid = 0;

            foreach (var r in rawList)
            {
                int logicalRef = (int)r.LogicalRef;
                string ficheNo = (string)r.FicheNo ?? "";
                string docode = (string)r.DoCode ?? "";
                string invType = (string)r.InvoiceType ?? "";
                DateTime invDate = (DateTime)r.InvoiceDate;
                DateTime dueDate = (DateTime)r.DueDate;
                int daysRemaining = (int)r.DaysRemaining;
                decimal netTotal = (decimal)r.NetTotal;
                decimal logoPaid = (decimal)r.LogoPaid;
                int logoClosed = (int)r.LogoClosed;

                string invoiceKey = $"{firm}_{logicalRef}";

                string status = "Odenmedi";
                decimal paidAmount = 0;
                string? note = null;

                if (manualPayments.TryGetValue(invoiceKey, out var mp))
                {
                    status = mp.Status;
                    paidAmount = mp.PaidAmount > 0 ? mp.PaidAmount : (status == "Odendi" ? netTotal : 0);
                    note = mp.Note;
                }
                else if (logoClosed == 1 || logoPaid >= netTotal && netTotal > 0)
                {
                    status = "Odendi";
                    paidAmount = logoPaid > 0 ? logoPaid : netTotal;
                }

                totalDebt += netTotal;
                if (status == "Odendi") totalPaid += netTotal;
                else totalUnpaid += (netTotal - paidAmount);

                transactions.Add(new
                {
                    invoiceKey,
                    logicalRef,
                    ficheNo,
                    docode,
                    invoiceType = invType,
                    invoiceDateFormatted = invDate.ToString("dd.MM.yyyy"),
                    dueDateFormatted = dueDate.ToString("dd.MM.yyyy"),
                    daysRemaining,
                    netTotal,
                    paymentStatus = status,
                    paidAmount,
                    remainingAmount = Math.Max(0, netTotal - paidAmount),
                    note
                });
            }

            return Ok(new
            {
                isSuccess = true,
                cariCode,
                summary = new
                {
                    totalInvoiceCount = transactions.Count,
                    totalDebt,
                    totalPaid,
                    totalUnpaid
                },
                transactions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cari hareketleri alınırken hata oluştu.");
            return Ok(new { isSuccess = false, message = "Cari hareketleri alınamadı: " + ex.Message });
        }
    }
}

public class UpdateInvoicePaymentRequest
{
    public string InvoiceKey { get; set; } = string.Empty;
    public string FicheNo { get; set; } = string.Empty;
    public string Status { get; set; } = "Odenmedi";
    public decimal PaidAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Note { get; set; }
    public string? UpdatedBy { get; set; }
}

public class TestSqlRequest
{
    public string SqlQuery { get; set; } = string.Empty;
    public string? FirmNo { get; set; }
    public string? PeriodNo { get; set; }
    public string? ClientRef { get; set; }
    public string? ItemRef { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}
