using System.Collections.Generic;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpItemStockDto
{
	public string ItemCode { get; set; } = string.Empty;

	public string ItemName { get; set; } = string.Empty;

	public string GroupCode { get; set; } = string.Empty;

	public string UnitName { get; set; } = string.Empty;

	public decimal TotalStock { get; set; }

	public decimal UnitPrice { get; set; }

	public double Vat { get; set; }

	public List<LogoErpWarehouseStockDto> WarehouseStocks { get; set; } = new List<LogoErpWarehouseStockDto>();
}
