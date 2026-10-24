using System.Threading;
using System.Threading.Tasks;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IEBelgeWebServiceClient
{
	Task<string> LoginAsync(CancellationToken ct = default(CancellationToken));

	Task<string> SendDocumentAsync(string sessionId, string base64ZippedXml, string documentType, CancellationToken ct = default(CancellationToken));

	Task<(string StatusCode, string StatusDescription)> GetDocumentStatusAsync(string sessionId, string zarfId, CancellationToken ct = default(CancellationToken));

	Task<bool> PingAsync(CancellationToken ct = default(CancellationToken));
}
