namespace LogoCrmTelegramIntegration.WebUI.Controllers;

public class SendGibRequest
{
	public string FicheNo { get; set; } = string.Empty;

	public string DocumentType { get; set; } = "DESPATCHADVICE";
}
