using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class WebUserDto
{
	public string Username { get; set; } = string.Empty;

	public string FullName { get; set; } = string.Empty;

	public string Role { get; set; } = "Admin";

	public DateTime CreatedAt { get; set; } = DateTime.Now;

	public bool CanAccessDashboard { get; set; } = true;

	public bool CanAccessSalesmen { get; set; } = true;

	public bool CanAccessLiveTransactions { get; set; } = true;

	public bool CanAccessXslt { get; set; } = true;

	public bool CanAccessAdminSettings { get; set; } = true;
}
