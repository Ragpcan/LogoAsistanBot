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
// ACCORDION LOGIC FOR SIDEBAR
// ==========================================================
function toggleAccordion(element) {
    if (!element) return;
    const submenu = element.nextElementSibling;
    if (!submenu || !submenu.classList.contains('submenu-container')) return;
    const isOpen = submenu.style.display === 'flex';
    
    // Close all other open submenus
    document.querySelectorAll('.submenu-container').forEach(s => s.style.display = 'none');
    document.querySelectorAll('.parent-nav').forEach(p => p.classList.remove('open'));
    
    if (!isOpen) {
        submenu.style.display = 'flex';
        element.classList.add('open');
    }
}
window.toggleAccordion = toggleAccordion;

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
    } else if (viewId === 'view-invoice-due-dates') {
        if (titleEl) titleEl.innerText = 'FATURA VADE & BORÇ TAKİBİ';
        if (iconEl) iconEl.className = 'fa-solid fa-calendar-check';
        loadInvoiceDueDatesView();
    } else if (viewId === 'view-whatsapp-bridge') {
        if (titleEl) titleEl.innerText = 'TOKENSIZ WHATSAPP WEB ENTEGRASYONU';
        if (iconEl) iconEl.className = 'fa-brands fa-whatsapp';
        loadWhatsAppBridgeView();
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
window.switchErpView = switchErpView;

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

// ==========================================================
// FATURA VADE & BORÇ TAKİBİ ENGINE
// ==========================================================
let _currentDueDatesData = [];
let _currentCariInvoices = [];
let _selectedNotifyChannel = 'whatsapp';
let _currentNotifyInvoice = null;

async function loadInvoiceDueDatesView() {
    const container = document.getElementById('view-invoice-due-dates');
    if (!container) return;

    // 1. Initial Container Shell if not rendered
    if (!document.getElementById('vadeFilterSearch')) {
        container.innerHTML = `
            <!-- KPI CARDS -->
            <div class="kpi-container" style="grid-template-columns:repeat(5, 1fr); margin-bottom:16px;">
                <div class="kpi-card" style="border-left:4px solid var(--primary);">
                    <div class="kpi-currency-bg">₺</div>
                    <h3><span>TOPLAM FATURA</span> <i class="fa-solid fa-receipt" style="color:var(--primary);"></i></h3>
                    <div style="font-size:1.3rem; font-weight:800; color:var(--secondary); margin-top:8px;" id="kpi-vade-total">0,00 ₺</div>
                    <small style="color:var(--text-muted); font-weight:600;" id="kpi-vade-total-count">0 Adet Fatura</small>
                </div>
                <div class="kpi-card" style="border-left:4px solid var(--danger); background:#fff5f5;">
                    <div class="kpi-currency-bg" style="color:var(--danger);">!</div>
                    <h3><span style="color:var(--danger);">VADESİ GEÇEN</span> <i class="fa-solid fa-triangle-exclamation" style="color:var(--danger);"></i></h3>
                    <div style="font-size:1.3rem; font-weight:800; color:var(--danger); margin-top:8px;" id="kpi-vade-overdue">0,00 ₺</div>
                    <small style="color:var(--danger); font-weight:700;" id="kpi-vade-overdue-count">0 Fatura Gecikti</small>
                </div>
                <div class="kpi-card" style="border-left:4px solid var(--warning); background:#fffbeb;">
                    <div class="kpi-currency-bg" style="color:var(--warning);">⌛</div>
                    <h3><span style="color:var(--warning);">BUGÜN DOLANLAR</span> <i class="fa-solid fa-clock" style="color:var(--warning);"></i></h3>
                    <div style="font-size:1.3rem; font-weight:800; color:var(--warning); margin-top:8px;" id="kpi-vade-today">0 Adet</div>
                    <small style="color:var(--text-muted); font-weight:600;">Acil Tahsilat/Ödeme</small>
                </div>
                <div class="kpi-card" style="border-left:4px solid var(--success); background:#f0fdf4;">
                    <div class="kpi-currency-bg" style="color:var(--success);">✓</div>
                    <h3><span style="color:var(--success);">ÖDENENLER</span> <i class="fa-solid fa-circle-check" style="color:var(--success);"></i></h3>
                    <div style="font-size:1.3rem; font-weight:800; color:var(--success); margin-top:8px;" id="kpi-vade-paid">0,00 ₺</div>
                    <small style="color:var(--success); font-weight:700;" id="kpi-vade-paid-count">0 Fatura Kapandı</small>
                </div>
                <div class="kpi-card" style="border-left:4px solid var(--info); background:#eff6ff;">
                    <div class="kpi-currency-bg" style="color:var(--info);">∑</div>
                    <h3><span style="color:var(--info);">AÇIK BAKİYE</span> <i class="fa-solid fa-wallet" style="color:var(--info);"></i></h3>
                    <div style="font-size:1.3rem; font-weight:800; color:var(--info); margin-top:8px;" id="kpi-vade-remaining">0,00 ₺</div>
                    <small style="color:var(--text-muted); font-weight:600;">Kalan Net Alacak/Borç</small>
                </div>
            </div>

            <!-- COMPREHENSIVE FILTER BAR -->
            <div class="vade-filter-grid">
                <div class="vade-filter-item" style="grid-column: span 2;">
                    <label><i class="fa-solid fa-magnifying-glass"></i> Arama (Fatura No, Belge No, Cari)</label>
                    <input type="text" id="vadeFilterSearch" class="vade-input" placeholder="Örn: GIB2026, Demir Çelik, 120.01..." onkeyup="if(event.key==='Enter') executeVadeFilter()">
                </div>
                <div class="vade-filter-item">
                    <label><i class="fa-solid fa-filter"></i> Vade Durumu</label>
                    <select id="vadeFilterMaturity" class="vade-select" onchange="executeVadeFilter()">
                        <option value="all">Tüm Vadeler</option>
                        <option value="overdue">🔴 Vadesi Geçmiş (Overdue)</option>
                        <option value="today">🟡 Bugün Vadesi Gelen</option>
                        <option value="future0_15">🟢 0 - 15 Gün Kalanlar</option>
                        <option value="future16_30">🟢 16 - 30 Gün Kalanlar</option>
                        <option value="future30plus">🔵 30+ Gün Kalanlar</option>
                    </select>
                </div>
                <div class="vade-filter-item">
                    <label><i class="fa-solid fa-money-bill-transfer"></i> Borç Kapama / Ödeme</label>
                    <select id="vadeFilterPayment" class="vade-select" onchange="executeVadeFilter()">
                        <option value="all">Tümü (Açık & Ödenen)</option>
                        <option value="Odenmedi" selected>🔴 Ödenmedi / Açık Faturalar</option>
                        <option value="Odendi">🟢 Elle / Sistemde Ödendi</option>
                        <option value="Kismi">🟡 Kısmi Ödenenler</option>
                    </select>
                </div>
                <div class="vade-filter-item">
                    <label><i class="fa-solid fa-tag"></i> Fatura Türü</label>
                    <select id="vadeFilterType" class="vade-select" onchange="executeVadeFilter()">
                        <option value="all">Tümü (Satış & Alış)</option>
                        <option value="satis">Satış Faturaları (Alacak)</option>
                        <option value="alis">Alış Faturaları (Borç)</option>
                    </select>
                </div>
                <div class="vade-filter-item">
                    <label><i class="fa-regular fa-calendar"></i> Başlangıç Tarihi</label>
                    <input type="date" id="vadeFilterStartDate" class="vade-input" onchange="executeVadeFilter()">
                </div>
                <div class="vade-filter-item">
                    <label><i class="fa-regular fa-calendar"></i> Bitiş Tarihi</label>
                    <input type="date" id="vadeFilterEndDate" class="vade-input" onchange="executeVadeFilter()">
                </div>
                <div class="vade-filter-item" style="display:flex; flex-direction:row; gap:6px;">
                    <button class="premium-btn" style="flex:1; justify-content:center;" onclick="executeVadeFilter()">
                        <i class="fa-solid fa-rotate"></i> Filtrele
                    </button>
                    <button class="premium-btn" style="background:var(--success); padding:7px 12px;" title="Excel İndir" onclick="exportDueDatesExcel()">
                        <i class="fa-solid fa-file-excel"></i>
                    </button>
                </div>
            </div>

            <!-- TABLE PANEL -->
            <div class="panel-card">
                <div class="panel-header" style="justify-content:space-between; flex-wrap:wrap; gap:8px;">
                    <h2>
                        <i class="fa-solid fa-calendar-check header-icon"></i>
                        <span>Fatura Vade & Borç Kapama Listesi</span>
                    </h2>
                    <span style="font-size:0.8rem; font-weight:700; color:var(--text-muted);" id="vadeListCountBadge">0 Kayıt</span>
                </div>
                <div class="panel-body table-responsive" style="padding:0;">
                    <table class="data-table" id="tableVadeDueDates">
                        <thead>
                            <tr>
                                <th>Fatura No & Tür</th>
                                <th>Fatura Tarihi</th>
                                <th>Vade Tarihi</th>
                                <th>Kalan Gün</th>
                                <th>Cari Kodu & Ünvanı</th>
                                <th>İletişim</th>
                                <th style="text-align:right;">Fatura Tutarı</th>
                                <th style="text-align:right;">Ödenen Tutar</th>
                                <th style="text-align:right;">Açık Kalan</th>
                                <th style="text-align:center;">Ödeme Durumu (Elle Kapama)</th>
                                <th style="text-align:center;">Hızlı Bildirim</th>
                            </tr>
                        </thead>
                        <tbody id="vadeTableBody">
                            <tr>
                                <td colspan="11" style="text-align:center; padding:30px;">
                                    <i class="fa-solid fa-circle-notch fa-spin" style="font-size:1.8rem; color:var(--primary); margin-bottom:8px;"></i>
                                    <br>Fatura ve vade kayıtları yükleniyor...
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        `;
    }

    await executeVadeFilter();
}

async function executeVadeFilter() {
    const search = document.getElementById('vadeFilterSearch')?.value || '';
    const maturityStatus = document.getElementById('vadeFilterMaturity')?.value || 'all';
    const paymentStatus = document.getElementById('vadeFilterPayment')?.value || 'all';
    const invoiceType = document.getElementById('vadeFilterType')?.value || 'all';
    const startDate = document.getElementById('vadeFilterStartDate')?.value || '';
    const endDate = document.getElementById('vadeFilterEndDate')?.value || '';

    const tbody = document.getElementById('vadeTableBody');
    if (tbody) {
        tbody.innerHTML = `<tr><td colspan="11" style="text-align:center; padding:30px;"><i class="fa-solid fa-circle-notch fa-spin" style="font-size:1.6rem; color:var(--primary);"></i><br>Filtre uygulanıyor...</td></tr>`;
    }

    try {
        const firm = window.currentFirm || '919';
        const period = window.currentPeriod || '01';

        const params = new URLSearchParams({
            firmNo: firm,
            periodNo: period,
            search: search,
            maturityStatus: maturityStatus,
            paymentStatus: paymentStatus,
            invoiceType: invoiceType,
            startDate: startDate,
            endDate: endDate
        });

        const res = await fetch(`${window.API_BASE}/reports/invoice-due-dates?${params.toString()}`);
        if (!res.ok) throw new Error('API Hatası: HTTP ' + res.status);
        const data = await res.json();
        if (!data.isSuccess) throw new Error(data.message || 'Veri çekilemedi.');

        _currentDueDatesData = data.items || data.invoices || [];

        // 1. Update KPI numbers
        const stats = data.summary || data.stats || {};
        const elTotal = document.getElementById('kpi-vade-total');
        const elTotalCount = document.getElementById('kpi-vade-total-count');
        const elOverdue = document.getElementById('kpi-vade-overdue');
        const elOverdueCount = document.getElementById('kpi-vade-overdue-count');
        const elToday = document.getElementById('kpi-vade-today');
        const elPaid = document.getElementById('kpi-vade-paid');
        const elPaidCount = document.getElementById('kpi-vade-paid-count');
        const elRemaining = document.getElementById('kpi-vade-remaining');
        const elBadgeCount = document.getElementById('vadeListCountBadge');

        const totalDebt = stats.totalAmount || stats.totalDebt || 0;
        const totalInvoices = stats.totalCount || stats.totalInvoices || _currentDueDatesData.length;
        const totalOverdue = stats.totalOverdue || 0;
        const overdueCount = stats.totalOverdueCount || stats.overdueInvoices || 0;
        const todayCount = stats.todayDueCount || stats.dueTodayInvoices || 0;
        const totalPaid = stats.totalPaid || 0;
        const totalRemaining = (stats.totalUnpaid !== undefined) ? stats.totalUnpaid : (stats.totalRemaining || (totalDebt - totalPaid));

        if (elTotal) elTotal.innerText = fmtTL(totalDebt);
        if (elTotalCount) elTotalCount.innerText = `${totalInvoices} Adet Fatura`;
        if (elOverdue) elOverdue.innerText = fmtTL(totalOverdue);
        if (elOverdueCount) elOverdueCount.innerText = `${overdueCount} Fatura Gecikti`;
        if (elToday) elToday.innerText = `${todayCount} Adet`;
        if (elPaid) elPaid.innerText = fmtTL(totalPaid);
        if (elPaidCount) elPaidCount.innerText = `Ödenen Tutar`;
        if (elRemaining) elRemaining.innerText = fmtTL(totalRemaining);
        if (elBadgeCount) elBadgeCount.innerText = `Toplam ${_currentDueDatesData.length} Kayıt`;

        // 2. Render Table Rows
        if (!tbody) return;

        if (_currentDueDatesData.length === 0) {
            tbody.innerHTML = `<tr><td colspan="11" style="text-align:center; padding:35px; color:var(--text-muted); font-weight:600;"><i class="fa-solid fa-inbox" style="font-size:2rem; margin-bottom:8px; display:block;"></i>Belirtilen kriterlere uygun fatura kaydı bulunamadı.</td></tr>`;
            return;
        }

        let html = '';
        _currentDueDatesData.forEach(inv => {
            const fNo = inv.ficheNo || inv.ficheno || '-';
            const fKey = inv.invoiceKey || fNo;
            const cName = inv.cariName || 'Tanımsız Cari';
            const dDate = inv.dueDateFormatted || inv.dueDate || '-';
            const iDate = inv.invoiceDateFormatted || inv.invoiceDate || '-';
            const pNote = inv.note || inv.paymentNote || '';
            const nTotal = inv.netTotal || 0;
            const pAmount = inv.paidAmount || 0;
            const rAmount = (inv.remainingAmount !== undefined) ? inv.remainingAmount : Math.max(0, nTotal - pAmount);

            // Days remaining badge
            let daysBadge = '';
            if (inv.daysRemaining < 0) {
                daysBadge = `<span class="badge badge-overdue"><i class="fa-solid fa-triangle-exclamation"></i> ${Math.abs(inv.daysRemaining)} Gün Gecikti</span>`;
            } else if (inv.daysRemaining === 0) {
                daysBadge = `<span class="badge badge-today"><i class="fa-solid fa-clock"></i> Bugün Vadesi</span>`;
            } else {
                daysBadge = `<span class="badge badge-future"><i class="fa-solid fa-circle-check"></i> ${inv.daysRemaining} Gün Kaldı</span>`;
            }

            // Payment status badge
            let payBadge = '';
            if (inv.paymentStatus === 'Odendi') {
                payBadge = `<span class="badge badge-odendi badge-clickable" onclick="openPaymentStatusModal('${fKey}', '${escapeQuotes(fNo)}', '${escapeQuotes(cName)}', '${dDate}', ${nTotal}, 'Odendi', ${pAmount}, '${escapeQuotes(pNote)}')"><i class="fa-solid fa-circle-check"></i> Ödendi</span>`;
            } else if (inv.paymentStatus === 'Kismi') {
                payBadge = `<span class="badge badge-kismi badge-clickable" onclick="openPaymentStatusModal('${fKey}', '${escapeQuotes(fNo)}', '${escapeQuotes(cName)}', '${dDate}', ${nTotal}, 'Kismi', ${pAmount}, '${escapeQuotes(pNote)}')"><i class="fa-solid fa-circle-half-stroke"></i> Kısmi (${fmtNum(pAmount)} ₺)</span>`;
            } else {
                payBadge = `<span class="badge badge-odenmedi badge-clickable" onclick="openPaymentStatusModal('${fKey}', '${escapeQuotes(fNo)}', '${escapeQuotes(cName)}', '${dDate}', ${nTotal}, 'Odenmedi', 0, '${escapeQuotes(pNote)}')"><i class="fa-solid fa-circle-xmark"></i> Ödenmedi / Açık</span>`;
            }

            const isSatis = String(inv.invoiceType).toUpperCase().includes('SATIŞ') || inv.invoiceType === 'Satış';
            const typeColor = isSatis ? 'color:var(--success);' : 'color:var(--warning);';
            const typeBadge = `<span style="font-size:0.7rem; font-weight:700; ${typeColor}; background:#f8fafc; padding:2px 6px; border-radius:4px; border:1px solid #e2e8f0; margin-left:4px;">${inv.invoiceType}</span>`;

            // Normalize inv for modal
            const normInv = {
                ...inv,
                ficheNo: fNo,
                cariName: cName,
                dueDate: dDate,
                date: iDate,
                netTotal: nTotal,
                paidAmount: pAmount,
                remainingAmount: rAmount
            };
            const invJson = encodeURIComponent(JSON.stringify(normInv));

            html += `
                <tr>
                    <td>
                        <div style="font-weight:700; color:var(--secondary);">${fNo} ${typeBadge}</div>
                        ${inv.docode ? `<small style="color:var(--text-muted); font-size:0.72rem;">Belge: ${inv.docode}</small>` : ''}
                    </td>
                    <td>${iDate}</td>
                    <td style="font-weight:700; color:var(--secondary);">${dDate}</td>
                    <td>${daysBadge}</td>
                    <td>
                        <a href="javascript:void(0)" onclick="openCariDetailModal('${escapeQuotes(inv.cariCode)}', '${escapeQuotes(cName)}')" style="font-weight:700; color:var(--primary); text-decoration:none;" title="Cari Hareketlerini İncele">
                            ${cName}
                        </a>
                        <br><small style="color:var(--text-muted); font-size:0.72rem;">${inv.cariCode || ''}</small>
                    </td>
                    <td>
                        ${inv.phone ? `<a href="tel:${inv.phone}" title="${inv.phone}" style="color:var(--text-main); margin-right:6px;"><i class="fa-solid fa-phone"></i></a>` : ''}
                        ${inv.email ? `<a href="mailto:${inv.email}" title="${inv.email}" style="color:var(--text-main);"><i class="fa-solid fa-envelope"></i></a>` : ''}
                        ${!inv.phone && !inv.email ? '<span style="color:var(--text-muted);">-</span>' : ''}
                    </td>
                    <td class="ag-num" style="font-weight:700;">${fmtTL(nTotal)}</td>
                    <td class="ag-num" style="color:var(--success);">${fmtTL(pAmount)}</td>
                    <td class="ag-num" style="font-weight:800; color:${rAmount > 0 ? 'var(--danger)' : 'var(--text-muted)'};">${fmtTL(rAmount)}</td>
                    <td style="text-align:center;">${payBadge}</td>
                    <td style="text-align:center;">
                        <div class="btn-icon-group" style="justify-content:center;">
                            <button class="btn-channel btn-wa" title="WhatsApp Mesajı Gönder" onclick="openNotificationModalWithInvoice('${invJson}', 'whatsapp')"><i class="fa-brands fa-whatsapp"></i></button>
                            <button class="btn-channel btn-sms" title="SMS Gönder" onclick="openNotificationModalWithInvoice('${invJson}', 'sms')"><i class="fa-solid fa-comment-sms"></i></button>
                            <button class="btn-channel btn-tg" title="Telegram Gönder" onclick="openNotificationModalWithInvoice('${invJson}', 'telegram')"><i class="fa-brands fa-telegram"></i></button>
                            <button class="btn-channel btn-mail" title="E-Posta Gönder" onclick="openNotificationModalWithInvoice('${invJson}', 'mail')"><i class="fa-solid fa-envelope"></i></button>
                        </div>
                    </td>
                </tr>
            `;
        });

        tbody.innerHTML = html;

    } catch (err) {
        if (tbody) {
            tbody.innerHTML = `<tr><td colspan="11" style="text-align:center; padding:30px; color:var(--danger);"><i class="fa-solid fa-triangle-exclamation" style="font-size:1.8rem; margin-bottom:8px;"></i><br>Hata: ${err.message}</td></tr>`;
        }
    }
}

function escapeQuotes(str) {
    if (!str) return '';
    return String(str).replace(/'/g, "\\'").replace(/"/g, '&quot;');
}

// ==========================================================
// MODAL 1: BORÇ KAPAMA / ELLE ÖDENDİ HANDLER
// ==========================================================
function openPaymentStatusModal(invoiceKey, ficheno, cariName, dueDate, netTotal, status, paidAmount, note) {
    document.getElementById('payModalInvoiceKey').value = invoiceKey;
    document.getElementById('payModalRawNetTotal').value = netTotal;
    document.getElementById('payModalFicheNo').innerText = ficheno;
    document.getElementById('payModalCariName').innerText = cariName;
    document.getElementById('payModalDueDate').innerText = dueDate || '-';
    document.getElementById('payModalNetTotal').innerText = fmtTL(netTotal);

    // Set radio
    const radios = document.getElementsByName('payModalStatusRadio');
    for (const r of radios) {
        r.checked = (r.value === status);
    }

    const paidInput = document.getElementById('payModalPaidAmount');
    if (paidInput) {
        paidInput.value = (status === 'Odendi' ? netTotal : (paidAmount || (status === 'Kismi' ? (netTotal / 2) : 0)));
    }

    const dateInput = document.getElementById('payModalDate');
    if (dateInput) {
        dateInput.value = new Date().toISOString().split('T')[0];
    }

    const noteInput = document.getElementById('payModalNote');
    if (noteInput) {
        noteInput.value = note || '';
    }

    openModal('modalPaymentStatus');
}

function onPaymentRadioChange() {
    const selected = document.querySelector('input[name="payModalStatusRadio"]:checked')?.value;
    const rawNetTotal = parseFloat(document.getElementById('payModalRawNetTotal')?.value || 0);
    const paidInput = document.getElementById('payModalPaidAmount');
    if (!paidInput) return;

    if (selected === 'Odendi') {
        paidInput.value = rawNetTotal;
    } else if (selected === 'Odenmedi') {
        paidInput.value = 0;
    }
}

async function savePaymentStatus() {
    const invoiceKey = document.getElementById('payModalInvoiceKey')?.value;
    const ficheNo = document.getElementById('payModalFicheNo')?.innerText || '';
    const status = document.querySelector('input[name="payModalStatusRadio"]:checked')?.value || 'Odenmedi';
    const paidAmount = parseFloat(document.getElementById('payModalPaidAmount')?.value || 0);
    const dateVal = document.getElementById('payModalDate')?.value;
    const note = document.getElementById('payModalNote')?.value || '';

    if (!invoiceKey) return;

    try {
        const res = await fetch(`${window.API_BASE}/reports/invoice-payment-status`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                invoiceKey: invoiceKey,
                ficheNo: ficheNo,
                status: status,
                paidAmount: paidAmount,
                paidDate: dateVal ? new Date(dateVal).toISOString() : null,
                note: note
            })
        });

        if (!res.ok) throw new Error('Kaydedilemedi: HTTP ' + res.status);
        const data = await res.json();
        if (!data.isSuccess) throw new Error(data.message || 'Hata oluştu.');

        closeModal('modalPaymentStatus');
        await executeVadeFilter();
    } catch (e) {
        alert('Ödeme durumu kaydedilemedi: ' + e.message);
    }
}

