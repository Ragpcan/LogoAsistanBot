using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class ProposalLogItem
{
	public Guid Id { get; set; } = Guid.NewGuid();

	public DateTime Timestamp { get; set; } = DateTime.Now;

	public string TelegramUser { get; set; } = string.Empty;

	public string RawMessage { get; set; } = string.Empty;

	public string FirmCode { get; set; } = string.Empty;

	public string ProductCode { get; set; } = string.Empty;

	public decimal Amount { get; set; }

	public bool IsSuccess { get; set; }

	public string Message { get; set; } = string.Empty;

	public Guid? ProposalOid { get; set; }

	public Guid? LineOid { get; set; }
}
