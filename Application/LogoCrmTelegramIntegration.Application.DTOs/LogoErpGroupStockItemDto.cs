namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpGroupStockItemDto
{
	public string ItemCode { get; set; } = string.Empty;

	public string ItemName { get; set; } = string.Empty;

	public string UnitName { get; set; } = string.Empty;

	public decimal TotalStock { get; set; }
}