// ==========================================================
// MODAL 2: CARİ HAREKETLERİ & VADE DETAY HANDLER
// ==========================================================
async function openCariDetailModal(cariCode, cariName) {
    document.getElementById('cariModalTitle').innerHTML = `<i class="fa-solid fa-user-check" style="color:var(--primary);"></i> ${cariName}`;
    document.getElementById('cariModalSub').innerText = `Cari Kodu: ${cariCode}`;

    const tbody = document.getElementById('cariModalTableBody');
    if (tbody) {
        tbody.innerHTML = `<tr><td colspan="9" style="text-align:center; padding:25px;"><i class="fa-solid fa-circle-notch fa-spin"></i> Cari faturaları getiriliyor...</td></tr>`;
    }

    openModal('modalCariTransactions');

    try {
        const firm = window.currentFirm || '919';
        const period = window.currentPeriod || '01';
        const res = await fetch(`${window.API_BASE}/reports/cari-transactions?cariCode=${encodeURIComponent(cariCode)}&firmNo=${firm}&periodNo=${period}`);
        if (!res.ok) throw new Error('HTTP ' + res.status);
        const data = await res.json();
        if (!data.isSuccess) throw new Error(data.message || 'Veri alınamadı.');

        _currentCariInvoices = data.transactions || data.invoices || data.items || [];

        // Tab counts
        const countAll = _currentCariInvoices.length;
        const countPaid = _currentCariInvoices.filter(i => i.paymentStatus === 'Odendi').length;
        const countUnpaid = countAll - countPaid;

        document.getElementById('countCariAll').innerText = countAll;
        document.getElementById('countCariUnpaid').innerText = countUnpaid;
        document.getElementById('countCariPaid').innerText = countPaid;

        filterCariModalTab('all');
    } catch (e) {
        if (tbody) {
            tbody.innerHTML = `<tr><td colspan="9" style="text-align:center; padding:20px; color:var(--danger);">${e.message}</td></tr>`;
        }
    }
}

