using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpOpenOrderLineDto
{
	public string ItemCode { get; set; } = string.Empty;

	public string ItemName { get; set; } = string.Empty;

	public decimal Amount { get; set; }

	public decimal ShippedAmount { get; set; }

	public decimal PendingAmount => Math.Max(0m, Amount - ShippedAmount);

	public string UnitName { get; set; } = "ADET";

	public decimal ListPrice { get; set; }

	public decimal DiscountRate { get; set; }

	public decimal NetPrice { get; set; }

	public decimal LineNetTotal { get; set; }

	public double VatRate { get; set; } = 20.0;

	public decimal DiscountAmount => (ListPrice > NetPrice && Amount > 0m) ? ((ListPrice - NetPrice) * Amount) : (Amount * (ListPrice > 0m ? ListPrice : NetPrice) * (DiscountRate / 100m));

	public decimal GrossLineTotal => (ListPrice > 0m ? ListPrice : NetPrice) * Amount;

	public decimal VatAmount => LineNetTotal * (decimal)(VatRate / 100.0);

	public decimal LineGrandTotal => LineNetTotal + VatAmount;
}
