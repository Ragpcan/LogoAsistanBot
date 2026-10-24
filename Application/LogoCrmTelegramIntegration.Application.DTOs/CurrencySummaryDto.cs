using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class CurrencySummaryDto
{
	public string CurrencySymbol { get; set; } = "TL";

	public decimal TotalDebit { get; set; }

	public decimal TotalCredit { get; set; }

	public DateTime? FilterStartDate { get; set; }

	public DateTime? FilterEndDate { get; set; }

	public decimal Balance => TotalDebit - TotalCredit;
}
