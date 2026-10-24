using LogoCrmTelegramIntegration.Application.Interfaces;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpOrderLineInput
{
	public string ProductCode { get; set; } = string.Empty;

	public LogoErpItemDetails ItemDetails { get; set; }

	public decimal Amount { get; set; }

	public decimal? UnitPrice { get; set; }

	public decimal DiscountRate { get; set; }
}
