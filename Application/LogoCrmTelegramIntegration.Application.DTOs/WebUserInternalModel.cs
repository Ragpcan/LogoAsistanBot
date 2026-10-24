namespace LogoCrmTelegramIntegration.Application.DTOs;

public class WebUserInternalModel : WebUserDto
{
	public string PasswordHash { get; set; } = string.Empty;
}
