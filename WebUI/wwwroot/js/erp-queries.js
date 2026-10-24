// ==========================================
// LOGO ERP SQL SORGULARI VE TABLO OLUŞTURUCU
// ==========================================

const ERP_QUERIES = {

    // ============ 1. CARİ YÖNETİMİ ============
    'view-cari-list': {
        title: 'Cari Kartlar Listesi',
        sql: `SELECT
                C.CODE        AS [Cari Kodu],
                C.DEFINITION_ AS [Cari Unvanı],
                C.TAXNR       AS [Vergi No],
                C.TAXOFFICE   AS [Vergi Dairesi],
                C.TELNRS1     AS [Telefon],
                C.EMAILADDR   AS [E-Posta],
                CASE C.CARDTYPE WHEN 1 THEN 'Alıcı' WHEN 2 THEN 'Satıcı' WHEN 3 THEN 'Alıcı+Satıcı' ELSE 'Diğer' END AS [Tip]
              FROM LG_001_CLCARD C WITH(NOLOCK)
              WHERE C.ACTIVE = 0
              ORDER BY C.DEFINITION_`
    },
    'view-cari-receivables': {
        title: 'Alacaklı Cari Listesi',
        sql: `SELECT
                C.CODE        AS [Cari Kodu],
                C.DEFINITION_ AS [Cari Unvanı],
                SUM(F.NETTOTAL) AS [Toplam Alacak (₺)]
              FROM LG_001_01_INVOICE F WITH(NOLOCK)
              JOIN LG_001_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
              WHERE F.TRCODE IN (7, 8, 9, 35)
                AND F.CANCELLED = 0
              GROUP BY C.CODE, C.DEFINITION_
              HAVING SUM(F.NETTOTAL) > 0
              ORDER BY SUM(F.NETTOTAL) DESC`
    },
    'view-cari-payables': {
        title: 'Borçlu Cari Listesi',
        sql: `SELECT
                C.CODE        AS [Cari Kodu],
                C.DEFINITION_ AS [Cari Unvanı],
                SUM(F.NETTOTAL) AS [Toplam Borç (₺)]
              FROM LG_001_01_INVOICE F WITH(NOLOCK)
              JOIN LG_001_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
              WHERE F.TRCODE IN (1, 2, 3, 4, 5, 6)
                AND F.CANCELLED = 0
              GROUP BY C.CODE, C.DEFINITION_
              HAVING SUM(F.NETTOTAL) > 0
              ORDER BY SUM(F.NETTOTAL) DESC`
    },
    'view-debt-tracking': {
        title: 'Borç Takip Raporu (Vade Yaşlandırma)',
        sql: `SELECT
                C.DEFINITION_ AS [Cari Unvanı],
                ISNULL(SUM(CASE WHEN DATEDIFF(day, T.PROCDATE, GETDATE()) BETWEEN 0  AND 30  THEN (T.TOTAL - T.PAID) ELSE 0 END), 0) AS [0-30 Gün (₺)],
                ISNULL(SUM(CASE WHEN DATEDIFF(day, T.PROCDATE, GETDATE()) BETWEEN 31 AND 60  THEN (T.TOTAL - T.PAID) ELSE 0 END), 0) AS [31-60 Gün (₺)],
                ISNULL(SUM(CASE WHEN DATEDIFF(day, T.PROCDATE, GETDATE()) BETWEEN 61 AND 90  THEN (T.TOTAL - T.PAID) ELSE 0 END), 0) AS [61-90 Gün (₺)],
                ISNULL(SUM(CASE WHEN DATEDIFF(day, T.PROCDATE, GETDATE()) > 90              THEN (T.TOTAL - T.PAID) ELSE 0 END), 0) AS [90+ Gün (₺)],
                ISNULL(SUM(T.TOTAL - T.PAID), 0) AS [Toplam Borç (₺)]
              FROM LG_001_01_PAYTRANS T WITH(NOLOCK)
              JOIN LG_001_CLCARD C WITH(NOLOCK) ON T.CARDREF = C.LOGICALREF
              WHERE T.SIGN = 1
                AND T.CANCELLED = 0
                AND (T.TOTAL - T.PAID) > 0
              GROUP BY C.DEFINITION_
              ORDER BY [Toplam Borç (₺)] DESC`
    },
    'view-risk-status': {
        title: 'Cari Risk Durum Raporu',
        sql: `SELECT
                C.CODE          AS [Cari Kodu],
                C.DEFINITION_   AS [Cari Unvanı],
                ISNULL(C.RISKLIMIT, 0)     AS [Risk Limiti (₺)],
                ISNULL(BAKIYE.TUTAR, 0) AS [Güncel Bakiye (₺)],
                CASE
                  WHEN ISNULL(C.RISKLIMIT, 0) = 0 THEN 'Limit Tanımsız'
                  WHEN ISNULL(BAKIYE.TUTAR, 0) >= C.RISKLIMIT THEN '🔴 Limit Aşıldı'
                  WHEN ISNULL(BAKIYE.TUTAR, 0) >= C.RISKLIMIT * 0.8 THEN '🟡 Limit Yakın'
                  ELSE '🟢 Normal'
                END AS [Risk Durumu]
              FROM LG_001_CLCARD C WITH(NOLOCK)
              LEFT JOIN (
                SELECT CARDREF, SUM(TOTAL - PAID) AS TUTAR
                FROM LG_001_01_PAYTRANS WITH(NOLOCK)
                WHERE SIGN = 0 AND CANCELLED = 0
                GROUP BY CARDREF
              ) BAKIYE ON BAKIYE.CARDREF = C.LOGICALREF
              WHERE C.ACTIVE = 0
              ORDER BY ISNULL(BAKIYE.TUTAR, 0) DESC`
    },

    // ============ 2. SATIŞ YÖNETİMİ ============
    'view-sales-orders': {
        title: 'Satış Siparişleri',
        sql: `SELECT
                F.FICHENO     AS [Fiş No],
                CONVERT(varchar, F.DATE_, 104) AS [Tarih],
                C.DEFINITION_ AS [Cari Unvanı],
                F.DOCODE      AS [Belge No],
                F.NETTOTAL    AS [Net Toplam (₺)],
                CASE F.STATUS WHEN 1 THEN 'Öneri' WHEN 2 THEN 'Sevk Edilemez' WHEN 3 THEN 'Onaylandı' WHEN 4 THEN 'Sevk Edilebilir' WHEN 8 THEN 'Kapandı' ELSE 'Diğer' END AS [Durum]
              FROM LG_001_01_ORFICHE F WITH(NOLOCK)
              LEFT JOIN LG_001_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
              WHERE F.TRCODE = 1 AND F.CANCELLED = 0
              ORDER BY F.DATE_ DESC, F.LOGICALREF DESC`
    },
    'view-sales-dispatches': {
        title: 'Satış İrsaliyeleri',
        sql: `SELECT
                F.FICHENO     AS [Fiş No],
                CONVERT(varchar, F.DATE_, 104) AS [Tarih],
                C.DEFINITION_ AS [Cari Unvanı],
                F.DOCODE      AS [Belge No],
                F.NETTOTAL    AS [Net Toplam (₺)]
              FROM LG_001_01_STFICHE F WITH(NOLOCK)
              LEFT JOIN LG_001_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
              WHERE F.TRCODE IN (7, 8) AND F.CANCELLED = 0
              ORDER BY F.DATE_ DESC, F.LOGICALREF DESC`
    },
    'view-sales-invoices': {
        title: 'Satış Faturaları',
        sql: `SELECT
                F.FICHENO     AS [Fiş No],
                CONVERT(varchar, F.DATE_, 104) AS [Tarih],
                C.DEFINITION_ AS [Cari Unvanı],
                F.DOCODE      AS [Belge No],
                F.GROSSTOTAL  AS [Mal Bedeli (₺)],
                F.TOTALDISCOUNTS AS [İskonto (₺)],
                F.TOTALVAT    AS [KDV (₺)],
                F.NETTOTAL    AS [Net Toplam (₺)]
              FROM LG_001_01_INVOICE F WITH(NOLOCK)
              LEFT JOIN LG_001_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
              WHERE F.TRCODE IN (7, 8)
                AND F.CANCELLED = 0
              ORDER BY F.DATE_ DESC, F.LOGICALREF DESC`
    },
    'view-sales-profitability': {
        title: 'Satış Karlılık Durum Raporu',
        sql: `SELECT
                I.CODE           AS [Malzeme Kodu],
                I.NAME           AS [Malzeme Adı],
                SUM(L.AMOUNT)    AS [Toplam Miktar],
                SUM(L.LINENET)   AS [Satış Tutarı (₺)],
                SUM(L.TOTAL)     AS [Brüt Tutar (₺)]
              FROM LG_001_01_STLINE L WITH(NOLOCK)
              JOIN LG_001_ITEMS I WITH(NOLOCK) ON L.STOCKREF = I.LOGICALREF
              WHERE L.TRCODE IN (7, 8)
                AND L.CANCELLED = 0
                AND L.LINETYPE = 0
              GROUP BY I.CODE, I.NAME
              ORDER BY SUM(L.LINENET) DESC`
    },

    // ============ 3. SATINALMA YÖNETİMİ ============
    'view-purchase-orders': {
        title: 'Alış Siparişleri',
        sql: `SELECT
                F.FICHENO     AS [Fiş No],
                CONVERT(varchar, F.DATE_, 104) AS [Tarih],
                C.DEFINITION_ AS [Tedarikçi],
                F.DOCODE      AS [Belge No],
                F.NETTOTAL    AS [Net Toplam (₺)],
                CASE F.STATUS WHEN 1 THEN 'Öneri' WHEN 2 THEN 'Onaylandı' WHEN 4 THEN 'Sevk Edilebilir' WHEN 8 THEN 'Kapandı' ELSE 'Diğer' END AS [Durum]
              FROM LG_001_01_ORFICHE F WITH(NOLOCK)
              LEFT JOIN LG_001_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
              WHERE F.TRCODE = 2 AND F.CANCELLED = 0
              ORDER BY F.DATE_ DESC, F.LOGICALREF DESC`
    },
    'view-purchase-dispatches': {
        title: 'Alış İrsaliyeleri',
        sql: `SELECT
                F.FICHENO     AS [Fiş No],
                CONVERT(varchar, F.DATE_, 104) AS [Tarih],
                C.DEFINITION_ AS [Tedarikçi],
                F.DOCODE      AS [Belge No],
                F.NETTOTAL    AS [Net Toplam (₺)]
              FROM LG_001_01_STFICHE F WITH(NOLOCK)
              LEFT JOIN LG_001_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
              WHERE F.TRCODE = 1 AND F.CANCELLED = 0
              ORDER BY F.DATE_ DESC, F.LOGICALREF DESC`
    },
    'view-purchase-invoices': {
        title: 'Alış Faturaları',
        sql: `SELECT
                F.FICHENO     AS [Fiş No],
                CONVERT(varchar, F.DATE_, 104) AS [Tarih],
                C.DEFINITION_ AS [Tedarikçi],
                F.DOCODE      AS [Belge No],
                F.GROSSTOTAL  AS [Mal Bedeli (₺)],
                F.TOTALVAT    AS [KDV (₺)],
                F.NETTOTAL    AS [Net Toplam (₺)]
              FROM LG_001_01_INVOICE F WITH(NOLOCK)
              LEFT JOIN LG_001_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
              WHERE F.TRCODE IN (1, 2, 4, 5, 6)
                AND F.CANCELLED = 0
              ORDER BY F.DATE_ DESC, F.LOGICALREF DESC`
    },
    'view-purchase-focused-caris': {
        title: 'Alış Ağırlıklı Cariler Listesi',
        sql: `SELECT
                C.CODE        AS [Cari Kodu],
                C.DEFINITION_ AS [Tedarikçi Unvanı],
                COUNT(F.LOGICALREF) AS [Fatura Sayısı],
                SUM(F.NETTOTAL) AS [Toplam Alım Tutarı (₺)]
              FROM LG_001_01_INVOICE F WITH(NOLOCK)
              JOIN LG_001_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
              WHERE F.TRCODE IN (1, 2, 4, 5, 6)
                AND F.CANCELLED = 0
              GROUP BY C.CODE, C.DEFINITION_
              ORDER BY SUM(F.NETTOTAL) DESC`
    },

    // ============ 4. STOK YÖNETİMİ ============
    'view-stock-list': {
        title: 'Stok & Malzeme Listesi (Ambar Bazlı)',
        sql: `SELECT
                I.CODE        AS [Stok Kodu],
                I.NAME        AS [Stok Adı],
                I.STGRPCODE   AS [Grup Kodu],
                W.NR          AS [Ambar No],
                W.NAME        AS [Ambar Adı],
                ISNULL(ST.ONHAND, 0) AS [Eldeki Miktar]
              FROM LG_001_ITEMS I WITH(NOLOCK)
              JOIN LG_001_01_STINVTOT ST WITH(NOLOCK) ON ST.STOCKREF = I.LOGICALREF
              JOIN LG_001_WAREHOUSE W WITH(NOLOCK) ON W.NR = ST.INVENNO
              WHERE I.ACTIVE = 0
                AND I.CARDTYPE = 1
                AND ISNULL(ST.ONHAND, 0) <> 0
              ORDER BY I.CODE, W.NR`
    },
    'view-fifo-cost': {
        title: 'Malzeme Maliyeti ve Stok Değer Raporu',
        sql: `SELECT
                I.CODE        AS [Stok Kodu],
                I.NAME        AS [Stok Adı],
                I.STGRPCODE   AS [Grup],
                SUM(L.AMOUNT) AS [Toplam Miktar],
                CASE WHEN SUM(L.AMOUNT) > 0
                  THEN SUM(L.TOTAL) / SUM(L.AMOUNT)
                  ELSE 0 END  AS [Ort. Birim Fiyat (₺)],
                SUM(L.TOTAL)  AS [Toplam Alış Değeri (₺)]
              FROM LG_001_01_STLINE L WITH(NOLOCK)
              JOIN LG_001_ITEMS I WITH(NOLOCK) ON L.STOCKREF = I.LOGICALREF
              WHERE L.TRCODE IN (1, 6, 13)
                AND L.CANCELLED = 0
                AND L.LINETYPE = 0
              GROUP BY I.CODE, I.NAME, I.STGRPCODE
              HAVING SUM(L.AMOUNT) > 0
              ORDER BY SUM(L.TOTAL) DESC`
    },

    // ============ 5. FİNANS & HAZİNE ============
    'view-avg-maturity': {
        title: 'Cari Ortalama Vade Raporu',
        sql: `SELECT
                C.CODE                      AS [Cari Kodu],
                C.DEFINITION_               AS [Cari Unvanı],
                COUNT(T.LOGICALREF)         AS [Açık İşlem Sayısı],
                SUM(T.TOTAL - T.PAID)       AS [Toplam Açık Bakiye (₺)],
                AVG(DATEDIFF(day, GETDATE(), T.PROCDATE)) AS [Ort. Vade (Gün)]
              FROM LG_001_01_PAYTRANS T WITH(NOLOCK)
              JOIN LG_001_CLCARD C WITH(NOLOCK) ON T.CARDREF = C.LOGICALREF
              WHERE T.CANCELLED = 0
                AND (T.TOTAL - T.PAID) > 0
              GROUP BY C.CODE, C.DEFINITION_
              ORDER BY SUM(T.TOTAL - T.PAID) DESC`
    },
    'view-vade-takip': {
        title: 'Master Vade Takip Raporu',
        sql: `SELECT 'MÜŞTERİ ÇEK / SENET' AS [Tahsilat Tipi],
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 0 AND 30  THEN AMOUNT ELSE 0 END) AS [0-30 Gün (₺)],
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 31 AND 60 THEN AMOUNT ELSE 0 END) AS [31-60 Gün (₺)],
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 61 AND 90 THEN AMOUNT ELSE 0 END) AS [61-90 Gün (₺)],
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) > 90              THEN AMOUNT ELSE 0 END) AS [90+ Gün (₺)],
                SUM(AMOUNT) AS [Genel Toplam (₺)]
              FROM LG_001_01_CSCARD WITH(NOLOCK) WHERE DOC IN (1,3) AND CURRSTAT = 1
              UNION ALL
              SELECT 'KREDİ KARTI / POS BLOKAJI',
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 0 AND 30  THEN (TOTAL-PAID) ELSE 0 END),
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 31 AND 60 THEN (TOTAL-PAID) ELSE 0 END),
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 61 AND 90 THEN (TOTAL-PAID) ELSE 0 END),
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) > 90              THEN (TOTAL-PAID) ELSE 0 END),
                SUM(TOTAL-PAID)
              FROM LG_001_01_PAYTRANS WITH(NOLOCK) WHERE MODULENR = 7 AND SIGN = 0 AND CANCELLED = 0 AND (TOTAL-PAID) > 0
              UNION ALL
              SELECT 'AÇIK HESAP FATURA',
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 0 AND 30  THEN (TOTAL-PAID) ELSE 0 END),
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 31 AND 60 THEN (TOTAL-PAID) ELSE 0 END),
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 61 AND 90 THEN (TOTAL-PAID) ELSE 0 END),
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) > 90              THEN (TOTAL-PAID) ELSE 0 END),
                SUM(TOTAL-PAID)
              FROM LG_001_01_PAYTRANS WITH(NOLOCK) WHERE MODULENR IN (4,5) AND SIGN = 0 AND CANCELLED = 0 AND (TOTAL-PAID) > 0`
    },
    'view-gider-takip': {
        title: 'Aylık Gider & Fatura Takibi',
        sql: `SELECT
                YEAR(F.DATE_)  AS [Yıl],
                MONTH(F.DATE_) AS [Ay],
                C.DEFINITION_  AS [Tedarikçi / Masraf Yeri],
                F.FICHENO      AS [Fiş No],
                CONVERT(varchar, F.DATE_, 104) AS [Tarih],
                F.NETTOTAL     AS [Net Tutar (₺)]
              FROM LG_001_01_INVOICE F WITH(NOLOCK)
              LEFT JOIN LG_001_CLCARD C WITH(NOLOCK) ON F.CLIENTREF = C.LOGICALREF
              WHERE F.TRCODE IN (1, 2, 3, 4)
                AND F.CANCELLED = 0
              ORDER BY F.DATE_ DESC`
    },

    // ============ 6. BANKALAR ============
    'view-banks': {
        title: 'Bankalar Listesi',
        sql: `SELECT
                CODE        AS [Banka Kodu],
                DEFINITION_ AS [Banka Adı],
                ADDR1       AS [Adres],
                TELNRS1     AS [Telefon]
              FROM LG_001_BNCARD WITH(NOLOCK)
              WHERE ACTIVE = 0
              ORDER BY DEFINITION_`
    },
    'view-bank-accounts': {
        title: 'Banka Hesapları',
        sql: `SELECT
                A.CODE        AS [Hesap Kodu],
                A.DEFINITION_ AS [Hesap Adı],
                B.DEFINITION_ AS [Banka Adı],
                A.BRANCHNAME  AS [Şube],
                A.IBAN        AS [IBAN],
                A.ACCNR       AS [Hesap No]
              FROM LG_001_BANKACC A WITH(NOLOCK)
              JOIN LG_001_BNCARD B WITH(NOLOCK) ON A.BANKREF = B.LOGICALREF
              WHERE A.ACTIVE = 0
              ORDER BY B.DEFINITION_, A.DEFINITION_`
    },
    'view-bank-transactions': {
        title: 'Banka Hesap Hareketleri',
        sql: `SELECT TOP 500
                CONVERT(varchar, L.DATE_, 104) AS [Tarih],
                L.TRANNO      AS [İşlem No],
                B.DEFINITION_ AS [Banka],
                A.DEFINITION_ AS [Hesap],
                L.LINEEXP     AS [Açıklama],
                CASE WHEN L.SIGN = 0 THEN L.AMOUNT ELSE 0 END AS [Giriş (₺)],
                CASE WHEN L.SIGN = 1 THEN L.AMOUNT ELSE 0 END AS [Çıkış (₺)]
              FROM LG_001_01_BNFLINE L WITH(NOLOCK)
              JOIN LG_001_BANKACC A WITH(NOLOCK) ON L.ACCOUNTREF = A.LOGICALREF
              JOIN LG_001_BNCARD  B WITH(NOLOCK) ON A.BANKREF = B.LOGICALREF
              WHERE L.CANCELLED = 0
              ORDER BY L.DATE_ DESC, L.LOGICALREF DESC`
    },

    // ============ 7. ÇEK & SENET ============
    'view-customer-checks': {
        title: 'Müşteri Çekleri (Portföydeki)',
        sql: `SELECT
                PORTFOLIONO AS [Portföy No],
                OWING       AS [Borçlu Adı],
                CONVERT(varchar, DUEDATE, 104) AS [Vade Tarihi],
                BANKNAME    AS [Banka],
                AMOUNT      AS [Tutar (₺)],
                CASE CURRSTAT WHEN 1 THEN 'Portföyde' WHEN 2 THEN 'Tahsilde' WHEN 3 THEN 'Tahsil Edildi' WHEN 5 THEN 'Karşılıksız' ELSE 'Diğer' END AS [Durum]
              FROM LG_001_01_CSCARD WITH(NOLOCK)
              WHERE DOC IN (1, 3)
              ORDER BY DUEDATE ASC`
    },
    'view-our-checks': {
        title: 'Kendi Çeklerimiz',
        sql: `SELECT
                PORTFOLIONO AS [Portföy No],
                CONVERT(varchar, DUEDATE, 104) AS [Vade Tarihi],
                BANKNAME    AS [Banka],
                BRANCHNAME  AS [Şube],
                AMOUNT      AS [Tutar (₺)],
                CASE CURRSTAT WHEN 1 THEN 'Verildi' WHEN 2 THEN 'Ödendi' WHEN 3 THEN 'İptal' ELSE 'Diğer' END AS [Durum]
              FROM LG_001_01_CSCARD WITH(NOLOCK)
              WHERE DOC = 2
              ORDER BY DUEDATE ASC`
    },
    'view-check-maturities': {
        title: 'Çek Vadeleri Raporu (Vade Yaşlandırma)',
        sql: `SELECT
                CASE DOC WHEN 1 THEN 'Müşteri Çeki' WHEN 3 THEN 'Müşteri Senedi' ELSE 'Diğer' END AS [Tür],
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 0 AND 30  THEN AMOUNT ELSE 0 END) AS [0-30 Gün (₺)],
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 31 AND 60 THEN AMOUNT ELSE 0 END) AS [31-60 Gün (₺)],
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 61 AND 90 THEN AMOUNT ELSE 0 END) AS [61-90 Gün (₺)],
                SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) > 90              THEN AMOUNT ELSE 0 END) AS [90+ Gün (₺)],
                SUM(AMOUNT) AS [Genel Toplam (₺)]
              FROM LG_001_01_CSCARD WITH(NOLOCK)
              WHERE DOC IN (1, 3)
                AND CURRSTAT = 1
              GROUP BY DOC
              ORDER BY DOC`
    },

    // ============ 8. CRM & HEDEF ============
    'view-budget-management': {
        title: 'Bütçe Yönetimi - Gelecek ve Yapılması Gereken Ödemeler',
        sql: `SELECT
                'ÇEK/SENET ÖDEME' AS [Ödeme Tipi],
                CONVERT(varchar, DUEDATE, 104) AS [Vade Tarihi],
                AMOUNT            AS [Tutar (₺)],
                BANKNAME          AS [Banka],
                CASE CURRSTAT WHEN 1 THEN 'Bekliyor' WHEN 2 THEN 'Ödendi' ELSE 'Diğer' END AS [Durum]
              FROM LG_001_01_CSCARD WITH(NOLOCK)
              WHERE DOC = 2
                AND CURRSTAT = 1
                AND DUEDATE >= GETDATE()
              UNION ALL
              SELECT
                'FATURA BORCU'    AS [Ödeme Tipi],
                CONVERT(varchar, PROCDATE, 104) AS [Vade Tarihi],
                (TOTAL - PAID)    AS [Tutar (₺)],
                ''                AS [Banka],
                'Bekliyor'        AS [Durum]
              FROM LG_001_01_PAYTRANS WITH(NOLOCK)
              WHERE SIGN = 1
                AND CANCELLED = 0
                AND (TOTAL - PAID) > 0
                AND PROCDATE >= GETDATE()
              ORDER BY [Vade Tarihi] ASC`
    }
};