function filterCariModalTab(tab) {
    document.querySelectorAll('.modal-tab-btn').forEach(b => b.classList.remove('active'));
    if (tab === 'all') document.getElementById('tabCariAll')?.classList.add('active');
    if (tab === 'unpaid') document.getElementById('tabCariUnpaid')?.classList.add('active');
    if (tab === 'paid') document.getElementById('tabCariPaid')?.classList.add('active');

    let list = _currentCariInvoices;
    if (tab === 'unpaid') {
        list = list.filter(i => i.paymentStatus !== 'Odendi');
    } else if (tab === 'paid') {
        list = list.filter(i => i.paymentStatus === 'Odendi');
    }

    const tbody = document.getElementById('cariModalTableBody');
    if (!tbody) return;

    if (list.length === 0) {
        tbody.innerHTML = `<tr><td colspan="9" style="text-align:center; padding:25px; color:var(--text-muted);">Bu sekmede kayıtlı fatura bulunmuyor.</td></tr>`;
        return;
    }

    let html = '';
    list.forEach(inv => {
        let daysBadge = '';
        if (inv.daysRemaining < 0) {
            daysBadge = `<span class="badge badge-overdue">${Math.abs(inv.daysRemaining)} Gün Gecikti</span>`;
        } else if (inv.daysRemaining === 0) {
            daysBadge = `<span class="badge badge-today">Bugün</span>`;
        } else {
            daysBadge = `<span class="badge badge-future">${inv.daysRemaining} Gün Kaldı</span>`;
        }

        let payBadge = '';
        if (inv.paymentStatus === 'Odendi') {
            payBadge = `<span class="badge badge-odendi"><i class="fa-solid fa-check"></i> Ödendi</span>`;
        } else if (inv.paymentStatus === 'Kismi') {
            payBadge = `<span class="badge badge-kismi"><i class="fa-solid fa-circle-half-stroke"></i> Kısmi</span>`;
        } else {
            payBadge = `<span class="badge badge-odenmedi"><i class="fa-solid fa-xmark"></i> Ödenmedi</span>`;
        }

        const fNo = inv.ficheNo || inv.ficheno || '-';
        const fKey = inv.invoiceKey || fNo;
        const cName = inv.cariName || document.getElementById('cariModalTitle')?.innerText || 'Cari';
        const dDate = inv.dueDateFormatted || inv.dueDate || '-';
        const iDate = inv.invoiceDateFormatted || inv.date || '-';
        const pNote = inv.note || inv.paymentNote || '';
        const nTotal = inv.netTotal || 0;
        const pAmount = inv.paidAmount || 0;
        const rAmount = (inv.remainingAmount !== undefined) ? inv.remainingAmount : Math.max(0, nTotal - pAmount);

        html += `
            <tr>
                <td style="font-weight:700;">${fNo}</td>
                <td>${iDate}</td>
                <td>${dDate}</td>
                <td>${daysBadge}</td>
                <td class="ag-num">${fmtTL(nTotal)}</td>
                <td class="ag-num" style="color:var(--success);">${fmtTL(pAmount)}</td>
                <td class="ag-num" style="font-weight:800; color:${rAmount > 0 ? 'var(--danger)' : 'var(--text-muted)'};">${fmtTL(rAmount)}</td>
                <td>${payBadge}</td>
                <td style="text-align:center;">
                    <button class="mini-btn" onclick="openPaymentStatusModal('${fKey}', '${escapeQuotes(fNo)}', '${escapeQuotes(cName)}', '${dDate}', ${nTotal}, '${inv.paymentStatus}', ${pAmount}, '${escapeQuotes(pNote)}')">
                        <i class="fa-solid fa-pen-to-square"></i> Düzenle
                    </button>
                </td>
            </tr>
        `;
    });

    tbody.innerHTML = html;
}

