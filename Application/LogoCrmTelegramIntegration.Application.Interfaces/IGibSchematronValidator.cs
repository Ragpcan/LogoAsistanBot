using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IGibSchematronValidator
{
	Task<SchematronResult> ValidateAsync(string ublXml, string documentType, CancellationToken ct = default(CancellationToken));

	bool IsSchematronAvailable(string documentType);
}
