using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class OrderWizardLine
{
	public string ProductCode { get; set; } = string.Empty;

	public string ProductName { get; set; } = string.Empty;

	public decimal Quantity { get; set; }

	public decimal ListPrice { get; set; }

	public decimal DiscountRate { get; set; }

	public decimal DiscountAmount { get; set; }

	public decimal UnitPrice { get; set; }

	public decimal EffectiveListPrice
	{
		get
		{
			if (!(ListPrice > 0m))
			{
				return UnitPrice;
			}
			return ListPrice;
		}
	}

	public decimal EffectiveUnitPrice
	{
		get
		{
			if (DiscountRate > 0m)
			{
				return EffectiveListPrice * (1m - DiscountRate / 100m);
			}
			if (DiscountAmount > 0m)
			{
				return Math.Max(0m, EffectiveListPrice - DiscountAmount);
			}
			if (!(UnitPrice > 0m))
			{
				return EffectiveListPrice;
			}
			return UnitPrice;
		}
	}

	public double VatRate { get; set; } = 20.0;

	public decimal LineTotal => Quantity * EffectiveUnitPrice;

	public decimal VatAmount => LineTotal * (decimal)(VatRate / 100.0);
}
