using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class ExecutiveReportService : IExecutiveReportService
{
    private readonly ILogoErpRepository _logoErpRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExecutiveReportService> _logger;

    public ExecutiveReportService(
        ILogoErpRepository logoErpRepository,
        IConfiguration configuration,
        ILogger<ExecutiveReportService> logger)
    {
        _logoErpRepository = logoErpRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<DailyExecutiveSummaryDto> GenerateDailySummaryAsync(CancellationToken ct = default)
    {
        var firmNo = _configuration["LogoErp:FirmNo"] ?? "001";
        var periodNo = _configuration["LogoErp:PeriodNo"] ?? "01";
        int warehouseNo = int.TryParse(_configuration["LogoErp:WarehouseNo"], out var w) ? w : 0;

        var dto = new DailyExecutiveSummaryDto();
        var today = DateTime.Today;

        try
        {
            // 1. Bugünkü Satış Faturaları
            var invoices = await _logoErpRepository.GetInvoicesAsync(null, today, today.AddDays(1).AddSeconds(-1), firmNo, periodNo, 100, ct);
            if (invoices != null && invoices.Any())
            {
                dto.TodaySalesFicheCount = invoices.Count;
                dto.TodaySalesTotal = invoices.Sum(i => i.NetTotal);
            }

            // 2. Açık / Bekleyen Siparişler
            var openOrders = await _logoErpRepository.GetOpenOrdersAsync(null, firmNo, periodNo, 100, ct);
            if (openOrders != null && openOrders.Any())
            {
                dto.PendingOrderCount = openOrders.Count;
                dto.PendingOrderTotal = openOrders.Sum(o => o.NetTotal);
            }

            // 3. Vadesi Geçen Alacaklar & Cari Listesi
            var borcluCariler = await _logoErpRepository.GetFirmListAsync("BORCLU", firmNo, periodNo, ct);
            if (borcluCariler != null && borcluCariler.Any())
            {
                dto.OverdueReceivablesTotal = borcluCariler.Sum(c => c.BalanceTL);
                dto.OverdueCustomerCount = borcluCariler.Count(c => c.BalanceTL > 0);
            }

            // 4. Kritik Stok Durumu
            dto.CriticalStockItems = await GetCriticalStockItemsAsync(5.0, ct);

            // 5. Telegram HTML Şablonu Oluşturma
            var sb = new StringBuilder();
            sb.AppendLine("📊 <b>GÜNLÜK YÖNETİCİ ÖZET RAPORU</b>");
            sb.AppendLine($"📅 <i>Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}</i>");
            sb.AppendLine("-----------------------------------------");
            sb.AppendLine($"📈 <b>Bugünkü Satış Cirosu:</b> {dto.TodaySalesTotal:N2} TL ({dto.TodaySalesFicheCount} Fatura)");
            sb.AppendLine($"📦 <b>Bekleyen Siparişler:</b> {dto.PendingOrderCount} Adet ({dto.PendingOrderTotal:N2} TL)");
            sb.AppendLine($"⚠️ <b>Toplam Alacak / Borçlu Cari:</b> {dto.OverdueReceivablesTotal:N2} TL ({dto.OverdueCustomerCount} Müşteri)");
            sb.AppendLine("-----------------------------------------");

            if (dto.CriticalStockItems.Any())
            {
                sb.AppendLine("🚨 <b>Kritik Stok Uyarısı (5 Adet ve Altı):</b>");
                foreach (var item in dto.CriticalStockItems.Take(8))
                {
                    sb.AppendLine($"• <code>{item.ItemCode}</code> - {item.ItemName}: <b>{item.CurrentOnHand} {item.Unit}</b>");
                }
                if (dto.CriticalStockItems.Count > 8)
                {
                    sb.AppendLine($"<i>...ve {dto.CriticalStockItems.Count - 8} ürün daha kritik seviyede.</i>");
                }
            }
            else
            {
                sb.AppendLine("✅ <b>Stok Durumu:</b> Kritik seviyede ürün bulunmamaktadır.");
            }

            sb.AppendLine("-----------------------------------------");
            sb.AppendLine("🤖 <i>Logo Asistan Bot Kurumsal Yönetim Sistemi</i>");

            dto.FormattedTelegramHtml = sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Günlük yönetici özet raporu oluşturulurken hata oluştu.");
            dto.FormattedTelegramHtml = "❌ <b>Rapor Oluşturma Hatası:</b> Veritabanı verileri alınırken bir sorun oluştu.";
        }

        return dto;
    }

    public async Task<List<CriticalStockItemDto>> GetCriticalStockItemsAsync(double minThreshold = 5.0, CancellationToken ct = default)
    {
        var list = new List<CriticalStockItemDto>();
        var firmNo = _configuration["LogoErp:FirmNo"] ?? "001";
        var periodNo = _configuration["LogoErp:PeriodNo"] ?? "01";
        int warehouseNo = int.TryParse(_configuration["LogoErp:WarehouseNo"], out var w) ? w : 0;

        try
        {
            var stocks = await _logoErpRepository.SearchItemStockAsync("", firmNo, periodNo, warehouseNo, ct);
            if (stocks != null && stocks.Any())
            {
                list = stocks
                    .Where(s => (double)s.TotalStock <= minThreshold)
                    .Select(s => new CriticalStockItemDto
                    {
                        ItemCode = s.ItemCode ?? "",
                        ItemName = s.ItemName ?? "",
                        CurrentOnHand = (double)s.TotalStock,
                        Unit = s.UnitName ?? "ADET"
                    })
                    .OrderBy(s => s.CurrentOnHand)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kritik stok ürünleri getirilirken hata oluştu.");
        }

        return list;
    }
}
