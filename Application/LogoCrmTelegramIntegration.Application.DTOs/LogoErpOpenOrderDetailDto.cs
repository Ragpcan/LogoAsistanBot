using System;
using System.Collections.Generic;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpOpenOrderDetailDto
{
	public int LogicalRef { get; set; }

	public string FicheNo { get; set; } = string.Empty;

	public DateTime Date { get; set; }

	public string FirmCode { get; set; } = string.Empty;

	public string FirmName { get; set; } = string.Empty;

	public string SalesmanName { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public decimal GrossTotal { get; set; }

	public decimal TotalDiscount { get; set; }

	public decimal NetTotal { get; set; }

	public decimal TotalVat => (Lines != null && Lines.Count > 0) ? Lines.Sum(l => l.VatAmount) : NetTotal * 0.20m;

	public decimal GrandTotal => NetTotal + TotalVat;

	public string StatusText { get; set; } = "Açık Sipariş (Sevk / İrsaliye Bekliyor)";

	public List<LogoErpOpenOrderLineDto> Lines { get; set; } = new List<LogoErpOpenOrderLineDto>();
}
