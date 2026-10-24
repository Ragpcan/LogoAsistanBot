namespace LogoCrmTelegramIntegration.Application.DTOs;

public class EBelgeDispatchRequest
{
	public string FicheNo { get; set; } = string.Empty;

	public string DocumentType { get; set; } = "DESPATCHADVICE";

	public string FirmNo { get; set; } = "001";

	public string PeriodNo { get; set; } = "01";
}
