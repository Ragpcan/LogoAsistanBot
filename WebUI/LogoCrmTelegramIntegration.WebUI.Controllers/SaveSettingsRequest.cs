namespace LogoCrmTelegramIntegration.WebUI.Controllers;

public class SaveSettingsRequest
{
	public string? BotToken { get; set; }

	public string? RawConnectionString { get; set; }

	public string? Server { get; set; }

	public string? Database { get; set; }

	public string? UserId { get; set; }

	public string? Password { get; set; }

	public bool TrustServerCertificate { get; set; } = true;

	public string? TargetSystem { get; set; }

	public string? FirmNo { get; set; }

	public string? PeriodNo { get; set; }

	public string? WarehouseNo { get; set; }

	public string? LicenseExpirationDate { get; set; }

	public string? InvoiceFichePrefix { get; set; }

	public string? DispatchFichePrefix { get; set; }

	public string? OrderFichePrefix { get; set; }

	public string? DocumentYear { get; set; }

	public string? AiApiKey { get; set; }

	public string? AiProvider { get; set; } = "OpenAI";

	public string? AiModel { get; set; } = "gpt-4o-mini";

	public bool AiIsEnabled { get; set; } = true;
}