// ==========================================================
// MODAL 3: ÇOK KANALLI BİLDİRİM GÖNDERİMİ
// ==========================================================
function openNotificationModalWithInvoice(encodedJson, defaultChannel) {
    try {
        const inv = JSON.parse(decodeURIComponent(encodedJson));
        _currentNotifyInvoice = inv;
        _selectedNotifyChannel = defaultChannel || 'whatsapp';

        document.getElementById('notifModalCariName').innerText = inv.cariName || 'Cari';
        document.getElementById('notifModalPhone').value = inv.phone || '';
        document.getElementById('notifModalEmail').value = inv.email || '';
        document.getElementById('notifModalAlert').style.display = 'none';

        selectNotifyChannel(_selectedNotifyChannel);
        applyNotifyTemplate();
        openModal('modalSendNotification');
    } catch (e) {
        console.error('Bildirim modalı açılamadı:', e);
    }
}

function selectNotifyChannel(ch) {
    _selectedNotifyChannel = ch;
    document.querySelectorAll('.channel-select-btn').forEach(b => b.classList.remove('active'));
    if (ch === 'whatsapp') document.getElementById('chBtnWhatsApp')?.classList.add('active');
    if (ch === 'sms') document.getElementById('chBtnSms')?.classList.add('active');
    if (ch === 'telegram') document.getElementById('chBtnTelegram')?.classList.add('active');
    if (ch === 'mail') document.getElementById('chBtnMail')?.classList.add('active');
}

