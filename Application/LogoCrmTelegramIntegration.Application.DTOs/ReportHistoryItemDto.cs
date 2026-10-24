using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class ReportHistoryItemDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? ReportId { get; set; }
    public string ReportTitle { get; set; } = string.Empty;
    public string Category { get; set; } = "Genel";
    public string ExecutedBy { get; set; } = "Sistem"; // Telegram kullanıcısı veya Web Admin
    public long? ChatId { get; set; }
    public string? SqlQuery { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.Now;
    public long DurationMs { get; set; }
    public int RowCount { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? ResultPreviewJson { get; set; }
}
