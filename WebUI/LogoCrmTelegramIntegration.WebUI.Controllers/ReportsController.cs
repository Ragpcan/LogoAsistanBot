using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using LogoCrmTelegramIntegration.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        ICustomReportStore customReportStore,
        IDynamicSqlExecutor dynamicSqlExecutor,
        IReportHistoryStore reportHistoryStore,
        IConfiguration configuration,
        ILogger<ReportsController> logger)
    {
        _customReportStore = customReportStore;
        _dynamicSqlExecutor = dynamicSqlExecutor;
        _reportHistoryStore = reportHistoryStore;
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
