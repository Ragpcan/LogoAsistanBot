using System.Text.Json;
using System.Text.Json.Nodes;
using LogoCrmTelegramIntegration.Application.Interfaces;
using LogoCrmTelegramIntegration.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Telegram.Bot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _env;
    private readonly ILogger<SettingsController> _logger;
    private readonly IEmailService? _emailService;
    private readonly ISmsService? _smsService;

    public SettingsController(
        IConfiguration configuration,
        IHostEnvironment env,
        ILogger<SettingsController> logger,
        IEmailService? emailService = null,
        ISmsService? smsService = null)
    {
        _configuration = configuration;
        _env = env;
        _logger = logger;
        _emailService = emailService;
        _smsService = smsService;
    }

    [HttpGet]
    public IActionResult GetSettings()
    {
        var connectionString = _configuration.GetConnectionString("LogoCrmConnection") ?? string.Empty;
        var botToken = _configuration["Telegram:BotToken"] ?? string.Empty;

        var targetSystem = _configuration["LogoSystem:TargetSystem"] ?? "CRM";
        var firmNo = _configuration["LogoSystem:FirmNo"] ?? "001";
        var periodNo = _configuration["LogoSystem:PeriodNo"] ?? "01";
        var warehouseNo = _configuration["LogoSystem:WarehouseNo"] ?? "0";
        var documentYear = _configuration["LogoErp:DocumentYear"] ?? DateTime.Now.Year.ToString();
        var invoiceFichePrefix = _configuration["LogoErp:InvoiceFichePrefix"] ?? "FAT";
        var dispatchFichePrefix = _configuration["LogoErp:DispatchFichePrefix"] ?? "IRS";
        var orderFichePrefix = _configuration["LogoErp:OrderFichePrefix"] ?? "SIP";

        var aiApiKey = _configuration["AiSettings:ApiKey"] ?? _configuration["OpenAI:ApiKey"] ?? _configuration["Groq:ApiKey"] ?? string.Empty;
        var aiProvider = _configuration["AiSettings:Provider"] ?? (_configuration["Groq:ApiKey"] != null ? "Groq" : "OpenAI");
        var aiModel = _configuration["AiSettings:Model"] ?? (_configuration["Groq:ApiKey"] != null ? "llama-3.3-70b-versatile" : "gpt-4o-mini");
        var aiIsEnabled = bool.TryParse(_configuration["AiSettings:IsEnabled"], out var enabled) ? enabled : true;

        var builder = new SqlConnectionStringBuilder(string.IsNullOrWhiteSpace(connectionString) ? "Server=localhost;Database=LOGO_CRM_DB;" : connectionString);

        return Ok(new
        {
            botToken = botToken,
            connectionString = connectionString,
            server = builder.DataSource,
            database = builder.InitialCatalog,
            userId = builder.UserID,
            password = builder.Password,
            trustServerCertificate = builder.TrustServerCertificate,
            targetSystem = targetSystem,
            firmNo = firmNo,
            periodNo = periodNo,
            warehouseNo = warehouseNo,
            documentYear = documentYear,
            invoiceFichePrefix = invoiceFichePrefix,
            dispatchFichePrefix = dispatchFichePrefix,
            orderFichePrefix = orderFichePrefix,
            aiApiKey = aiApiKey,
            aiProvider = aiProvider,
            aiModel = aiModel,
            aiIsEnabled = aiIsEnabled
        });
    }

    [HttpPost]
    public async Task<IActionResult> SaveSettings([FromBody] SaveSettingsRequest request)
    {
        try
        {
            var appsettingsPath = Path.Combine(_env.ContentRootPath, "appsettings.json");
            
            if (!System.IO.File.Exists(appsettingsPath))
            {
                var parentAppsettings = Path.Combine(_env.ContentRootPath, "..", "Worker", "appsettings.json");
                if (System.IO.File.Exists(parentAppsettings))
                {
                    appsettingsPath = parentAppsettings;
                }
            }

            string jsonContent = System.IO.File.Exists(appsettingsPath)
                ? await System.IO.File.ReadAllTextAsync(appsettingsPath)
                : "{}";

            var jsonNode = JsonNode.Parse(jsonContent) ?? new JsonObject();

            string connectionString;
            if (!string.IsNullOrWhiteSpace(request.RawConnectionString))
            {
                connectionString = request.RawConnectionString;
            }
            else
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = request.Server ?? "localhost",
                    InitialCatalog = request.Database ?? "LOGO_CRM_DB",
                    UserID = request.UserId ?? "sa",
                    Password = request.Password ?? "",
                    TrustServerCertificate = request.TrustServerCertificate
                };
                connectionString = builder.ConnectionString;
            }

            if (jsonNode["ConnectionStrings"] is not JsonObject connObj)
            {
                connObj = new JsonObject();
                jsonNode["ConnectionStrings"] = connObj;
            }
            connObj["LogoCrmConnection"] = connectionString;

            if (jsonNode["Telegram"] is not JsonObject telegramObj)
            {
                telegramObj = new JsonObject();
                jsonNode["Telegram"] = telegramObj;
            }
            telegramObj["BotToken"] = request.BotToken ?? string.Empty;

            if (jsonNode["LogoSystem"] is not JsonObject systemObj)
            {
                systemObj = new JsonObject();
                jsonNode["LogoSystem"] = systemObj;
            }
            systemObj["TargetSystem"] = request.TargetSystem ?? "CRM";
            systemObj["FirmNo"] = request.FirmNo ?? "001";
            systemObj["PeriodNo"] = request.PeriodNo ?? "01";
            systemObj["WarehouseNo"] = request.WarehouseNo ?? "0";

            // LogoErp alanlarını kaydet
            if (jsonNode["LogoErp"] is not JsonObject erpObj)
            {
                erpObj = new JsonObject();
                jsonNode["LogoErp"] = erpObj;
            }
            if (!string.IsNullOrWhiteSpace(request.DocumentYear))
                erpObj["DocumentYear"] = request.DocumentYear;
            if (!string.IsNullOrWhiteSpace(request.InvoiceFichePrefix))
                erpObj["InvoiceFichePrefix"] = request.InvoiceFichePrefix;
            if (!string.IsNullOrWhiteSpace(request.DispatchFichePrefix))
                erpObj["DispatchFichePrefix"] = request.DispatchFichePrefix;
            if (!string.IsNullOrWhiteSpace(request.OrderFichePrefix))
                erpObj["OrderFichePrefix"] = request.OrderFichePrefix;

            // Yapay Zeka (AiSettings) alanlarını kaydet (Otomatik Akıllı Sağlayıcı & Model Tespiti)
            if (jsonNode["AiSettings"] is not JsonObject aiObj)
            {
                aiObj = new JsonObject();
                jsonNode["AiSettings"] = aiObj;
            }

            string aiKey = (request.AiApiKey ?? string.Empty).Trim();
            string aiProvider = request.AiProvider ?? "Gemini";
            string aiModel = request.AiModel ?? "gemini-3.6-flash";

            if (aiKey.StartsWith("AQ.", StringComparison.OrdinalIgnoreCase) || aiKey.StartsWith("AIza", StringComparison.OrdinalIgnoreCase))
            {
                aiProvider = "Gemini";
                if (string.IsNullOrWhiteSpace(aiModel) || aiModel.StartsWith("gpt", StringComparison.OrdinalIgnoreCase))
                    aiModel = "gemini-3.6-flash";
            }
            else if (aiKey.StartsWith("gsk_", StringComparison.OrdinalIgnoreCase))
            {
                aiProvider = "Groq";
                if (string.IsNullOrWhiteSpace(aiModel) || aiModel.StartsWith("gpt", StringComparison.OrdinalIgnoreCase) || aiModel.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
                    aiModel = "llama-3.3-70b-versatile";
            }
            else if (aiKey.StartsWith("sk-", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(aiProvider) || aiProvider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
                    aiProvider = "OpenAI";
                if (string.IsNullOrWhiteSpace(aiModel) || aiModel.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
                    aiModel = "gpt-4o-mini";
            }

            aiObj["ApiKey"] = aiKey;
            aiObj["Provider"] = aiProvider;
            aiObj["Model"] = aiModel;
            aiObj["IsEnabled"] = request.AiIsEnabled;

            var options = new JsonSerializerOptions { WriteIndented = true };
            await System.IO.File.WriteAllTextAsync(appsettingsPath, jsonNode.ToJsonString(options));

            _configuration["ConnectionStrings:LogoCrmConnection"] = connectionString;
            _configuration["Telegram:BotToken"] = request.BotToken ?? string.Empty;
            _configuration["LogoSystem:TargetSystem"] = request.TargetSystem ?? "CRM";
            _configuration["LogoSystem:FirmNo"] = request.FirmNo ?? "001";
            _configuration["LogoSystem:PeriodNo"] = request.PeriodNo ?? "01";
            _configuration["LogoSystem:WarehouseNo"] = request.WarehouseNo ?? "0";
            if (!string.IsNullOrWhiteSpace(request.DocumentYear))
                _configuration["LogoErp:DocumentYear"] = request.DocumentYear;
            if (!string.IsNullOrWhiteSpace(request.InvoiceFichePrefix))
                _configuration["LogoErp:InvoiceFichePrefix"] = request.InvoiceFichePrefix;
            if (!string.IsNullOrWhiteSpace(request.DispatchFichePrefix))
                _configuration["LogoErp:DispatchFichePrefix"] = request.DispatchFichePrefix;
            if (!string.IsNullOrWhiteSpace(request.OrderFichePrefix))
                _configuration["LogoErp:OrderFichePrefix"] = request.OrderFichePrefix;

            _configuration["AiSettings:ApiKey"] = request.AiApiKey ?? string.Empty;
            _configuration["AiSettings:Provider"] = request.AiProvider ?? "OpenAI";
            _configuration["AiSettings:Model"] = request.AiModel ?? "gpt-4o-mini";
            _configuration["AiSettings:IsEnabled"] = request.AiIsEnabled.ToString();

            _logger.LogInformation("Uygulama ve Yapay Zeka ayarları başarıyla güncellendi.");

            return Ok(new { isSuccess = true, message = "Tüm ayarlar ve Yapay Zeka Token'ı başarıyla kaydedildi!" });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayarlar kaydedilirken hata oluştu.");
            return BadRequest(new { isSuccess = false, message = $"Ayarlar kaydedilemedi: {ex.Message}" });
        }
    }

    [HttpPost("test-db")]
    public async Task<IActionResult> TestDatabaseConnection([FromBody] TestDbRequest request)
    {
        try
        {
            string connectionString = request.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = request.Server,
                    InitialCatalog = request.Database,
                    UserID = request.UserId,
                    Password = request.Password,
                    TrustServerCertificate = request.TrustServerCertificate
                };
                connectionString = builder.ConnectionString;
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var targetSystem = request.TargetSystem ?? "CRM";
            var firmNo = string.IsNullOrWhiteSpace(request.FirmNo) ? "001" : request.FirmNo;

            if (targetSystem.Equals("ERP", StringComparison.OrdinalIgnoreCase))
            {
                var clTable = $"LG_{firmNo}_CLCARD";
                var itemTable = $"LG_{firmNo}_ITEMS";

                using var cmdFirm = new SqlCommand($"SELECT COUNT(*) FROM {clTable}", connection);
                var firmCount = Convert.ToInt32(await cmdFirm.ExecuteScalarAsync());

                using var cmdProduct = new SqlCommand($"SELECT COUNT(*) FROM {itemTable}", connection);
                var productCount = Convert.ToInt32(await cmdProduct.ExecuteScalarAsync());

                return Ok(new
                {
                    isSuccess = true,
                    message = $"SQL Server Bağlantısı BAŞARILI (Logo ERP / Tiger Modu)!\n- Firma Numarası: {firmNo}\n- {clTable} (Cari) Kayıt Sayısı: {firmCount}\n- {itemTable} (Malzeme) Kayıt Sayısı: {productCount}",
                    firmCount,
                    productCount
                });
            }
            else
            {
                using var cmdFirm = new SqlCommand("SELECT COUNT(*) FROM MT_Firm", connection);
                var firmCount = Convert.ToInt32(await cmdFirm.ExecuteScalarAsync());

                using var cmdProduct = new SqlCommand("SELECT COUNT(*) FROM MT_Product", connection);
                var productCount = Convert.ToInt32(await cmdProduct.ExecuteScalarAsync());

                return Ok(new
                {
                    isSuccess = true,
                    message = $"SQL Server Bağlantısı BAŞARILI (Logo CRM Modu)!\n- MT_Firm (Cari) Kayıt Sayısı: {firmCount}\n- MT_Product (Malzeme) Kayıt Sayısı: {productCount}",
                    firmCount,
                    productCount
                });
            }
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                isSuccess = false,
                message = $"SQL Server bağlantısı BAŞARISIZ:\n{ex.Message}"
            });
        }
    }

    [HttpPost("test-telegram")]
    public async Task<IActionResult> TestTelegramToken([FromBody] TestTelegramRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BotToken))
        {
            return Ok(new { isSuccess = false, message = "Telegram Bot Token boş olamaz!" });
        }

        try
        {
            var botClient = new TelegramBotClient(request.BotToken);
            var me = await botClient.GetMe();

            return Ok(new
            {
                isSuccess = true,
                message = $"Telegram Bot Token Doğrulandı!\n- Bot Kullanıcı Adı: @{me.Username}\n- Bot Adı: {me.FirstName}\n- Bot ID: {me.Id}",
                username = me.Username,
                botId = me.Id,
                botName = me.FirstName
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                isSuccess = false,
                message = $"Telegram Bot Token Geçersiz veya API Hatası:\n{ex.Message}"
            });
        }
    }

    [HttpPost("test-mail")]
    public async Task<IActionResult> TestMail([FromBody] TestMailSettingsRequest? request)
    {
        try
        {
            var mailSettings = _configuration.GetSection("MailSettings");
            var host = request?.Host ?? mailSettings["SmtpHost"] ?? "";
            var port = request?.Port > 0 ? request.Port : (int.TryParse(mailSettings["SmtpPort"], out int p) ? p : 587);
            var user = request?.User ?? mailSettings["SmtpUser"] ?? "";
            var pass = request?.Pass ?? mailSettings["SmtpPassword"] ?? "";
            var recipient = request?.Recipient ?? user;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user))
            {
                return Ok(new { isSuccess = false, message = "Lütfen SMTP Sunucu Adresi ve E-Posta Kullanıcı Adını doldurunuz." });
            }

            var emailReq = new EmailSendRequest
            {
                To = recipient,
                Subject = "✅ Logo Asistan Bot - E-Posta Bağlantı Testi",
                Body = $"<p>Bu e-posta <b>{DateTime.Now:dd.MM.yyyy HH:mm}</b> tarihinde SMTP bağlantınızın çalıştığını doğrulamak için gönderilmiştir.</p>",
                IsBodyHtml = true,
                SmtpHost = host,
                SmtpPort = port,
                SmtpUser = user,
                SmtpPassword = pass,
                FromAddress = user,
                FromDisplayName = "Logo Asistan Bot"
            };

            if (_emailService != null)
            {
                var res = await _emailService.SendEmailAsync(emailReq);
                return Ok(new { isSuccess = res.IsSuccess, message = res.IsSuccess ? "✅ Test e-postası başarıyla gönderildi!" : $"❌ E-Posta Gönderim Hatası: {res.ErrorMessage}" });
            }

            return Ok(new { isSuccess = true, message = "✅ Test e-posta isteği başarılı." });
        }
        catch (Exception ex)
        {
            return Ok(new { isSuccess = false, message = $"❌ Test E-Posta Hatası: {ex.Message}" });
        }
    }

    [HttpPost("test-sms")]
    public async Task<IActionResult> TestSms([FromBody] TestSmsSettingsRequest? request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Phone))
            {
                return Ok(new { isSuccess = false, message = "Lütfen geçerli bir telefon numarası giriniz." });
            }

            if (_smsService != null)
            {
                var smsReq = new SmsSendRequest
                {
                    To = request.Phone,
                    Message = $"Logo Asistan Bot - SMS Bağlantı Testi ({DateTime.Now:HH:mm})"
                };
                var res = await _smsService.SendSmsAsync(smsReq);
                return Ok(new { isSuccess = res.IsSuccess, message = res.IsSuccess ? "✅ Test SMS'i başarıyla gönderildi!" : $"❌ SMS Gönderim Hatası: {res.ErrorMessage}" });
            }
            return Ok(new { isSuccess = true, message = "✅ Test SMS isteği başarılı." });
        }
        catch (Exception ex)
        {
            return Ok(new { isSuccess = false, message = $"❌ Test SMS Hatası: {ex.Message}" });
        }
    }

    [HttpPost("test-ai")]
    public async Task<IActionResult> TestAiToken([FromBody] TestAiSettingsRequest? request)
    {
        var apiKey = request?.ApiKey ?? _configuration["AiSettings:ApiKey"] ?? _configuration["OpenAI:ApiKey"] ?? _configuration["Groq:ApiKey"] ?? "";
        var provider = request?.Provider ?? _configuration["AiSettings:Provider"] ?? "";
        var model = request?.Model ?? _configuration["AiSettings:Model"] ?? "";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Ok(new { isSuccess = false, message = "Lütfen geçerli bir Yapay Zeka API Key / Token giriniz." });
        }

        apiKey = apiKey.Trim();
        
        // Akıllı Sağlayıcı Format Tespiti
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

        try
        {
            using var client = new HttpClient();
            bool isGemini = provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase) ||
                            apiKey.StartsWith("AQ.", StringComparison.OrdinalIgnoreCase) ||
                            apiKey.StartsWith("AIza", StringComparison.OrdinalIgnoreCase);

            if (isGemini)
            {
                var candidateModels = new List<string>();
                string cleanModel = (model ?? "").Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(cleanModel) && !cleanModel.Equals("gemini", StringComparison.OrdinalIgnoreCase) && !cleanModel.Equals("gpt-4o-mini", StringComparison.OrdinalIgnoreCase))
                {
                    candidateModels.Add(cleanModel);
                }

                var defaultFallbacks = new[] { "gemini-3.6-flash", "gemini-1.5-flash", "gemini-1.5-flash-latest", "gemini-1.5-pro-latest" };
                foreach (var fb in defaultFallbacks)
                {
                    if (!candidateModels.Contains(fb)) candidateModels.Add(fb);
                }

                string lastError = string.Empty;
                string originalModelError = string.Empty;

                foreach (var testModel in candidateModels)
                {
                    foreach (var apiVer in new[] { "v1beta", "v1" })
                    {
                        string endpoint = $"https://generativelanguage.googleapis.com/{apiVer}/models/{testModel}:generateContent?key={apiKey}";
                        var payload = new
                        {
                            contents = new[]
                            {
                                new { parts = new[] { new { text = "Merhaba, bu bir test sorgusudur. Lütfen sadece 'OK' yanıtını ver." } } }
                            },
                            generationConfig = new { maxOutputTokens = 10 }
                        };

                        using var reqMsg = new HttpRequestMessage(HttpMethod.Post, endpoint);
                        reqMsg.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                        var resp = await client.SendAsync(reqMsg);
                        if (resp.IsSuccessStatusCode)
                        {
                            return Ok(new { isSuccess = true, message = $"✅ Yapay Zeka API Token Doğrulandı! (Google Gemini - Model: {testModel})" });
                        }
                        else
                        {
                            var errStr = await resp.Content.ReadAsStringAsync();
                            lastError = errStr;
                            if (string.IsNullOrWhiteSpace(originalModelError) && testModel == cleanModel) 
                            {
                                originalModelError = errStr;
                            }
                        }
                    }
                }

                return Ok(new { isSuccess = false, message = $"❌ Yapay Zeka API Hatası: {(string.IsNullOrWhiteSpace(originalModelError) ? lastError : originalModelError)}" });
            }
            else
            {
                string endpoint;
                string finalModel = model ?? "";

                if (provider.Equals("Groq", StringComparison.OrdinalIgnoreCase) || apiKey.StartsWith("gsk_"))
                {
                    endpoint = "https://api.groq.com/openai/v1/chat/completions";
                    if (string.IsNullOrWhiteSpace(finalModel) || finalModel.StartsWith("gemini", StringComparison.OrdinalIgnoreCase) || finalModel.StartsWith("gpt", StringComparison.OrdinalIgnoreCase))
                    {
                        finalModel = "llama-3.3-70b-versatile";
                    }
                }
                else if (provider.Equals("DeepSeek", StringComparison.OrdinalIgnoreCase))
                {
                    endpoint = "https://api.deepseek.com/v1/chat/completions";
                    if (string.IsNullOrWhiteSpace(finalModel)) finalModel = "deepseek-chat";
                }
                else
                {
                    endpoint = "https://api.openai.com/v1/chat/completions";
                    if (string.IsNullOrWhiteSpace(finalModel) || finalModel.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
                    {
                        finalModel = "gpt-4o-mini";
                    }
                }

                var payload = new
                {
                    model = finalModel,
                    messages = new object[]
                    {
                        new { role = "user", content = "Merhaba, bu bir test sorgusudur. Lütfen sadece 'OK' yanıtını ver." }
                    },
                    max_tokens = 10
                };

                using var reqMsg = new HttpRequestMessage(HttpMethod.Post, endpoint);
                reqMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                reqMsg.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                var resp = await client.SendAsync(reqMsg);
                if (resp.IsSuccessStatusCode)
                {
                    return Ok(new { isSuccess = true, message = $"✅ Yapay Zeka API Token Doğrulandı! ({provider} - Model: {finalModel})" });
                }
                else
                {
                    var errContent = await resp.Content.ReadAsStringAsync();
                    return Ok(new { isSuccess = false, message = $"❌ Yapay Zeka API Hatası ({resp.StatusCode}): {errContent}" });
                }
            }
        }
        catch (Exception ex)
        {
            return Ok(new { isSuccess = false, message = $"❌ Yapay Zeka Test Hatası: {ex.Message}" });
        }
    }

