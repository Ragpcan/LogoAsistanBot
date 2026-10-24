using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpOpenOrderSummaryDto
{
	public int LogicalRef { get; set; }

	public string FicheNo { get; set; } = string.Empty;

	public DateTime Date { get; set; }

	public string FirmCode { get; set; } = string.Empty;

	public string FirmName { get; set; } = string.Empty;

	public int LineCount { get; set; }

	public decimal GrossTotal { get; set; }

	public decimal NetTotal { get; set; }

	public string StatusText { get; set; } = "Açık Sipariş";

	public int EDespatch { get; set; }

	public int EInvoice { get; set; }

	public int EStatus { get; set; }

	public string GibStatusText
	{
		get
		{
			if (EDespatch == 1 || EInvoice == 1 || EInvoice == 2)
			{
				return EStatus switch
				{
					0 => "⏳ Taslak (Gönderilmedi)", 
					1 => "\ud83d\udce4 Entegratöre / GİB'e İletildi", 
					2 => "✅ GİB Tarafından Onaylandı", 
					3 => "❌ GİB Tarafından Reddedildi", 
					_ => $"ℹ\ufe0f GİB Durum ({EStatus})", 
				};
			}
			return "\ud83d\udcc4 Kağıt Belge";
		}
	}
}
