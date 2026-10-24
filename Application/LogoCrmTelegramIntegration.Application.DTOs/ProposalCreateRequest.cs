namespace LogoCrmTelegramIntegration.Application.DTOs;

public class ProposalCreateRequest
{
	public string FirmCode { get; set; } = string.Empty;

	public string ProductCode { get; set; } = string.Empty;

	public decimal Amount { get; set; }
}
