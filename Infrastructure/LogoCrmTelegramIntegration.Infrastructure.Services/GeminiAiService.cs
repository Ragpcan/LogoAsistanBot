using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAiService> _logger;

    public GeminiAiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiAiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private (string apiKey, string provider, string model) GetAiConfig()
    {
        string apiKey = (_configuration["AiSettings:ApiKey"]
                      ?? _configuration["OpenAI:ApiKey"]
                      ?? _configuration["Groq:ApiKey"]
                      ?? string.Empty).Trim();

        string provider = (_configuration["AiSettings:Provider"] ?? string.Empty).Trim();
        string model = (_configuration["AiSettings:Model"] ?? string.Empty).Trim();

        // Akıllı Sağlayıcı & Model Tespiti
        if (apiKey.StartsWith("AQ.", StringComparison.OrdinalIgnoreCase) || apiKey.StartsWith("AIza", StringComparison.OrdinalIgnoreCase))
        {
            provider = "Gemini";
            if (string.IsNullOrWhiteSpace(model) || model.StartsWith("gpt", StringComparison.OrdinalIgnoreCase))
                model = "gemini-3.6-flash";
        }
        else if (apiKey.StartsWith("gsk_", StringComparison.OrdinalIgnoreCase))
        {
            provider = "Groq";
            if (string.IsNullOrWhiteSpace(model) || model.StartsWith("gpt", StringComparison.OrdinalIgnoreCase) || model.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
                model = "llama-3.3-70b-versatile";
        }
        else if (apiKey.StartsWith("sk-", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(provider) || provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                provider = "OpenAI";
            if (string.IsNullOrWhiteSpace(model) || model.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
                model = "gpt-4o-mini";
        }
        else if (string.IsNullOrWhiteSpace(provider))
        {
            provider = "Gemini";
            if (string.IsNullOrWhiteSpace(model)) model = "gemini-3.6-flash";
        }

        return (apiKey, provider, model);
    }

    private async Task<string> CallAiAsync(string systemInstruction, string userMessage, CancellationToken ct)
    {
        var (apiKey, provider, model) = GetAiConfig();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "Sistemde Yapay Zeka (AI) API Anahtarı tanımlı değil. Lütfen Yönetim Paneli > Ayarlar bölümünden API Anahtarınızı giriniz.";
        }

        bool isGemini = provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase) ||
                        apiKey.StartsWith("AQ.", StringComparison.OrdinalIgnoreCase) ||
                        apiKey.StartsWith("AIza", StringComparison.OrdinalIgnoreCase);

        if (isGemini)
        {
            return await CallGeminiWithFallbackAsync(apiKey, model, systemInstruction, userMessage, ct);
        }
        else
        {
            return await CallOpenAiCompatibleAsync(apiKey, provider, model, systemInstruction, userMessage, ct);
        }
    }

    private async Task<string> CallGeminiWithFallbackAsync(string apiKey, string model, string systemInstruction, string userMessage, CancellationToken ct)
    {
        // Candidate model list in case the selected model is not available or deprecated (e.g. 404)
        var candidateModels = new List<string>();
        string cleanModel = model.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(cleanModel))
        {
            candidateModels.Add(cleanModel);
        }

        // Çalışan Gemini modelleri (2024-2025 itibariyle aktif olanlar)
        var defaultFallbacks = new[] { "gemini-3.6-flash", "gemini-3.6-flash-lite", "gemini-1.5-flash", "gemini-1.5-flash-latest", "gemini-1.5-pro-latest" };
        foreach (var fb in defaultFallbacks)
        {
            if (!candidateModels.Contains(fb))
            {
                candidateModels.Add(fb);
            }
        }

        string lastError = string.Empty;

        foreach (var testModel in candidateModels)
        {
            // Try v1beta then v1
            var apiVersions = new[] { "v1beta", "v1" };
            foreach (var apiVer in apiVersions)
            {
                try
                {
                    string url = $"https://generativelanguage.googleapis.com/{apiVer}/models/{testModel}:generateContent?key={apiKey}";

                    object payload;
                    if (apiVer == "v1beta" && !string.IsNullOrWhiteSpace(systemInstruction))
                    {
                        payload = new
                        {
                            system_instruction = new { parts = new { text = systemInstruction } },
                            contents = new[]
                            {
                                new { parts = new[] { new { text = userMessage } } }
                            },
                            generationConfig = new { temperature = 0.2 }
                        };
                    }
                    else
                    {
                        string combinedPrompt = string.IsNullOrWhiteSpace(systemInstruction)
                            ? userMessage
                            : $"{systemInstruction}\n\nKullanıcı Talebi: {userMessage}";

                        payload = new
                        {
                            contents = new[]
                            {
                                new { parts = new[] { new { text = combinedPrompt } } }
                            },
                            generationConfig = new { temperature = 0.2 }
                        };
                    }

                    string jsonPayload = JsonSerializer.Serialize(payload);
                    using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(url, content, ct);
                    string responseString = await response.Content.ReadAsStringAsync(ct);

                    if (response.IsSuccessStatusCode)
                    {
                        using var document = JsonDocument.Parse(responseString);
                        var candidates = document.RootElement.GetProperty("candidates");
                        if (candidates.GetArrayLength() > 0)
                        {
                            var contentElem = candidates[0].GetProperty("content");
                            var parts = contentElem.GetProperty("parts");
                            if (parts.GetArrayLength() > 0)
                            {
                                var textResult = parts[0].GetProperty("text").GetString();
                                return textResult?.Trim() ?? string.Empty;
                            }
                        }
                    }
                    else
                    {
                        lastError = $"Gemini API ({apiVer}/{testModel}): {response.StatusCode} - {responseString}";
                        _logger.LogWarning("Gemini API denemesi başarısız: {Error}", lastError);
                    }
                }
                catch (Exception ex) when (!ct.IsCancellationRequested)
                {
                    lastError = ex.Message;
                    _logger.LogWarning(ex, "Gemini API çağrı hatası ({ApiVer}/{TestModel})", apiVer, testModel);
                }
            }
        }

        _logger.LogError("Tüm Gemini model denemeleri başarısız oldu. Son hata: {Error}", lastError);
        return $"Yapay zeka servisi yanıt vermedi. ({lastError})";
    }

    private async Task<string> CallOpenAiCompatibleAsync(string apiKey, string provider, string model, string systemInstruction, string userMessage, CancellationToken ct)
    {
        string endpoint;
        if (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase) || apiKey.StartsWith("gsk_"))
        {
            endpoint = "https://api.groq.com/openai/v1/chat/completions";
            if (string.IsNullOrWhiteSpace(model) || model.StartsWith("gemini", StringComparison.OrdinalIgnoreCase) || model.StartsWith("gpt", StringComparison.OrdinalIgnoreCase))
            {
                model = "llama-3.3-70b-versatile";
            }
        }
        else if (provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
        {
            endpoint = "https://api.deepseek.com/v1/chat/completions";
            if (string.IsNullOrWhiteSpace(model)) model = "deepseek-chat";
        }
        else
        {
            endpoint = "https://api.openai.com/v1/chat/completions";
            if (string.IsNullOrWhiteSpace(model) || model.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
            {
                model = "gpt-4o-mini";
            }
        }

        try
        {
            var messages = new List<object>();
            if (!string.IsNullOrWhiteSpace(systemInstruction))
            {
                messages.Add(new { role = "system", content = systemInstruction });
            }
            messages.Add(new { role = "user", content = userMessage });

            var payload = new
            {
                model = model,
                messages = messages,
                temperature = 0.2
            };

            string jsonPayload = JsonSerializer.Serialize(payload);
            using var reqMsg = new HttpRequestMessage(HttpMethod.Post, endpoint);
            reqMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            reqMsg.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(reqMsg, ct);
            string responseString = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI/Groq API hatası: {StatusCode} - {Body}", response.StatusCode, responseString);
                return $"Yapay zeka servisi yanıt vermedi. (Hata: {response.StatusCode} - {responseString})";
            }

            using var document = JsonDocument.Parse(responseString);
            var choices = document.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() > 0)
            {
                var messageElem = choices[0].GetProperty("message");
                var contentText = messageElem.GetProperty("content").GetString();
                return contentText?.Trim() ?? string.Empty;
            }

            return "Yapay zeka yanıtı boş döndü.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI/Groq API çağrısı sırasında istisna oluştu.");
            return $"Yapay zeka isteği işlenemedi: {ex.Message}";
        }
    }

    public async Task<string> GenerateSqlAsync(string userMessage, string firmNo, string periodNo, string? conversationHistory = null, CancellationToken ct = default)
    {
        string historyContext = string.IsNullOrWhiteSpace(conversationHistory) ? "" : $"\n\nGeçmiş Sohbet Bağlamı:\n{conversationHistory}\n";

        string systemPrompt = $@"Sen Logo ERP SQL Veritabanı ve Şirket Yönetim Uzmanı olan akıllı bir Yapay Zeka Ajanısın (AI Agent).
Kullanıcının Türkçe talebini ve varsa sohbet geçmişini analiz edip, Logo ERP SQL Server veritabanından en doğru sonucu getirecek tek bir salt-okunur (SELECT) SQL sorgusu üreteceksin.
Eğer kullanıcının mesajı kesinlikle bir veritabanı sorgusu DEĞİLSE (örneğin sadece 'naber', 'selam' gibi selamlama ise), SADECE 'NOT_SQL' yaz.
Sorgunun başına ve sonuna ```sql veya benzeri markdown formatları KOYMA. SADECE DÜZ SQL SORGUSU YAZ.

Logo ERP Veritabanı Bilgileri:
Firma No: {firmNo}, Dönem No: {periodNo}

Tablolar ve Kesin Sütunlar (DİKKATLE İNCELE):
1. Cari Kartlar: LG_{firmNo}_CLCARD
   - Sütunlar: LOGICALREF, CODE (Cari Kodu), DEFINITION_ (Cari Ünvanı), TAXNR, TCKNO, TELNRS1, CITY, BALANCE, ACTIVE (0=Aktif, 1=Pasif), CARDTYPE
   - DİKKAT: CLCARD tablosunda 'CANCELLED' adında bir sütun YOKTUR! Asla CLCARD üzerinde CANCELLED filtresi yazma!

2. Malzemeler (Stok): LG_{firmNo}_ITEMS
   - Sütunlar: LOGICALREF, CODE (Malzeme Kodu), NAME (Malzeme Adı), STGRPCODE (Grup Kodu), PRODSTATUS, ACTIVE (0=Aktif, 1=Pasif)
   - DİKKAT: ITEMS tablosunda 'CANCELLED' sütunu YOKTUR!

3. Faturalar (Satış & Alış): LG_{firmNo}_{periodNo}_INVOICE
   - Sütunlar: LOGICALREF, FICHENO (Fatura No), DATE_ (Tarih), CLIENTREF (CLCARD.LOGICALREF), NETTOTAL (Net Tutar), GROSSTOTAL, TOTALVAT, TRCODE (8=Toptan Satış, 7=Perakende Satış, 1=Alış), CANCELLED (0=Geçerli, 1=İptal)

4. İrsaliyeler: LG_{firmNo}_{periodNo}_STFICHE
   - Sütunlar: LOGICALREF, FICHENO (İrsaliye No), DATE_ (Tarih), CLIENTREF, GROSSTOTAL, NETTOTAL, TRCODE, BILLED (0=Açık İrsaliye, 1=Faturalanmış), CANCELLED (0=Geçerli, 1=İptal)

5. Fatura & İrsaliye Satırları: LG_{firmNo}_{periodNo}_STLINE
   - Sütunlar: LOGICALREF, INVOICEREF (INVOICE.LOGICALREF), STFICHE_REF (STFICHE.LOGICALREF), STOCKREF (ITEMS.LOGICALREF), CLIENTREF, AMOUNT (Miktar), PRICE (Birim Fiyat), TOTAL (Tutar), DATE_, TRCODE (1=Alış, 7,8=Satış), LINETYPE (0=Malzeme), CANCELLED (0=Geçerli, 1=İptal)

6. Siparişler: LG_{firmNo}_{periodNo}_ORFICHE (TRCODE: 1=Alınan Sipariş, STATUS: 1=Öneri, 2=Onaylı, 4=Sevk Edilebilir, CANCELLED) & LG_{firmNo}_{periodNo}_ORFLINE
7. Bankalar: LG_{firmNo}_BNCARD (LOGICALREF, CODE, DEFINITION_) & LG_{firmNo}_BANKACC (LOGICALREF, BANKREF, CODE, DEFINITION_, IBAN)
8. Kasalar: LG_{firmNo}_KSCARD (LOGICALREF, CODE, NAME) & LG_{firmNo}_{periodNo}_KSLINES (CANCELLED)
9. Çek/Senet: LG_{firmNo}_{periodNo}_CSCARD (DOC: 1=Çek, 2=Senet, CURRSTAT: 1=Portföyde, PORTFOYNO=Çek No) JOIN LG_{firmNo}_{periodNo}_CSTRANS ON CSTRANS.CSREF = CSCARD.LOGICALREF (CSTRANS.CARDREF = CLCARD.LOGICALREF)
10. Muhasebe: LG_{firmNo}_EMUHACC (LOGICALREF, CODE, DEFINITION_) & LG_{firmNo}_{periodNo}_EMFLINE (ACCOUNTREF, DATE_, TRNET, SIGN (0=Borç, 1=Alacak), CANCELLED)

Kritik Sorgulama Kuralları:
- Cari ararken: SELECT TOP 15 LOGICALREF, CODE AS CariKodu, DEFINITION_ AS CariUnvani, CITY AS Sehir, TELNRS1 AS Telefon FROM LG_{firmNo}_CLCARD WITH (NOLOCK) WHERE (CODE LIKE '%aranan%' OR DEFINITION_ LIKE '%aranan%') AND CARDTYPE <> 22 ORDER BY CODE ASC
- Bir carinin aldığı ürünleri (Satış) ararken: STLINE TRCODE IN (7, 8). Cari bizden 'alış' yaptıysa TRCODE IN (1) kullan (Satınalma/Alış).
- Mizan (Trial Balance) Hesaplama: EMFLINE tablosunu EMUHACC (EMFLINE.ACCOUNTREF = EMUHACC.LOGICALREF) ile JOIN yap. SIGN=0 ise TRNET Borçtur, SIGN=1 ise TRNET Alacaktır. DATE_ filtresi uygulayarak SUM(TRNET) hesapla.
- 'CANCELLED = 0' filtresini SADECE INVOICE, STFICHE, STLINE, ORFICHE, KSLINES, BNFLINE, EMFLINE tablolarında kullan. CLCARD, ITEMS, BNCARD, BANKACC tablolarında KESİNLİKLE KULLANMA!{historyContext}

Kullanıcı Talebi: '{userMessage}'
Sadece T-SQL sorgusunu yaz (veya 'NOT_SQL'):";

        string response = await CallAiAsync(systemPrompt, userMessage, ct);
        response = response.Replace("```sql", "").Replace("```", "").Trim();
        return response;
    }

    public async Task<string> GenerateHumanResponseAsync(string userMessage, string sqlQuery, string jsonData, string? conversationHistory = null, CancellationToken ct = default)
    {
        string historyContext = string.IsNullOrWhiteSpace(conversationHistory) ? "" : $"\n\nGeçmiş Sohbet Bağlamı:\n{conversationHistory}\n";

        string systemPrompt = $@"Sen Logo ERP / CRM sistemine tam entegre, OpenClaw/Hermes/ChatGPT/Gemini seviyesinde çalışan akıllı bir Kurumsal Asistansın (AI Data Agent).
Kullanıcının sorusuna Logo ERP veritabanından çekilen canlı JSON verilerini inceleyerek Türkçe, samimi, net, profesyonel ve analitik bir yanıt ver.

Kurallar:
1. Sayıları, miktarları ve para tutarlarını (TL cinsinden) okunaklı formatta belirt (örn: 10 Adet, 45.000,00 TL).
2. Tarihleri GG.AA.YYYY formatında sun.
3. Eğer JSON verisi içinde kayıtlar varsa; ürün kodları, ürün adları, miktarlar, birim fiyatlar ve fatura numaralarıyla madde madde anlaşılır şekilde listele.
4. Eğer kullanıcı 'bu ay' gibi belirli bir dönem sormuş ancak o dönemde işlem bulunamamışsa veya genel liste gelmişse, 'Bu ay için işlem bulunamadı, ancak carinin gerçekleştirdiği son alımlar şunlardır:' diyerek kullanıcıyı bilgilendir.
5. Eğer cari listesi geldiyse; carilerin kodlarını ve unvanlarını listeleyerek hangi cari hakkında detay istediğini sor.{historyContext}";

        string input = $"Kullanıcı Sorusu: {userMessage}\n\nÇalıştırılan SQL: {sqlQuery}\n\nVeritabanı Sonucu (JSON):\n{jsonData}";
        return await CallAiAsync(systemPrompt, input, ct);
    }

    public async Task<string> GenerateGeneralResponseAsync(string userMessage, string? conversationHistory = null, CancellationToken ct = default)
    {
        string historyContext = string.IsNullOrWhiteSpace(conversationHistory) ? "" : $"\n\nGeçmiş Sohbet Bağlamı:\n{conversationHistory}\n";

        string systemPrompt = $@"Sen Logo ERP & CRM Telegram Akıllı Asistanısın (OpenClaw / Hermes tarzı zeki bir asistan).
Kullanıcı seninle sohbet ediyor, önceki konuşmayı devam ettiriyor veya yardım istiyor.
Sohbet geçmişini ve kullanıcının mesajını dikkate alarak samimi, bilgili ve doğrudan konuya odaklı Türkçe bir yanıt ver.{historyContext}";

        return await CallAiAsync(systemPrompt, userMessage, ct);
    }

    /// <summary>
    /// Gerçek çok turlu (multi-turn) sohbet API çağrısı.
    /// Gemini için: contents dizisini user/model sırasıyla doldurur.
    /// Groq/OpenAI için: messages dizisini user/assistant sırasıyla doldurur.
    /// </summary>
    public async Task<string> ChatAsync(string systemPrompt, List<(string Role, string Content)> history, string newUserMessage, CancellationToken ct = default)
    {
        var (apiKey, provider, model) = GetAiConfig();

        if (string.IsNullOrWhiteSpace(apiKey))
            return "⚠️ AI API anahtarı tanımlı değil. Ayarlar > AI Ayarları bölümünden ekleyiniz.";

        bool isGemini = provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase)
                     || apiKey.StartsWith("AQ.", StringComparison.OrdinalIgnoreCase)
                     || apiKey.StartsWith("AIza", StringComparison.OrdinalIgnoreCase);

        if (isGemini)
            return await GeminiMultiTurnChatAsync(apiKey, model, systemPrompt, history, newUserMessage, ct);
        else
            return await OpenAiMultiTurnChatAsync(apiKey, provider, model, systemPrompt, history, newUserMessage, ct);
    }

    private async Task<string> GeminiMultiTurnChatAsync(string apiKey, string model, string systemPrompt, List<(string Role, string Content)> history, string newUserMessage, CancellationToken ct)
    {
        var candidateModels = new List<string>();
        if (!string.IsNullOrWhiteSpace(model)) candidateModels.Add(model.Trim().ToLowerInvariant());
        foreach (var fb in new[] { "gemini-3.6-flash", "gemini-1.5-flash", "gemini-1.5-pro-latest" })
            if (!candidateModels.Contains(fb)) candidateModels.Add(fb);

        // Gemini "contents" dizisi: user / model sıralı turlar
        var contents = new List<object>();
        foreach (var (role, content) in history)
        {
            string geminiRole = role.Equals("assistant", StringComparison.OrdinalIgnoreCase) || role.Equals("model", StringComparison.OrdinalIgnoreCase) ? "model" : "user";
            contents.Add(new { role = geminiRole, parts = new[] { new { text = content } } });
        }
        // Son kullanıcı mesajı
        contents.Add(new { role = "user", parts = new[] { new { text = newUserMessage } } });

        string lastError = "";
        foreach (var testModel in candidateModels)
        {
            foreach (var apiVer in new[] { "v1beta", "v1" })
            {
                try
                {
                    string url = $"https://generativelanguage.googleapis.com/{apiVer}/models/{testModel}:generateContent?key={apiKey}";
                    object payload;
                    if (apiVer == "v1beta" && !string.IsNullOrWhiteSpace(systemPrompt))
                    {
                        payload = new
                        {
                            system_instruction = new { parts = new { text = systemPrompt } },
                            contents = contents,
                            generationConfig = new { temperature = 0.7, maxOutputTokens = 2048 }
                        };
                    }
                    else
                    {
                        payload = new
                        {
                            contents = contents,
                            generationConfig = new { temperature = 0.7, maxOutputTokens = 2048 }
                        };
                    }

                    string jsonPayload = JsonSerializer.Serialize(payload);
                    using var reqContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(url, reqContent, ct);
                    string responseStr = await response.Content.ReadAsStringAsync(ct);

                    if (response.IsSuccessStatusCode)
                    {
                        using var doc = JsonDocument.Parse(responseStr);
                        var candidates = doc.RootElement.GetProperty("candidates");
                        if (candidates.GetArrayLength() > 0)
                        {
                            var parts = candidates[0].GetProperty("content").GetProperty("parts");
                            if (parts.GetArrayLength() > 0)
                                return parts[0].GetProperty("text").GetString()?.Trim() ?? "";
                        }
                    }
                    else
                    {
                        lastError = $"{testModel}/{apiVer}: {response.StatusCode}";
                        _logger.LogWarning("Gemini chat denemesi başarısız: {Err}", lastError);
                    }
                }
                catch (Exception ex) when (!ct.IsCancellationRequested)
                {
                    lastError = ex.Message;
                    _logger.LogWarning(ex, "Gemini chat hatası ({Model}/{Ver})", testModel, apiVer);
                }
            }
        }
        return $"⚠️ AI yanıt veremedi. ({lastError})";
    }

    private async Task<string> OpenAiMultiTurnChatAsync(string apiKey, string provider, string model, string systemPrompt, List<(string Role, string Content)> history, string newUserMessage, CancellationToken ct)
    {
        string endpoint;
        if (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase) || apiKey.StartsWith("gsk_"))
        {
            endpoint = "https://api.groq.com/openai/v1/chat/completions";
            if (string.IsNullOrWhiteSpace(model) || model.StartsWith("gemini") || model.StartsWith("gpt"))
                model = "llama-3.3-70b-versatile";
        }
        else if (provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
        {
            endpoint = "https://api.deepseek.com/v1/chat/completions";
            if (string.IsNullOrWhiteSpace(model)) model = "deepseek-chat";
        }
        else
        {
            endpoint = "https://api.openai.com/v1/chat/completions";
            if (string.IsNullOrWhiteSpace(model) || model.StartsWith("gemini")) model = "gpt-4o-mini";
        }

        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(new { role = "system", content = systemPrompt });
        foreach (var (role, content) in history)
        {
            string oaiRole = role.Equals("model", StringComparison.OrdinalIgnoreCase) ? "assistant" : role.ToLowerInvariant();
            messages.Add(new { role = oaiRole, content = content });
        }
        messages.Add(new { role = "user", content = newUserMessage });

        try
        {
            var payload = new { model, messages, temperature = 0.7 };
            string jsonPayload = JsonSerializer.Serialize(payload);
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(req, ct);
            string responseStr = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                return $"⚠️ AI servisi yanıt vermedi. ({response.StatusCode})";
            using var doc = JsonDocument.Parse(responseStr);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()?.Trim() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI/Groq multi-turn chat hatası");
            return $"⚠️ Bağlantı hatası: {ex.Message}";
        }
    }
}
