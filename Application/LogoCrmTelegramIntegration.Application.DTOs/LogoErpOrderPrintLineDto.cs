namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpOrderPrintLineDto
{
	public string ItemCode { get; set; } = string.Empty;

	public string ItemName { get; set; } = string.Empty;

	public decimal Amount { get; set; }

	public string UnitName { get; set; } = "ADET";

	public decimal ListPrice { get; set; }

	public decimal DiscountRate { get; set; }

	public decimal DiscountAmount { get; set; }

	public decimal UnitPrice { get; set; }

	public double VatRate { get; set; } = 20.0;

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

	public decimal TotalDiscountAmount
	{
		get
		{
			if (!(DiscountRate > 0m))
			{
				if (!(DiscountAmount > 0m))
				{
					return Amount * (EffectiveListPrice - UnitPrice);
				}
				return DiscountAmount * Amount;
			}
			return Amount * EffectiveListPrice * (DiscountRate / 100m);
		}
	}

	public decimal EffectiveUnitPrice
	{
		get
		{
			if (!(UnitPrice > 0m))
			{
				return EffectiveListPrice - ((DiscountRate > 0m) ? (EffectiveListPrice * (DiscountRate / 100m)) : DiscountAmount);
			}
			return UnitPrice;
		}
	}

	public decimal LineTotal => Amount * EffectiveUnitPrice;

	public decimal VatAmount => LineTotal * (decimal)(VatRate / 100.0);
}
