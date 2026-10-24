using System.Collections.Generic;
using LogoCrmTelegramIntegration.Domain.Entities;

namespace LogoCrmTelegramIntegration.Application.Interfaces
{
    public interface IInvoicePaymentStore
    {
        InvoicePaymentRecord? GetPaymentStatus(string invoiceKey);
        Dictionary<string, InvoicePaymentRecord> GetAll();
        bool SetPaymentStatus(string invoiceKey, string ficheNo, string status, decimal paidAmount, string? note, string? updatedBy, System.DateTime? paidDate = null);
    }
}
