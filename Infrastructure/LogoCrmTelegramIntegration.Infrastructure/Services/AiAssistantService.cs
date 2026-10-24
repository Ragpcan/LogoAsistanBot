using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LogoCrmTelegramIntegration.Infrastructure.Persistence;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class AiAssistantService : IAiAssistantService
{
    private readonly IConfiguration _configuration;
    private readonly ILogoErpRepository _logoErpRepository;
    private readonly ILogger<AiAssistantService> _logger;
    private readonly IAiService _geminiAiService;
    private readonly IDynamicSqlExecutor _dynamicSqlExecutor;
    private readonly ICustomReportStore? _customReportStore;
    private readonly IReportHistoryStore? _reportHistoryStore;
    private static readonly HttpClient _httpClient = new HttpClient();

    public AiAssistantService(
        IConfiguration configuration,
        ILogoErpRepository logoErpRepository,
        ILogger<AiAssistantService> logger,
        IAiService geminiAiService,
        IDynamicSqlExecutor dynamicSqlExecutor,
        ICustomReportStore? customReportStore = null,
        IReportHistoryStore? reportHistoryStore = null)
    {
        _configuration = configuration;
        _logoErpRepository = logoErpRepository;
        _logger = logger;
        _geminiAiService = geminiAiService;
        _dynamicSqlExecutor = dynamicSqlExecutor;
        _customReportStore = customReportStore;
        _reportHistoryStore = reportHistoryStore;
    }

    public async Task<AiIntentResult> ParseIntentAndParametersAsync(string textInput, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(textInput))
        {
            return new AiIntentResult { Intent = "Unknown", Explanation = "Metin boş" };
        }

        string apiKey = _configuration["OpenAI:ApiKey"] ?? _configuration["Groq:ApiKey"] ?? _configuration["SpeechToText:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                var apiResult = await CallLlmApiAsync(textInput, apiKey, ct);
                if (apiResult != null) return apiResult;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "LLM API analizi başarısız oldu, yerel NLP motoruna geçiliyor.");
            }
        }

        // Yerel NLP Kural Motoru (Fallback)
        return ParseWithLocalNlp(textInput);
    }

    private async Task<AiIntentResult?> CallLlmApiAsync(string textInput, string apiKey, CancellationToken ct)
    {
        string endpoint = _configuration["Groq:ApiKey"] != null 
            ? "https://api.groq.com/openai/v1/chat/completions"
            : "https://api.openai.com/v1/chat/completions";
        string model = _configuration["Groq:ApiKey"] != null ? "llama-3.3-70b-versatile" : "gpt-4o-mini";

        var systemPrompt = @"Sen bir ERP/CRM Asistanısın. Kullanıcı Türkçe sesli veya yazılı komut verir.
Aşağıdaki JSON formatında yanıt dönmek zorundasın. Başka hiçbir açıklama yazma!
JSON Formatı:
{
  ""intent"": ""Order"" | ""Dispatch"" | ""Risk"" | ""Report"" | ""Unknown"",
  ""customerCode"": ""cari kodu veya null"",
  ""customerTitle"": ""cari unvanı veya null"",
  ""productCode"": ""ürün kodu veya null"",
  ""productName"": ""ürün adı veya null"",
  ""quantity"": 1.0,
  ""discountPercent"": 0.0,
  ""notes"": ""varsa ek notlar veya null"",
  ""explanation"": ""Kullanıcı isteğinin kısa Türkçe özeti""
}";

        var payload = new
        {
            model = model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = textInput }
            },
            temperature = 0.1,
            response_format = new { type = "json_object" }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var resp = await _httpClient.SendAsync(req, ct);
        if (resp.IsSuccessStatusCode)
        {
            var jsonStr = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(jsonStr);
            var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            if (!string.IsNullOrWhiteSpace(content))
            {
                using var parsedDoc = JsonDocument.Parse(content);
                var root = parsedDoc.RootElement;
                return new AiIntentResult
                {
                    Intent = root.TryGetProperty("intent", out var i) ? i.GetString() ?? "Unknown" : "Unknown",
                    CustomerCode = root.TryGetProperty("customerCode", out var cc) ? cc.GetString() : null,
                    CustomerTitle = root.TryGetProperty("customerTitle", out var ct2) ? ct2.GetString() : null,
                    ProductCode = root.TryGetProperty("productCode", out var pc) ? pc.GetString() : null,
                    ProductName = root.TryGetProperty("productName", out var pn) ? pn.GetString() : null,
                    Quantity = root.TryGetProperty("quantity", out var q) && q.TryGetDouble(out var qv) ? qv : 1.0,
                    DiscountPercent = root.TryGetProperty("discountPercent", out var d) && d.TryGetDouble(out var dv) ? dv : 0.0,
                    Notes = root.TryGetProperty("notes", out var n) ? n.GetString() : null,
                    Explanation = root.TryGetProperty("explanation", out var exp) ? exp.GetString() ?? "" : ""
                };
            }
        }
        return null;
    }

    private AiIntentResult ParseWithLocalNlp(string input)
    {
        string lower = input.ToLower();
        var result = new AiIntentResult();

        if (lower.Contains("sipariş") || lower.Contains("siparis"))
        {
            result.Intent = "Order";
        }
        else if (lower.Contains("irsaliye") || lower.Contains("sevk"))
        {
            result.Intent = "Dispatch";
        }
        else if (lower.Contains("risk") || lower.Contains("bakiye") || lower.Contains("borç") || lower.Contains("borc"))
        {
            result.Intent = "Risk";
        }
        else if (lower.Contains("rapor") || lower.Contains("özet") || lower.Contains("ozet") || lower.Contains("ciro"))
        {
            result.Intent = "Report";
        }
        else
        {
            result.Intent = "Order"; // Varsayılan olarak sipariş taslağı adayı
        }

        // Miktar Çıkarma (Örn: 50 adet, 10 koli)
        var qtyMatch = Regex.Match(input, @"(\d+(?:[.,]\d+)?)\s*(?:adet|koli|tane|kg|paket|parça|parca)?", RegexOptions.IgnoreCase);
        if (qtyMatch.Success && double.TryParse(qtyMatch.Groups[1].Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture, out double qty))
        {
            result.Quantity = qty;
        }

        // İskonto Çıkarma (Örn: %10, yüzde 15)
        var discMatch = Regex.Match(input, @"(?:%|yüzde|yuzde)\s*(\d+(?:[.,]\d+)?)", RegexOptions.IgnoreCase);
        if (discMatch.Success && double.TryParse(discMatch.Groups[1].Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture, out double disc))
        {
            result.DiscountPercent = disc;
        }

        result.Explanation = $"Yerel NLP Algılaması: {result.Intent} işlemi, Miktar: {result.Quantity}, İskonto: %{result.DiscountPercent}";
        return result;
    }

    public async Task<CustomerRiskReport> EvaluateCustomerRiskAsync(string customerCodeOrTitle, CancellationToken ct = default)
    {
        var report = new CustomerRiskReport
        {
            CustomerCode = customerCodeOrTitle,
            CustomerTitle = customerCodeOrTitle
        };

        try
        {
            var firmNo = _configuration["LogoErp:FirmNo"] ?? "001";
            var periodNo = _configuration["LogoErp:PeriodNo"] ?? "01";

            var statement = await _logoErpRepository.GetFirmStatementAsync(customerCodeOrTitle, firmNo, periodNo, null, null, ct);
            if (statement != null)
            {
                report.CustomerCode = string.IsNullOrWhiteSpace(statement.FirmCode) ? customerCodeOrTitle : statement.FirmCode;
                report.CustomerTitle = string.IsNullOrWhiteSpace(statement.FirmName) ? customerCodeOrTitle : statement.FirmName;
                report.TotalBalance = statement.Balance;

                var txList = statement.RecentTransactions ?? new List<LogoErpStatementLineDto>();
                var debitTxList = txList.Where(t => t.Debit > 0).ToList();
                report.OpenInvoiceCount = debitTxList.Count;

                var now = DateTime.Now;
                var overdueTxList = debitTxList.Where(t => (now - t.Date).TotalDays > 30).ToList();
                report.OverdueInvoiceCount = overdueTxList.Count;
                report.OverdueBalance = overdueTxList.Sum(t => t.Debit);

                if (overdueTxList.Any())
                {
                    report.AveragePaymentDelayDays = (int)overdueTxList.Average(t => (now - t.Date).TotalDays);
                }
            }

            // Risk Seviyesi Değerlendirmesi
            if (report.OverdueInvoiceCount > 2 || report.OverdueBalance > 50000 || report.AveragePaymentDelayDays > 45)
            {
                report.RiskLevel = "YÜKSEK";
                report.AiRecommendation = $"⚠️ **YÜKSEK RİSK**: Müşterinin {report.OverdueInvoiceCount} adet vadesi geçmiş borcu ({report.OverdueBalance:N2} TL) bulunmaktadır. Yeni sipariş veya irsaliye öncesi mutlaka tahsilat yapılması önerilir.";
            }
            else if (report.OverdueInvoiceCount > 0 || report.TotalBalance > 20000)
            {
                report.RiskLevel = "ORTA";
                report.AiRecommendation = $"⚡ **ORTA RİSK**: Müşterinin {report.TotalBalance:N2} TL açık bakiyesi mevcuttur. İşlem öncesi bakiye kontrolü yapılması tavsiye edilir.";
            }
            else
            {
                report.RiskLevel = "DÜŞÜK";
                report.AiRecommendation = "✅ **GÜVENLİ**: Müşterinin vadesi geçmiş borcu bulunmamaktadır. Sipariş veya irsaliye güvenle onaylanabilir.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cari risk analizi hesaplanırken hata oluştu: {Customer}", customerCodeOrTitle);
            report.RiskLevel = "BİLİNMİYOR";
            report.AiRecommendation = "ℹ️ Cari hesap verileri alınırken hata oluştu.";
        }

        return report;
    }

    // =====================================================================
    // AKILLI YÖNLENDIRICI: Sorguyu analiz edip doğru pipeline'a gönderir
    // Öncelik: Yerel Motor → Cari Yönlendirme → API (sadece karmaşık sorgular)
    // =====================================================================

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<long, AgentSession> _agentSessions = new();

    private class AgentSession
    {
        public string? PendingIntent { get; set; }      // "urun_raporu", "ekstre", "cek", vb.
        public string? PendingQuery { get; set; }       // Kullanıcının orijinal sorusu
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public List<(string Role, string Content)> History { get; set; } = new();
        public bool IsSohbetMode { get; set; } = false; // true = direkt AI sohbet modu
    }

    private static readonly HashSet<string> _greetings = new(StringComparer.OrdinalIgnoreCase)
    {
        "naber", "ne haber", "selam", "merhaba", "günaydın", "gunaydin",
        "iyi akşamlar", "iyi günler", "nasılsın", "nasilsin", "hey", "alo",
        "kimsin", "yardım", "yardim", "hi", "hello", "kolay gelsin"
    };

    private static bool IsGreeting(string q) =>
        _greetings.Contains(q.Trim()) || (_greetings.Any(g => q.Trim().Equals(g, StringComparison.OrdinalIgnoreCase)));

    // Sorguda cari adı var mı ve ne tür işlem isteniyor?
    private static (bool HasCariHint, string CariTerm, string IntentType) ExtractCariIntent(string query)
    {
        string q = query.ToLowerInvariant();

        // İşlem tipi tespiti
        string intent = "genel";
        if (q.Contains("satt") || q.Contains("verdiğimiz") || q.Contains("aldığı ürün") || q.Contains("hangi ürün") || q.Contains("fatura")) intent = "urun_raporu";
        else if (q.Contains("ekstre") || q.Contains("bakiye") || q.Contains("borç") || q.Contains("hareket")) intent = "ekstre";
        else if (q.Contains("çek") || q.Contains("senet")) intent = "cek";
        else if (q.Contains("sipariş") || q.Contains("siparişi")) intent = "siparis";
        else if (q.Contains("iskonto") || q.Contains("fiyat") || q.Contains("indirim")) intent = "fiyat";

        // Cari ipuçları
        string[] cariSignalWords = { "carisi", "carisine", "carisinin", "müşterisi", "müşterisine", "firması", "firmasına" };
        bool hasCariSignal = cariSignalWords.Any(w => q.Contains(w));

        // Cari adını ayıkla
        string cleanTerm = Regex.Replace(query, @"(?i)\b(carisine|carisi|carisinin|müşterisine|müşterisi|firmasına|firması|bu\s+ay|geçen\s+ay|bu\s+yıl|son|satmışım|sattım|sattık|verdiğimiz|aldığı|aldıkları|satılan|ürünler|ürünleri|malzeme|malzemeleri|hangi|liste|getir|göster|kadar|bakiye|ekstre|borç|hareket|raporlar?|raporu|fatura|çek|senet|nelerdir|nedir|ne\s+kadar)\b", " ").Trim();
        cleanTerm = Regex.Replace(cleanTerm, @"\s+", " ").Trim();

        bool hasCari = hasCariSignal || (!string.IsNullOrWhiteSpace(cleanTerm) && cleanTerm.Length >= 2);

        return (hasCari, cleanTerm, intent);
    }

    private static bool IsComplexQuery(string q)
    {
        string qLow = q.ToLowerInvariant();
        // Tarihsel analiz, karşılaştırma, özet gibi karmaşık sorgular
        return qLow.Contains("karşılaştır") || qLow.Contains("analiz") || qLow.Contains("trend")
            || qLow.Contains("özet") || qLow.Contains("geçen yıl") || qLow.Contains("yüzde")
            || qLow.Contains("en çok") || qLow.Contains("en az") || qLow.Contains("toplam satış")
            || qLow.Contains("büyüme") || qLow.Contains("performans") || qLow.Contains("tahmin");
    }

    public async Task<string> AnswerUserQueryAsync(string userQuery, CancellationToken ct = default)
    {
        var result = await AnswerUserQueryWithButtonsAsync(0, userQuery, ct);
        return result.MessageText;
    }

    public async Task<AiQueryResult> AnswerUserQueryWithButtonsAsync(string userQuery, CancellationToken ct = default)
    {
        return await AnswerUserQueryWithButtonsAsync(0, userQuery, ct);
    }

    public async Task<AiQueryResult> AnswerUserQueryWithButtonsAsync(long chatId, string userQuery, CancellationToken ct = default)
    {
        var result = new AiQueryResult();
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            result.MessageText = "Lütfen bir soru veya talimat yazınız.";
            return result;
        }

        string cleanQuery = userQuery.Trim();
        // Bot prefix temizle
        foreach (var prefix in new[] { "bot:", "yapayzeka:", "/bot", "/ai" })
        {
            if (cleanQuery.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                cleanQuery = cleanQuery.Substring(prefix.Length).Trim();
                break;
            }
        }

        var firmNo = _configuration["LogoErp:FirmNo"] ?? "001";
        var periodNo = _configuration["LogoErp:PeriodNo"] ?? "01";

        // Session al veya oluştur (oturum durumu ve geçmiş)
        var session = _agentSessions.GetOrAdd(chatId, _ => new AgentSession());
        lock (session) { session.LastActivity = DateTime.UtcNow; }

        try
        {
            // ─────────────────────────────────────────────────────────
            // ADIM 1: SELAMLAMA → Hızlı yerel yanıt, API yok
            // ─────────────────────────────────────────────────────────
            if (IsGreeting(cleanQuery))
            {
                result.MessageText = $"👋 **Merhaba!** Ben Logo ERP Akıllı Asistanınızım.\n\nSize nasıl yardımcı olabilirim?\n\n💡 *Örnek kullanımlar:*\n• `RAGIP` yazarak o isimle başlayan carileri bulursunuz\n• `Ragıp carisine bu ay ne sattık?` → Cari seçtikten sonra raporu gösteririm\n• `Intel stok` → Stok durumunu gösteririm\n• `Bekleyen siparişler` → Açık siparişleri listelerim";
                result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
                result.Buttons.Add(new AiButtonDto { Text = "📊 Hazır Raporlar", CallbackData = "firmam_cat:ALL" });
                return result;
            }

            // ─────────────────────────────────────────────────────────
            // ADIM 2: CARİ ADINA GÖRE ARAMA + ÇOKLU EŞLEŞMEde BUTON
            // Cari içeren her türlü sorgu için → Önce cari bul, sonra devam et
            // ─────────────────────────────────────────────────────────
            var (hasCariHint, cariTerm, intentType) = ExtractCariIntent(cleanQuery);

            if (hasCariHint && !string.IsNullOrWhiteSpace(cariTerm) && cariTerm.Length >= 2)
            {
                string sqlCari = $@"
SELECT TOP 12 LOGICALREF AS ClRef, CODE AS ClCode, DEFINITION_ AS ClName, TELNRS1 AS Tel, CITY AS Sehir
FROM LG_{firmNo}_CLCARD WITH (NOLOCK)
WHERE (CODE LIKE '%{cariTerm.Replace("'", "''")}%' OR DEFINITION_ LIKE '%{cariTerm.Replace("'", "''")}%') AND CARDTYPE <> 22
ORDER BY CODE ASC";

                string jsonCari = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(sqlCari, ct);
                try
                {
                    using var docCari = JsonDocument.Parse(jsonCari);
                    var rootCari = docCari.RootElement;
                    int count = rootCari.ValueKind == JsonValueKind.Array ? rootCari.GetArrayLength() : 0;

                    if (count > 1)
                    {
                        // Birden fazla eşleşme → Kullanıcıya seç
                        // Orijinal niyeti kaydet, seçim sonrası kullanacağız
                        lock (session)
                        {
                            session.PendingIntent = intentType;
                            session.PendingQuery = cleanQuery;
                        }
                        var sb2 = new StringBuilder();
                        sb2.AppendLine($"🔍 **'{cariTerm}'** aramasında **{count} cari hesap** bulundu.\n");
                        sb2.AppendLine("👇 *Hangi cariyi kastediyordunuz?*");
                        foreach (var cl in rootCari.EnumerateArray())
                        {
                            string code = cl.GetProperty("ClCode").GetString() ?? "";
                            string name = cl.GetProperty("ClName").GetString() ?? "";
                            string tel = cl.TryGetProperty("Tel", out var tp) ? tp.GetString() ?? "" : "";
                            string sehir = cl.TryGetProperty("Sehir", out var sp) ? sp.GetString() ?? "" : "";
                            string btnTitle = $"🏢 {code} - {(name.Length > 24 ? name.Substring(0, 21) + "..." : name)}";
                            result.Buttons.Add(new AiButtonDto { Text = btnTitle, CallbackData = $"cari_sel:{code}" });
                        }
                        result.Buttons.Add(new AiButtonDto { Text = "🔙 İptal", CallbackData = "firmam_menu" });
                        result.MessageText = sb2.ToString();
                        return result;
                    }
                    else if (count == 1)
                    {
                        // Tek eşleşme → Direkt işleme git
                        var cl = rootCari[0];
                        string clRef = cl.GetProperty("ClRef").ToString();
                        string clCode = cl.GetProperty("ClCode").GetString() ?? "";
                        string clName = cl.GetProperty("ClName").GetString() ?? "";

                        return await BuildCariReportResult(clRef, clCode, clName, intentType, cleanQuery, firmNo, periodNo, ct);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cari arama hatası (term: {Term})", cariTerm);
                }
            }

            // ─────────────────────────────────────────────────────────
            // ADIM 3: YEREL AKILLI MOTOR (Stok, Sipariş, vb.)
            // ─────────────────────────────────────────────────────────
            var localResult = await ExecuteLocalSmartQueryWithButtonsAsync(cleanQuery, firmNo, periodNo, ct);

            // Yerel motor anlamlı bir cevap verdiyse (hata / bulunamadı mesajı değilse) döndür
            bool localFailed = localResult.MessageText.Contains("bulunamadı") || localResult.MessageText.Contains("eşleşen");
            if (!localFailed)
            {
                lock (session)
                {
                    session.History.Add(("Kullanıcı", cleanQuery));
                    session.History.Add(("Asistan", localResult.MessageText));
                    if (session.History.Count > 12) session.History.RemoveRange(0, session.History.Count - 12);
                }
                return localResult;
            }

            // ─────────────────────────────────────────────────────────
            // ADIM 4: KARMAŞIK SORGULAR → Gemini/OpenAI API
            // ─────────────────────────────────────────────────────────
            // Kullanıcı ne yazarsa yazsın AI'ın yorumlamasına izin ver (Ezber bozan)
            if (true)
            {
                string apiKey = _configuration["AiSettings:ApiKey"]
                             ?? _configuration["OpenAI:ApiKey"]
                             ?? _configuration["Groq:ApiKey"]
                             ?? "";

                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    string histText;
                    lock (session)
                    {
                        var sbH = new StringBuilder();
                        foreach (var msg in session.History.TakeLast(6))
                            sbH.AppendLine($"{msg.Role}: {msg.Content}");
                        histText = sbH.ToString();
                    }

                    var (aiResp, aiButtons) = await QueryLlmDatabaseAssistantAsync(cleanQuery, apiKey,
                        _configuration["AiSettings:Provider"] ?? "Gemini",
                        _configuration["AiSettings:Model"] ?? "gemini-3.6-flash",
                        firmNo, periodNo, histText, ct);

                    if (!string.IsNullOrWhiteSpace(aiResp))
                    {
                        result.MessageText = aiResp;
                        if (aiButtons?.Count > 0) result.Buttons.AddRange(aiButtons);
                        result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
                        lock (session)
                        {
                            session.History.Add(("Kullanıcı", cleanQuery));
                            session.History.Add(("Asistan", aiResp));
                            if (session.History.Count > 12) session.History.RemoveRange(0, session.History.Count - 12);
                        }
                        return result;
                    }
                }
            }

            // ─────────────────────────────────────────────────────────
            // ADIM 5: Hiç eşleşme yok → Yardımcı rehber mesaj
            // ─────────────────────────────────────────────────────────
            result.MessageText = $"🤔 **'{cleanQuery}'** için bir kayıt bulunamadı.\n\n💡 *Ne yapabilirim?*\n• Cari aramak için cari adını yazın (Örn: `RAGIP`, `AHMET TİCARET`)\n• Stok için malzeme adını yazın (Örn: `INTEL I7 STOK`)\n• `bekleyen siparişler` → Açık siparişler\n• `raporlar` → Hazır rapor menüsü";
            result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
            result.Buttons.Add(new AiButtonDto { Text = "📊 Hazır Raporlar", CallbackData = "firmam_cat:ALL" });
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent sorgulama hatası: {Query}", userQuery);
            result.MessageText = $"❌ Sorgu işlenirken bir hata oluştu: {ex.Message}";
            return result;
        }
    }

    // Seçilen cari için işlem tipine göre rapor üret
    private async Task<AiQueryResult> BuildCariReportResult(string clRef, string clCode, string clName, string intentType, string originalQuery, string firmNo, string periodNo, CancellationToken ct)
    {
        var result = new AiQueryResult();
        var sb = new StringBuilder();

        if (intentType == "urun_raporu" || originalQuery.ToLowerInvariant().Contains("satt") || originalQuery.ToLowerInvariant().Contains("ürün") || originalQuery.ToLowerInvariant().Contains("fatura"))
        {
            // Ürün raporu
            // Tarih filtresi çıkar
            string dateSql = "";
            if (originalQuery.ToLowerInvariant().Contains("bu ay"))
                dateSql = $"AND YEAR(L.DATE_) = YEAR(GETDATE()) AND MONTH(L.DATE_) = MONTH(GETDATE())";
            else if (originalQuery.ToLowerInvariant().Contains("bu yıl"))
                dateSql = $"AND YEAR(L.DATE_) = YEAR(GETDATE())";
            else if (originalQuery.ToLowerInvariant().Contains("geçen ay"))
                dateSql = $"AND YEAR(L.DATE_) = YEAR(DATEADD(MONTH,-1,GETDATE())) AND MONTH(L.DATE_) = MONTH(DATEADD(MONTH,-1,GETDATE()))";

            string sqlProducts = $@"
SELECT TOP 20
    I.CODE AS UrunKodu, I.NAME AS UrunAdi,
    SUM(L.AMOUNT) AS ToplamMiktar, AVG(L.PRICE) AS OrtBirimFiyat, SUM(L.TOTAL) AS ToplamTutar,
    MAX(CONVERT(VARCHAR(10), L.DATE_, 104)) AS SonTarih,
    MAX(ISNULL(INV.FICHENO, STF.FICHENO)) AS SonBelgeNo
FROM LG_{firmNo}_{periodNo}_STLINE L WITH (NOLOCK)
INNER JOIN LG_{firmNo}_ITEMS I WITH (NOLOCK) ON L.STOCKREF = I.LOGICALREF
LEFT JOIN LG_{firmNo}_{periodNo}_INVOICE INV WITH (NOLOCK) ON L.INVOICEREF = INV.LOGICALREF
LEFT JOIN LG_{firmNo}_{periodNo}_STFICHE STF WITH (NOLOCK) ON L.STFICHE_REF = STF.LOGICALREF
WHERE L.CLIENTREF = {clRef} AND L.TRCODE IN (7, 8) AND L.LINETYPE = 0 AND (L.CANCELLED = 0 OR L.CANCELLED IS NULL)
{dateSql}
GROUP BY I.CODE, I.NAME
ORDER BY SUM(L.TOTAL) DESC";

            string json = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(sqlProducts, ct);
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    string donemLabel = originalQuery.ToLowerInvariant().Contains("bu ay") ? "Bu Ay" :
                                        originalQuery.ToLowerInvariant().Contains("bu yıl") ? "Bu Yıl" :
                                        originalQuery.ToLowerInvariant().Contains("geçen ay") ? "Geçen Ay" : "Tüm Zamanlar";
                    sb.AppendLine($"📊 **{clName}** carisine **{donemLabel}** yapılan satışlar:\n");
                    decimal grandTotal = 0;
                    int idx = 1;
                    foreach (var p in root.EnumerateArray())
                    {
                        string uKod = p.GetProperty("UrunKodu").GetString() ?? "";
                        string uAd = p.GetProperty("UrunAdi").GetString() ?? "";
                        decimal.TryParse(p.GetProperty("ToplamMiktar").ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal mkt);
                        decimal.TryParse(p.GetProperty("ToplamTutar").ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal tpl);
                        string tarih = p.TryGetProperty("SonTarih", out var dt) ? dt.ToString() : "";
                        grandTotal += tpl;
                        sb.AppendLine($"{idx}. 📦 **[{uKod}] {uAd}**");
                        sb.AppendLine($"   └ Miktar: **{mkt:N2} Adet** | Tutar: **{tpl:N2} TL** | Son: {tarih}\n");
                        idx++;
                    }
                    sb.AppendLine($"─────────────────────────────");
                    sb.AppendLine($"💰 **Toplam: {grandTotal:N2} TL**");
                    result.MessageText = sb.ToString();
                    result.Buttons.Add(new AiButtonDto { Text = "👤 Cari İşlem Menüsü", CallbackData = $"cari_sel:{clCode}" });
                    result.Buttons.Add(new AiButtonDto { Text = "📄 Ekstre (PDF)", CallbackData = $"act:pdf:{clCode}" });
                    result.Buttons.Add(new AiButtonDto { Text = "🛒 Sipariş Oluştur", CallbackData = $"act:order:{clCode}" });
                }
                else
                {
                    string donemLabel = originalQuery.ToLowerInvariant().Contains("bu ay") ? "bu aya" :
                                        originalQuery.ToLowerInvariant().Contains("bu yıl") ? "bu yıla" : "bu kriterlere";
                    sb.AppendLine($"ℹ️ **{clName}** carisinde **{donemLabel}** ait satış kaydı bulunamadı.");
                    sb.AppendLine("\n_En son yapılan satışları görmek ister misiniz?_");
                    result.MessageText = sb.ToString();
                    result.Buttons.Add(new AiButtonDto { Text = "📦 Tüm Satışları Göster", CallbackData = $"rpt_run_cari:rpt_cari_urun_alimlari:{clCode}" });
                    result.Buttons.Add(new AiButtonDto { Text = "👤 Cari İşlem Menüsü", CallbackData = $"cari_sel:{clCode}" });
                }
            }
            catch { result.MessageText = $"⚠️ Veri alınırken bir sorun oluştu."; }
        }
        else if (intentType == "ekstre" || intentType == "genel")
        {
            // Ekstre / Bakiye 
            sb.AppendLine($"🏢 **{clName}** (`{clCode}`) cari hesap bilgileri:");
            result.MessageText = sb.ToString();
            result.Buttons.Add(new AiButtonDto { Text = "📄 Ekstre (Metin)", CallbackData = $"act:ekstre:{clCode}" });
            result.Buttons.Add(new AiButtonDto { Text = "📑 Ekstre (PDF)", CallbackData = $"act:pdf:{clCode}" });
            result.Buttons.Add(new AiButtonDto { Text = "📦 Son Satılan Ürünler", CallbackData = $"rpt_run_cari:rpt_cari_urun_alimlari:{clCode}" });
            result.Buttons.Add(new AiButtonDto { Text = "👤 Tüm İşlem Menüsü", CallbackData = $"cari_sel:{clCode}" });
        }
        else if (intentType == "cek")
        {
            result.MessageText = $"📑 **{clName}** cari hesabının çek/senet raporu:";
            result.Buttons.Add(new AiButtonDto { Text = "📑 Çek Raporu (Metin)", CallbackData = $"act:cek:{clCode}" });
            result.Buttons.Add(new AiButtonDto { Text = "📜 Çek Raporu (PDF)", CallbackData = $"act:cekpdf:{clCode}" });
            result.Buttons.Add(new AiButtonDto { Text = "👤 Cari İşlem Menüsü", CallbackData = $"cari_sel:{clCode}" });
        }
        else if (intentType == "siparis")
        {
            result.MessageText = $"🛒 **{clName}** cari hesabı için sipariş oluşturmak ister misiniz?";
            result.Buttons.Add(new AiButtonDto { Text = "🛒 Sipariş Oluştur", CallbackData = $"act:order:{clCode}" });
            result.Buttons.Add(new AiButtonDto { Text = "👤 Cari İşlem Menüsü", CallbackData = $"cari_sel:{clCode}" });
        }

        if (!result.Buttons.Any(b => b.CallbackData == "firmam_menu"))
            result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });

        return result;
    }

    private async Task<(string? ResponseText, List<AiButtonDto> Buttons)> QueryLlmDatabaseAssistantAsync(string query, string apiKey, string provider, string model, string firmNo, string periodNo, string conversationHistory, CancellationToken ct)
    {
        var buttons = new List<AiButtonDto>();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (_geminiAiService != null && _dynamicSqlExecutor != null)
            {
                int maxAttempts = 3;
                string jsonResult = "";
                string generatedSql = "";
                bool isError = false;
                string currentPrompt = query;

                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    generatedSql = await _geminiAiService.GenerateSqlAsync(currentPrompt, firmNo, periodNo, conversationHistory, ct);
                    
                    if (string.IsNullOrWhiteSpace(generatedSql) || !generatedSql.ToUpperInvariant().Contains("SELECT"))
                    {
                        break;
                    }

                    _logger.LogInformation("NL2SQL Üretildi (Deneme {Attempt}): {Sql}", attempt, generatedSql);

                    jsonResult = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(generatedSql, ct);
                    isError = jsonResult.StartsWith("[GÜVENLİK İHLALİ]") || jsonResult.StartsWith("[SQL HATASI]");

                    if (!isError && jsonResult != "[]" && jsonResult != "null")
                    {
                        // Veri bulundu!
                        break;
                    }

                    if (attempt < maxAttempts)
                    {
                        string feedback = isError 
                            ? $"SQL Hatası aldım: {jsonResult}. Lütfen kolon adlarını veya kısıtlamaları düzeltip yeni bir SQL yaz." 
                            : $"Bu sorgu boş döndü. Lütfen tarih kısıtlamalarını genişleterek (örn. son 1 yıla veya tüm zamanlara bakarak) veya farklı bir eşleşme arayarak tekrar yeni bir SQL yaz.";
                        currentPrompt = $"{query}\n\nÖnceki deneme sonucu: {feedback}\n\nSadece yeni SQL sorgusunu ver.";
                    }
                }

                if (!string.IsNullOrWhiteSpace(generatedSql) && generatedSql.ToUpperInvariant().Contains("SELECT"))
                {
                    sw.Stop();


                    // JSON sonucundan dinamik butonları çıkar (Cari veya Stok)
                    if (!isError && !string.IsNullOrWhiteSpace(jsonResult) && (jsonResult.StartsWith("[") || jsonResult.StartsWith("{")))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(jsonResult);
                            var root = doc.RootElement;
                            if (root.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var elem in root.EnumerateArray())
                                {
                                    string? cariKodu = null;
                                    string? cariUnvan = null;

                                    if (elem.TryGetProperty("CariKodu", out var propCode) || elem.TryGetProperty("CODE", out propCode) || elem.TryGetProperty("code", out propCode))
                                        cariKodu = propCode.GetString();

                                    if (elem.TryGetProperty("CariUnvani", out var propDef) || elem.TryGetProperty("DEFINITION_", out propDef) || elem.TryGetProperty("definition_", out propDef))
                                        cariUnvan = propDef.GetString();

                                    if (!string.IsNullOrWhiteSpace(cariKodu))
                                    {
                                        string btnTitle = !string.IsNullOrWhiteSpace(cariUnvan) ? $"🏢 {cariKodu} - {cariUnvan}" : $"🏢 {cariKodu}";
                                        if (btnTitle.Length > 35) btnTitle = btnTitle.Substring(0, 32) + "...";
                                        if (!buttons.Any(b => b.CallbackData == $"cari_sel:{cariKodu}"))
                                        {
                                            buttons.Add(new AiButtonDto { Text = btnTitle, CallbackData = $"cari_sel:{cariKodu}" });
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    string humanResponse = await _geminiAiService.GenerateHumanResponseAsync(query, generatedSql, jsonResult, conversationHistory, ct);

                    if (!string.IsNullOrWhiteSpace(humanResponse))
                    {
                        if (!isError && _customReportStore != null)
                        {
                            try
                            {
                                string title = query.Length > 35 ? query.Substring(0, 32) + "..." : query;
                                string btnText = query.Length > 22 ? query.Substring(0, 19) + "..." : query;
                                _customReportStore.SaveOrUpdateReport(new CustomReportDto
                                {
                                    Title = $"🤖 {title}",
                                    Description = $"Yapay Zeka tarafından öğrenilen rapor ({DateTime.Now:dd.MM.yyyy HH:mm}): {query}",
                                    Category = "YapayZeka",
                                    SqlQuery = generatedSql,
                                    TelegramButtonText = $"🤖 {btnText}",
                                    Icon = "🤖",
                                    ShowInFirmamMenu = true,
                                    IsActive = true
                                }, out _);
                            }
                            catch (Exception exReport)
                            {
                                _logger.LogWarning(exReport, "Yapay zeka raporu otomatik kaydedilemedi.");
                            }
                        }

                        if (_reportHistoryStore != null)
                        {
                            _reportHistoryStore.AddHistoryItem(new ReportHistoryItemDto
                            {
                                ReportTitle = $"🤖 {query}",
                                Category = "YapayZeka",
                                ExecutedBy = "Telegram AI",
                                SqlQuery = generatedSql,
                                DurationMs = sw.ElapsedMilliseconds,
                                RowCount = 1,
                                IsSuccess = !isError,
                                ErrorMessage = isError ? jsonResult : null,
                                ResultPreviewJson = jsonResult.Length > 300 ? jsonResult.Substring(0, 300) + "..." : jsonResult
                            });
                        }

                        return (humanResponse, buttons);
                    }
                }
                else
                {
                    string generalResponse = await _geminiAiService.GenerateGeneralResponseAsync(query, conversationHistory, ct);
                    if (!string.IsNullOrWhiteSpace(generalResponse))
                    {
                        return (generalResponse, buttons);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM asistan çağrısı sırasında hata oluştu.");
        }

        return (null, buttons);
    }

    private async Task<AiQueryResult> ExecuteLocalSmartQueryWithButtonsAsync(string query, string firmNo, string periodNo, CancellationToken ct)
    {
        var result = new AiQueryResult();
        string qLower = query.Trim().ToLowerInvariant();
        var sb = new StringBuilder();

        // 0. Karşılama ve Selamlama Kontrolü
        if (qLower == "naber" || qLower == "selam" || qLower == "merhaba" || qLower == "nasılsın" || qLower == "nasilsin" 
            || qLower == "gunaydin" || qLower == "günaydın" || qLower == "iyi akşamlar" || qLower == "iyi aksamlar" 
            || qLower == "kimsin" || qLower == "yardım" || qLower == "yardim" || qLower == "hey" || qLower == "alo")
        {
            result.MessageText = "🤖 **Merhaba!** Ben **Logo ERP Akıllı Asistanınızım**.\n\nSize nasıl yardımcı olabilirim?\n• 🏢 Şirket kokpitini açmak için aşağıdaki butona tıklayabilir veya `firmam` yazabilirsiniz.\n• 👤 Cari hesap ekstresi ve son satılan ürünleri sorgulayabilirsiniz (Örn: `RAGIP`).\n• 📦 Stok ve ambar miktarları için malzeme adını yazabilirsiniz (Örn: `INTEL STOK`).\n• 📊 Hazır SQL raporları için `raporlar` yazabilirsiniz.";
            result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
            result.Buttons.Add(new AiButtonDto { Text = "📊 Hazır Raporlar", CallbackData = "firmam_cat:ALL" });
            return result;
        }

        // 1. CARİYE SON VERİLEN / SATILAN ÜRÜNLER TESPİTİ
        bool isProductHistoryQuery = qLower.Contains("verdiğimiz ürün") || qLower.Contains("verdigimiz urun")
                                  || qLower.Contains("aldığı ürün") || qLower.Contains("aldigi urun")
                                  || qLower.Contains("aldığı malzem") || qLower.Contains("aldigi malzem")
                                  || qLower.Contains("satılan ürün") || qLower.Contains("satilan urun")
                                  || qLower.Contains("hangi ürün") || qLower.Contains("hangi urun")
                                  || qLower.Contains("son ürünler") || qLower.Contains("son urunler");

        if (isProductHistoryQuery)
        {
            string cariSearchTerm = Regex.Replace(query, @"(?i)\b(carisine|carisi|müşterisine|müşterisi|firmasına|firması|son|verdiğimiz|verdigimiz|aldığı|aldigi|aldıkları|aldiklari|satılan|satilan|ürünler|urunler|ürünleri|urunleri|malzemeler|malzemeleri|nelerdir|nedir|hangileridir|göster|listele|bakar\s+mısın|lütfen)\b", " ").Trim();
            cariSearchTerm = Regex.Replace(cariSearchTerm, @"\s+", " ").Trim();

            if (!string.IsNullOrWhiteSpace(cariSearchTerm))
            {
                string sqlClSearch = $@"
SELECT TOP 8 LOGICALREF AS ClRef, CODE AS ClCode, DEFINITION_ AS ClName 
FROM LG_{firmNo}_CLCARD WITH (NOLOCK)
WHERE (CODE LIKE '%{cariSearchTerm}%' OR DEFINITION_ LIKE '%{cariSearchTerm}%') AND CARDTYPE <> 22
ORDER BY CODE ASC";

                string jsonCl = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(sqlClSearch, ct);
                try
                {
                    using var docCl = JsonDocument.Parse(jsonCl);
                    var rootCl = docCl.RootElement;

                    if (rootCl.ValueKind == JsonValueKind.Array && rootCl.GetArrayLength() > 1)
                    {
                        sb.AppendLine($"🤔 **'{cariSearchTerm}' aramasıyla birden fazla cari hesap bulundu.**\n\n👇 *Lütfen işlem yapmak istediğiniz cari hesaba tıklayınız:*");
                        foreach (var cl in rootCl.EnumerateArray())
                        {
                            string code = cl.GetProperty("ClCode").GetString() ?? "";
                            string name = cl.GetProperty("ClName").GetString() ?? "";
                            string btnTitle = $"🏢 {code} - {(name.Length > 24 ? name.Substring(0, 21) + "..." : name)}";
                            result.Buttons.Add(new AiButtonDto { Text = btnTitle, CallbackData = $"cari_sel:{code}" });
                        }
                        result.Buttons.Add(new AiButtonDto { Text = "🔙 Geri Dön", CallbackData = "firmam_menu" });
                        result.MessageText = sb.ToString();
                        return result;
                    }
                    else if (rootCl.ValueKind == JsonValueKind.Array && rootCl.GetArrayLength() == 1)
                    {
                        var cl = rootCl[0];
                        string clRef = cl.GetProperty("ClRef").ToString();
                        string clCode = cl.GetProperty("ClCode").GetString() ?? "";
                        string clName = cl.GetProperty("ClName").GetString() ?? "";

                        string sqlSoldProducts = $@"
SELECT TOP 15
    I.CODE AS [UrunKodu],
    I.NAME AS [UrunAdi],
    L.AMOUNT AS [Miktar],
    L.PRICE AS [BirimFiyat],
    L.TOTAL AS [ToplamTutar],
    CONVERT(VARCHAR(10), L.DATE_, 104) AS [Tarih],
    ISNULL(INV.FICHENO, STF.FICHENO) AS [BelgeNo],
    CASE WHEN INV.FICHENO IS NOT NULL THEN 'Satış Faturası' ELSE 'Satış İrsaliyesi' END AS [BelgeTuru]
FROM LG_{firmNo}_{periodNo}_STLINE L WITH (NOLOCK)
INNER JOIN LG_{firmNo}_ITEMS I WITH (NOLOCK) ON L.STOCKREF = I.LOGICALREF
LEFT JOIN LG_{firmNo}_{periodNo}_INVOICE INV WITH (NOLOCK) ON L.INVOICEREF = INV.LOGICALREF
LEFT JOIN LG_{firmNo}_{periodNo}_STFICHE STF WITH (NOLOCK) ON L.STFICHE_REF = STF.LOGICALREF
WHERE L.CLIENTREF = {clRef} AND L.TRCODE IN (7, 8) AND L.LINETYPE = 0 AND (L.CANCELLED = 0 OR L.CANCELLED IS NULL)
ORDER BY L.DATE_ DESC, L.LOGICALREF DESC";

                        string jsonProducts = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(sqlSoldProducts, ct);
                        using var docP = JsonDocument.Parse(jsonProducts);
                        var rootP = docP.RootElement;

                        if (rootP.ValueKind == JsonValueKind.Array && rootP.GetArrayLength() > 0)
                        {
                            sb.AppendLine($"🤖 **{clName}** (`{clCode}`) carisine son kesilen fatura ve sevk edilen ürünler:\n");
                            int pIdx = 1;
                            foreach (var p in rootP.EnumerateArray())
                            {
                                string uKod = p.GetProperty("UrunKodu").GetString() ?? "";
                                string uAd = p.GetProperty("UrunAdi").GetString() ?? "";
                                decimal miktar = 0, birimFiyat = 0, toplam = 0;
                                if (p.TryGetProperty("Miktar", out var mProp)) decimal.TryParse(mProp.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out miktar);
                                if (p.TryGetProperty("BirimFiyat", out var bfProp)) decimal.TryParse(bfProp.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out birimFiyat);
                                if (p.TryGetProperty("ToplamTutar", out var tProp)) decimal.TryParse(tProp.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out toplam);
                                string tarih = p.TryGetProperty("Tarih", out var dtProp) ? dtProp.ToString() : "";
                                string belgeNo = p.TryGetProperty("BelgeNo", out var bProp) ? bProp.ToString() : "";
                                string belgeTuru = p.TryGetProperty("BelgeTuru", out var btProp) ? btProp.ToString() : "Fatura";

                                sb.AppendLine($"{pIdx}. 📦 **[{uKod}] {uAd}**");
                                sb.AppendLine($"   └ *Miktar:* **{miktar:N2} Adet** | *Birim Fiyat:* {birimFiyat:N2} TL | *Tutar:* **{toplam:N2} TL**");
                                sb.AppendLine($"   └ *Belge:* `{belgeNo}` ({belgeTuru} - {tarih})\n");
                                pIdx++;
                            }

                            result.MessageText = sb.ToString();
                            result.Buttons.Add(new AiButtonDto { Text = "👤 Cari İşlem Menüsü", CallbackData = $"cari_sel:{clCode}" });
                            result.Buttons.Add(new AiButtonDto { Text = "📄 Ekstre (PDF)", CallbackData = $"act:pdf:{clCode}" });
                            result.Buttons.Add(new AiButtonDto { Text = "🛒 Sipariş Oluştur", CallbackData = $"act:order:{clCode}" });
                            result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
                            return result;
                        }
                        else
                        {
                            result.MessageText = $"ℹ️ **{clName}** (`{clCode}`) carisine ait henüz bir satış faturası veya teslim edilmiş ürün kaydı bulunmuyor.";
                            result.Buttons.Add(new AiButtonDto { Text = "👤 Cari İşlem Menüsü", CallbackData = $"cari_sel:{clCode}" });
                            result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
                            return result;
                        }
                    }
                }
                catch (Exception exCl)
                {
                    _logger.LogError(exCl, "Cari ürün sorgusu sırasında hata oluştu.");
                }
            }
        }

        // 2. GENEL CARİ ARAMASI & EKSTRE SORGUSU
        string clCleanTerm = Regex.Replace(query, @"(?i)\b(carisi|carisine|ekstresi|ekstre|bakiye|bakiyesi|borcu|borç|hesap|hareketleri|durumu)\b", " ").Trim();
        clCleanTerm = Regex.Replace(clCleanTerm, @"\s+", " ").Trim();
        if (string.IsNullOrWhiteSpace(clCleanTerm)) clCleanTerm = query.Trim();

        string sqlClGeneralSearch = $@"
SELECT TOP 8 LOGICALREF AS ClRef, CODE AS ClCode, DEFINITION_ AS ClName 
FROM LG_{firmNo}_CLCARD WITH (NOLOCK)
WHERE (CODE LIKE '%{clCleanTerm}%' OR DEFINITION_ LIKE '%{clCleanTerm}%') AND CARDTYPE <> 22
ORDER BY CODE ASC";

        string jsonGenCl = await _dynamicSqlExecutor.ExecuteReadOnlyQueryAsync(sqlClGeneralSearch, ct);
        try
        {
            using var docGenCl = JsonDocument.Parse(jsonGenCl);
            var rootGenCl = docGenCl.RootElement;
            if (rootGenCl.ValueKind == JsonValueKind.Array && rootGenCl.GetArrayLength() > 1)
            {
                sb.AppendLine($"🤔 **'{clCleanTerm}' aramasıyla birden fazla cari hesap bulundu.**\n\n👇 *Lütfen işlem yapmak istediğiniz cari hesaba tıklayınız:*");
                foreach (var cl in rootGenCl.EnumerateArray())
                {
                    string code = cl.GetProperty("ClCode").GetString() ?? "";
                    string name = cl.GetProperty("ClName").GetString() ?? "";
                    string btnTitle = $"🏢 {code} - {(name.Length > 24 ? name.Substring(0, 21) + "..." : name)}";
                    result.Buttons.Add(new AiButtonDto { Text = btnTitle, CallbackData = $"cari_sel:{code}" });
                }
                result.Buttons.Add(new AiButtonDto { Text = "🔙 Geri Dön", CallbackData = "firmam_menu" });
                result.MessageText = sb.ToString();
                return result;
            }
        }
        catch { }

        // Cari Ekstre / Banka / Hareket Analizi
        var statement = await _logoErpRepository.GetFirmStatementAsync(clCleanTerm, firmNo, periodNo, null, null, ct);
        if (statement != null)
        {
            sb.AppendLine("🤖 **AKILLI CARİ HESAP & HAREKET RAPORU**");
            sb.AppendLine($"🏢 **Cari:** `{statement.FirmCode}` - **{statement.FirmName}**");
            sb.AppendLine($"💰 **Net Bakiye:** `{statement.Balance:N2} TL` (Borç: {statement.TotalDebit:N2} TL / Alacak: {statement.TotalCredit:N2} TL)");
            sb.AppendLine("-----------------------------------------");

            var txs = statement.RecentTransactions ?? new List<LogoErpStatementLineDto>();
            if (qLower.Contains("banka") || qLower.Contains("tahsilat") || qLower.Contains("havale") || qLower.Contains("ödeme"))
            {
                txs = txs.Where(t => (t.TranType ?? "").Contains("Tahsilat", StringComparison.OrdinalIgnoreCase) 
                                 || (t.TranType ?? "").Contains("Ödeme", StringComparison.OrdinalIgnoreCase)
                                 || (t.TranType ?? "").Contains("Havale", StringComparison.OrdinalIgnoreCase)
                                 || (t.TranType ?? "").Contains("Çek", StringComparison.OrdinalIgnoreCase)).ToList();
                sb.AppendLine("🏦 **Banka & Ödeme / Tahsilat Hareketleri:**");
            }
            else
            {
                sb.AppendLine("📋 **Son Ekstre Hareketleri:**");
            }

            if (txs.Any())
            {
                foreach (var tx in txs.Take(10))
                {
                    string yon = tx.Debit > 0 ? "Borç" : "Alacak";
                    decimal tutar = tx.Debit > 0 ? tx.Debit : tx.Credit;
                    sb.AppendLine($"• **{tx.Date:dd.MM.yyyy}** | `{tx.TranNo}` | **{tx.TranType}** | {tx.LineExp} | **{tutar:N2} TL** ({yon})");
                }
            }
            else
            {
                sb.AppendLine("_Belirtilen kriterlere uygun hareket kaydı bulunamadı._");
            }

            result.MessageText = sb.ToString();
            result.Buttons.Add(new AiButtonDto { Text = "📦 Son Aldığı Ürünler", CallbackData = $"rpt_run_cari:rpt_cari_urun_alimlari:{statement.FirmCode}" });
            result.Buttons.Add(new AiButtonDto { Text = "📄 Ekstre (PDF)", CallbackData = $"act:pdf:{statement.FirmCode}" });
            result.Buttons.Add(new AiButtonDto { Text = "👤 Tüm İşlem Menüsü", CallbackData = $"cari_sel:{statement.FirmCode}" });
            result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
            return result;
        }

        // 3. STOK SORGUSU
        string itemCleanTerm = Regex.Replace(query, @"(?i)\b(stok|stogu|stoğu|fiyat|fiyati|fiyatı|adedi|miktarı|kaç\s+tane|var\s+mı)\b", " ").Trim();
        if (string.IsNullOrWhiteSpace(itemCleanTerm)) itemCleanTerm = query.Trim();

        var stocks = await _logoErpRepository.SearchItemStockAsync(itemCleanTerm, firmNo, periodNo, 0, ct);
        if (stocks != null && stocks.Any())
        {
            if (stocks.Count > 1)
            {
                sb.AppendLine($"📦 **'{itemCleanTerm}' ile eşleşen malzemeler:**\n");
                foreach (var st in stocks.Take(10))
                {
                    sb.AppendLine($"• `[{st.ItemCode}]` **{st.ItemName}**\n  └ *Eldeki Stok:* **{st.TotalStock} {st.UnitName}**\n");
                    string btnTitle = $"📦 {st.ItemCode} - {(st.ItemName.Length > 24 ? st.ItemName.Substring(0, 21) + "..." : st.ItemName)}";
                    result.Buttons.Add(new AiButtonDto { Text = btnTitle, CallbackData = $"stk_sel:{st.ItemCode}" });
                }
                result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
                result.MessageText = sb.ToString();
                return result;
            }
            else
            {
                var st = stocks[0];
                sb.AppendLine($"📦 **[{st.ItemCode}] {st.ItemName}**");
                sb.AppendLine($"• **Toplam Eldeki Stok:** `{st.TotalStock} {st.UnitName}`");
                result.MessageText = sb.ToString();
                result.Buttons.Add(new AiButtonDto { Text = "📦 Ambar Dağılımı", CallbackData = $"stk_sel:{st.ItemCode}" });
                result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
                return result;
            }
        }

        // 4. BEKLEYEN SİPARİŞLER
        if (qLower.Contains("sipariş") || qLower.Contains("açık") || qLower.Contains("bekleyen"))
        {
            var openOrders = await _logoErpRepository.GetOpenOrdersAsync(null, firmNo, periodNo, 20, ct);
            if (openOrders != null && openOrders.Any())
            {
                sb.AppendLine("🤖 **AKILLI BEKLEYEN SİPARİŞLER RAPORU**");
                sb.AppendLine($"📦 **Toplam Bekleyen Sipariş:** {openOrders.Count} Adet ({openOrders.Sum(o => o.NetTotal):N2} TL)");
                sb.AppendLine("-----------------------------------------");
                foreach (var ord in openOrders.Take(10))
                {
                    sb.AppendLine($"• `{ord.FicheNo}` | {ord.Date:dd.MM.yyyy} | **{ord.FirmName}** | **{ord.NetTotal:N2} TL**");
                }
                result.MessageText = sb.ToString();
                result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
                return result;
            }
        }

        result.MessageText = $"🤖 **Yapay Zeka Asistanı:** '{query}' sorusu için eşleşen bir cari veya malzeme kaydı bulunamadı.\n\n💡 *Örnek Sorular:*\n• `Ahmet carisine verdiğimiz ürünler`\n• `Ragıp bakiyesi ne kadar?`\n• `Intel i7 stok durumu`\n• `Bekleyen siparişleri listele`";
        result.Buttons.Add(new AiButtonDto { Text = "🏢 Firmam Kokpiti", CallbackData = "firmam_menu" });
        return result;
    }

    // =====================================================================
    // SOHBET MODU: Direkt AI sohbeti (Gemini/Groq multi-turn)
    // =====================================================================

    private static readonly string _sohbetSystemPrompt = @"Sen Logo ERP & CRM Telegram Akıllı Asistanısın.
Kullanıcı bir ticaret firmasında çalışıyor ve hem ERP işlemleri hakkında hem de genel konularda seninle konuşuyor.
Türkçe, samimi, akıllı ve doğrudan yanıt ver. Gereksiz uzun açıklama yapma.
ERP ile ilgili sorularda (cari, stok, fatura, sipariş, bakiye vb.) elindeki bilgilerle yönlendirici ol.
Sohbet modunda olduğunu bil — kullanıcı isterse ERP işlemi için çıkmasını söyle: '/erp yazarak ERP moduna dönebilirsin'.
Genel sohbette samimi ve zeki bir asistan gibi davran.";

    public async Task<string> DirectChatAsync(long chatId, string userMessage, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage)) return "Lütfen bir mesaj yazınız.";

        var session = _agentSessions.GetOrAdd(chatId, _ => new AgentSession());
        var firmNo = _configuration["LogoErp:FirmNo"] ?? "001";
        var periodNo = _configuration["LogoErp:PeriodNo"] ?? "01";

        List<(string Role, string Content)> historySnapshot;
        lock (session)
        {
            session.LastActivity = DateTime.UtcNow;
            historySnapshot = session.History.TakeLast(20).ToList();
        }

        try
        {
            // ─────────────────────────────────────────────────────────
            // HİBRİT AJAN: Önce ERP verisi çek, sonra Gemini ile sun
            // Kullanıcı ERP ile ilgili bir şey sorduysa → gerçek veri al
            // ─────────────────────────────────────────────────────────
            string erpDataContext = "";
            string erpButtonHint = "";

            // ERP sorgusu mu? → Akıllı router'ı çalıştır
            bool looksLikeErpQuery = IsErpRelatedQuery(userMessage);
            if (looksLikeErpQuery)
            {
                try
                {
                    // Akıllı yönlendirici ile ERP verisini çek
                    var erpResult = await AnswerUserQueryWithButtonsAsync(chatId, userMessage, ct);

                    bool erpHasData = !string.IsNullOrWhiteSpace(erpResult.MessageText)
                                   && !erpResult.MessageText.Contains("bulunamadı")
                                   && !erpResult.MessageText.Contains("eşleşen")
                                   && !erpResult.MessageText.Contains("kayıt bulunamadı");

                    if (erpHasData)
                    {
                        // ERP verisi var → Gemini'ye "bunu doğal dille sun" de
                        erpDataContext = $"\n\n[ERP SİSTEMİNDEN GERÇEK VERİ]:\n{erpResult.MessageText}";

                        // Buton ipuçlarını da aktar
                        if (erpResult.Buttons?.Count > 0)
                        {
                            erpButtonHint = "\n\n💡 *Daha fazla işlem için butonlara tıklayabilirsin.*";
                        }
                    }
                    else if (erpResult.Buttons?.Count > 1) // Cari seçim listesi geldi (birden fazla buton = seçim ekranı)
                    {
                        // Seçim gerekiyor → direkt ERP yanıtını dön (sohbet formatında değil)
                        string reply2 = erpResult.MessageText + erpButtonHint;
                        lock (session)
                        {
                            session.History.Add(("user", userMessage));
                            session.History.Add(("model", reply2));
                            if (session.History.Count > 40) session.History.RemoveRange(0, session.History.Count - 40);
                        }
                        return reply2; // Buton listesi → zaten hazır
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "DirectChat ERP sorgusu başarısız, saf sohbete devam ediliyor.");
                }
            }

            // ─────────────────────────────────────────────────────────
            // Gemini'ye gönder: ERP verisi varsa onu da prompt'a ekle
            // ─────────────────────────────────────────────────────────
            string systemPrompt = _sohbetSystemPrompt;
            if (!string.IsNullOrEmpty(erpDataContext))
            {
                systemPrompt += $@"

ÖNEMLİ: Aşağıdaki ERP verisi az önce gerçek zamanlı olarak veritabanından çekildi.
Bu veriyi kullanarak kullanıcıya Türkçe, samimi ve analitik bir yanıt ver.
Veriyi yorumla, önemli noktaları vurgula, gerekirse öneri sun.
{erpDataContext}";
            }

            string userPrompt = string.IsNullOrEmpty(erpDataContext)
                ? userMessage
                : $"Kullanıcı sorusu: {userMessage}\n\nYukarıdaki ERP verisine göre yanıtla.";

            string aiReply = await _geminiAiService.ChatAsync(systemPrompt, historySnapshot, userPrompt, ct);

            // Buton ipuçlarını ekle (varsa)
            if (!string.IsNullOrEmpty(erpButtonHint))
                aiReply += erpButtonHint;

            lock (session)
            {
                session.History.Add(("user", userMessage));
                session.History.Add(("model", aiReply));
                if (session.History.Count > 40) session.History.RemoveRange(0, session.History.Count - 40);
            }

            return aiReply;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DirectChatAsync hatası (chatId:{Id})", chatId);
            return $"⚠️ Yanıt sırasında hata: {ex.Message}";
        }
    }

    /// <summary>Mesajın ERP ile ilgili olup olmadığını hızlıca tespit eder.</summary>
    private static bool IsErpRelatedQuery(string msg)
    {
        string q = msg.ToLowerInvariant();
        return q.Contains("cari") || q.Contains("carisine") || q.Contains("carisi")
            || q.Contains("stok") || q.Contains("fatura") || q.Contains("irsaliye")
            || q.Contains("sipariş") || q.Contains("bakiye") || q.Contains("borç")
            || q.Contains("ekstre") || q.Contains("ürün") || q.Contains("malzeme")
            || q.Contains("satış") || q.Contains("sattık") || q.Contains("sattım")
            || q.Contains("aldı") || q.Contains("alım") || q.Contains("tahsilat")
            || q.Contains("ödeme") || q.Contains("çek") || q.Contains("senet")
            || q.Contains("rapor") || q.Contains("hareket") || q.Contains("hesap")
            || q.Contains("firma") || q.Contains("müşteri") || q.Contains("tedarikçi");
    }

    public bool ToggleSohbetMode(long chatId)
    {
        var session = _agentSessions.GetOrAdd(chatId, _ => new AgentSession());
        lock (session)
        {
            session.IsSohbetMode = !session.IsSohbetMode;
            session.LastActivity = DateTime.UtcNow;
            return session.IsSohbetMode;
        }
    }

    public bool IsSohbetModeActive(long chatId)
    {
        if (_agentSessions.TryGetValue(chatId, out var session))
        {
            lock (session) { return session.IsSohbetMode; }
        }
        return false;
    }

    public void ClearChatHistory(long chatId)
    {
        if (_agentSessions.TryGetValue(chatId, out var session))
        {
            lock (session)
            {
                session.History.Clear();
                session.IsSohbetMode = false;
            }
        }
    }
}
