using System;

namespace LogoCrmTelegramIntegration.Domain.Entities;

public class Firm
{
	public Guid Oid { get; set; }

	public string FirmCode { get; set; } = string.Empty;
}
