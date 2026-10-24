using System.Collections.Generic;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class EBelgeDispatchResult
{
	public bool IsSuccess { get; set; }

	public string? ZarfId { get; set; }

	public string? EttnUuid { get; set; }

	public string? StatusCode { get; set; }

	public string? StatusDescription { get; set; }

	public string? ErrorMessage { get; set; }

	public List<SchematronValidationError> ValidationErrors { get; set; } = new List<SchematronValidationError>();
}
