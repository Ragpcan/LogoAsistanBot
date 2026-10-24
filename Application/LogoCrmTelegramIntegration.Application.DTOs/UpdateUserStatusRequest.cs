namespace LogoCrmTelegramIntegration.Application.DTOs;

public class UpdateUserStatusRequest
{
	public long ChatId { get; set; }

	public string Status { get; set; } = "Approved";
}
