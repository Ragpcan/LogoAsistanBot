using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Persistence;

public class DraftOrderStore : IDraftOrderStore
{
	private readonly ConcurrentDictionary<string, DraftOrderDto> _drafts = new ConcurrentDictionary<string, DraftOrderDto>();

	private readonly ILogger<DraftOrderStore> _logger;

	public DraftOrderStore(ILogger<DraftOrderStore> logger)
	{
		_logger = logger;
	}

	public string SaveDraft(long chatId, string firmCode, string firmName, int clientRef, List<LogoErpOrderLineInput> lines)
	{
		DateTime cutoff = DateTime.Now.AddMinutes(-30.0);
		foreach (string item in (from kvp in _drafts
			where kvp.Value.CreatedAt < cutoff
			select kvp.Key).ToList())
		{
			_drafts.TryRemove(item, out DraftOrderDto _);
		}
		string text = "draft_" + Guid.NewGuid().ToString("N").Substring(0, 8);
		DraftOrderDto value2 = new DraftOrderDto
		{
			DraftId = text,
			ChatId = chatId,
			FirmCode = firmCode,
			FirmName = firmName,
			ClientRef = clientRef,
			Lines = (lines ?? new List<LogoErpOrderLineInput>()),
			CreatedAt = DateTime.Now
		};
		_drafts[text] = value2;
		_logger.LogInformation("Yeni Excel sipariş taslağı oluşturuldu: {DraftId} (Cari: {FirmCode}, Kalem: {Count})", text, firmCode, lines?.Count ?? 0);
		return text;
	}

	public DraftOrderDto? GetDraft(string draftId)
	{
		if (string.IsNullOrWhiteSpace(draftId))
		{
			return null;
		}
		_drafts.TryGetValue(draftId, out DraftOrderDto value);
		return value;
	}

	public void RemoveDraft(string draftId)
	{
		if (!string.IsNullOrWhiteSpace(draftId))
		{
			_drafts.TryRemove(draftId, out DraftOrderDto _);
		}
	}

	public LogoErpOrderPrintDto CreatePrintDtoFromDraft(DraftOrderDto draft, string salesmanName = "")
	{
		return new LogoErpOrderPrintDto
		{
			FicheNo = "TASLAK-" + draft.DraftId.Replace("draft_", "").ToUpper(),
			Date = DateTime.Now,
			FirmCode = draft.FirmCode,
			FirmName = draft.FirmName,
			SalesmanName = salesmanName,
			Description = "Excel'den Yüklenen Sipariş Önizleme Taslağı",
			Lines = draft.Lines.Select((LogoErpOrderLineInput l) => new LogoErpOrderPrintLineDto
			{
				ItemCode = l.ProductCode,
				ItemName = ((l.ItemDetails != null) ? ("Malzeme (" + l.ProductCode + ")") : l.ProductCode),
				Amount = l.Amount,
				UnitName = "ADET",
				ListPrice = l.UnitPrice.GetValueOrDefault(),
				DiscountRate = l.DiscountRate,
				UnitPrice = ((!l.UnitPrice.HasValue) ? 0m : ((l.DiscountRate > 0m) ? (l.UnitPrice.Value * (1m - l.DiscountRate / 100m)) : l.UnitPrice.Value)),
				VatRate = (l.ItemDetails?.Vat ?? 20.0)
			}).ToList()
		};
	}
}
