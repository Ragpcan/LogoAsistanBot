using System.Collections.Generic;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IProposalLogStore
{
	void AddLog(ProposalLogItem item);

	IReadOnlyList<ProposalLogItem> GetLogs(int count = 50);

	void ClearLogs();
}
