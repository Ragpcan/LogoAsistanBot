using System;

namespace LogoCrmTelegramIntegration.Application.DTOs;

public class LogoErpStatementLineDto
{
	public DateTime Date { get; set; }

	public string TranNo { get; set; } = string.Empty;

	/// <summary>Fiş türü kodu (Logo TRCODE değeri)</summary>
	public int TranTypeCode { get; set; }

	/// <summary>Fiş türü metin açıklaması (Toptan Satış Faturası, Açılış Fişi, vb.)</summary>
	public string TranType { get; set; } = string.Empty;

	public string LineExp { get; set; } = string.Empty;

	public decimal Debit { get; set; }

	public decimal Credit { get; set; }

	public int TrCurr { get; set; }

	public string CurrencySymbol { get; set; } = "TL";

	public decimal FxAmount { get; set; }

	/// <summary>Bu satır itibariyle kümülatif bakiye (PDF'de Bakiye kolonu için)</summary>
	public decimal RunningBalance { get; set; }
}
