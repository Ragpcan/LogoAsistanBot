using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface ICariStatementPdfGenerator
{
	byte[] GenerateStatementPdf(LogoErpFirmStatementDto statement);
}
