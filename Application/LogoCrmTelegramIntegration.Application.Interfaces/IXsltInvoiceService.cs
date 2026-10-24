using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IXsltInvoiceService
{
	Task<string> RenderInvoiceXsltHtmlAsync(LogoErpDispatchDto invoice, string? customXsltPath = null, CancellationToken ct = default(CancellationToken));

	Task<byte[]> RenderInvoiceXsltBytesAsync(LogoErpDispatchDto invoice, string? customXsltPath = null, CancellationToken ct = default(CancellationToken));

	Task<byte[]> RenderInvoicePdfAsync(LogoErpDispatchDto invoice, string? customXsltPath = null, CancellationToken ct = default(CancellationToken));

	Task<string> DetermineInvoiceTypeAsync(string firmCode, CancellationToken ct = default(CancellationToken));
}
