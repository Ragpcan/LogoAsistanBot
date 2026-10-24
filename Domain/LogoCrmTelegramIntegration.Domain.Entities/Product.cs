using System;

namespace LogoCrmTelegramIntegration.Domain.Entities;

public class Product
{
	public Guid Oid { get; set; }

	public string ProductCode { get; set; } = string.Empty;
}
