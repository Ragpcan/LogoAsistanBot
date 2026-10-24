// ==========================================================
// LOGO ERP PORTAL - CORE CONTROLLER & DASHBOARD ENGINE
// ==========================================================
window.API_BASE = '/api';

let currentFirm = '919';
let currentPeriod = '01';
window.currentFirm = currentFirm;
window.currentPeriod = currentPeriod;

// Formatters
const fmtTL = (val) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val || 0);
const fmtUSD = (val) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(val || 0);
const fmtEUR = (val) => new Intl.NumberFormat('de-DE', { style: 'currency', currency: 'EUR' }).format(val || 0);
const fmtGBP = (val) => new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP' }).format(val || 0);
const fmtNum = (val) => new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(val || 0);

// ==========================================================
// VIEW SWITCHER
// ==========================================================
function switchErpView(viewId, element) {
    // 1. Hide all views
    document.querySelectorAll('.erp-view-container').forEach(c => {
        c.classList.remove('active');
        c.style.display = 'none';
    });

    // 2. Remove active class from all menu links
    document.querySelectorAll('.erp-menu-link').forEach(l => l.classList.remove('active'));

    // 3. Highlight current link
    if (element) {
        element.classList.add('active');
    }

    // 4. Update Header Title
    const titleEl = document.getElementById('viewTitle');
    const iconEl = document.getElementById('viewHeaderIcon');
    
    // 5. Show target container
    const container = document.getElementById(viewId);
    if (container) {
        container.style.display = 'block';
        container.classList.add('active');
    }

    if (viewId === 'view-dashboard') {
        if (titleEl) titleEl.innerText = 'DASHBOARD (ÖZET)';
        if (iconEl) iconEl.className = 'fa-solid fa-gauge-high';
        loadDashboardData();
    } else if (window.ERP_QUERIES && window.ERP_QUERIES[viewId]) {
        const q = window.ERP_QUERIES[viewId];
        if (titleEl) titleEl.innerText = q.title || 'Rapor';
        if (iconEl) iconEl.className = 'fa-solid fa-table-list';
        executeAndRenderQuery(viewId);
    } else if (viewId === 'view-live-console') {
        if (titleEl) titleEl.innerText = 'CANLI BOT KONSOLU';
        if (iconEl) iconEl.className = 'fa-solid fa-terminal';
    } else if (viewId === 'view-sms') {
        if (titleEl) titleEl.innerText = 'İŞLEMLER (SMS / E-POSTA / PDF)';
        if (iconEl) iconEl.className = 'fa-solid fa-envelope';
    }
}

// ==========================================================
// SQL QUERY HELPER
// ==========================================================
async function querySql(sql) {
    try {
        const res = await fetch(window.API_BASE + '/reports/test-sql', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ sqlQuery: sql, firmNo: currentFirm, periodNo: currentPeriod })
        });
        if (!res.ok) return { isSuccess: false, message: 'HTTP ' + res.status };
        const data = await res.json();

        // Fallback: parse resultJson into rows/columns if needed
        if (data.isSuccess && (!data.rows || data.rows.length === 0) && data.resultJson) {
            try {
                const parsed = JSON.parse(data.resultJson);
                if (Array.isArray(parsed) && parsed.length > 0) {
                    data.columns = Object.keys(parsed[0]);
                    data.rows = parsed.map(obj => Object.values(obj));
                }
            } catch (e) {}
        }

        return data;
    } catch (e) {
        return { isSuccess: false, message: e.message };
    }
}

// ==========================================================
// INIT SETTINGS & DASHBOARD DATA LOADER
// ==========================================================
async function initPortalSettings() {
    try {
        const res = await fetch(window.API_BASE + '/settings');
        if (res.ok) {
            const settings = await res.json();
            if (settings) {
                if (settings.firmNo) currentFirm = String(settings.firmNo).padStart(3, '0');
                if (settings.periodNo) currentPeriod = String(settings.periodNo).padStart(2, '0');
                window.currentFirm = currentFirm;
                window.currentPeriod = currentPeriod;

                const badge = document.getElementById('portalFirmInfo');
                if (badge) badge.innerText = `Firma: ${currentFirm} | Dönem: ${currentPeriod}`;
            }
        }
    } catch (e) {
        console.warn('Ayarlar alınamadı:', e);
    }
}