function applyNotifyTemplate() {
    if (!_currentNotifyInvoice) return;
    const tpl = document.getElementById('notifModalTemplate')?.value || 'hatirlatma';
    const inv = _currentNotifyInvoice;
    const txtArea = document.getElementById('notifModalMessage');
    if (!txtArea) return;

    let msg = '';
    if (tpl === 'hatirlatma') {
        msg = `Sayın ${inv.cariName},\n${inv.ficheno} numaralı ve ${fmtTL(inv.netTotal)} tutarındaki faturanızın son ödeme tarihi ${inv.dueDate} olarak görünmektedir. Ödemenizi vadesinde gerçekleştirmenizi rica ederiz.\nİyi çalışmalar dileriz.`;
    } else if (tpl === 'gecikme') {
        const gecikme = Math.abs(inv.daysRemaining || 0);
        msg = `ÖNEMLİ VADE BİLDİRİMİ:\nSayın ${inv.cariName},\n${inv.ficheno} numaralı ${fmtTL(inv.netTotal)} tutarındaki faturanızın vadesi (${inv.dueDate}) üzerinden ${gecikme} gün geçmiş olup açık bakiye ${fmtTL(inv.remainingAmount)}'dir. Lütfen ödemenizi ivedilikle gerçekleştiriniz.`;
    } else if (tpl === 'tesekkur') {
        msg = `Sayın ${inv.cariName},\n${inv.ficheno} numaralı faturanıza ait ${fmtTL(inv.paidAmount || inv.netTotal)} tutarındaki ödemeniz muhasebe kayıtlarımıza geçmiştir. İş birliğiniz için teşekkür ederiz.`;
    } else {
        msg = `HESAP ÖZETİ:\nFirma: ${inv.cariName}\nFatura No: ${inv.ficheno}\nFatura Tarihi: ${inv.date}\nVade Tarihi: ${inv.dueDate}\nFatura Tutarı: ${fmtTL(inv.netTotal)}\nKalan Bakiye: ${fmtTL(inv.remainingAmount)}`;
    }

    txtArea.value = msg;
}

