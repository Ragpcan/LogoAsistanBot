using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IProposalRepository
{
	Task<Guid?> GetFirmOidByCodeAsync(string firmCode, CancellationToken ct = default(CancellationToken));

	Task<Guid?> GetProductOidByCodeAsync(string productCode, CancellationToken ct = default(CancellationToken));

	Task<(Guid proposalOid, Guid lineOid)> CreateProposalWithLineTransactionAsync(Guid firmOid, Guid productOid, decimal amount, string description = "Telegram'dan Hızlı Sipariş", CancellationToken ct = default(CancellationToken));
}