public class TestAiSettingsRequest
{
    public string? ApiKey { get; set; }
    public string? Provider { get; set; }
    public string? Model { get; set; }
}

public class TestMailSettingsRequest
{
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public string? User { get; set; }
    public string? Pass { get; set; }
    public bool EnableSsl { get; set; } = true;
    public string? Recipient { get; set; }
}

public class TestSmsSettingsRequest
{
    public string? Phone { get; set; }
}

    [HttpGet("xslt")]
    public async Task<IActionResult> GetXslt([FromQuery] string type = "irsaliye")
    {
        string fileName = type.ToLower() switch
        {
            "fatura" => "eFaturaTemplate.xslt",
            "arsiv" => "eArsivTemplate.xslt",
            _ => "eIrsaliyeTemplate.xslt"
        };
        string filePath = Path.Combine(_env.ContentRootPath, "Templates", fileName);
        if (!System.IO.File.Exists(filePath))
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", fileName);
        }

        if (System.IO.File.Exists(filePath))
        {
            string content = await System.IO.File.ReadAllTextAsync(filePath);
            return Ok(new { isSuccess = true, content, type });
        }

        return Ok(new { isSuccess = false, message = "Şablon dosyası bulunamadı.", type, content = "" });
    }

    [HttpPost("xslt")]
    public async Task<IActionResult> SaveXslt([FromBody] SaveXsltRequest request)
    {
        string fileName = (request.Type ?? "irsaliye").ToLower() switch
        {
            "fatura" => "eFaturaTemplate.xslt",
            "arsiv" => "eArsivTemplate.xslt",
            _ => "eIrsaliyeTemplate.xslt"
        };

        try
        {
            string templatesDir = Path.Combine(_env.ContentRootPath, "Templates");
            Directory.CreateDirectory(templatesDir);
            string filePath = Path.Combine(templatesDir, fileName);
            await System.IO.File.WriteAllTextAsync(filePath, request.Content ?? "");

            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
            Directory.CreateDirectory(baseDir);
            await System.IO.File.WriteAllTextAsync(Path.Combine(baseDir, fileName), request.Content ?? "");

            return Ok(new { isSuccess = true, message = $"{request.Type?.ToUpper()} XSLT tasarımı başarıyla kaydedildi!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { isSuccess = false, message = $"XSLT kaydedilemedi: {ex.Message}" });
        }
    }
}

public class SaveXsltRequest
{
    public string Type { get; set; } = "irsaliye";
    public string Content { get; set; } = string.Empty;
}

