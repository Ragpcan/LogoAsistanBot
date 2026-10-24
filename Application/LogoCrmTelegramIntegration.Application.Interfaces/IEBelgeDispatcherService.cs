using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IEBelgeDispatcherService
{
	Task<EBelgeDispatchResult> DispatchDocumentAsync(EBelgeDispatchRequest request, CancellationToken ct = default(CancellationToken));

	Task<EBelgeDispatchResult> PollDocumentStatusAsync(string zarfId, string documentType, CancellationToken ct = default(CancellationToken));
}
