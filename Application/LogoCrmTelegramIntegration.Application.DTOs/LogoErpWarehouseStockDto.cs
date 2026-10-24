namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpWarehouseStockDto
{
	public int WarehouseNo { get; set; }

	public string WarehouseName { get; set; } = string.Empty;

	public decimal StockAmount { get; set; }
}