async function executeSendNotification() {
    const alertEl = document.getElementById('notifModalAlert');
    const btn = document.getElementById('btnSubmitNotify');
    const msg = document.getElementById('notifModalMessage')?.value || '';
    const phone = document.getElementById('notifModalPhone')?.value || '';
    const email = document.getElementById('notifModalEmail')?.value || '';

    if (!msg.trim()) {
        alert('Lütfen gönderilecek mesaj metnini girin.');
        return;
    }

    if (alertEl) {
        alertEl.style.display = 'block';
        alertEl.style.background = '#eff6ff';
        alertEl.style.color = 'var(--primary)';
        alertEl.innerHTML = `<i class="fa-solid fa-circle-notch fa-spin"></i> Mesaj gönderiliyor...`;
    }
    if (btn) btn.disabled = true;

    try {
        if (_selectedNotifyChannel === 'whatsapp') {
            const res = await fetch(`${window.API_BASE}/notification/send-whatsapp`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ phone: phone, message: msg })
            });
            const data = await res.json();
            if (data.via === 'web_link' && data.webUrl) {
                window.open(data.webUrl, '_blank');
                alertEl.style.background = '#f0fdf4';
                alertEl.style.color = 'var(--success)';
                alertEl.innerHTML = `<i class="fa-solid fa-circle-check"></i> WhatsApp Web açıldı! Mesajınızı WhatsApp ekranında Gönder tuşuna basarak iletebilirsiniz.`;
            } else {
                alertEl.style.background = '#f0fdf4';
                alertEl.style.color = 'var(--success)';
                alertEl.innerHTML = `<i class="fa-solid fa-circle-check"></i> WhatsApp mesajı yerel servis üzerinden anında iletildi!`;
            }
        } else if (_selectedNotifyChannel === 'sms') {
            const res = await fetch(`${window.API_BASE}/notification/send-sms`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ to: phone, message: msg })
            });
            const data = await res.json();
            if (!data.success) throw new Error(data.message || 'SMS gönderilemedi');
            alertEl.style.background = '#f0fdf4';
            alertEl.style.color = 'var(--success)';
            alertEl.innerHTML = `<i class="fa-solid fa-circle-check"></i> SMS başarıyla iletildi.`;
        } else if (_selectedNotifyChannel === 'telegram') {
            const res = await fetch(`${window.API_BASE}/notification/send-telegram`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: msg })
            });
            const data = await res.json();
            if (!data.success) throw new Error(data.message || 'Telegram gönderilemedi');
            alertEl.style.background = '#f0fdf4';
            alertEl.style.color = 'var(--success)';
            alertEl.innerHTML = `<i class="fa-solid fa-circle-check"></i> Telegram bildirimi iletildi.`;
        } else if (_selectedNotifyChannel === 'mail') {
            const res = await fetch(`${window.API_BASE}/notification/send-mail`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ to: email, subject: 'Fatura Vade Bildirimi', body: msg.replace(/\n/g, '<br>') })
            });
            const data = await res.json();
            if (!data.success) throw new Error(data.message || 'E-posta gönderilemedi');
            alertEl.style.background = '#f0fdf4';
            alertEl.style.color = 'var(--success)';
            alertEl.innerHTML = `<i class="fa-solid fa-circle-check"></i> E-posta başarıyla iletildi.`;
        }
    } catch (e) {
        if (alertEl) {
            alertEl.style.background = '#fee2e2';
            alertEl.style.color = 'var(--danger)';
            alertEl.innerHTML = `<i class="fa-solid fa-triangle-exclamation"></i> Gönderim hatası: ${e.message}`;
        }
    } finally {
        if (btn) btn.disabled = false;
    }
}

