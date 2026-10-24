using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface ICustomReportStore
{
    List<CustomReportDto> GetAllReports();
    List<CustomReportDto> GetReportsByCategory(string category);
    List<CustomReportDto> GetFirmamReports();
    CustomReportDto? GetReportById(string id);
    bool SaveOrUpdateReport(CustomReportDto report, out string errorMessage);
    bool DeleteReport(string id, out string errorMessage);
    string ResolveSqlPlaceholders(string sql, string firmNo, string periodNo, string? clientRef = null, string? itemRef = null, string? startDate = null, string? endDate = null);
}
