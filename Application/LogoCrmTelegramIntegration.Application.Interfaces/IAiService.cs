using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public interface IAiService
{
    Task<string> GenerateSqlAsync(string userMessage, string firmNo, string periodNo, string? conversationHistory = null, CancellationToken ct = default);
    Task<string> GenerateHumanResponseAsync(string userMessage, string sqlQuery, string jsonData, string? conversationHistory = null, CancellationToken ct = default);
    Task<string> GenerateGeneralResponseAsync(string userMessage, string? conversationHistory = null, CancellationToken ct = default);

    /// <summary>
    /// Gerçek çok turlu sohbet: Geçmiş mesajları içeren tam chat API çağrısı yapar.
    /// </summary>
    /// <param name="systemPrompt">Bota verilecek rol/talimat</param>
    /// <param name="history">Önceki mesajlar (role: "user"/"model", content: metin)</param>
    /// <param name="newUserMessage">Kullanıcının yeni mesajı</param>
    Task<string> ChatAsync(string systemPrompt, List<(string Role, string Content)> history, string newUserMessage, CancellationToken ct = default);
}