// Excel Export
function exportDueDatesExcel() {
    const table = document.getElementById('tableVadeDueDates');
    if (!table) return;
    if (typeof XLSX !== 'undefined') {
        const wb = XLSX.utils.table_to_book(table, { sheet: 'VadeTakip' });
        XLSX.writeFile(wb, `Logo_Fatura_Vade_Takip_${new Date().toISOString().split('T')[0]}.xlsx`);
    } else {
        alert('Excel kütüphanesi yüklenemedi.');
    }
}

// ==========================================================
// TOKENSIZ WHATSAPP WEB ENTEGRASYONU ENGINE
// ==========================================================
async function loadWhatsAppBridgeView() {
    const container = document.getElementById('view-whatsapp-bridge');
    if (!container) return;

    container.innerHTML = `
        <div class="grid-row row-2-cols">
            <!-- DURUM VE QR KOD KARTI -->
            <div class="panel-card">
                <div class="panel-header">
                    <h2><i class="fa-brands fa-whatsapp header-icon" style="color:#25d366;"></i> WhatsApp Web Bağlantı Durumu</h2>
                    <button class="mini-btn" onclick="refreshWhatsAppStatus()"><i class="fa-solid fa-rotate"></i> Yenile</button>
                </div>
                <div class="panel-body" id="waStatusContainer" style="text-align:center; padding:30px;">
                    <i class="fa-solid fa-circle-notch fa-spin" style="font-size:2rem; color:var(--primary);"></i>
                    <p style="margin-top:12px; color:var(--text-muted); font-size:0.85rem;">WhatsApp Köprüsü (port 5001) kontrol ediliyor...</p>
                </div>
            </div>

            <!-- ANLIK MESAJ GÖNDERİM & TEST -->
            <div class="panel-card">
                <div class="panel-header">
                    <h2><i class="fa-solid fa-paper-plane header-icon" style="color:var(--primary);"></i> WhatsApp Test & Hızlı Mesaj Gönder</h2>
                </div>
                <div class="panel-body" style="padding:20px;">
                    <div style="margin-bottom:12px;">
                        <label style="font-size:0.75rem; font-weight:700; color:var(--text-muted); display:block; margin-bottom:4px;">TELEFON NUMARASI (905...)</label>
                        <input type="text" id="waTestPhone" class="vade-input" placeholder="0532 123 45 67" style="width:100%;">
                    </div>
                    <div style="margin-bottom:12px;">
                        <label style="font-size:0.75rem; font-weight:700; color:var(--text-muted); display:block; margin-bottom:4px;">MESAJ İÇERİĞİ</label>
                        <textarea id="waTestMsg" class="vade-input" style="width:100%; height:90px; resize:none;" placeholder="Merhaba, bu mesaj Logo Asistan Bot WhatsApp Köprüsü üzerinden iletilmektedir."></textarea>
                    </div>
                    <button class="premium-btn" style="background:#25d366; width:100%; justify-content:center;" onclick="sendTestWhatsAppMessage()">
                        <i class="fa-brands fa-whatsapp"></i> Mesajı Gönder
                    </button>
                    <div id="waTestAlert" style="display:none; font-size:0.8rem; padding:8px 12px; border-radius:6px; margin-top:10px;"></div>
                </div>
            </div>
        </div>

        <!-- YÖNERGE PANELİ -->
        <div class="panel-card" style="margin-top:16px;">
            <div class="panel-header">
                <h2><i class="fa-solid fa-circle-info header-icon"></i> Tokensiz WhatsApp Web Entegrasyonu Hakkında</h2>
            </div>
            <div class="panel-body" style="padding:20px; line-height:1.7; color:var(--text-main); font-size:0.85rem;">
                <p><b>Tokensiz WhatsApp Nasıl Çalışır?</b></p>
                <ul style="padding-left:20px; margin-bottom:12px;">
                    <li>Meta WhatsApp Cloud API için token, kredi kartı veya aylık ücret ödemenize gerek yoktur.</li>
                    <li>Sistem, WhatsApp Web protokolünü (Baileys) kullanarak bilgisayarınızda yerel bir oturum (port 5001) açar.</li>
                    <li>Telefonunuzdaki WhatsApp uygulamasından <b>Bağlı Cihazlar &gt; Cihaz Bağla</b> seçeneğiyle QR kodu bir kez taratmanız yeterlidir.</li>
                    <li>Müşterileriniz WhatsApp'tan <i>"bakiye"</i>, <i>"ekstre"</i> veya <i>"vade"</i> yazdığında, Telegram botunda olduğu gibi Logo ERP'den anında otomatik yanıt alırlar!</li>
                </ul>
            </div>
        </div>
    `;

    await refreshWhatsAppStatus();
}

