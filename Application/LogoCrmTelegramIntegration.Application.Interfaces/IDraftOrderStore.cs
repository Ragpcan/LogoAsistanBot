using System.Collections.Generic;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IDraftOrderStore
{
	string SaveDraft(long chatId, string firmCode, string firmName, int clientRef, List<LogoErpOrderLineInput> lines);

	DraftOrderDto? GetDraft(string draftId);

	void RemoveDraft(string draftId);

	LogoErpOrderPrintDto CreatePrintDtoFromDraft(DraftOrderDto draft, string salesmanName = "");
}
