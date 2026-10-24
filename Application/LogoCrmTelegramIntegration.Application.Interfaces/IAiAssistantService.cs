using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LogoCrmTelegramIntegration.Application.Interfaces;

public class AiIntentResult
{
    public string Intent { get; set; } = "Unknown"; // Order, Dispatch, Risk, Report, General
    public string? CustomerCode { get; set; }
    public string? CustomerTitle { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public double Quantity { get; set; } = 1;
    public double DiscountPercent { get; set; } = 0;
    public string? Notes { get; set; }
    public double Confidence { get; set; } = 1.0;
    public string Explanation { get; set; } = string.Empty;
}

public class CustomerRiskReport
{
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerTitle { get; set; } = string.Empty;
    public decimal TotalBalance { get; set; }
    public decimal OverdueBalance { get; set; }
    public int OpenInvoiceCount { get; set; }
    public int OverdueInvoiceCount { get; set; }
    public int AveragePaymentDelayDays { get; set; }
    public string RiskLevel { get; set; } = "Düşük"; // Düşük, Orta, Yüksek
    public string AiRecommendation { get; set; } = string.Empty;
}

public class AiQueryResult
{
    public string MessageText { get; set; } = string.Empty;
    public List<AiButtonDto> Buttons { get; set; } = new List<AiButtonDto>();
}

public class AiButtonDto
{
    public string Text { get; set; } = string.Empty;
    public string CallbackData { get; set; } = string.Empty;
}

public interface IAiAssistantService
{
    Task<AiIntentResult> ParseIntentAndParametersAsync(string textInput, CancellationToken ct = default);
    Task<CustomerRiskReport> EvaluateCustomerRiskAsync(string customerCodeOrTitle, CancellationToken ct = default);
    Task<string> AnswerUserQueryAsync(string userQuery, CancellationToken ct = default);
    Task<AiQueryResult> AnswerUserQueryWithButtonsAsync(string userQuery, CancellationToken ct = default);
    Task<AiQueryResult> AnswerUserQueryWithButtonsAsync(long chatId, string userQuery, CancellationToken ct = default);

    /// <summary>
    /// Sohbet modunda kullanıcı mesajını direkt AI'ya iletir ve yanıt döner.
    /// Geçmiş otomatik yönetilir (per-chatId oturum).
    /// </summary>
    Task<string> DirectChatAsync(long chatId, string userMessage, CancellationToken ct = default);

    /// <summary>Belirtilen chatId için sohbet modunu açar/kapatır.</summary>
    bool ToggleSohbetMode(long chatId);

    /// <summary>Belirtilen chatId için sohbet modunun aktif olup olmadığını döner.</summary>
    bool IsSohbetModeActive(long chatId);

    /// <summary>Sohbet geçmişini temizler.</summary>
    void ClearChatHistory(long chatId);
}

