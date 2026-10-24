using System;
using System.Collections.Generic;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public class DraftOrderDto
{
	public string DraftId { get; set; } = string.Empty;

	public long ChatId { get; set; }

	public string FirmCode { get; set; } = string.Empty;

	public string FirmName { get; set; } = string.Empty;

	public int ClientRef { get; set; }

	public List<LogoErpOrderLineInput> Lines { get; set; } = new List<LogoErpOrderLineInput>();

	public DateTime CreatedAt { get; set; } = DateTime.Now;
}
