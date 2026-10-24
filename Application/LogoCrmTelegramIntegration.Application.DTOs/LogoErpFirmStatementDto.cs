using System;
using System.Collections.Generic;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpFirmStatementDto
{
	public string FirmCode { get; set; } = string.Empty;

	public string FirmName { get; set; } = string.Empty;

	public decimal TotalDebit { get; set; }

	public decimal TotalCredit { get; set; }

	public DateTime? FilterStartDate { get; set; }

	public DateTime? FilterEndDate { get; set; }

	public decimal Balance => TotalDebit - TotalCredit;

	public List<CurrencySummaryDto> CurrencySummaries { get; set; } = new List<CurrencySummaryDto>();

	public List<LogoErpStatementLineDto> RecentTransactions { get; set; } = new List<LogoErpStatementLineDto>();
}
