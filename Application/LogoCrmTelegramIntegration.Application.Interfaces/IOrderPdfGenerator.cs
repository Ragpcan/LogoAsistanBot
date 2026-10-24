using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IOrderPdfGenerator
{
	byte[] GenerateOrderPdf(LogoErpOrderPrintDto order);
}