async function loadDashboardData() {
    try {
        await initPortalSettings();

        const F = currentFirm;
        const P = currentPeriod;

        // Date range from inputs
        const basInput = document.getElementById('dashBasTarih');
        const bitInput = document.getElementById('dashBitTarih');
        const basTarih = basInput ? basInput.value : '';
        const bitTarih = bitInput ? bitInput.value : '';

        // 1. DÖVİZ KPI KARTLARI (TL, USD, EUR, GBP)
        // Kasa/Banka Giriş & Çıkış
        let qKpiKasaBanka = `
            SELECT 
                ISNULL(F.TRCURR, 0) AS CurrType,
                ISNULL(SUM(CASE WHEN F.SIGN = 0 THEN F.AMOUNT ELSE 0 END), 0) AS GirisTL,
                ISNULL(SUM(CASE WHEN F.SIGN = 1 THEN F.AMOUNT ELSE 0 END), 0) AS CikisTL,
                ISNULL(SUM(CASE WHEN F.SIGN = 0 THEN F.TRNET ELSE 0 END), 0) AS GirisDoviz,
                ISNULL(SUM(CASE WHEN F.SIGN = 1 THEN F.TRNET ELSE 0 END), 0) AS CikisDoviz
            FROM LG_${F}_${P}_BNFLINE F WITH(NOLOCK)
            WHERE F.CANCELLED = 0
            GROUP BY F.TRCURR
        `;

        // Fatura Giriş (Satış) & Çıkış (Alış)
        let qKpiFaturalar = `
            SELECT 
                ISNULL(TRCURR, 0) AS CurrType,
                ISNULL(SUM(CASE WHEN TRCODE IN (7, 8, 9) THEN NETTOTAL ELSE 0 END), 0) AS SatisTL,
                ISNULL(SUM(CASE WHEN TRCODE IN (1, 2, 4, 5, 6) THEN NETTOTAL ELSE 0 END), 0) AS AlisTL,
                ISNULL(SUM(CASE WHEN TRCODE IN (7, 8, 9) THEN REPORTNET ELSE 0 END), 0) AS SatisDoviz,
                ISNULL(SUM(CASE WHEN TRCODE IN (1, 2, 4, 5, 6) THEN REPORTNET ELSE 0 END), 0) AS AlisDoviz
            FROM LG_${F}_${P}_INVOICE WITH(NOLOCK)
            WHERE CANCELLED = 0
            GROUP BY TRCURR
        `;

        // Çek/Senet Giriş (Müşteri Çeki) & Çıkış (Kendi Çekimiz)
        let qKpiCekler = `
            SELECT 
                ISNULL(TRCURR, 0) AS CurrType,
                ISNULL(SUM(CASE WHEN DOC IN (1, 3) AND CURRSTAT = 1 THEN AMOUNT ELSE 0 END), 0) AS MusteriCekTL,
                ISNULL(SUM(CASE WHEN DOC IN (2, 4) AND CURRSTAT = 1 THEN AMOUNT ELSE 0 END), 0) AS KendiCekTL,
                ISNULL(SUM(CASE WHEN DOC IN (1, 3) AND CURRSTAT = 1 THEN TRNET ELSE 0 END), 0) AS MusteriCekDoviz,
                ISNULL(SUM(CASE WHEN DOC IN (2, 4) AND CURRSTAT = 1 THEN TRNET ELSE 0 END), 0) AS KendiCekDoviz
            FROM LG_${F}_${P}_CSCARD WITH(NOLOCK)
            GROUP BY TRCURR
        `;

        const [kpiKBRes, kpiFatRes, kpiCekRes] = await Promise.all([
            querySql(qKpiKasaBanka),
            querySql(qKpiFaturalar),
            querySql(qKpiCekler)
        ]);

        // Helper to aggregate values
        const getKpiVals = (res, currCodes) => {
            let giris = 0, cikis = 0;
            if (res.isSuccess && res.rows && res.rows.length > 0) {
                res.rows.forEach(r => {
                    const c = parseInt(r[0]) || 0;
                    if (currCodes.includes(c)) {
                        const isTL = currCodes.includes(0);
                        giris += parseFloat(isTL ? r[1] : (r[3] || r[1])) || 0;
                        cikis += parseFloat(isTL ? r[2] : (r[4] || r[2])) || 0;
                    }
                });
            }
            return { giris, cikis };
        };

        const renderKpiBody = (bodyId, fmtFn, kbVals, fatVals, cekVals) => {
            const el = document.getElementById(bodyId);
            if (!el) return;
            el.innerHTML = `
                <tr><td>Kasa / Banka</td><td style="color:var(--success);">${fmtFn(kbVals.giris)}</td><td style="color:var(--danger);">${fmtFn(kbVals.cikis)}</td></tr>
                <tr><td>Ticari Fatura</td><td style="color:var(--success);">${fmtFn(fatVals.giris)}</td><td style="color:var(--danger);">${fmtFn(fatVals.cikis)}</td></tr>
                <tr><td>Çek / Senet</td><td style="color:var(--success);">${fmtFn(cekVals.giris)}</td><td style="color:var(--danger);">${fmtFn(cekVals.cikis)}</td></tr>
            `;
        };

        // Render 4 Currency Cards
        renderKpiBody('kpi-tl-body', fmtTL, getKpiVals(kpiKBRes, [0]), getKpiVals(kpiFatRes, [0]), getKpiVals(kpiCekRes, [0]));
        renderKpiBody('kpi-usd-body', fmtUSD, getKpiVals(kpiKBRes, [1]), getKpiVals(kpiFatRes, [1]), getKpiVals(kpiCekRes, [1]));
        renderKpiBody('kpi-eur-body', fmtEUR, getKpiVals(kpiKBRes, [2, 20]), getKpiVals(kpiFatRes, [2, 20]), getKpiVals(kpiCekRes, [2, 20]));
        renderKpiBody('kpi-gbp-body', fmtGBP, getKpiVals(kpiKBRes, [3, 17]), getKpiVals(kpiFatRes, [3, 17]), getKpiVals(kpiCekRes, [3, 17]));

        // 2. NET BAKİYE BARI
        let qKasa = `SELECT ISNULL(SUM(CASE WHEN L.SIGN=0 THEN L.AMOUNT ELSE -L.AMOUNT END),0) FROM LG_${F}_KSCARD K WITH(NOLOCK) JOIN LG_${F}_${P}_KSLINES L WITH(NOLOCK) ON K.LOGICALREF=L.CARDREF WHERE ISNULL(L.CANCELLED,0)=0`;
        let qBanka = `SELECT ISNULL(SUM(CASE WHEN F.SIGN=0 THEN F.AMOUNT ELSE -F.AMOUNT END),0) FROM LG_${F}_BANKACC B WITH(NOLOCK) JOIN LG_${F}_${P}_BNFLINE F WITH(NOLOCK) ON B.LOGICALREF=F.BNACCREF WHERE F.CANCELLED=0`;
        let qCek = `SELECT ISNULL(SUM(AMOUNT),0) FROM LG_${F}_${P}_CSCARD WITH(NOLOCK) WHERE DOC IN (1,3) AND CURRSTAT=1`;
        let qKendiCek = `SELECT ISNULL(SUM(AMOUNT),0) FROM LG_${F}_${P}_CSCARD WITH(NOLOCK) WHERE DOC IN (2,4) AND CURRSTAT=1`;

        const [kasaRes, bankaRes, cekRes, kendiCekRes] = await Promise.all([
            querySql(qKasa),
            querySql(qBanka),
            querySql(qCek),
            querySql(qKendiCek)
        ]);

        const kasaVal = (kasaRes.isSuccess && kasaRes.rows && kasaRes.rows[0]) ? parseFloat(kasaRes.rows[0][0]) || 0 : 0;
        const bankaVal = (bankaRes.isSuccess && bankaRes.rows && bankaRes.rows[0]) ? parseFloat(bankaRes.rows[0][0]) || 0 : 0;
        const cekVal = (cekRes.isSuccess && cekRes.rows && cekRes.rows[0]) ? parseFloat(cekRes.rows[0][0]) || 0 : 0;
        const kendiCekVal = (kendiCekRes.isSuccess && kendiCekRes.rows && kendiCekRes.rows[0]) ? parseFloat(kendiCekRes.rows[0][0]) || 0 : 0;
        const netVal = kasaVal + bankaVal;

        // Update Net Bakiye Bar
        const elKasa = document.getElementById('nb-kasa');
        const elBanka = document.getElementById('nb-banka');
        const elNet = document.getElementById('nb-net');
        const elCek = document.getElementById('nb-cek');
        const elKCek = document.getElementById('nb-kendi-cek');

        if (elKasa) elKasa.innerText = fmtTL(kasaVal);
        if (elBanka) elBanka.innerText = fmtTL(bankaVal);
        if (elNet) {
            elNet.innerText = fmtTL(netVal);
            elNet.style.color = netVal >= 0 ? 'var(--success)' : 'var(--danger)';
        }
        if (elCek) elCek.innerText = fmtTL(cekVal);
        if (elKCek) elKCek.innerText = fmtTL(kendiCekVal);

        // 3. BANKALAR TABLOSU
        let qBankList = `
            SELECT TOP 10
                BC.DEFINITION_ AS Banka,
                ISNULL(SUM(CASE WHEN ISNULL(F.TRCURR,0)=0 THEN (CASE WHEN F.SIGN=0 THEN F.AMOUNT ELSE -F.AMOUNT END) ELSE 0 END),0) AS TL,
                ISNULL(SUM(CASE WHEN F.TRCURR=1 THEN (CASE WHEN F.SIGN=0 THEN F.TRNET ELSE -F.TRNET END) ELSE 0 END),0) AS USD,
                ISNULL(SUM(CASE WHEN F.TRCURR IN (2,20) THEN (CASE WHEN F.SIGN=0 THEN F.TRNET ELSE -F.TRNET END) ELSE 0 END),0) AS EUR
            FROM LG_${F}_BANKACC B WITH(NOLOCK)
            INNER JOIN LG_${F}_BNCARD BC WITH(NOLOCK) ON B.BANKREF = BC.LOGICALREF
            LEFT JOIN LG_${F}_${P}_BNFLINE F WITH(NOLOCK) ON B.LOGICALREF = F.BNACCREF AND F.CANCELLED = 0
            GROUP BY BC.CODE, BC.DEFINITION_
            ORDER BY TL DESC
        `;
        const bankListRes = await querySql(qBankList);
        const bTable = document.getElementById('dashBankaTable');
        const bCount = document.getElementById('bankaCountBadge');
        if (bTable) {
            if (bankListRes.isSuccess && bankListRes.rows && bankListRes.rows.length > 0) {
                if (bCount) bCount.innerText = `${bankListRes.rows.length} Banka`;
                let html = '';
                bankListRes.rows.forEach(r => {
                    html += `<tr>
                        <td><strong>${r[0]}</strong></td>
                        <td style="text-align:right;font-weight:700;color:var(--secondary);">${fmtTL(r[1])}</td>
                        <td style="text-align:right;color:var(--success);">${fmtUSD(r[2])}</td>
                        <td style="text-align:right;color:var(--info);">${fmtEUR(r[3])}</td>
                    </tr>`;
                });
                bTable.innerHTML = html;
            } else {
                bTable.innerHTML = `<tr><td colspan="4" style="text-align:center;color:var(--text-muted);padding:14px;">Banka kaydı bulunamadı.</td></tr>`;
            }
        }

        // 4. TOP 5 ALACAKLI CARİLER
        let qTopReceivables = `
            SELECT TOP 5 
                C.CODE + ' - ' + C.DEFINITION_ AS Cari,
                ISNULL(SUM(T.TOTAL - T.PAID),0) AS Bakiye
            FROM LG_${F}_${P}_PAYTRANS T WITH(NOLOCK)
            INNER JOIN LG_${F}_CLCARD C WITH(NOLOCK) ON T.CARDREF = C.LOGICALREF
            WHERE T.SIGN = 0 AND T.CANCELLED = 0 AND (T.TOTAL - T.PAID) > 0
            GROUP BY C.CODE, C.DEFINITION_
            ORDER BY Bakiye DESC
        `;
        const recRes = await querySql(qTopReceivables);
        const recTable = document.getElementById('dashReceivablesTable');
        if (recTable) {
            if (recRes.isSuccess && recRes.rows && recRes.rows.length > 0) {
                let html = '';
                recRes.rows.forEach(r => {
                    html += `<tr>
                        <td title="${r[0]}">${r[0]}</td>
                        <td style="text-align:right;font-weight:700;color:var(--success);">${fmtTL(r[1])}</td>
                    </tr>`;
                });
                recTable.innerHTML = html;
            } else {
                recTable.innerHTML = `<tr><td colspan="2" style="text-align:center;color:var(--text-muted);padding:14px;">Açık alacak kaydı bulunamadı.</td></tr>`;
            }
        }

        // 5. YAŞLANDIRMA PROJEKSİYONU
        let qAging = `
            SELECT 
                'Açık Fatura Alacakları' AS Kanal,
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 0 AND 30 THEN (TOTAL-PAID) ELSE 0 END),0) AS G30,
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 31 AND 60 THEN (TOTAL-PAID) ELSE 0 END),0) AS G60,
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 61 AND 90 THEN (TOTAL-PAID) ELSE 0 END),0) AS G90,
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 91 AND 120 THEN (TOTAL-PAID) ELSE 0 END),0) AS G120,
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) > 120 THEN (TOTAL-PAID) ELSE 0 END),0) AS G120U,
                ISNULL(SUM(TOTAL-PAID),0) AS Toplam
            FROM LG_${F}_${P}_PAYTRANS WITH(NOLOCK) WHERE MODULENR IN (4,5) AND SIGN=0 AND CANCELLED=0 AND (TOTAL-PAID)>0
            UNION ALL
            SELECT 
                'Müşteri Çek / Senet Vadeleri' AS Kanal,
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 0 AND 30 THEN AMOUNT ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 31 AND 60 THEN AMOUNT ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 61 AND 90 THEN AMOUNT ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 91 AND 120 THEN AMOUNT ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) > 120 THEN AMOUNT ELSE 0 END),0),
                ISNULL(SUM(AMOUNT),0)
            FROM LG_${F}_${P}_CSCARD WITH(NOLOCK) WHERE DOC IN (1,3) AND CURRSTAT=1
            UNION ALL
            SELECT 
                'Borç Fatura Vadeleri' AS Kanal,
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 0 AND 30 THEN (TOTAL-PAID) ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 31 AND 60 THEN (TOTAL-PAID) ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 61 AND 90 THEN (TOTAL-PAID) ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) BETWEEN 91 AND 120 THEN (TOTAL-PAID) ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), PROCDATE) > 120 THEN (TOTAL-PAID) ELSE 0 END),0),
                ISNULL(SUM(TOTAL-PAID),0)
            FROM LG_${F}_${P}_PAYTRANS WITH(NOLOCK) WHERE MODULENR IN (4,5) AND SIGN=1 AND CANCELLED=0 AND (TOTAL-PAID)>0
            UNION ALL
            SELECT 
                'Kendi Çeklerimiz Vadeleri' AS Kanal,
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 0 AND 30 THEN AMOUNT ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 31 AND 60 THEN AMOUNT ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 61 AND 90 THEN AMOUNT ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) BETWEEN 91 AND 120 THEN AMOUNT ELSE 0 END),0),
                ISNULL(SUM(CASE WHEN DATEDIFF(day, GETDATE(), DUEDATE) > 120 THEN AMOUNT ELSE 0 END),0),
                ISNULL(SUM(AMOUNT),0)
            FROM LG_${F}_${P}_CSCARD WITH(NOLOCK) WHERE DOC IN (2,4) AND CURRSTAT=1
        `;
        const agingRes = await querySql(qAging);
        const agingTable = document.getElementById('dashAgingTable');
        if (agingTable) {
            if (agingRes.isSuccess && agingRes.rows && agingRes.rows.length > 0) {
                let html = '';
                agingRes.rows.forEach((r, idx) => {
                    const isIncome = idx < 2;
                    const color = isIncome ? 'color:var(--success);' : 'color:var(--danger);';
                    const icon = isIncome ? 'fa-arrow-down' : 'fa-arrow-up';
                    html += `<tr>
                        <td style="font-weight:700;${color}"><i class="fa-solid ${icon}" style="margin-right:6px;"></i>${r[0]}</td>
                        <td class="ag-num">${fmtTL(r[1])}</td>
                        <td class="ag-num">${fmtTL(r[2])}</td>
                        <td class="ag-num">${fmtTL(r[3])}</td>
                        <td class="ag-num">${fmtTL(r[4])}</td>
                        <td class="ag-num">${fmtTL(r[5])}</td>
                        <td class="ag-total" style="font-weight:800;${color}">${fmtTL(r[6])}</td>
                    </tr>`;
                });
                agingTable.innerHTML = html;
            } else {
                agingTable.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--text-muted);padding:14px;">Yaşlandırma verisi bulunamadı.</td></tr>`;
            }
        }

        // 6. AYLIK SATIŞ & ALIŞ CİROSU
        let qSalesMonthly = `
            SELECT 
                TRCODE,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 1 THEN NETTOTAL ELSE 0 END),0) AS M1,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 2 THEN NETTOTAL ELSE 0 END),0) AS M2,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 3 THEN NETTOTAL ELSE 0 END),0) AS M3,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 4 THEN NETTOTAL ELSE 0 END),0) AS M4,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 5 THEN NETTOTAL ELSE 0 END),0) AS M5,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 6 THEN NETTOTAL ELSE 0 END),0) AS M6,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 7 THEN NETTOTAL ELSE 0 END),0) AS M7,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 8 THEN NETTOTAL ELSE 0 END),0) AS M8,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 9 THEN NETTOTAL ELSE 0 END),0) AS M9,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 10 THEN NETTOTAL ELSE 0 END),0) AS M10,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 11 THEN NETTOTAL ELSE 0 END),0) AS M11,
                ISNULL(SUM(CASE WHEN MONTH(DATE_) = 12 THEN NETTOTAL ELSE 0 END),0) AS M12,
                ISNULL(SUM(NETTOTAL),0) AS Yillik
            FROM LG_${F}_${P}_INVOICE WITH(NOLOCK)
            WHERE CANCELLED = 0 AND TRCODE IN (1, 8)
            GROUP BY TRCODE
        `;
        const salesRes = await querySql(qSalesMonthly);
        const salesTableBody = document.getElementById('dashSalesTableBody');
        if (salesTableBody) {
            if (salesRes.isSuccess && salesRes.rows && salesRes.rows.length > 0) {
                let satisRow = salesRes.rows.find(r => r[0] == 8) || [8,0,0,0,0,0,0,0,0,0,0,0,0,0];
                let alisRow = salesRes.rows.find(r => r[0] == 1) || [1,0,0,0,0,0,0,0,0,0,0,0,0,0];

                let html = '';
                // Satış Satırı
                html += `<tr style="background:#f0fdf4;">
                    <td style="font-weight:700;color:var(--success);"><i class="fa-solid fa-arrow-up" style="margin-right:6px;"></i>SATIŞLAR</td>`;
                for(let m=1; m<=12; m++) {
                    html += `<td style="text-align:right;color:var(--success);font-weight:600;">${fmtNum(satisRow[m])}</td>`;
                }
                html += `<td style="text-align:right;background:#dcfce7;font-weight:800;color:var(--success);">${fmtNum(satisRow[13])}</td></tr>`;

                // Alış Satırı
                html += `<tr style="background:#fff7ed;">
                    <td style="font-weight:700;color:var(--warning);"><i class="fa-solid fa-arrow-down" style="margin-right:6px;"></i>ALIŞLAR</td>`;
                for(let m=1; m<=12; m++) {
                    html += `<td style="text-align:right;color:var(--warning);font-weight:600;">${fmtNum(alisRow[m])}</td>`;
                }
                html += `<td style="text-align:right;background:#ffedd5;font-weight:800;color:var(--warning);">${fmtNum(alisRow[13])}</td></tr>`;

                // Net Satırı
                html += `<tr style="background:#eff6ff;border-top:2px solid #93c5fd;">
                    <td style="font-weight:800;color:var(--primary);"><i class="fa-solid fa-equals" style="margin-right:6px;"></i>NET FARK</td>`;
                for(let m=1; m<=12; m++) {
                    let diff = (parseFloat(satisRow[m]) || 0) - (parseFloat(alisRow[m]) || 0);
                    let color = diff >= 0 ? 'color:var(--primary);' : 'color:var(--danger);';
                    html += `<td style="text-align:right;font-weight:800;${color}">${fmtNum(diff)}</td>`;
                }
                let diffTotal = (parseFloat(satisRow[13]) || 0) - (parseFloat(alisRow[13]) || 0);
                html += `<td style="text-align:right;background:#dbeafe;font-weight:900;color:var(--primary);">${fmtNum(diffTotal)}</td></tr>`;

                salesTableBody.innerHTML = html;
            } else {
                salesTableBody.innerHTML = `<tr><td colspan="14" style="text-align:center;color:var(--text-muted);padding:14px;">Fatura verisi bulunamadı.</td></tr>`;
            }
        }

    } catch (err) {
        console.error('loadDashboardData error:', err);
    }
}
