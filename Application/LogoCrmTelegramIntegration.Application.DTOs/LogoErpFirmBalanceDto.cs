namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpFirmBalanceDto
{
	public string FirmCode { get; set; } = string.Empty;

	public string FirmName { get; set; } = string.Empty;

	public decimal BalanceTL { get; set; }

	public decimal BalanceUSD { get; set; }

	public decimal BalanceEUR { get; set; }

	public decimal BalanceGBP { get; set; }

	public decimal NetBalanceLocal { get; set; }
}
