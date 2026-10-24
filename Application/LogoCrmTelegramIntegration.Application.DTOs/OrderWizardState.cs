using System;
using System.Collections.Generic;
using System.Linq;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class OrderWizardState
{
	public long ChatId { get; set; }

	public string FirmCode { get; set; } = string.Empty;

	public string FirmName { get; set; } = string.Empty;

	public string Step { get; set; } = string.Empty;

	public string CurrentProductCode { get; set; } = string.Empty;

	public string CurrentProductName { get; set; } = string.Empty;

	public decimal CurrentQuantity { get; set; }

	public decimal CurrentItemPrice { get; set; }

	public decimal GeneralDiscountRate { get; set; }

	public decimal GeneralDiscountAmount { get; set; }

	public bool IsDispatchMode { get; set; }

	public bool IsInvoiceMode { get; set; }

	public bool IsCollectionMode { get; set; }

	public int CollectionTrCode { get; set; } = 35;

	public decimal CollectionAmount { get; set; }

	public string CarrierOrDriverInfo { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public int EditingLineIndex { get; set; } = -1;

	public List<OrderWizardLine> Lines { get; set; } = new List<OrderWizardLine>();

	public decimal SubTotalBeforeGeneralDiscount => Lines.Sum((OrderWizardLine l) => l.LineTotal);

	public decimal OverallDiscountTotal
	{
		get
		{
			if (GeneralDiscountRate > 0m)
			{
				return SubTotalBeforeGeneralDiscount * (GeneralDiscountRate / 100m);
			}
			if (GeneralDiscountAmount > 0m)
			{
				return Math.Min(SubTotalBeforeGeneralDiscount, GeneralDiscountAmount);
			}
			return 0m;
		}
	}

	public decimal GrandTotal => Math.Max(0m, SubTotalBeforeGeneralDiscount - OverallDiscountTotal);
}
