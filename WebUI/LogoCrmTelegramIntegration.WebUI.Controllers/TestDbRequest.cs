namespace LogoCrmTelegramIntegration.WebUI.Controllers;

public class TestDbRequest
{
	public string ConnectionString { get; set; } = string.Empty;

	public string Server { get; set; } = string.Empty;

	public string Database { get; set; } = string.Empty;

	public string UserId { get; set; } = string.Empty;

	public string Password { get; set; } = string.Empty;

	public bool TrustServerCertificate { get; set; } = true;

	public string? TargetSystem { get; set; }

	public string? FirmNo { get; set; }
}
