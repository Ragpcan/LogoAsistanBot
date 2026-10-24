namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpDispatchLineDto
{
	public string ItemCode { get; set; } = string.Empty;

	public string ItemName { get; set; } = string.Empty;

	public decimal Amount { get; set; }

	public string UnitName { get; set; } = "ADET";

	public decimal UnitPrice { get; set; }

	public decimal DiscountRate { get; set; }

	public decimal LineTotal { get; set; }

	public double VatRate { get; set; } = 20.0;

	public decimal DiscountAmount => (UnitPrice > 0m && DiscountRate > 0m) ? (Amount * UnitPrice * (DiscountRate / 100m)) : (Amount * UnitPrice > LineTotal ? (Amount * UnitPrice - LineTotal) : 0m);

	public decimal GrossLineTotal => Amount * UnitPrice;

	public decimal VatAmount => LineTotal * (decimal)(VatRate / 100.0);

	public decimal LineGrandTotal => LineTotal + VatAmount;
}
