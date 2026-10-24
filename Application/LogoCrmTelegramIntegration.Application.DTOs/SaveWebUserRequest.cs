namespace LogoCrmTelegramIntegration.Application.DTOs;

public class SaveWebUserRequest
{
	public string Username { get; set; } = string.Empty;

	public string FullName { get; set; } = string.Empty;

	public string? Password { get; set; }

	public string Role { get; set; } = "TelegramOperator";

	public bool CanAccessDashboard { get; set; } = true;

	public bool CanAccessSalesmen { get; set; } = true;

	public bool CanAccessLiveTransactions { get; set; } = true;

	public bool CanAccessXslt { get; set; }

	public bool CanAccessAdminSettings { get; set; }
}
