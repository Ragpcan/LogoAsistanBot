namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpFirmRiskDto
{
	public string FirmCode { get; set; } = string.Empty;

	public string FirmName { get; set; } = string.Empty;

	public decimal RiskLimit { get; set; }

	public decimal Debit { get; set; }

	public decimal Credit { get; set; }

	public decimal NetBalance => Debit - Credit;

	public decimal UsableLimit => RiskLimit - NetBalance;

	public int AverageMaturityDays { get; set; } = 0;
	public string RiskStatus { get; set; } = "Bilinmiyor";
}
