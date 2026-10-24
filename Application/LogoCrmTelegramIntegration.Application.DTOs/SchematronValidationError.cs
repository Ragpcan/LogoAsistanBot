namespace LogoCrmTelegramIntegration.Application.DTOs;

public class SchematronValidationError
{
	public string RuleId { get; set; } = string.Empty;

	public string Message { get; set; } = string.Empty;

	public string Location { get; set; } = string.Empty;

	public string Severity { get; set; } = "error";
}
