using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;

namespace LogoCrmTelegramIntegration.Application.Services;

public class ProposalLogStore : IProposalLogStore
{
	private readonly ConcurrentQueue<ProposalLogItem> _logs = new ConcurrentQueue<ProposalLogItem>();

	private const int MaxLogCount = 100;

	public void AddLog(ProposalLogItem item)
	{
		_logs.Enqueue(item);
		while (_logs.Count > 100)
		{
			_logs.TryDequeue(out ProposalLogItem _);
		}
	}

	public IReadOnlyList<ProposalLogItem> GetLogs(int count = 50)
	{
		return _logs.Reverse().Take(count).ToList();
	}

	public void ClearLogs()
	{
		_logs.Clear();
	}
}
