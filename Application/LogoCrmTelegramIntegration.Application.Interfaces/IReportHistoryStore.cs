using System.Collections.Generic;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IReportHistoryStore
{
    List<ReportHistoryItemDto> GetHistory(int take = 100);
    void AddHistoryItem(ReportHistoryItemDto item);
    void ClearHistory();
}
