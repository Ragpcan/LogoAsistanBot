using System.Collections.Generic;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IChequeReportPdfGenerator
{
	byte[] GenerateChequeReportPdf(string firmCode, List<LogoErpChequeItemDto> cheques);
}
