using System.Collections.Generic;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class SchematronResult
{
	public bool IsValid { get; set; }

	public List<SchematronValidationError> Errors { get; set; } = new List<SchematronValidationError>();

	public string? RawSvrl { get; set; }
}