window.ERP_QUERIES = ERP_QUERIES;

// ==========================================================
// DYNAMIC SQL EXECUTION & PREMIUM DATATABLE RENDERER
// ==========================================================
async function executeAndRenderQuery(viewId) {
    const queryObj = window.ERP_QUERIES ? window.ERP_QUERIES[viewId] : null;
    if (!queryObj) return;

    const container = document.getElementById(viewId);
    if (!container) return;

    container.style.display = 'block';

    container.innerHTML = `
        <div class="panel-card" style="padding:4rem 2rem; text-align:center;">
            <i class="fa-solid fa-circle-notch fa-spin" style="font-size:2.8rem; color:var(--primary); margin-bottom:1.2rem;"></i>
            <h3 style="color:var(--secondary); font-size:1.15rem; font-weight:700; margin-bottom:0.4rem;">${queryObj.title}</h3>
            <p style="color:var(--text-muted); font-size:0.85rem;">Logo ERP veri tabanından canlı veriler çekiliyor, lütfen bekleyin...</p>
        </div>`;

    try {
        const firm = window.currentFirm || '001';
        const period = window.currentPeriod || '01';

        // Adjust SQL for current firm and period
        let sql = queryObj.sql.replace(/LG_001_01_/g, `LG_${firm}_${period}_`).replace(/LG_001_/g, `LG_${firm}_`);

        const response = await fetch((window.API_BASE || '/api') + '/reports/test-sql', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ sqlQuery: sql, firmNo: firm, periodNo: period })
        });

        if (!response.ok) throw new Error('Sunucu Yanıtı: ' + response.status + ' ' + response.statusText);
        const data = await response.json();
        if (!data.isSuccess) throw new Error(data.message || 'Sorgu yürütülemedi.');

        // Extract rows & columns
        let columns = data.columns || [];
        let rows = data.rows || [];

        if ((!rows || rows.length === 0) && data.resultJson) {
            try {
                const parsed = JSON.parse(data.resultJson);
                if (Array.isArray(parsed) && parsed.length > 0) {
                    columns = Object.keys(parsed[0]);
                    rows = parsed.map(obj => Object.values(obj));
                }
            } catch (e) {}
        }

        if (!rows || rows.length === 0) {
            container.innerHTML = `
                <div class="panel-card" style="padding:3.5rem 2rem; text-align:center;">
                    <div style="width:60px; height:60px; border-radius:50%; background:#f1f5f9; color:var(--text-muted); display:inline-flex; align-items:center; justify-content:center; font-size:1.6rem; margin-bottom:1rem;">
                        <i class="fa-solid fa-inbox"></i>
                    </div>
                    <h3 style="color:var(--secondary); font-weight:700; font-size:1.1rem; margin-bottom:0.5rem;">${queryObj.title}</h3>
                    <p style="color:var(--text-muted); font-size:0.85rem;">Bu rapor kriterine ait Logo ERP üzerinde kayıtlı veri bulunamadı.</p>
                </div>`;
            return;
        }

        const fmt = (val) => {
            if (val === null || val === undefined) return '-';
            const s = String(val).trim();
            if (s === '') return '-';
            const n = parseFloat(s);
            if (!isNaN(n) && s.match(/^-?\d+(\.\d+)?$/)) {
                return n.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            }
            return s;
        };

        const tableId = 'table_' + viewId.replace(/-/g, '_');

        let html = `
        <div class="panel-card">
            <div class="panel-header" style="flex-wrap:wrap; gap:10px;">
                <div style="display:flex; align-items:center; gap:10px;">
                    <i class="fa-solid fa-table-list header-icon"></i>
                    <div>
                        <h2>${queryObj.title}</h2>
                        <span style="font-size:0.75rem; color:var(--text-muted); font-weight:500;">Toplam ${rows.length} kayıt listelendi</span>
                    </div>
                </div>
                <div style="display:flex; align-items:center; gap:8px;">
                    <div style="position:relative;">
                        <input type="text" id="search_${tableId}" placeholder="Tabloda ara..." 
                               onkeyup="filterReportTable('${tableId}')" 
                               style="padding:6px 12px 6px 30px; border-radius:8px; border:1px solid var(--border-color); font-size:0.8rem; width:200px; outline:none;">
                        <i class="fa-solid fa-magnifying-glass" style="position:absolute; left:10px; top:8px; color:var(--text-muted); font-size:0.75rem;"></i>
                    </div>
                    <button class="premium-btn" style="background:var(--success); padding:6px 12px; font-size:0.78rem;" onclick="exportReportExcel('${tableId}', '${queryObj.title}')">
                        <i class="fa-solid fa-file-excel"></i> Excel
                    </button>
                    <button class="mini-btn" onclick="executeAndRenderQuery('${viewId}')">
                        <i class="fa-solid fa-rotate"></i>
                    </button>
                </div>
            </div>
            <div class="panel-body" style="padding:0;">
                <div class="table-responsive">
                    <table class="data-table" id="${tableId}">
                        <thead>
                            <tr>`;

        columns.forEach((col, idx) => {
            const isNumeric = rows.some(r => r[idx] !== null && !isNaN(parseFloat(r[idx])) && String(r[idx]).match(/^-?\d+(\.\d+)?$/));
            const align = isNumeric ? 'text-align:right;' : 'text-align:left;';
            html += `<th style="${align}">${col}</th>`;
        });
        html += `</tr></thead><tbody>`;

        rows.forEach(row => {
            html += `<tr>`;
            row.forEach((cell, idx) => {
                const isNumeric = cell !== null && !isNaN(parseFloat(cell)) && String(cell).match(/^-?\d+(\.\d+)?$/);
                const align = isNumeric ? 'text-align:right; font-family:\'JetBrains Mono\', monospace; font-weight:600;' : 'text-align:left;';
                html += `<td style="${align}">${fmt(cell)}</td>`;
            });
            html += `</tr>`;
        });
        html += `</tbody></table></div></div></div>`;

        container.innerHTML = html;

    } catch (err) {
        container.innerHTML = `
            <div class="panel-card" style="padding:3.5rem 2rem; text-align:center;">
                <div style="width:60px; height:60px; border-radius:50%; background:#fee2e2; color:var(--danger); display:inline-flex; align-items:center; justify-content:center; font-size:1.6rem; margin-bottom:1rem;">
                    <i class="fa-solid fa-triangle-exclamation"></i>
                </div>
                <h3 style="color:var(--danger); font-weight:800; font-size:1.1rem; margin-bottom:0.5rem;">Rapor Verisi Alınamadı</h3>
                <p style="color:var(--text-muted); font-size:0.85rem; max-width:500px; margin:0 auto;">${err.message}</p>
                <button class="premium-btn" style="margin:1.2rem auto 0 auto;" onclick="executeAndRenderQuery('${viewId}')">
                    <i class="fa-solid fa-rotate"></i> Yeniden Dene
                </button>
            </div>`;
    }
}

// Table Filter Function
function filterReportTable(tableId) {
    const input = document.getElementById('search_' + tableId);
    if (!input) return;
    const filter = input.value.toLowerCase();
    const table = document.getElementById(tableId);
    if (!table) return;
    const trs = table.getElementsByTagName('tbody')[0].getElementsByTagName('tr');
    for (let i = 0; i < trs.length; i++) {
        const text = trs[i].textContent.toLowerCase();
        trs[i].style.display = text.indexOf(filter) > -1 ? '' : 'none';
    }
}

// Excel Export Function
function exportReportExcel(tableId, title) {
    const table = document.getElementById(tableId);
    if (!table) return;
    if (typeof XLSX !== 'undefined') {
        const wb = XLSX.utils.table_to_book(table, { sheet: 'Rapor' });
        XLSX.writeFile(wb, (title || 'Logo_ERP_Rapor').replace(/[^a-zA-Z0-9_-]/g, '_') + '.xlsx');
    } else {
        alert('Excel kütüphanesi yüklenemedi.');
    }
}
