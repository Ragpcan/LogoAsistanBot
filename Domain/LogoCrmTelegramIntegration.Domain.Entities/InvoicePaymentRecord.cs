using System;

namespace LogoCrmTelegramIntegration.Domain.Entities
{
    public class InvoicePaymentRecord
    {
        public string InvoiceKey { get; set; } = string.Empty; // e.g. "919_12345" or FicheNo
        public string FicheNo { get; set; } = string.Empty;
        public string Status { get; set; } = "Odenmedi"; // "Odendi", "Odenmedi", "Kismi"
        public decimal PaidAmount { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? Note { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
