using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IProposalAppService
{
	Task<ProposalCreateResult> ProcessTelegramMessageAsync(string messageText, long? chatId = null, CancellationToken ct = default(CancellationToken));

	Task<byte[]?> GetStatementPdfAsync(string firmCode, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default(CancellationToken));

	Task<byte[]?> GetChequeReportPdfAsync(string firmCode, DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default(CancellationToken));

	Task<byte[]?> GetOrderPdfAsync(LogoErpOrderPrintDto orderPrintDto, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpFirmDto>> SearchFirmsAsync(string searchTerm, CancellationToken ct);

	Task<List<LogoErpFirmDto>> SearchFirmsAsync(string searchTerm, string? firmCyphCode = null, CancellationToken ct = default(CancellationToken));

	Task<List<LogoErpItemStockDto>> SearchItemStockAsync(string searchTerm, CancellationToken ct);

	Task<List<LogoErpItemStockDto>> SearchItemStockAsync(string searchTerm, string? itemCyphCode = null, CancellationToken ct = default(CancellationToken));

	Task<byte[]> GetFirmListExcelAsync(string listType, CancellationToken ct);

	Task<byte[]> GetFirmListExcelAsync(string listType, string? firmCyphCode = null, CancellationToken ct = default(CancellationToken));

	Task<byte[]> GetStockListExcelAsync(CancellationToken ct);

	Task<byte[]> GetStockListExcelAsync(string? itemCyphCode = null, CancellationToken ct = default(CancellationToken));
}
