namespace LogoCrmTelegramIntegration.Application.DTOs;

public class EBelgeWebServiceSettings
{
	public string ServiceUrl { get; set; } = string.Empty;

	public string Username { get; set; } = string.Empty;

	public string Password { get; set; } = string.Empty;

	public string MukellefVkn { get; set; } = string.Empty;

	public string FirmaKodu { get; set; } = string.Empty;

	public EBelgeDocumentTypeConfig DocumentTypes { get; set; } = new EBelgeDocumentTypeConfig();

	public string SchematronDirectory { get; set; } = "Schematron";

	public int StatusPollIntervalMinutes { get; set; } = 5;

	public bool TestMode { get; set; } = true;

	public int SessionTimeoutMinutes { get; set; } = 30;
}