async function refreshWhatsAppStatus() {
    const el = document.getElementById('waStatusContainer');
    if (!el) return;

    try {
        const res = await fetch(`${window.API_BASE}/notification/whatsapp-bridge-status`);
        const data = await res.json();

        if (data.online && data.status === 'connected') {
            el.innerHTML = `
                <div style="width:60px; height:60px; border-radius:50%; background:#dcfce7; color:var(--success); display:inline-flex; align-items:center; justify-content:center; font-size:2rem; margin-bottom:12px;">
                    <i class="fa-brands fa-whatsapp"></i>
                </div>
                <h3 style="font-size:1.15rem; font-weight:800; color:var(--success); margin-bottom:4px;">🟢 WhatsApp Bağlı ve Aktif!</h3>
                <p style="color:var(--text-muted); font-size:0.85rem;">Bağlı Telefon: <b>${data.userPhone || 'Yetkili'}</b></p>
                <small style="color:var(--text-muted);">Gelen ve giden mesajlar arka planda dinleniyor.</small>
            `;
        } else if (data.online && data.qr) {
            el.innerHTML = `
                <h3 style="font-size:1.05rem; font-weight:800; color:var(--secondary); margin-bottom:8px;">📱 WhatsApp QR Kodunu Okutun</h3>
                <p style="color:var(--text-muted); font-size:0.82rem; margin-bottom:14px;">Telefonunuzdan <b>WhatsApp &gt; Ayarlar &gt; Bağlı Cihazlar &gt; Cihaz Bağla</b> yolunu izleyip QR kodu taratın:</p>
                <div style="display:inline-block; padding:10px; background:#fff; border:2px dashed #cbd5e1; border-radius:12px; box-shadow:0 4px 12px rgba(0,0,0,0.06);">
                    <img src="${data.qr}" alt="WhatsApp QR Code" style="width:240px; height:240px; display:block;">
                </div>
                <br>
                <button class="mini-btn" style="margin-top:14px;" onclick="refreshWhatsAppStatus()"><i class="fa-solid fa-rotate"></i> QR Kodu Yenile</button>
            `;
        } else {
            el.innerHTML = `
                <div style="width:60px; height:60px; border-radius:50%; background:#fef2f2; color:var(--danger); display:inline-flex; align-items:center; justify-content:center; font-size:1.8rem; margin-bottom:12px;">
                    <i class="fa-brands fa-whatsapp"></i>
                </div>
                <h3 style="font-size:1.05rem; font-weight:800; color:var(--danger); margin-bottom:4px;">WhatsApp Köprüsü Çevrimdışı</h3>
                <p style="color:var(--text-muted); font-size:0.82rem; max-width:400px; margin:0 auto 14px auto;">
                    Yerel WhatsApp Web Bridge servisi (port 5001) şu anda çalışmıyor.
                </p>
                <button class="premium-btn" style="background:#25d366; margin:0 auto;" onclick="refreshWhatsAppStatus()"><i class="fa-solid fa-rotate"></i> Yeniden Kontrol Et</button>
            `;
        }
    } catch (e) {
        el.innerHTML = `
            <div style="color:var(--danger); font-size:0.9rem;">
                <i class="fa-solid fa-triangle-exclamation"></i> Durum kontrol edilemedi: ${e.message}
            </div>
        `;
    }
}

async function sendTestWhatsAppMessage() {
    const phone = document.getElementById('waTestPhone')?.value;
    const msg = document.getElementById('waTestMsg')?.value;
    const alertEl = document.getElementById('waTestAlert');

    if (!phone || !msg) {
        alert('Lütfen telefon numarası ve test mesajı girin.');
        return;
    }

    if (alertEl) {
        alertEl.style.display = 'block';
        alertEl.style.background = '#eff6ff';
        alertEl.style.color = 'var(--primary)';
        alertEl.innerHTML = `<i class="fa-solid fa-circle-notch fa-spin"></i> Gönderiliyor...`;
    }

    try {
        const res = await fetch(`${window.API_BASE}/notification/send-whatsapp`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ phone: phone, message: msg })
        });
        const data = await res.json();
        if (data.via === 'web_link' && data.webUrl) {
            window.open(data.webUrl, '_blank');
            if (alertEl) {
                alertEl.style.background = '#f0fdf4';
                alertEl.style.color = 'var(--success)';
                alertEl.innerHTML = `<i class="fa-solid fa-circle-check"></i> WhatsApp Web açıldı. Mesajınızı WhatsApp ekranında Gönder tuşuna basarak iletebilirsiniz.`;
            }
        } else {
            if (alertEl) {
                alertEl.style.background = '#f0fdf4';
                alertEl.style.color = 'var(--success)';
                alertEl.innerHTML = `<i class="fa-solid fa-circle-check"></i> Mesaj başarıyla iletildi!`;
            }
        }
    } catch (e) {
        if (alertEl) {
            alertEl.style.background = '#fee2e2';
            alertEl.style.color = 'var(--danger)';
            alertEl.innerHTML = `<i class="fa-solid fa-triangle-exclamation"></i> Hata: ${e.message}`;
        }
    }
}

