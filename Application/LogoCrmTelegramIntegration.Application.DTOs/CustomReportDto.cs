using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class CustomReportDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Genel"; // Genel, FirmaKokpit, Cari, Malzeme, Kasa, Banka, CekSenet, Fatura, Irsaliye, Siparis, Mizan, Uretim
    public string SqlQuery { get; set; } = string.Empty;
    public string TelegramButtonText { get; set; } = string.Empty;
    public string Icon { get; set; } = "ğŸ“Š";
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool ShowInFirmamMenu { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
