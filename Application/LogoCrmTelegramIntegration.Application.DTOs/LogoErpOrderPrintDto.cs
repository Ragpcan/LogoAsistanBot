using System;
using System.Collections.Generic;
using System.Linq;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpOrderPrintDto
{
	public string FicheNo { get; set; } = string.Empty;

	public DateTime Date { get; set; } = DateTime.Now;

	public string FirmCode { get; set; } = string.Empty;

	public string FirmName { get; set; } = string.Empty;

	public string SalesmanName { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public decimal GeneralDiscountRate { get; set; }

	public decimal GeneralDiscountAmount { get; set; }

	public List<LogoErpOrderPrintLineDto> Lines { get; set; } = new List<LogoErpOrderPrintLineDto>();

	public decimal RawGrossTotal => Lines.Sum((LogoErpOrderPrintLineDto l) => l.Amount * l.ListPrice);

	public decimal LineDiscounts => Lines.Sum((LogoErpOrderPrintLineDto l) => l.TotalDiscountAmount);

	public decimal SubTotalBeforeGeneralDiscount => Lines.Sum((LogoErpOrderPrintLineDto l) => l.LineTotal);

	public decimal GeneralDiscountTotal
	{
		get
		{
			if (!(GeneralDiscountRate > 0m))
			{
				if (!(GeneralDiscountAmount > 0m))
				{
					return 0m;
				}
				return Math.Min(SubTotalBeforeGeneralDiscount, GeneralDiscountAmount);
			}
			return SubTotalBeforeGeneralDiscount * (GeneralDiscountRate / 100m);
		}
	}

	public decimal SubTotal => Math.Max(0m, SubTotalBeforeGeneralDiscount - GeneralDiscountTotal);

	public decimal TotalVat
	{
		get
		{
			if (Lines != null && Lines.Count > 0)
			{
				decimal sum = Lines.Sum((LogoErpOrderPrintLineDto l) => l.VatAmount);
				if (GeneralDiscountTotal > 0m && SubTotalBeforeGeneralDiscount > 0m)
				{
					decimal ratio = SubTotal / SubTotalBeforeGeneralDiscount;
					return sum * ratio;
				}
				return sum;
			}
			return SubTotal * 0.20m;
		}
	}

	public decimal NetTotal => SubTotal + TotalVat;
}
