using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IStandardPdfGenerator
{
	byte[] GenerateStandardDispatchPdf(LogoErpDispatchDto dispatch, bool isInvoice = false);
}
