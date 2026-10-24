using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IXsltDispatchService
{
	Task<string> RenderDispatchXsltHtmlAsync(LogoErpDispatchDto dispatch, string? customXsltPath = null, CancellationToken ct = default(CancellationToken));

	Task<byte[]> RenderDispatchXsltBytesAsync(LogoErpDispatchDto dispatch, string? customXsltPath = null, CancellationToken ct = default(CancellationToken));

	Task<byte[]> RenderDispatchPdfAsync(LogoErpDispatchDto dispatch, string? customXsltPath = null, CancellationToken ct = default(CancellationToken));

	Task<string> BuildUblTrXmlPublicAsync(LogoErpDispatchDto dispatch, LogoCompanyInfoDto? compInfo = null, CancellationToken ct = default(CancellationToken));
}
