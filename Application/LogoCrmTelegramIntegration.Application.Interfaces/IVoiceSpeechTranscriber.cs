using System.Threading;
using System.Threading.Tasks;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IVoiceSpeechTranscriber
{
	Task<string?> TranscribeAudioAsync(byte[] audioBytes, string fileName, CancellationToken ct = default(CancellationToken));
}
