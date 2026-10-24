using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LogoCrmTelegramIntegration.Application.DTOs;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Persistence;

public class CustomReportStore : ICustomReportStore
{
    private readonly string _filePath;
    private readonly object _lock = new object();
    private readonly Dictionary<string, CustomReportDto> _reports = new Dictionary<string, CustomReportDto>(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<CustomReportStore> _logger;

    public CustomReportStore(ILogger<CustomReportStore> logger)
    {
        _logger = logger;
        _filePath = Path.Combine(AppContext.BaseDirectory, "custom_reports.json");
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
                    var list = JsonSerializer.Deserialize<List<CustomReportDto>>(File.ReadAllText(_filePath));
                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            _reports[item.Id] = item;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "custom_reports.json okunurken hata oluştu.");
            }

            EnsureDefaultReports();
        }
    }

    private void EnsureDefaultReports()
    {
        var defaultReports = new List<CustomReportDto>
        {
            // 1. KASA DURUMU
            new CustomReportDto
            {
                Id = "rpt_kasa_bakiye",
                Title = "💰 Kasa Bakiyeleri & Durumu",
                Description = "Aktif kasaların kod, isim ve güncel bakiye özetini getirir.",
                Category = "Kasa",
                Icon = "💰",
                TelegramButtonText = "💰 Kasa Bakiyeleri",
                DisplayOrder = 1,
                ShowInFirmamMenu = true,
                SqlQuery = @"SELECT 
    K.CODE AS [Kasa Kodu],
    K.NAME AS [Kasa Adı],
    ISNULL(ROUND(SUM(CASE WHEN L.SIGN = 0 THEN L.AMOUNT ELSE -L.AMOUNT END), 2), 0) AS [Kasa Bakiyesi (TL)]
FROM LG_{firmNo}_KSCARD K WITH(NOLOCK)
LEFT JOIN LG_{firmNo}_{periodNo}_KSLINES L WITH(NOLOCK) ON K.LOGICALREF = L.CARDREF AND L.CANCELLED = 0
WHERE K.ACTIVE = 0
GROUP BY K.CODE, K.NAME
ORDER BY [Kasa Bakiyesi (TL)] DESC"
            },

            // 2. BANKA DURUMU
            new CustomReportDto
            {
                Id = "rpt_banka_bakiye",
                Title = "🏦 Banka Hesap Bakiyeleri",
                Description = "Tüm banka ticari ve vadesiz hesaplarının toplam bakiyelerini listeler.",
                Category = "Banka",
                Icon = "🏦",
                TelegramButtonText = "🏦 Banka Bakiyeleri",
                DisplayOrder = 2,
                ShowInFirmamMenu = true,
                SqlQuery = @"SELECT 
    B.CODE AS [Banka Kodu],
    B.DEFINITION_ AS [Banka Adı],
    BA.ACCOUNTNO AS [Hesap No],
    BA.IBAN AS [IBAN],
    ISNULL(ROUND(SUM(CASE WHEN L.SIGN = 0 THEN L.AMOUNT ELSE -L.AMOUNT END), 2), 0) AS [Hesap Bakiyesi (TL)]
FROM LG_{firmNo}_BNCARD B WITH(NOLOCK)
JOIN LG_{firmNo}_BANKACC BA WITH(NOLOCK) ON B.LOGICALREF = BA.BANKREF
LEFT JOIN LG_{firmNo}_{periodNo}_BNFLINE L WITH(NOLOCK) ON BA.LOGICALREF = L.BNACCREF AND L.TRANSTYPE IN (1,2,3,4,5,6) AND L.CANCELLED = 0
GROUP BY B.CODE, B.DEFINITION_, BA.ACCOUNTNO, BA.IBAN
ORDER BY [Hesap Bakiyesi (TL)] DESC"
            },

            // 3. ÇEK / SENET PORTFÖYÜ
            new CustomReportDto
            {
                Id = "rpt_cek_portfoy",
                Title = "📑 Portföydeki Müşteri Çekleri",
                Description = "Portföyde tahsilat bekleyen vadeli müşteri çeklerini vadesine göre listeler.",
                Category = "CekSenet",
                Icon = "📑",
                TelegramButtonText = "📑 Portföydeki Çekler",
                DisplayOrder = 3,
                ShowInFirmamMenu = true,
                SqlQuery = @"SELECT TOP 15
    C.NEWSERENO AS [Çek No],
    C.BANKNAME AS [Banka],
    CONVERT(VARCHAR(10), C.DUEDATE, 104) AS [Vade Tarihi],
    ISNULL(CL.DEFINITION_, 'Belirtilmemiş') AS [Borçlu / Keşideci],
    ROUND(C.AMOUNT, 2) AS [Çek Tutarı (TL)]
FROM LG_{firmNo}_{periodNo}_CSCARD C WITH(NOLOCK)
LEFT JOIN LG_{firmNo}_CLCARD CL WITH(NOLOCK) ON C.CLIENTREF = CL.LOGICALREF
WHERE C.DOC = 1 AND C.CURRSTAT = 1
ORDER BY C.DUEDATE ASC"
            },

            // 4. SATIŞ FATURALARI (INVOICE Tablosu)
            new CustomReportDto
            {
                Id = "rpt_satis_faturalari",
                Title = "📈 Son Satış Faturaları (Günlük/Aylık)",
                Description = "Son kesilen satış faturalarını cari ünvanı ve net tutarları ile listeler.",
                Category = "Fatura",
                Icon = "📈",
                TelegramButtonText = "📈 Satış Faturaları",
                DisplayOrder = 4,
                ShowInFirmamMenu = true,
                SqlQuery = @"SELECT TOP 15
    CONVERT(VARCHAR(10), I.DATE_, 104) AS [Tarih],
    I.FICHENO AS [Fatura No],
    CASE I.TRCODE 
        WHEN 8 THEN 'Toptan Satış' 
        WHEN 7 THEN 'Perakende Satış' 
        WHEN 9 THEN 'Verilen Hizmet' 
        ELSE 'Satış' END AS [Fatura Türü],
    ISNULL(C.DEFINITION_, 'Perakende / Muhtelif') AS [Müşteri / Cari],
    ROUND(I.NETTOTAL, 2) AS [Net Tutar (TL)]
FROM LG_{firmNo}_{periodNo}_INVOICE I WITH(NOLOCK)
LEFT JOIN LG_{firmNo}_CLCARD C WITH(NOLOCK) ON I.CLIENTREF = C.LOGICALREF
WHERE I.TRCODE IN (7, 8, 9) AND I.CANCELLED = 0
ORDER BY I.DATE_ DESC, I.LOGICALREF DESC"
            },

            // 5. ALIŞ FATURALARI (INVOICE Tablosu)
            new CustomReportDto
            {
                Id = "rpt_alis_faturalari",
                Title = "🛒 Son Satınalma / Alış Faturaları",
                Description = "Tedarikçilerden gelen son satınalma faturalarını listeler.",
                Category = "Fatura",
                Icon = "🛒",
                TelegramButtonText = "🛒 Alış Faturaları",
                DisplayOrder = 5,
                ShowInFirmamMenu = true,
                SqlQuery = @"SELECT TOP 15
    CONVERT(VARCHAR(10), I.DATE_, 104) AS [Tarih],
    I.FICHENO AS [Fatura No],
    CASE I.TRCODE 
        WHEN 1 THEN 'Satınalma' 
        WHEN 4 THEN 'Alınan Hizmet' 
        WHEN 5 THEN 'Alınan Proforma' 
        ELSE 'Alış' END AS [Fatura Türü],
    ISNULL(C.DEFINITION_, 'Tedarikçi') AS [Tedarikçi / Cari],
    ROUND(I.NETTOTAL, 2) AS [Net Tutar (TL)]
FROM LG_{firmNo}_{periodNo}_INVOICE I WITH(NOLOCK)
LEFT JOIN LG_{firmNo}_CLCARD C WITH(NOLOCK) ON I.CLIENTREF = C.LOGICALREF
WHERE I.TRCODE IN (1, 4) AND I.CANCELLED = 0
ORDER BY I.DATE_ DESC, I.LOGICALREF DESC"
            },

            // 6. İRSALİYE RAPORLARI (STFICHE Tablosu - Faturalanmamış Açık İrsaliyeler)
            new CustomReportDto
            {
                Id = "rpt_acik_irsaliyeler",
                Title = "🚚 Açık (Faturalanmamış) Sevk İrsaliyeleri",
                Description = "Sevk edilmiş ancak henüz faturası kesilmemiş açık irsaliyeleri listeler.",
                Category = "Irsaliye",
                Icon = "🚚",
                TelegramButtonText = "🚚 Açık İrsaliyeler",
                DisplayOrder = 6,
                ShowInFirmamMenu = true,
                SqlQuery = @"SELECT TOP 15
    CONVERT(VARCHAR(10), S.DATE_, 104) AS [İrsaliye Tarihi],
    S.FICHENO AS [İrsaliye No],
    CASE S.TRCODE 
        WHEN 8 THEN 'Toptan Satış İrs.' 
        WHEN 7 THEN 'Perakende Satış İrs.' 
        WHEN 1 THEN 'Satınalma İrs.' 
        ELSE 'İrsaliye' END AS [Tür],
    ISNULL(C.DEFINITION_, 'Cari Belirtilmemiş') AS [Cari Ünvanı],
    ROUND(S.GROSSTOTAL, 2) AS [Tutar (TL)]
FROM LG_{firmNo}_{periodNo}_STFICHE S WITH(NOLOCK)
LEFT JOIN LG_{firmNo}_CLCARD C WITH(NOLOCK) ON S.CLIENTREF = C.LOGICALREF
WHERE S.TRCODE IN (1, 7, 8) AND S.BILLED = 0 AND S.CANCELLED = 0
ORDER BY S.DATE_ DESC, S.LOGICALREF DESC"
            },

            // 7. BEKLEYEN AÇIK SİPARİŞLER (ORFICHE)
            new CustomReportDto
            {
                Id = "rpt_acik_siparisler",
                Title = "📋 Bekleyen Açık Müşteri Siparişleri",
                Description = "Henüz sevk edilmemiş / teslimatı bekleyen müşteri siparişlerini listeler.",
                Category = "Siparis",
                Icon = "📋",
                TelegramButtonText = "📋 Bekleyen Siparişler",
                DisplayOrder = 7,
                ShowInFirmamMenu = true,
                SqlQuery = @"SELECT TOP 15
    CONVERT(VARCHAR(10), O.DATE_, 104) AS [Sipariş Tarihi],
    O.FICHENO AS [Sipariş No],
    ISNULL(C.DEFINITION_, 'Cari') AS [Müşteri],
    ROUND(O.NETTOTAL, 2) AS [Sipariş Tutarı (TL)]
FROM LG_{firmNo}_{periodNo}_ORFICHE O WITH(NOLOCK)
LEFT JOIN LG_{firmNo}_CLCARD C WITH(NOLOCK) ON O.CLIENTREF = C.LOGICALREF
WHERE O.TRCODE = 1 AND O.STATUS = 4 AND O.CANCELLED = 0
ORDER BY O.DATE_ DESC"
            },

            // 8. GENEL MİZAN RAPORU (EMUHACC & EMFLINE)
            new CustomReportDto
            {
                Id = "rpt_genel_mizan",
                Title = "📚 Genel Muhasebe Mizan Özeti (Ana Hesaplar)",
                Description = "Logo ERP genel muhasebe ana hesaplarının borç/alacak ve bakiye mizan özetini getirir.",
                Category = "Mizan",
                Icon = "📚",
                TelegramButtonText = "📚 Genel Mizan",
                DisplayOrder = 8,
                ShowInFirmamMenu = true,
                SqlQuery = @"SELECT TOP 20
    A.CODE AS [Hesap Kodu],
    A.DEFINITION_ AS [Hesap Adı],
    ROUND(ISNULL(SUM(CASE WHEN L.SIGN = 0 THEN L.AMOUNT ELSE 0 END), 0), 2) AS [Borç Tutarı],
    ROUND(ISNULL(SUM(CASE WHEN L.SIGN = 1 THEN L.AMOUNT ELSE 0 END), 0), 2) AS [Alacak Tutarı],
    ROUND(ISNULL(SUM(CASE WHEN L.SIGN = 0 THEN L.AMOUNT ELSE -L.AMOUNT END), 0), 2) AS [Net Bakiye]
FROM LG_{firmNo}_EMUHACC A WITH(NOLOCK)
LEFT JOIN LG_{firmNo}_{periodNo}_EMFLINE L WITH(NOLOCK) ON A.LOGICALREF = L.ACCOUNTREF AND L.CANCELLED = 0
WHERE A.LEVEL_ = 0
GROUP BY A.CODE, A.DEFINITION_
HAVING ISNULL(SUM(L.AMOUNT), 0) > 0
ORDER BY A.CODE ASC"
            },

            // 9. ÜRETİM EMİRLERİ (PRODORD)
            new CustomReportDto
            {
                Id = "rpt_uretim_emirleri",
                Title = "🏭 Açık / Devam Eden Üretim Emirleri",
                Description = "İmalatta devam eden veya planlanmış üretim emirlerini ve mamul durumunu listeler.",
                Category = "Uretim",
                Icon = "🏭",
                TelegramButtonText = "🏭 Üretim Emirleri",
                DisplayOrder = 9,
                ShowInFirmamMenu = true,
                SqlQuery = @"SELECT TOP 15
    P.FICHENO AS [Üretim Emri No],
    CONVERT(VARCHAR(10), P.PLANDATE, 104) AS [Planlanan Tarih],
    ISNULL(I.NAME, 'Mamul') AS [Üretilen Mamul],
    ROUND(P.PLNAMOUNT, 2) AS [Planlanan Miktar],
    ROUND(P.ACTAMOUNT, 2) AS [Gerçekleşen Miktar],
    CASE P.STATUS 
        WHEN 0 THEN 'Planlandı' 
        WHEN 1 THEN 'Başladı / Üretimde' 
        WHEN 2 THEN 'Tamamlandı' 
        WHEN 3 THEN 'İptal' 
        ELSE 'Devam Ediyor' END AS [Durum]
FROM LG_{firmNo}_{periodNo}_PRODORD P WITH(NOLOCK)
LEFT JOIN LG_{firmNo}_ITEMS I WITH(NOLOCK) ON P.ITEMREF = I.LOGICALREF
WHERE P.STATUS IN (0, 1)
ORDER BY P.PLANDATE DESC"
            },

            // 10. CARİ ÖZEL: ÜRÜN BAZLI ALIMLAR
            new CustomReportDto
            {
                Id = "rpt_cari_urun_alimlari",
                Title = "👤 Carinin En Çok Satın Aldığı Ürünler",
                Description = "Seçili cari hesabın faturayla en çok satın aldığı ürünleri ve adetlerini listeler.",
                Category = "Cari",
                Icon = "👤",
                TelegramButtonText = "📦 Aldığı Ürünler",
                DisplayOrder = 10,
                ShowInFirmamMenu = false,
                SqlQuery = @"SELECT TOP 10
    I.CODE AS [Ürün Kodu],
    I.NAME AS [Ürün Adı],
    ROUND(SUM(L.AMOUNT), 2) AS [Toplam Adet],
    ROUND(SUM(L.TOTAL), 2) AS [Toplam Tutar (TL)]
FROM LG_{firmNo}_{periodNo}_STLINE L WITH(NOLOCK)
JOIN LG_{firmNo}_ITEMS I WITH(NOLOCK) ON L.STOCKREF = I.LOGICALREF
WHERE L.CLIENTREF = {clientRef} AND L.TRCODE IN (7, 8) AND L.LINETYPE = 0 AND L.CANCELLED = 0
GROUP BY I.CODE, I.NAME
ORDER BY [Toplam Adet] DESC"
            }
        };

        foreach (var r in defaultReports)
        {
            _reports[r.Id] = r;
        }

        SaveToFile();
    }

    public void SaveToFile()
    {
        try
        {
            var json = JsonSerializer.Serialize(_reports.Values.OrderBy(x => x.DisplayOrder).ToList(), new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "custom_reports.json kaydedilirken hata oluştu.");
        }
    }

    public List<CustomReportDto> GetAllReports()
    {
        lock (_lock)
        {
            return _reports.Values.OrderBy(x => x.DisplayOrder).ToList();
        }
    }

    public List<CustomReportDto> GetReportsByCategory(string category)
    {
        lock (_lock)
        {
            return _reports.Values
                .Where(x => x.IsActive && string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.DisplayOrder)
                .ToList();
        }
    }

    public List<CustomReportDto> GetFirmamReports()
    {
        lock (_lock)
        {
            return _reports.Values
                .Where(x => x.IsActive && x.ShowInFirmamMenu)
                .OrderBy(x => x.DisplayOrder)
                .ToList();
        }
    }

    public CustomReportDto? GetReportById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        lock (_lock)
        {
            if (_reports.TryGetValue(id, out var report))
            {
                return report;
            }

            // Alias Map
            var cleanId = id.Trim().ToLowerInvariant();
            string? mappedId = cleanId switch
            {
                "kasa_durumu" or "kasa" => "rpt_kasa_bakiye",
                "banka_durumu" or "banka" => "rpt_banka_bakiye",
                "cek_portfoy" or "cek" => "rpt_cek_portfoy",
                "satis_fatura" or "satis_faturalari" => "rpt_satis_faturalari",
                "alis_fatura" or "alis_faturalari" => "rpt_alis_faturalari",
                "acik_irsaliye" or "acik_irsaliyeler" => "rpt_acik_irsaliyeler",
                "bekleyen_siparis" or "acik_siparisler" => "rpt_acik_siparisler",
                "genel_mizan" or "mizan" => "rpt_genel_mizan",
                "uretim_emirleri" or "uretim" => "rpt_uretim_emirleri",
                "cari_urun_alimlari" => "rpt_cari_urun_alimlari",
                _ => null
            };

            if (mappedId != null && _reports.TryGetValue(mappedId, out var mappedReport))
            {
                return mappedReport;
            }

            // Fuzzy lookup
            return _reports.Values.FirstOrDefault(r => 
                r.Id.Equals(id, StringComparison.OrdinalIgnoreCase) ||
                r.Id.EndsWith(id, StringComparison.OrdinalIgnoreCase) ||
                r.Title.Contains(id, StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool SaveOrUpdateReport(CustomReportDto report, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (report == null || string.IsNullOrWhiteSpace(report.Title))
        {
            errorMessage = "Rapor başlığı boş olamaz.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(report.SqlQuery))
        {
            errorMessage = "SQL sorgusu boş olamaz.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(report.Id))
        {
            report.Id = "rpt_" + Guid.NewGuid().ToString("N").Substring(0, 10);
        }

        if (string.IsNullOrWhiteSpace(report.TelegramButtonText))
        {
            report.TelegramButtonText = report.Title.Length > 25 ? report.Title.Substring(0, 25) : report.Title;
        }

        lock (_lock)
        {
            _reports[report.Id] = report;
            SaveToFile();
            return true;
        }
    }

    public bool DeleteReport(string id, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(id))
        {
            errorMessage = "Geçersiz rapor ID.";
            return false;
        }

        lock (_lock)
        {
            if (_reports.Remove(id))
            {
                SaveToFile();
                return true;
            }
            errorMessage = "Rapor bulunamadı.";
            return false;
        }
    }

    public string ResolveSqlPlaceholders(string sql, string firmNo, string periodNo, string? clientRef = null, string? itemRef = null, string? startDate = null, string? endDate = null)
    {
        if (string.IsNullOrWhiteSpace(sql)) return string.Empty;

        string cleanFirm = (firmNo ?? "001").PadLeft(3, '0');
        string cleanPeriod = (periodNo ?? "01").PadLeft(2, '0');

        string resolved = sql
            .Replace("{firmNo}", cleanFirm, StringComparison.OrdinalIgnoreCase)
            .Replace("{periodNo}", cleanPeriod, StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(clientRef))
        {
            resolved = resolved.Replace("{clientRef}", clientRef, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            resolved = resolved.Replace("{clientRef}", "0", StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(itemRef))
        {
            resolved = resolved.Replace("{itemRef}", itemRef, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            resolved = resolved.Replace("{itemRef}", "0", StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(startDate))
        {
            resolved = resolved.Replace("{startDate}", startDate, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            resolved = resolved.Replace("{startDate}", DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"), StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(endDate))
        {
            resolved = resolved.Replace("{endDate}", endDate, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            resolved = resolved.Replace("{endDate}", DateTime.Now.ToString("yyyy-MM-dd"), StringComparison.OrdinalIgnoreCase);
        }

        return resolved;
    }
}
