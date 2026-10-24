using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpChequeItemDto
{
	public string PortfoyNo { get; set; } = string.Empty;

	public string Owing { get; set; } = string.Empty;

	public DateTime DueDate { get; set; }

	public decimal Amount { get; set; }

	public string StatusText { get; set; } = string.Empty;
}
