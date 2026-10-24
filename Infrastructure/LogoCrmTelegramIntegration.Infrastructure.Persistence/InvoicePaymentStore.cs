using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LogoCrmTelegramIntegration.Application.Interfaces;
using LogoCrmTelegramIntegration.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Persistence
{
    public class InvoicePaymentStore : IInvoicePaymentStore
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private readonly Dictionary<string, InvoicePaymentRecord> _records = new Dictionary<string, InvoicePaymentRecord>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<InvoicePaymentStore> _logger;

        public InvoicePaymentStore(ILogger<InvoicePaymentStore> logger)
        {
            _logger = logger;
            _filePath = Path.Combine(AppContext.BaseDirectory, "invoice_payment_status.json");
            LoadFromFile();
        }

        private void LoadFromFile()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_filePath))
                    {
                        var json = File.ReadAllText(_filePath);
                        var list = JsonSerializer.Deserialize<List<InvoicePaymentRecord>>(json);
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                if (!string.IsNullOrWhiteSpace(item.InvoiceKey))
                                {
                                    _records[item.InvoiceKey] = item;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fatura ödeme durumları JSON dosyasından okunurken hata oluştu.");
                }
            }
        }

        private void SaveToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_records.Values.ToList(), new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatura ödeme durumları dosyaya yazılırken hata oluştu.");
            }
        }

        public InvoicePaymentRecord? GetPaymentStatus(string invoiceKey)
        {
            if (string.IsNullOrWhiteSpace(invoiceKey)) return null;
            lock (_lock)
            {
                _records.TryGetValue(invoiceKey, out var record);
                return record;
            }
        }

        public Dictionary<string, InvoicePaymentRecord> GetAll()
        {
            lock (_lock)
            {
                return new Dictionary<string, InvoicePaymentRecord>(_records, StringComparer.OrdinalIgnoreCase);
            }
        }

        public bool SetPaymentStatus(string invoiceKey, string ficheNo, string status, decimal paidAmount, string? note, string? updatedBy, DateTime? paidDate = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceKey)) return false;

            lock (_lock)
            {
                if (!_records.TryGetValue(invoiceKey, out var record))
                {
                    record = new InvoicePaymentRecord
                    {
                        InvoiceKey = invoiceKey,
                        FicheNo = ficheNo ?? string.Empty
                    };
                    _records[invoiceKey] = record;
                }

                record.Status = string.IsNullOrWhiteSpace(status) ? "Odenmedi" : status;
                record.PaidAmount = paidAmount;
                record.PaidDate = (record.Status == "Odendi" && paidDate == null) ? DateTime.Now : paidDate;
                record.Note = note;
                record.UpdatedBy = updatedBy ?? "admin";
                record.UpdatedAt = DateTime.Now;

                SaveToFile();
                return true;
            }
        }
    }
}
