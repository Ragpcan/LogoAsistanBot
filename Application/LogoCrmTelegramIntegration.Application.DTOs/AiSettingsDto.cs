namespace LogoCrmTelegramIntegration.Application.DTOs;

public class AiSettingsDto
{
    public string? Provider { get; set; } = "Gemini";
    public string? ApiKey { get; set; }
}
