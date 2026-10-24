// ============================================================
// Logo Premium Viewer - app.js
// Liste → Göz → Render akışı
// ============================================================
const CONFIG = { backendUrl: '', apiKey: 'logo-pro-secret-2026' };
let currentType = 'fatura';
let xsltList = [];
let rules = [];
let selectedXslts = { fatura: '', arsiv: '', irsaliye: '', siparis: '', talep: '' };
let currentData = null;
let allDocuments = []; // Filtered local copy

// --- DOM Refs ---
const navItems = document.querySelectorAll('.nav-item');
const pageTitle = document.getElementById('pageTitle');
const listView = document.getElementById('listView');
const previewView = document.getElementById('previewView');
const manualView = document.getElementById('manualView');
const listHeader = document.getElementById('listHeader');
const manualHeader = document.getElementById('manualHeader');
const htmlContent = document.getElementById('htmlContent');
const manualHtmlContent = document.getElementById('manualHtmlContent');
const autoMailView = document.getElementById('autoMailView');
const autoMailLog = document.getElementById('autoMailLog');

// Default dates: last 30 days
(function setDefaultDates() {
    const today = new Date();
    const from = new Date();
    from.setDate(today.getDate() - 30);
    document.getElementById('dateFrom').value = from.toISOString().split('T')[0];
    document.getElementById('dateTo').value = today.toISOString().split('T')[0];
})();

// ---- NAVIGATION ----
navItems.forEach(item => {
    item.addEventListener('click', () => {
        navItems.forEach(i => i.classList.remove('active'));
        item.classList.add('active');
        currentType = item.dataset.type;

        if (currentType === 'manual') {
            pageTitle.innerText = 'MANUEL';
            listView.classList.add('hidden');
            previewView.classList.add('hidden');
            autoMailView.classList.add('hidden');
            manualView.classList.remove('hidden');
            listHeader.classList.add('hidden');
            manualHeader.classList.remove('hidden');
        } else if (currentType === 'automail') {
            pageTitle.innerText = 'OTOMATİK GÖNDERİM';
            listView.classList.add('hidden');
            previewView.classList.add('hidden');
            manualView.classList.add('hidden');
            autoMailView.classList.remove('hidden');
            listHeader.classList.add('hidden');
            manualHeader.classList.add('hidden');
            loadAutoMailConfig();
        } else {
            const labels = { 
                fatura: 'e-Fatura', 
                arsiv: 'e-Arşiv', 
                irsaliye: 'e-İrsaliye',
                siparis: 'Siparişler',
                talep: 'Talepler'
            };
            pageTitle.innerText = labels[currentType] || currentType;
            manualView.classList.add('hidden');
            autoMailView.classList.add('hidden');
            manualHeader.classList.add('hidden');
            listHeader.classList.remove('hidden');
            previewView.classList.add('hidden');
            listView.classList.remove('hidden');
            
            // Seçimleri temizle
            clearSelection();
            
            // Auto-list on tab change
            fetchDocumentList();
        }
    });
});

// ---- DOCUMENT LIST ----
document.getElementById('btnList').addEventListener('click', fetchDocumentList);

async function fetchDocumentList() {
    const dateFrom = document.getElementById('dateFrom').value;
    const dateTo = document.getElementById('dateTo').value;
    const tbody = document.getElementById('docTableBody');
    const tableWrap = document.getElementById('listTableWrap');
    const emptyState = document.getElementById('listEmptyState');

    emptyState.classList.add('hidden');
    tableWrap.classList.add('hidden');
    tbody.innerHTML = '<tr><td colspan="7" class="loading-row"><i class="fa-solid fa-spinner fa-spin"></i> Yükleniyor...</td></tr>';
    tableWrap.classList.remove('hidden');

    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/list-documents`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify({ type: currentType, dateFrom, dateTo, dbConfig: getStoredDbConfig() })
        });
        const data = await res.json();
        if (!res.ok) throw new Error(data.error || 'Sorgu hatası');
        
        allDocuments = data.documents || [];
        renderTable(allDocuments);
        updateDashboard(data.stats);
        
        document.getElementById('listTableWrap').classList.remove('hidden');
        document.getElementById('listCount').innerText = `${data.documents.length} kayıt bulundu.`;
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="7" class="error-row"><i class="fa-solid fa-triangle-exclamation"></i> Liste alınamadı: ${err.message}</td></tr>`;
    }
}

// ---- DASHBOARD & FILTER LOGIC ----
function updateDashboard(stats) {
    const dash = document.getElementById('dashboardStats');
    if (!stats || stats.count === 0) {
        dash.classList.add('hidden');
        return;
    }
    dash.classList.remove('hidden');
    
    const fmt = (v) => new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2 }).format(v);
    
    document.getElementById('statBrut').innerText = fmt(stats.totalBrut);
    document.getElementById('statKdv').innerText = fmt(stats.totalKdv);
    document.getElementById('statNet').innerText = fmt(stats.totalNet);
}

// ---- SELECTION LOGIC ----
let selectedDocs = new Set();

function toggleSelectAll(master) {
    const checks = document.querySelectorAll('.doc-checkbox');
    selectedDocs.clear();
    checks.forEach(c => {
        c.checked = master.checked;
        if (c.checked) selectedDocs.add(c.dataset.no);
    });
    updateBulkToolbar();
}

function toggleSelect(no) {
    if (selectedDocs.has(no)) selectedDocs.delete(no);
    else selectedDocs.add(no);
    
    document.getElementById('selectAll').checked = (selectedDocs.size === allDocuments.length && allDocuments.length > 0);
    updateBulkToolbar();
}

function updateBulkToolbar() {
    const toolbar = document.getElementById('bulkToolbar');
    const text = document.getElementById('selectionText');
    if (selectedDocs.size > 0) {
        toolbar.classList.remove('hidden');
        text.innerText = `${selectedDocs.size} belge seçildi`;
    } else {
        toolbar.classList.add('hidden');
    }
}

function clearSelection() {
    selectedDocs.clear();
    const selectAll = document.getElementById('selectAll');
    if (selectAll) selectAll.checked = false;
    document.querySelectorAll('.doc-checkbox').forEach(c => c.checked = false);
    updateBulkToolbar();
}

function setQuickDate(mode) {
    const from = document.getElementById('dateFrom');
    const to = document.getElementById('dateTo');
    const now = new Date();
    
    const fmt = (d) => d.toISOString().split('T')[0];

    if (mode === 'today') {
        from.value = to.value = fmt(now);
    } else if (mode === 'yesterday') {
        const yesterday = new Date(now);
        yesterday.setDate(now.getDate() - 1);
        from.value = to.value = fmt(yesterday);
    } else if (mode === 'thisMonth') {
        from.value = fmt(new Date(now.getFullYear(), now.getMonth(), 1));
        to.value = fmt(new Date(now.getFullYear(), now.getMonth() + 1, 0));
    } else if (mode === 'lastMonth') {
        from.value = fmt(new Date(now.getFullYear(), now.getMonth() - 1, 1));
        to.value = fmt(new Date(now.getFullYear(), now.getMonth(), 0));
    }
    
    // Otomatik listele
    fetchDocumentList();
}

// Filter by search text
document.getElementById('searchFilter').addEventListener('input', (e) => {
    const q = e.target.value.toLowerCase();
    const filtered = allDocuments.filter(d =>
        (d.No || '').toLowerCase().includes(q) ||
        (d.CariUnvan || '').toLowerCase().includes(q) ||
        (d.VKN || '').includes(q)
    );
    renderTable(filtered);
});

function renderTable(docs) {
    const tbody = document.getElementById('docTableBody');
    const tableWrap = document.getElementById('listTableWrap');
    const emptyState = document.getElementById('listEmptyState');

    if (docs.length === 0) {
        tableWrap.classList.add('hidden');
        emptyState.classList.remove('hidden');
        document.getElementById('listEmptyState').querySelector('h2').innerText = 'Belge Bulunamadı';
        return;
    }

    tableWrap.classList.remove('hidden');
    emptyState.classList.add('hidden');
    document.getElementById('listCount').innerHTML = `<i class="fa-solid fa-list-ol"></i> ${docs.length} belge listelendi`;
    
    const fmt = (n) => n ? Number(n).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '0,00';

    tbody.innerHTML = docs.map(d => `
        <tr class="doc-row" onclick="viewDocument('${d.No}', '${currentType}')">
            <td style="text-align:center;" onclick="event.stopPropagation()">
                <input type="checkbox" class="doc-checkbox" data-no="${d.No}" ${selectedDocs.has(d.No) ? 'checked' : ''} onclick="toggleSelect('${d.No}')">
            </td>
            <td class="font-mono doc-no">${d.No || ''}</td>
            <td>${d.Tarih || ''}</td>
            <td class="cari">${d.CariUnvan || '---'}</td>
            <td class="vkn">${d.VKN || d.TCKN || '---'}</td>
            <td class="amount">${fmt(d.Net)}</td>
            <td class="amount kdv">${fmt(d.KDV)}</td>
            <td class="action-cell">
                <div style="display:flex; gap:5px; justify-content:center;">
                    <button class="btn-eye" onclick="event.stopPropagation(); viewDocument('${d.No}', '${currentType}')" title="Görüntüle">
                        <i class="fa-solid fa-eye"></i>
                    </button>
                    <button class="btn-eye" style="background:rgba(230,126,34,0.1); color:#e67e22;" onclick="event.stopPropagation(); sendQuickSms('${d.No}')" title="SMS Gönder">
                        <i class="fa-solid fa-comment-sms"></i>
                    </button>
                    <button class="btn-eye" style="background:rgba(52,152,219,0.1); color:#3498db;" onclick="event.stopPropagation(); sendQuickEmail('${d.No}', '${currentType}')" title="E-Posta Gönder">
                        <i class="fa-solid fa-envelope"></i>
                    </button>
                </div>
            </td>
        </tr>
    `).join('');
}

// ---- VIEW DOCUMENT ----
async function viewDocument(ficheNo, typeOverride = null) {
    const docType = typeOverride || currentType;
    listView.classList.add('hidden');
    previewView.classList.remove('hidden');
    document.getElementById('btnClosePreview').classList.remove('hidden');
    htmlContent.innerHTML = '<div class="loading-center"><i class="fa-solid fa-spinner fa-spin fa-2x"></i><br>Belge yükleniyor...</div>';
    document.getElementById('docNo').innerText = ficheNo;

    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/fetch-document`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify({ type: docType, ficheNo, dbConfig: getStoredDbConfig() })
        });
        const result = await res.json();
        if (!res.ok) throw new Error(result.error || 'Belge yüklenemedi');
        
        currentData = { ...result, _type: docType };
        document.getElementById('docDate').innerText = result.header.Tarih
            ? new Date(result.header.Tarih).toLocaleDateString('tr-TR') : '';
        
        // Type badge colors
        const typeMeta = { 
            fatura: ['e-FATURA', '#6d28d9'], 
            arsiv: ['e-ARŞİV', '#be185d'], 
            irsaliye: ['e-İRSALİYE', '#065f46'],
            siparis: ['SİPARİŞ FORMU', '#e67e22'],
            talep: ['TALEP FORMU', '#3498db']
        };
        const [label, color] = typeMeta[docType] || ['BELGE', '#64748b'];
        const badge = document.getElementById('docType');
        badge.innerText = label;
        badge.style.background = color + '22';
        badge.style.color = color;
        badge.style.borderColor = color + '44';

        await renderDocument(result, docType);
    } catch (err) {
        htmlContent.innerHTML = `<div style="color:var(--danger);padding:30px;text-align:center;">
            <i class="fa-solid fa-circle-exclamation" style="font-size:2rem;margin-bottom:15px;display:block;"></i>
            <b>${err.message}</b>
        </div>`;
    }
}

function closePreview() {
    previewView.classList.add('hidden');
    listView.classList.remove('hidden');
    document.getElementById('btnClosePreview').classList.add('hidden');
}

// ---- RENDER ENGINE ----
// UBL XML Builder - Logo Connect
function jsonToUbl(data, type) {
    const h = data.header;
    const lines = data.lines || [];
    const firma = h.FirmaBilgisi || {};
    
    // Ayarlardan gelen veriyi zorla (Force Company Info)
    const storedComp = JSON.parse(localStorage.getItem('logoViewerCompany') || '{}');
    if (storedComp.name) {
        firma.name = storedComp.name;
        firma.vkn = storedComp.vkn || firma.vkn;
        firma.addr = storedComp.addr || firma.addr;
        firma.city = storedComp.city || firma.city;
        firma.town = storedComp.town || firma.town;
        firma.taxOffice = storedComp.taxOffice || firma.taxOffice;
        firma.tel = storedComp.tel || firma.tel;
        firma.email = storedComp.email || firma.email;
    }
    
    // UBL Root and Namespaces
    const isDespatch = type === 'irsaliye';
    const root = isDespatch ? 'DespatchAdvice' : 'Invoice';
    const xmlns = isDespatch ? 'urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2' : 'urn:oasis:names:specification:ubl:schema:xsd:Invoice-2';
    
    const esc = (s) => String(s == null ? '' : s)
        .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
        
    const cur = h.ParaBirimi || 'TRY';
    const tarih = h.Tarih ? String(h.Tarih).split('T')[0] : '';
    const hasVkn = !!h.VKN;
    
    const profileId = isDespatch ? 'TEMELIRSALIYE' : (type === 'arsiv' ? 'EARSIVFATURA' : 'TICARIFATURA');
    const typeCode = isDespatch ? 'SEVK' : 'SATIS';
    const uuid = esc(h.EFaturaNo || '1' + Math.random().toString(16).slice(2, 33).padStart(31, '0'));

    let xml = `<?xml version="1.0" encoding="UTF-8"?>
<${root} xmlns="${xmlns}"
         xmlns:cac="urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"
         xmlns:cbc="urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"
         xmlns:ext="urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2">
  <cbc:UBLVersionID>2.1</cbc:UBLVersionID>
  <cbc:CustomizationID>TR1.2</cbc:CustomizationID>
  <cbc:ProfileID>${profileId}</cbc:ProfileID>
  <cbc:ID>${esc(h.No)}</cbc:ID>
  <cbc:UUID>${uuid}</cbc:UUID>
  <cbc:IssueDate>${tarih}</cbc:IssueDate>
  <cbc:IssueTime>12:00:00</cbc:IssueTime>
  <cbc:${root}TypeCode>${typeCode}</cbc:${root}TypeCode>
  <cbc:DocumentCurrencyCode>${esc(cur)}</cbc:DocumentCurrencyCode>`;

    if (h.Aciklama) xml += `\n  <cbc:Note>${esc(h.Aciklama)}</cbc:Note>`;

    // Rules
    if (h.RuleFields) {
        for (const [k, v] of Object.entries(h.RuleFields)) {
            if (k !== '__return') xml += `\n  <cbc:Note nameCode="${esc(k)}">${esc(String(v))}</cbc:Note>`;
        }
    }

    // SUPPLIER (Gönderen)
    const supTag = isDespatch ? 'DespatchSupplierParty' : 'AccountingSupplierParty';
    xml += `
  <cac:${supTag}><cac:Party>
    <cac:PartyName><cbc:Name>${esc(firma.name)}</cbc:Name></cac:PartyName>
    <cac:PostalAddress>
      <cbc:StreetName>${esc(firma.addr)}</cbc:StreetName>
      <cbc:BuildingName>${esc(firma.road || '')}</cbc:BuildingName>
      <cbc:BuildingNumber>${esc(firma.doorNr || '')}</cbc:BuildingNumber>
      <cbc:CitySubdivisionName>${esc(firma.town || '')}</cbc:CitySubdivisionName>
      <cbc:CityName>${esc(firma.city || '')}</cbc:CityName>
      <cbc:PostalZone>${esc(firma.zip || '')}</cbc:PostalZone>
      <cac:Country><cbc:IdentificationCode>TR</cbc:IdentificationCode><cbc:Name>TURKIYE</cbc:Name></cac:Country>
    </cac:PostalAddress>
    <cac:PartyTaxScheme>
      <cbc:CompanyID>${esc(firma.vkn)}</cbc:CompanyID>
      <cac:TaxScheme><cbc:Name>${esc(firma.taxOffice || 'VKN')}</cbc:Name></cac:TaxScheme>
    </cac:PartyTaxScheme>
    <cac:PartyLegalEntity>
      <cbc:RegistrationName>${esc(firma.name)}</cbc:RegistrationName>
      <cbc:CompanyID>${esc(firma.vkn)}</cbc:CompanyID>
    </cac:PartyLegalEntity>
    <cac:Contact>
      <cbc:Telephone>${esc(firma.tel || '')}</cbc:Telephone>
      <cbc:Telefax>${esc(firma.faks || '')}</cbc:Telefax>
      <cbc:ElectronicMail>${esc(firma.email || '')}</cbc:ElectronicMail>
    </cac:Contact>
  </cac:Party></cac:${supTag}>`;

    // CUSTOMER (Alıcı)
    const cusTag = isDespatch ? 'DeliveryCustomerParty' : 'AccountingCustomerParty';
    xml += `
  <cac:${cusTag}><cac:Party>
    <cac:PartyName><cbc:Name>${esc(h.CariUnvan)}</cbc:Name></cac:PartyName>
    <cac:PostalAddress>
      <cbc:StreetName>${esc(h.AliciAdres)}</cbc:StreetName>
      <cbc:BuildingName>${esc(h.AliciAdres2 || '')}</cbc:BuildingName>
      <cbc:CitySubdivisionName>${esc(h.AliciIlce || '')}</cbc:CitySubdivisionName>
      <cbc:CityName>${esc(h.AliciIl || '')}</cbc:CityName>
      <cbc:PostalZone>${esc(h.AliciPostakodu || '')}</cbc:PostalZone>
      <cac:Country><cbc:IdentificationCode>${esc(h.AliciUlke || 'TR')}</cbc:IdentificationCode><cbc:Name>TURKIYE</cbc:Name></cac:Country>
    </cac:PostalAddress>
    <cac:PartyTaxScheme>
      <cbc:CompanyID>${esc(hasVkn ? h.VKN : h.TCKN)}</cbc:CompanyID>
      <cac:TaxScheme><cbc:Name>${esc(h.VergiDairesi || (hasVkn ? 'VKN' : 'TCKN'))}</cbc:Name></cac:TaxScheme>
    </cac:PartyTaxScheme>
    <cac:PartyLegalEntity>
      <cbc:RegistrationName>${esc(h.CariUnvan)}</cbc:RegistrationName>
      <cbc:CompanyID>${esc(hasVkn ? h.VKN : h.TCKN)}</cbc:CompanyID>
    </cac:PartyLegalEntity>
    <cac:Contact><cbc:ElectronicMail>${esc(h.AliciEmail || '')}</cbc:ElectronicMail></cac:Contact>
  </cac:Party></cac:${cusTag}>`;

    if (isDespatch) {
        const sDate = h.SevkTarihi ? String(h.SevkTarihi).split('T')[0] : tarih;
        const sTime = h.SevkZamani ? (Number(h.SevkZamani) > 10000 
            ? String(h.SevkZamani).replace(/(..)(..)(..)/, '$1:$2:$3') 
            : '12:00:00') : '12:00:00';

        xml += `
  <cac:Shipment>
    <cac:Delivery>
      <cbc:ActualDespatchDate>${sDate}</cbc:ActualDespatchDate>
      <cbc:ActualDespatchTime>${sTime}</cbc:ActualDespatchTime>
      <cac:DeliveryAddress>
        <cbc:StreetName>${esc(h.AliciAdres)}</cbc:StreetName>
        <cbc:CitySubdivisionName>${esc(h.AliciIlce)}</cbc:CitySubdivisionName>
        <cbc:CityName>${esc(h.AliciIl)}</cbc:CityName>
        <cac:Country><cbc:Name>TURKIYE</cbc:Name></cac:Country>
      </cac:DeliveryAddress>
    </cac:Delivery>
    <cac:ShipmentStage>
      <cac:TransportMeans>
        <cac:RoadTransportMeans>
           <cbc:LicensePlateID>${esc(h.Plaka || '')}</cbc:LicensePlateID>
        </cac:RoadTransportMeans>
      </cac:TransportMeans>
      <cac:DriverPerson>
        <cbc:FirstName>${esc(h.SoforAd || '')}</cbc:FirstName>
        <cbc:FamilyName>${esc(h.SoforSoyad || '')}</cbc:FamilyName>
        <cac:IdentityDocumentReference><cbc:ID>${esc(h.SoforTCKN || '')}</cbc:ID></cac:IdentityDocumentReference>
      </cac:DriverPerson>
    </cac:ShipmentStage>
    <cac:TransportHandlingUnit>
       <cac:TransportMeans>
          <cac:RoadTransportMeans><cbc:LicensePlateID>${esc(h.DorsePlaka || '')}</cbc:LicensePlateID></cac:RoadTransportMeans>
       </cac:TransportMeans>
    </cac:TransportHandlingUnit>
  </cac:Shipment>`;

        if (h.TasiyiciVKN) {
            xml += `
  <cac:CarrierParty>
    <cac:PartyName><cbc:Name>${esc(h.TasiyiciUnvan || h.TasiyiciVKN)}</cbc:Name></cac:PartyName>
    <cac:PartyTaxScheme>
      <cbc:CompanyID>${esc(h.TasiyiciVKN)}</cbc:CompanyID>
      <cac:TaxScheme><cbc:Name>KDV</cbc:Name></cac:TaxScheme>
    </cac:PartyTaxScheme>
  </cac:CarrierParty>`;
        }
    }

    if (!isDespatch) {
        xml += `
  <cac:TaxTotal><cbc:TaxAmount currencyID="${esc(cur)}">${h.KDV || 0}</cbc:TaxAmount></cac:TaxTotal>
  <cac:LegalMonetaryTotal>
    <cbc:LineExtensionAmount currencyID="${esc(cur)}">${h.Net || 0}</cbc:LineExtensionAmount>
    <cbc:TaxExclusiveAmount currencyID="${esc(cur)}">${h.Net || 0}</cbc:TaxExclusiveAmount>
    <cbc:TaxInclusiveAmount currencyID="${esc(cur)}">${h.Brut || 0}</cbc:TaxInclusiveAmount>
    <cbc:PayableAmount currencyID="${esc(cur)}">${h.Brut || 0}</cbc:PayableAmount>
  </cac:LegalMonetaryTotal>`;
    }

    // Exchange Rates
    const rate = Number(h.Kur || 1).toFixed(4);
    if (cur !== 'TRY' && cur !== 'TRL') {
        xml += `
  <cac:TaxExchangeRate>
    <cbc:SourceCurrencyCode>${cur}</cbc:SourceCurrencyCode>
    <cbc:TargetCurrencyCode>TRY</cbc:TargetCurrencyCode>
    <cbc:CalculationRate>${rate}</cbc:CalculationRate>
  </cac:TaxExchangeRate>`;
    }

    // Lines
    lines.forEach((l, i) => {
        const ln = isDespatch ? 'Despatch' : 'Invoice';
        const qn = isDespatch ? 'Delivered' : 'Invoiced';
        const lineTotal = Number(l.SatirToplam || 0).toFixed(4);
        const kdvTutar = Number(l.SatirToplam * (l.KdvOran || 0) / 100).toFixed(4);
        
        xml += `
  <cac:${ln}Line>
    <cbc:ID>${i + 1}</cbc:ID>
    <cbc:Note>${esc(l.Aciklama || '')}</cbc:Note>
    <cbc:${qn}Quantity unitCode="${esc(l.Birim || 'ADT')}">${Number(l.Miktar || 0).toFixed(4)}</cbc:${qn}Quantity>
    <cac:Item>
      <cbc:Description>${esc(l.Aciklama || '')}</cbc:Description>
      <cbc:Name>${esc(l.Ad || l.Aciklama || 'Bilinmeyen')}</cbc:Name>
      <cac:SellersItemIdentification><cbc:ID>${esc(l.StokKodu || '')}</cbc:ID></cac:SellersItemIdentification>
    </cac:Item>`;
        
        if (!isDespatch) {
            xml += `
    <cbc:LineExtensionAmount currencyID="${esc(cur)}">${lineTotal}</cbc:LineExtensionAmount>
    <cac:TaxTotal>
      <cbc:TaxAmount currencyID="${esc(cur)}">${kdvTutar}</cbc:TaxAmount>
      <cac:TaxSubtotal>
        <cbc:TaxableAmount currencyID="${esc(cur)}">${lineTotal}</cbc:TaxableAmount>
        <cbc:TaxAmount currencyID="${esc(cur)}">${kdvTutar}</cbc:TaxAmount>
        <cbc:Percent>${l.KdvOran || 0}</cbc:Percent>
        <cac:TaxCategory><cac:TaxScheme><cbc:Name>KDV</cbc:Name></cac:TaxScheme></cac:TaxCategory>
      </cac:TaxSubtotal>
    </cac:TaxTotal>
    <cac:Price><cbc:PriceAmount currencyID="${esc(cur)}">${Number(l.Fiyat || 0).toFixed(6)}</cbc:PriceAmount></cac:Price>`;
        }
        
        xml += `\n  </cac:${ln}Line>`;
    });

    xml += `\n</${root}>`;
    return xml;
}

async function renderDocument(data, type) {
    const docType = type || currentType;
    const xsltPath = selectedXslts[docType];
    const iframe = document.getElementById('previewIframe');
    const h = data.header;
    
    // Tarayıcı sekme başlığını düzelt (yazdırma için)
    document.title = `${h.No} - ${h.CariUnvan || ''}`;
    
    // Banner temizliği (Eski uyarıları kaldır)
    const oldBanners = document.querySelectorAll('.warning-banner');
    oldBanners.forEach(b => b.remove());

    // Sipariş ve Talep her zaman varsayılan (özel) görseli kullanmalı
    if (docType === 'siparis' || docType === 'talep') {
        console.log(`[RENDER] ${docType} için özel görsel yükleniyor...`);
        await renderFallback(data, docType);
        return;
    }

    if (!xsltPath) {
        const typeLabels = { fatura: 'e-Fatura', arsiv: 'e-Arşiv', irsaliye: 'e-İrsaliye', siparis: 'Sipariş', talep: 'Talep' };
        const label = typeLabels[docType] || docType;
        const msg = `<b>${label}</b> için XSLT tasarımı seçilmemiş — varsayılan görünüm kullanılıyor.`;
        
        const noXsltBanner = `<div class="warning-banner" style="background:rgba(109,40,217,0.1);border:1px solid rgba(109,40,217,0.3);border-radius:10px;padding:10px 16px;margin-bottom:12px;display:flex;align-items:center;gap:12px;font-size:0.8rem;color:var(--accent-light);">
            <i class="fa-solid fa-circle-info"></i>
            <span>${msg}</span>
            <button onclick="document.getElementById('loginModal').classList.add('show'); showTab('designs')" 
                style="margin-left:auto;padding:5px 12px;background:var(--accent);color:white;border:none;border-radius:7px;cursor:pointer;font-size:0.78rem;flex-shrink:0;">Tasarım Seç</button>
        </div>`;
        htmlContent.insertAdjacentHTML('beforebegin', noXsltBanner);
        await renderFallback(data, docType);
        return;
    }
    
    // XSLT dosyası adını kısa göster
    const xsltFileName = xsltPath.split('\\').pop().split('/').pop();
    console.log(`[RENDER] Tip: ${docType}, XSLT: "${xsltPath}"`);
    
    try {
        htmlContent.innerHTML = `<div class="loading-center"><i class="fa-solid fa-spinner fa-spin fa-2x"></i><br>Tasarım uygulanıyor...<br><small style="opacity:0.5;">${xsltFileName}</small></div>`;
        
        const xsltRes = await fetch(`${CONFIG.backendUrl}/api/get-xslt-raw`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify({ path: xsltPath })
        });
        if (!xsltRes.ok) {
            const errJson = await xsltRes.json().catch(() => ({}));
            throw new Error(`XSLT bulunamadı: ${xsltPath}`);
        }
        const xsltText = await xsltRes.text();
        const ublXml = jsonToUbl(data, docType);
        
        console.log('[UBL XML]', ublXml.substring(0, 500));
        
        const parser = new DOMParser();
        const xmlDoc = parser.parseFromString(ublXml, 'text/xml');
        const xsltDoc = parser.parseFromString(xsltText, 'text/xml');
        
        const xmlErr = xmlDoc.getElementsByTagName('parsererror');
        const xslErr = xsltDoc.getElementsByTagName('parsererror');
        if (xmlErr.length > 0) throw new Error('UBL Parse Hatası');
        if (xslErr.length > 0) throw new Error('XSLT Parse Hatası');
        
        const processor = new XSLTProcessor();
        processor.importStylesheet(xsltDoc);
        const resultDoc = processor.transformToFragment(xmlDoc, document);
        
        // Bilgi çubuğu
        htmlContent.innerHTML = `<div style="background:rgba(16,185,129,0.08);border:1px solid rgba(16,185,129,0.2);border-radius:10px;padding:8px 16px;margin-bottom:10px;display:flex;align-items:center;gap:10px;font-size:0.75rem;color:#10b981;">
            <i class="fa-solid fa-palette"></i><span>Tasarım: <b>${xsltFileName}</b></span>
        </div>`;
        
        // iframe'e yaz (CSS izolasyonu)
        const resultDiv = document.createElement('div');
        resultDiv.appendChild(resultDoc);
        const iframeContent = `<!DOCTYPE html><html><head><meta charset="UTF-8"><title>${h.No}</title>
            <style>body{margin:0;padding:10px;background:white;} @media print { .no-printer-header { display:none!important; } }</style>
        </head><body>${resultDiv.innerHTML}</body></html>`;
        
        const pif = document.getElementById('previewIframe');
        pif.style.display = 'block';
        pif.srcdoc = iframeContent;
        
        // iframe yüklendikten sonra yüksekliği ayarla
        pif.onload = () => {
            try {
                const body = pif.contentDocument.body;
                pif.style.height = (body.scrollHeight + 50) + 'px';
            } catch(e) { pif.style.height = '800px'; }
        };
        return;
    } catch (e) {
        console.error('[XSLT HATA]', e);
        htmlContent.innerHTML = `
            <div style="color:var(--danger);padding:20px;background:rgba(244,63,94,0.08);border-radius:12px;border:1px solid rgba(244,63,94,0.2);">
                <div style="display:flex;align-items:center;gap:10px;margin-bottom:12px;">
                    <i class="fa-solid fa-triangle-exclamation fa-lg"></i>
                    <b>XSLT Render Hatası – ${xsltFileName}</b>
                </div>
                <div style="font-size:0.85rem;color:rgba(255,255,255,0.7);background:rgba(0,0,0,0.3);padding:12px;border-radius:8px;font-family:monospace;margin-bottom:14px;line-height:1.6;white-space:pre-wrap;">
                    ${e.message}
                </div>
                <div style="display:flex;gap:10px;flex-wrap:wrap;">
                    <button onclick="renderFallback(currentData)" 
                        style="padding:8px 18px;background:var(--accent);color:white;border:none;border-radius:8px;cursor:pointer;font-size:0.82rem;">
                        <i class="fa-solid fa-eye-slash"></i> Fallback Görünüm
                    </button>
                    <button onclick="document.getElementById('loginModal').classList.add('show')"
                        style="padding:8px 18px;background:rgba(255,255,255,0.1);color:white;border:1px solid var(--glass-border);border-radius:8px;cursor:pointer;font-size:0.82rem;">
                        <i class="fa-solid fa-gear"></i> Tasarım Değiştir
                    </button>
                </div>
            </div>`;
        document.getElementById('previewIframe').style.display = 'none';
    }
}

// ---- PDF/PRINT ----
function printPreview() {
    const pif = document.getElementById('previewIframe');
    if (pif && pif.contentWindow && pif.srcdoc) {
        pif.contentWindow.print();
    } else {
        // Fallback: print iframe content in new window
        const fallbackHtml = htmlContent.innerHTML;
        const w = window.open('', '_blank');
        w.document.write(`<!DOCTYPE html><html><head><meta charset="UTF-8"><title>Fatura Çıktı</title></head><body>${fallbackHtml}</body></html>`);
        w.document.close();
        w.print();
    }
}

async function renderFallback(data, type) {
    const docType = type || currentType;
    const h = data.header;
    const f = h.FirmaBilgisi || {};
    
    // Premium Fallback Design for Sipariş and Talep
    if (docType === 'siparis' || docType === 'talep') {
        const linesHtml = (data.lines || []).map((l, i) => `
            <tr style="border-bottom: 1px solid #f1f5f9;">
                <td style="padding:12px; font-size:0.85rem;">${i+1}</td>
                <td style="padding:12px; font-size:0.85rem;">
                    <div style="font-weight:600;">${l.Ad || ''}</div>
                    <div style="font-size:0.75rem; color:#64748b;">${l.StokKodu || ''}</div>
                </td>
                <td style="padding:12px; font-size:0.85rem; text-align:center;">${l.Miktar} ${l.Birim || ''}</td>
                <td style="padding:12px; font-size:0.85rem; text-align:right;">${Number(l.Fiyat || 0).toLocaleString('tr-TR', {minimumFractionDigits:2})}</td>
                <td style="padding:12px; font-size:0.85rem; text-align:right; font-weight:600;">${Number(l.SatirToplam || 0).toLocaleString('tr-TR', {minimumFractionDigits:2})}</td>
            </tr>`).join('');

        const html = `
        <div style="max-width:800px; margin:20px auto; background:white; padding:40px; border-radius:12px; box-shadow:0 10px 25px rgba(0,0,0,0.05); color:#1e293b; line-height:1.5;">
            <div style="display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:40px; border-bottom:2px solid #f1f5f9; padding-bottom:20px;">
                <div>
                    <div style="font-size:1.5rem; font-weight:800; color:#3b82f6; margin-bottom:8px;">${f.name || 'Firma Ünvanı'}</div>
                    <div style="font-size:0.85rem; color:#64748b;">
                        ${f.addr || ''} ${f.town || ''} / ${f.city || ''}<br>
                        ${f.vkn ? `VKN: ${f.vkn}` : ''} ${f.taxOffice ? ` — ${f.taxOffice}` : ''}<br>
                        ${f.tel ? `T: ${f.tel}` : ''} ${f.email ? ` — ${f.email}` : ''}
                    </div>
                </div>
                <div style="text-align:right;">
                    <div style="font-size:1.2rem; font-weight:700; color:#1e293b; text-transform:uppercase;">${docType === 'siparis' ? 'SİPARİŞ FİŞİ' : 'TALEP FİŞİ'}</div>
                    <div style="font-size:1.1rem; font-weight:600; color:#3b82f6; margin-top:5px;">${h.No}</div>
                    <div style="font-size:0.9rem; color:#64748b; margin-top:5px;">Tarih: ${h.Tarih ? new Date(h.Tarih).toLocaleDateString('tr-TR') : ''}</div>
                </div>
            </div>

            <div style="display:grid; grid-template-columns: 1fr 1fr; gap:40px; margin-bottom:40px;">
                <div>
                    <div style="font-size:0.75rem; font-weight:700; color:#94a3b8; text-transform:uppercase; margin-bottom:10px; border-bottom:1px solid #f1f5f9; padding-bottom:5px;">MÜŞTERİ BİLGİLERİ</div>
                    <div style="font-size:0.95rem; font-weight:700; margin-bottom:5px;">${h.CariUnvan || ''}</div>
                    <div style="font-size:0.85rem; color:#64748b;">
                        ${h.AliciAdres || ''}<br>
                        ${h.AliciIlce || ''} / ${h.AliciIl || ''}<br>
                        ${h.VKN || h.TCKN ? `VKN/TCKN: ${h.VKN || h.TCKN}` : ''}
                    </div>
                </div>
                ${h.Aciklama ? `
                <div>
                    <div style="font-size:0.75rem; font-weight:700; color:#94a3b8; text-transform:uppercase; margin-bottom:10px; border-bottom:1px solid #f1f5f9; padding-bottom:5px;">AÇIKLAMA</div>
                    <div style="font-size:0.85rem; color:#64748b;">${h.Aciklama}</div>
                </div>` : ''}
            </div>

            <table style="width:100%; border-collapse:collapse; margin-bottom:30px;">
                <thead>
                    <tr style="background:#f8fafc; color:#64748b; font-size:0.75rem; text-transform:uppercase; font-weight:700;">
                        <th style="padding:12px; text-align:left; border-radius:8px 0 0 8px;">#</th>
                        <th style="padding:12px; text-align:left;">Ürün / Açıklama</th>
                        <th style="padding:12px; text-align:center;">Miktar</th>
                        <th style="padding:12px; text-align:right;">Birim Fiyat</th>
                        <th style="padding:12px; text-align:right; border-radius:0 8px 8px 0;">Toplam</th>
                    </tr>
                </thead>
                <tbody>
                    ${linesHtml}
                </tbody>
            </table>

            <div style="display:flex; justify-content:flex-end;">
                <div style="width:250px; background:#f8fafc; padding:20px; border-radius:12px;">
                    <div style="display:flex; justify-content:space-between; margin-bottom:10px; font-size:0.9rem;">
                        <span style="color:#64748b;">Ara Toplam</span>
                        <span style="font-weight:600;">${Number(h.Net || 0).toLocaleString('tr-TR', {minimumFractionDigits:2})}</span>
                    </div>
                    <div style="display:flex; justify-content:space-between; margin-bottom:15px; font-size:0.9rem; padding-bottom:10px; border-bottom:1px solid #e2e8f0;">
                        <span style="color:#64748b;">KDV Toplam</span>
                        <span style="font-weight:600;">${Number(h.KDV || 0).toLocaleString('tr-TR', {minimumFractionDigits:2})}</span>
                    </div>
                    <div style="display:flex; justify-content:space-between; align-items:center;">
                        <span style="font-size:0.8rem; font-weight:700; color:#64748b; text-transform:uppercase;">GENEL TOPLAM</span>
                        <span style="font-size:1.25rem; font-weight:800; color:#3b82f6;">${Number(h.Brut || 0).toLocaleString('tr-TR', {minimumFractionDigits:2})} TL</span>
                    </div>
                </div>
            </div>
            
            <div style="margin-top:60px; border-top:1px solid #f1f5f9; padding-top:20px; text-align:center; font-size:0.75rem; color:#94a3b8;">
                Bu belge dijital ortamda Debisoft Mail ve Sms servisi ile oluşturulmuştur.
            </div>
        </div>`;
        
        const pif = document.getElementById('previewIframe');
        pif.style.display = 'block';
        pif.srcdoc = `<!DOCTYPE html><html><head><meta charset="UTF-8">
            <style>body{margin:0;padding:20px;background:#f1f5f9;font-family:'Inter', sans-serif;}</style>
            <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700;800&display=swap" rel="stylesheet">
        </head><body>${html}</body></html>`;
        pif.onload = () => {
            try { pif.style.height = (pif.contentDocument.body.scrollHeight + 50) + 'px'; } catch(e) {}
        };
        return;
    }

    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/render-view`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify({ data, type: docType })
        });
        const htmlText = await res.text();
        htmlContent.innerHTML = '';
        const pif = document.getElementById('previewIframe');
        pif.style.display = 'block';
        pif.srcdoc = `<!DOCTYPE html><html><head><meta charset="UTF-8">
            <style>body{margin:0;padding:0;background:white;font-family:Arial,sans-serif;}</style>
        </head><body>${htmlText}</body></html>`;
        pif.onload = () => {
            try { pif.style.height = (pif.contentDocument.body.scrollHeight + 30) + 'px'; } catch(e) {}
        };
    } catch (err) {
        htmlContent.innerHTML = `<div style="color:var(--danger);padding:20px;">Hata: ${err.message}</div>`;
    }
}

// ---- MANUAL MODE ----
function populateManualXsltSelect() {
    const sel = document.getElementById('manualXsltSelect');
    if (!sel) return;
    while (sel.options.length > 1) sel.remove(1);
    xsltList.forEach(x => {
        const opt = document.createElement('option');
        opt.value = x.path;
        opt.textContent = `[${(x.type || '?').toUpperCase()}] ${x.name}`;
        sel.appendChild(opt);
    });
}

document.getElementById('xsltFileInput').addEventListener('change', (e) => {
    const f = e.target.files[0];
    if (f) {
        document.getElementById('manualXsltFileName').textContent = '📄 ' + f.name;
        document.getElementById('manualXsltFileName').style.display = 'block';
        document.getElementById('manualXsltSelect').value = '';
    }
});

document.getElementById('btnRenderManual').addEventListener('click', async () => {
    const xmlFile = document.getElementById('xmlFileInput').files[0];
    const xsltFileInput = document.getElementById('xsltFileInput').files[0];
    const selectVal = document.getElementById('manualXsltSelect').value;
    const extraDataText = document.getElementById('manualDataInput').value.trim();

    if (!xmlFile) return alert('Lütfen bir XML dosyası seçin!');
    
    manualHtmlContent.innerHTML = '<div class="loading-center"><i class="fa-solid fa-spinner fa-spin fa-2x"></i><br>İşleniyor...</div>';

    try {
        let xmlText = await xmlFile.text();
        
        // JSON → cbc:Note enjeksiyonu
        if (extraDataText) {
            try {
                const extraData = JSON.parse(extraDataText);
                let notes = '';
                for (const [k, v] of Object.entries(extraData)) {
                    notes += `<cbc:Note nameCode="${k}">${v}</cbc:Note>\n`;
                }
                for (const tag of ['</Invoice>', '</DespatchAdvice>', '</CreditNote>']) {
                    if (xmlText.includes(tag)) { xmlText = xmlText.replace(tag, `${notes}${tag}`); break; }
                }
            } catch (e) { alert('JSON Hatası: ' + e.message); }
        }

        // XML Render
        const parser = new DOMParser();
        const xmlDoc = parser.parseFromString(xmlText, 'text/xml');

        let xsltDoc = null;
        if (selectVal) {
            const r = await fetch(`${CONFIG.backendUrl}/api/get-xslt-raw`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
                body: JSON.stringify({ path: selectVal })
            });
            if (!r.ok) throw new Error('Kayıtlı tasarım okunamadı');
            xsltDoc = parser.parseFromString(await r.text(), 'text/xml');
        } else if (xsltFileInput) {
            xsltDoc = parser.parseFromString(await xsltFileInput.text(), 'text/xml');
        }

        if (xsltDoc) {
            if (xsltDoc.getElementsByTagName('parsererror').length > 0) throw new Error('XSLT parse hatası');
            const processor = new XSLTProcessor();
            processor.importStylesheet(xsltDoc);
            const result = processor.transformToFragment(xmlDoc, document);
            manualHtmlContent.innerHTML = '';
            manualHtmlContent.appendChild(result);
        } else {
            manualHtmlContent.innerHTML = `<pre style="color:#a5b4fc;padding:20px;overflow:auto;">${xmlText.replace(/</g,'&lt;')}</pre>`;
        }

        const docNo = xmlDoc.getElementsByTagName('cbc:ID')[0]?.textContent || 'MANUEL';
        const docDate = xmlDoc.getElementsByTagName('cbc:IssueDate')[0]?.textContent || '';
        document.getElementById('docNo').innerText = docNo;
        document.getElementById('docDate').innerText = docDate;
    } catch (err) {
        manualHtmlContent.innerHTML = `<div style="color:var(--danger);padding:20px;background:rgba(244,63,94,0.1);border-radius:12px;"><b>⚠ Hata:</b> ${err.message}</div>`;
    }
});

// ---- SETTINGS ----
const loginModal = document.getElementById('loginModal');
const settingsModal = document.getElementById('settingsModal');

logoTrigger.addEventListener('dblclick', () => loginModal.classList.add('show'));
document.getElementById('btnSettingsOpen').addEventListener('click', () => loginModal.classList.add('show'));

document.getElementById('btnLogin').addEventListener('click', () => {
    if (document.getElementById('adminPass').value === 'admin') {
        loginModal.classList.remove('show');
        document.getElementById('adminPass').value = '';
        loadConfigAndOpen();
    } else {
        alert('Hatalı Şifre!');
        document.getElementById('adminPass').value = '';
    }
});
document.getElementById('adminPass').addEventListener('keydown', e => { if (e.key === 'Enter') document.getElementById('btnLogin').click(); });

function switchTab(tabId) {
    document.querySelectorAll('.tab-content').forEach(t => t.classList.add('hidden'));
    document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    document.getElementById('tab-' + tabId).classList.remove('hidden');
    event.currentTarget.classList.add('active');
}

async function loadConfigAndOpen() {
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/get-config`, { headers: { 'x-api-key': CONFIG.apiKey } });
        const cfg = await res.json();
        xsltList = cfg.xsltList || [];
        rules = cfg.rules || [];
        selectedXslts = { fatura: '', arsiv: '', irsaliye: '', siparis: '', talep: '', ...(cfg.selectedXslts || {}) };
        console.log('[CONFIG] selectedXslts:', selectedXslts);
        
        displayXsltList();
        displayRuleList();

        const db = getStoredDbConfig();
        const comp = cfg.companyInfo || JSON.parse(localStorage.getItem('logoViewerCompany') || '{}');
        
        document.getElementById('sqlServer').value = db.server || '';
        document.getElementById('sqlDb').value = db.database || '';
        document.getElementById('sqlUser').value = db.user || '';
        document.getElementById('sqlPass').value = db.password || '';
        document.getElementById('sqlFirma').value = db.firma || '';
        document.getElementById('sqlDonem').value = db.donem || '';
        document.getElementById('compName').value = comp.name || '';
        document.getElementById('compVkn').value = comp.vkn || '';
        document.getElementById('compCity').value = comp.city || '';
        document.getElementById('compTown').value = comp.town || '';
        document.getElementById('compAddr').value = comp.addr || '';
        document.getElementById('compTaxOffice').value = comp.taxOffice || '';
        document.getElementById('compTel').value = comp.tel || '';
        document.getElementById('compFax').value = comp.fax || '';
        document.getElementById('compEmail').value = comp.email || '';

        // Notification Settings
        const nSet = cfg.notificationSettings || {};
        document.getElementById('smsUser').value = nSet.sms?.usercode || '';
        document.getElementById('smsPass').value = nSet.sms?.password || '';
        document.getElementById('smsHeader').value = nSet.sms?.msgheader || '';
        document.getElementById('smtpHost').value = nSet.email?.host || '';
        document.getElementById('smtpPort').value = nSet.email?.port || '587';
        document.getElementById('smtpUser').value = nSet.email?.user || '';
        document.getElementById('smtpPass').value = nSet.email?.pass || '';
        document.getElementById('smtpFrom').value = nSet.email?.from || '';
        document.getElementById('emailMethod').value = nSet.email?.method || 'smtp';
        
        toggleEmailMethodUI(nSet.email?.method || 'smtp');
        if (nSet.email?.method === 'outlook') {
            await loadOutlookAccounts(nSet.email?.outlookAccount);
        }

        const firmLabel = `Firma: ${db.firma || '---'}${comp.name ? ' – ' + comp.name : ''}`;
        document.getElementById('activeFirmDisplay').innerHTML = `<i class="fa-solid fa-building"></i> <span>${firmLabel}</span>`;
        if (comp.name) CONFIG.companyName = comp.name;

        settingsModal.classList.add('show');
    } catch (e) { alert('Konfigürasyon yüklenemedi: ' + e.message); }
}

// Test DB button
document.getElementById('btnTestDb').addEventListener('click', async () => {
    const btn = document.getElementById('btnTestDb');
    const result = document.getElementById('dbTestResult');
    btn.disabled = true; btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Test ediliyor...';
    result.style.display = 'none';
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/test-db`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ dbConfig: {
                server: document.getElementById('sqlServer').value,
                database: document.getElementById('sqlDb').value,
                user: document.getElementById('sqlUser').value,
                password: document.getElementById('sqlPass').value
            }})
        });
        const data = await res.json();
        result.style.display = 'block';
        if (data.success) {
            result.style.background = 'rgba(16,185,129,0.1)'; result.style.color = '#10b981';
            result.innerHTML = '<i class="fa-solid fa-circle-check"></i> Bağlantı başarılı! Firmalar yüklendi.';
            loadFirms(); // Bağlantı başarılıysa firmaları yükle
        } else {
            result.style.background = 'rgba(244,63,94,0.1)'; result.style.color = '#f43f5e';
            result.innerHTML = '<i class="fa-solid fa-circle-xmark"></i> Hata: ' + (data.error || 'Bağlantı başarısız');
        }
    } catch (e) {
        result.style.display = 'block'; result.style.background = 'rgba(244,63,94,0.1)'; result.style.color = '#f43f5e';
        result.innerHTML = '<i class="fa-solid fa-circle-xmark"></i> ' + e.message;
    }
    btn.disabled = false; btn.innerHTML = '<i class="fa-solid fa-plug-circle-bolt"></i> Bağlantıyı Test Et';
});

// Save Settings
document.getElementById('btnSaveSettings').addEventListener('click', async () => {
    const dbConfig = {
        server: document.getElementById('sqlServer').value,
        database: document.getElementById('sqlDb').value,
        user: document.getElementById('sqlUser').value,
        password: document.getElementById('sqlPass').value,
        firma: document.getElementById('sqlFirma').value,
        donem: document.getElementById('sqlDonem').value
    };
    const companyInfo = {
        name: document.getElementById('compName').value,
        vkn: document.getElementById('compVkn').value,
        city: document.getElementById('compCity').value,
        town: document.getElementById('compTown').value,
        addr: document.getElementById('compAddr').value,
        taxOffice: document.getElementById('compTaxOffice').value,
        tel: document.getElementById('compTel').value,
        fax: document.getElementById('compFax').value,
        email: document.getElementById('compEmail').value
    };
    
    const notificationSettings = {
        sms: {
            usercode: document.getElementById('smsUser').value,
            password: document.getElementById('smsPass').value,
            msgheader: document.getElementById('smsHeader').value
        },
        email: {
            host: document.getElementById('smtpHost').value,
            port: document.getElementById('smtpPort').value,
            user: document.getElementById('smtpUser').value,
            pass: document.getElementById('smtpPass').value,
            from: document.getElementById('smtpFrom').value,
            method: document.getElementById('emailMethod').value,
            outlookAccount: document.getElementById('outlookAccount').value
        }
    };
    
    localStorage.setItem('logoViewerDb', JSON.stringify(dbConfig));
    
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/save-settings`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ dbConfig, companyInfo, xsltList, rules, selectedXslts, notificationSettings })
        });
        const result = await res.json();
        if (result.success) {
            if (result.company) {
                localStorage.setItem('logoViewerCompany', JSON.stringify(result.company));
                CONFIG.companyName = result.company.name;
                const firmLabel = `Firma: ${dbConfig.firma}${result.company.name ? ' – ' + result.company.name : ''}`;
                document.getElementById('activeFirmDisplay').innerHTML = `<i class="fa-solid fa-building"></i> <span>${firmLabel}</span>`;
                if (result.company.name) {
                    document.getElementById('compName').value = result.company.name;
                    document.getElementById('compVkn').value = result.company.vkn || '';
                    document.getElementById('compCity').value = result.company.city || '';
                    document.getElementById('compTown').value = result.company.town || '';
                    document.getElementById('compAddr').value = result.company.addr || '';
                    document.getElementById('compTaxOffice').value = result.company.taxOffice || '';
                    document.getElementById('compTel').value = result.company.tel || '';
                    document.getElementById('compFax').value = result.company.fax || '';
                    document.getElementById('compEmail').value = result.company.email || '';
                }
            }
            settingsModal.classList.remove('show');
            alert('✅ Ayarlar kaydedildi!');
        } else { alert('Kayıt Hatası: ' + JSON.stringify(result)); }
    } catch (e) { alert('Kayıt Hatası: ' + e.message); }
});

function getStoredDbConfig() {
    return JSON.parse(localStorage.getItem('logoViewerDb') || '{}');
}

// ---- XSLT MANAGEMENT ----
document.getElementById('btnBrowseXslt').addEventListener('click', () => document.getElementById('xsltFileDialog').click());
document.getElementById('xsltFileDialog').addEventListener('change', async () => {
    const file = document.getElementById('xsltFileDialog').files[0];
    if (!file) return;
    const formData = new FormData();
    formData.append('xslt', file);
    try {
        const btn = document.getElementById('btnBrowseXslt');
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i>';
        const res = await fetch(`${CONFIG.backendUrl}/api/upload-xslt`, {
            method: 'POST', headers: { 'x-api-key': CONFIG.apiKey }, body: formData
        });
        const result = await res.json();
        if (result.path) {
            document.getElementById('xsltPathInput').value = result.path;
            btn.innerHTML = '<i class="fa-solid fa-check" style="color:var(--success)"></i>';
            setTimeout(() => btn.innerHTML = '<i class="fa-solid fa-folder-open"></i>', 2000);
        } else throw new Error(result.error);
    } catch (e) { alert('Yükleme Hatası: ' + e.message); document.getElementById('btnBrowseXslt').innerHTML = '<i class="fa-solid fa-folder-open"></i>'; }
});

document.getElementById('btnAddXslt').addEventListener('click', () => {
    const type = document.getElementById('xsltTypeInput').value;
    const name = document.getElementById('xsltNameInput').value.trim();
    const path = document.getElementById('xsltPathInput').value.trim();
    if (!name || !path) return alert('Tasarım adı ve yolu gerekli!');
    xsltList.push({ type, name, path });
    document.getElementById('xsltNameInput').value = '';
    document.getElementById('xsltPathInput').value = '';
    displayXsltList();
});

function displayXsltList() {
    const list = document.getElementById('xsltList');
    const typeColors = { fatura: '#6d28d9', arsiv: '#ec4899', irsaliye: '#10b981', siparis: '#e67e22', talep: '#3498db' };
    const typeLabels = { fatura: '🟣 e-Fatura', arsiv: '🩷 e-Arşiv', irsaliye: '🟢 e-İrsaliye', siparis: '🟠 Sipariş', talep: '🔵 Talep' };
    list.innerHTML = xsltList.map((x, i) => {
        const isSelected = selectedXslts[x.type] === x.path;
        const col = typeColors[x.type] || '#64748b';
        return `<div class="list-item" style="border-left:4px solid ${col};">
            <div class="info">
                <div style="font-size:0.6rem;text-transform:uppercase;color:var(--text-dim)">${typeLabels[x.type] || x.type}</div>
                <div class="name">${x.name}</div>
                <div class="path" style="font-size:0.65rem;color:var(--text-dim);overflow:hidden;text-overflow:ellipsis;white-space:nowrap;max-width:300px;">${x.path}</div>
            </div>
            <div class="actions">
                <button class="btn-primary" style="padding:4px 10px;font-size:0.7rem;${isSelected ? 'background:var(--success);' : ''}" onclick="selectXslt(${i})">
                    ${isSelected ? '<i class="fa-solid fa-check"></i> Seçildi' : 'Varsayılan Yap'}
                </button>
                <button class="btn-del" onclick="removeXslt(${i})"><i class="fa-solid fa-trash"></i></button>
            </div>
        </div>`;
    }).join('');
    populateManualXsltSelect();
}

window.selectXslt = (i) => { selectedXslts[xsltList[i].type] = xsltList[i].path; displayXsltList(); };
window.removeXslt = (i) => { xsltList.splice(i, 1); displayXsltList(); };

// ---- RULE MANAGEMENT ----
let activeRuleIndex = null;
document.getElementById('btnAddRule').addEventListener('click', () => {
    rules.push({ name: 'Yeni Kural', code: '// App.Doc.SetField("ALAN", değer) kullanın' });
    displayRuleList();
    editRule(rules.length - 1);
});

function displayRuleList() {
    const list = document.getElementById('ruleList');
    list.innerHTML = rules.map((r, i) => `
        <div class="list-item" onclick="editRule(${i})" style="cursor:pointer;${activeRuleIndex === i ? 'border-color:var(--accent);' : ''}">
            <span class="name">${r.name}</span>
            <button class="btn-del" onclick="event.stopPropagation();removeRule(${i})"><i class="fa-solid fa-trash"></i></button>
        </div>`).join('');
}

window.editRule = (i) => {
    activeRuleIndex = i;
    displayRuleList();
    document.getElementById('ruleEditor').classList.remove('hidden');
    document.getElementById('ruleNameInput').value = rules[i].name;
    document.getElementById('ruleCodeInput').value = rules[i].code;
};

document.getElementById('btnSaveRule').addEventListener('click', () => {
    if (activeRuleIndex === null) return;
    rules[activeRuleIndex].name = document.getElementById('ruleNameInput').value;
    rules[activeRuleIndex].code = document.getElementById('ruleCodeInput').value;
    displayRuleList();
});

window.removeRule = (i) => {
    rules.splice(i, 1);
    activeRuleIndex = null;
    document.getElementById('ruleEditor').classList.add('hidden');
    displayRuleList();
};

// ---- BACKEND STATUS CHECK ----
async function checkBackendStatus() {
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/get-config`, { headers: { 'x-api-key': CONFIG.apiKey } });
        const badge = document.getElementById('connStatus');
        if (res.ok) {
            badge.innerHTML = '<span class="dot"></span> <span>Backend: Bağlı</span>';
            badge.style.borderColor = 'rgba(16,185,129,0.3)';
        } else throw new Error();
    } catch {
        const badge = document.getElementById('connStatus');
        badge.innerHTML = '<span class="dot" style="background:var(--danger);box-shadow:0 0 10px var(--danger);"></span> <span>Backend: Offline</span>';
        badge.style.borderColor = 'rgba(244,63,94,0.3)';
    }
}

// --- INIT ---
checkBackendStatus();

// Load firm info on startup
(async function init() {
    try {
        const db = getStoredDbConfig();
        const comp = JSON.parse(localStorage.getItem('logoViewerCompany') || '{}');
        if (db.firma) {
            const firmLabel = `Firma: ${db.firma}${comp.name ? ' – ' + comp.name : ''}`;
            document.getElementById('activeFirmDisplay').innerHTML = `<i class="fa-solid fa-building"></i> <span>${firmLabel}</span>`;
        }
        
        // Config yükle ve Firmaları/Dönemleri doldur
        const cfgRes = await fetch(`${CONFIG.backendUrl}/api/get-config`, { headers: { 'x-api-key': CONFIG.apiKey } });
        if (cfgRes.ok) {
            const cfg = await cfgRes.json();
            xsltList = cfg.xsltList || [];
            rules = cfg.rules || [];
            selectedXslts = { fatura: '', arsiv: '', irsaliye: '', siparis: '', talep: '', ...(cfg.selectedXslts || {}) };
            populateManualXsltSelect();
            
            // Eğer veritabanı ayarları varsa firmaları yükle
            if (db.server) {
                await loadFirms();
                if (db.firma) await loadPeriods(db.firma);
            }
        }
    } catch (e) {
        console.error('[INIT] Hata:', e);
    }
})();

// OTOMATİK FİRMA/DÖNEM KEŞFİ
async function loadFirms() {
    console.log('[DISCOVERY] Firmalar yükleniyor...');
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/list-firms`, { 
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });
        const data = await res.json();
        if (data.success) {
            const list = document.getElementById('firmaList');
            const input = document.getElementById('sqlFirma');
            list.innerHTML = data.firms.map(f => `<option value="${String(f.Nr).padStart(3, '0')}">${f.Name}</option>`).join('');
            
            // Input değiştiğinde dönemleri yüklemeyi dene
            input.oninput = () => {
                const val = input.value.padStart(3, '0');
                if (input.value.length >= 2) loadPeriods(input.value);
            };
        }
    } catch (e) { console.error('Firma listesi hatası:', e); }
}

async function loadPeriods(firmNr) {
    if (!firmNr) return;
    console.log(`[DISCOVERY] ${firmNr} için dönemler yükleniyor...`);
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/list-periods`, { 
            method: 'POST', 
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify({ firma: firmNr })
        });
        const data = await res.json();
        if (data.success) {
            const list = document.getElementById('donemList');
            list.innerHTML = data.periods.map(p => {
                const nrStr = String(p.Nr).padStart(2, '0');
                return `<option value="${nrStr}">${p.Nr} - ${p.Name || ''}</option>`;
            }).join('');
        }
    } catch (e) { console.error('Dönem listesi hatası:', e); }
}

function printPreview() {
    const iframe = document.getElementById('previewIframe');
    if (iframe && iframe.contentWindow) {
        iframe.contentWindow.focus();
        iframe.contentWindow.print();
    } else {
        window.print();
    }
}

// ---- BULK ACTIONS ----
async function sendBulkSms() { startBulkProcess('sms'); }

async function sendBulkEmail() { startBulkProcess('email'); }

// QUICK ACTIONS FROM LIST
async function sendQuickSms(no) {
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/fetch-document`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify({ type: currentType, ficheNo: no, dbConfig: getStoredDbConfig() })
        });
        const doc = await res.json();
        if (!doc.header) throw new Error('Belge verisi alınamadı');
        
        const rawPhone = doc.header.AliciTel || '';
        const cleanPhone = rawPhone.replace(/\s+/g, '').replace(/[()\-]/g, '');
        const phone = cleanPhone.startsWith('0') ? cleanPhone.substring(1) : cleanPhone;
        
        const message = prompt('SMS Mesajı:', `Sayın ${doc.header.CariUnvan}, ${doc.header.No} nolu belgeniz oluşturulmuştur. Tutar: ${doc.header.Net} TL`);
        if (!message) return;

        const smsRes = await fetch(`${CONFIG.backendUrl}/api/send-sms`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify({ phone, message })
        });
        const result = await smsRes.json();
        if (result.success) alert('✅ SMS başarıyla gönderildi.');
        else alert('❌ SMS Hatası: ' + (result.error || 'Bilinmiyor'));
    } catch (e) { alert('Hata: ' + e.message); }
}

// ============================================================
// ---- MAIL COMPOSE MODAL (Tekli Gönderim) ----
// ============================================================
let _mailComposeDoc = null;  // Aktif belge verisi
let _mailComposePdfHtml = null;

async function sendQuickEmail(no) {
    // Belge verisini çek, sonra modal'ı aç
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/fetch-document`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify({ type: currentType, ficheNo: no, dbConfig: getStoredDbConfig() })
        });
        const doc = await res.json();
        if (!doc.header) throw new Error('Belge verisi alınamadı');

        // Hazır içerik doldur
        const h = doc.header;
        document.getElementById('mailComposeDocInfo').textContent =
            `${no} — ${h.CariUnvan || ''} — ${h.Tarih ? new Date(h.Tarih).toLocaleDateString('tr-TR') : ''}`;
        document.getElementById('mailComposeTo').value = h.AliciEmail || '';
        document.getElementById('mailComposeSubject').value =
            `Belge Bildirimi — ${no}${h.CariUnvan ? ` (${h.CariUnvan})` : ''}`;
        document.getElementById('mailComposeBody').value =
            `Sayın ${h.CariUnvan || 'Müşterimiz'},\n\n${no} nolu belgeniz hazırlanmıştır. Ekte PDF olarak gönderilmiştir.\n\nBelge Tarihi : ${h.Tarih ? new Date(h.Tarih).toLocaleDateString('tr-TR') : ''}\nTutar        : ${h.Net ? Number(h.Net).toLocaleString('tr-TR', {minimumFractionDigits:2}) : '0,00'} TL (KDV Hariç)\nGenel Toplam : ${h.Brut ? Number(h.Brut).toLocaleString('tr-TR', {minimumFractionDigits:2}) : '0,00'} TL\n\nSaygılarımızla.`;

        _mailComposeDoc = doc;
        _mailComposePdfHtml = null;
        document.getElementById('mailComposePdfStatus').textContent = 'PDF hazırlanıyor...';

        // PDF HTML'ini arkaplanda al
        fetch(`${CONFIG.backendUrl}/api/render-view`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify({ data: doc, type: currentType, xsltPath: selectedXslts[currentType] })
        }).then(r => r.ok ? r.text() : null).then(html => {
            _mailComposePdfHtml = html;
            document.getElementById('mailComposePdfStatus').textContent = html ? '✅ PDF hazır' : '⚠️ PDF oluşturulamadı';
        }).catch(() => {
            document.getElementById('mailComposePdfStatus').textContent = '⚠️ PDF alınamadı';
        });

        document.getElementById('mailComposeModal').classList.add('show');
    } catch (e) {
        alert('❌ Hata: ' + e.message);
    }
}

function closeMailCompose() {
    document.getElementById('mailComposeModal').classList.remove('show');
    _mailComposeDoc = null;
    _mailComposePdfHtml = null;
}

async function submitMailCompose() {
    const to = document.getElementById('mailComposeTo').value.trim();
    const subject = document.getElementById('mailComposeSubject').value.trim();
    const body = document.getElementById('mailComposeBody').value.trim();
    const addPdf = document.getElementById('mailComposePdf').checked;

    if (!to) { alert('Lütfen alıcı e-posta adresini girin.'); return; }
    if (!subject) { alert('Lütfen konu alanını doldurun.'); return; }

    const btn = document.getElementById('btnMailComposeSend');
    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Gönderiliyor...';

    try {
        const resCfg = await fetch(`${CONFIG.backendUrl}/api/get-config`, { headers: { 'x-api-key': CONFIG.apiKey } });
        const cfg = await resCfg.json();
        const method = cfg.notificationSettings?.email?.method || 'smtp';
        const outlookAccount = cfg.notificationSettings?.email?.outlookAccount || '';

        const mailBody = {
            to, subject, body,
            docData: _mailComposeDoc,
            html: _mailComposePdfHtml, // XSLT Render sonucunu mail body olarak gönder
            pdfHtml: (addPdf && _mailComposePdfHtml) ? _mailComposePdfHtml : null,
            pdfFilename: _mailComposeDoc?.header?.No ? `${_mailComposeDoc.header.No}.pdf` : 'belge.pdf'
        };
        if (method === 'outlook') mailBody.fromAccount = outlookAccount;

        const res = await fetch(`${CONFIG.backendUrl}/api/send-email${method === 'outlook' ? '-outlook' : ''}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify(mailBody)
        });
        const result = await res.json();
        if (result.success) {
            closeMailCompose();
            alert('✅ ' + result.message);
        } else {
            alert('❌ E-Posta gönderilemedi:\n' + (result.error || 'Bilinmeyen hata'));
        }
    } catch (e) {
        alert('❌ Gönderim hatası: ' + e.message);
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="fa-solid fa-paper-plane"></i> Gönder';
    }
}

// ============================================================
// ---- TOPLU MAİL MODAL ----
// ============================================================
let _bulkMailDocs = []; // { no, cari, email, ref }

async function sendBulkEmail() {
    if (selectedDocs.size === 0) {
        alert('Lütfen en az bir belge seçin.');
        return;
    }

    const nos = [...selectedDocs];
    document.getElementById('bulkMailInfo').textContent = 'Belge bilgileri yükleniyor...';
    document.getElementById('bulkMailModal').classList.add('show');
    document.getElementById('bulkMailTableBody').innerHTML =
        `<tr><td colspan="3" style="padding:20px; text-align:center; color:#94a3b8;"><i class="fa-solid fa-spinner fa-spin"></i> Yükleniyor...</td></tr>`;

    // Her seçili belgenin mail adresini çek
    _bulkMailDocs = [];
    for (const no of nos) {
        try {
            const r = await fetch(`${CONFIG.backendUrl}/api/fetch-document`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
                body: JSON.stringify({ type: currentType, ficheNo: no, dbConfig: getStoredDbConfig() })
            });
            const d = await r.json();
            _bulkMailDocs.push({
                no,
                cari: d.header?.CariUnvan || '---',
                email: d.header?.AliciEmail || '',
                docData: d
            });
        } catch {
            _bulkMailDocs.push({ no, cari: '---', email: '', docData: null });
        }
    }

    renderBulkMailTable();
}

function renderBulkMailTable() {
    const tbody = document.getElementById('bulkMailTableBody');
    let ok = 0, err = 0;
    const emailRx = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    tbody.innerHTML = _bulkMailDocs.map((d, i) => {
        const valid = emailRx.test(d.email);
        if (valid) ok++; else err++;
        const rowBg = valid ? '' : 'background:#fffbeb;';
        const icon = valid ? '✅' : '⚠️';
        return `<tr style="border-bottom:1px solid #f1f5f9; ${rowBg}">
            <td style="padding:10px 14px; font-weight:600; font-size:0.85rem; font-family:monospace;">${d.no}</td>
            <td style="padding:10px 14px; color:#374151; font-size:0.85rem;">${d.cari}</td>
            <td style="padding:6px 10px;">
                <div style="display:flex; align-items:center; gap:6px;">
                    <span style="font-size:0.9rem;">${icon}</span>
                    <input type="email" value="${d.email}" data-idx="${i}"
                        onchange="_bulkMailDocs[${i}].email = this.value; renderBulkMailTable();"
                        style="flex:1; padding:7px 10px; border:1.5px solid ${valid ? '#d1fae5' : '#fde68a'}; border-radius:6px; font-size:0.85rem; font-family:inherit; background:${valid ? '#f0fdf4' : '#fffbeb'}; outline:none;"
                        onfocus="this.style.borderColor='#6d28d9'"
                        onblur="this.style.borderColor='${valid ? '#d1fae5' : '#fde68a'}'">
                </div>
            </td>
        </tr>`;
    }).join('');

    document.getElementById('bulkMailTotal').textContent = _bulkMailDocs.length;
    document.getElementById('bulkMailOk').textContent = ok;
    document.getElementById('bulkMailErr').textContent = err;
    document.getElementById('bulkMailInfo').textContent = `${_bulkMailDocs.length} belge — ${ok} geçerli adres`;
}

function closeBulkMailModal() {
    document.getElementById('bulkMailModal').classList.remove('show');
    _bulkMailDocs = [];
}

async function executeBulkMail() {
    const emailRx = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const validDocs = _bulkMailDocs.filter(d => emailRx.test(d.email));

    if (validDocs.length === 0) {
        alert('Geçerli mail adresi olan belge bulunamadı.');
        return;
    }

    const btn = document.getElementById('btnBulkMailSend');
    const status = document.getElementById('bulkMailSendStatus');
    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Gönderiliyor...';

    const resCfg = await fetch(`${CONFIG.backendUrl}/api/get-config`, { headers: { 'x-api-key': CONFIG.apiKey } });
    const cfg = await resCfg.json();
    const method = cfg.notificationSettings?.email?.method || 'smtp';
    const outlookAccount = cfg.notificationSettings?.email?.outlookAccount || '';

    let sent = 0, failed = 0;
    for (const d of validDocs) {
        const idx = sent + failed + 1;
        status.textContent = `📄 PDF hazırlanıyor: ${d.no} (${idx}/${validDocs.length})`;

        // Her belge için PDF HTML'ini al
        let pdfHtmlContent = null;
        try {
            const rvRes = await fetch(`${CONFIG.backendUrl}/api/render-view`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
                body: JSON.stringify({ data: d.docData, type: currentType, xsltPath: selectedXslts[currentType] })
            });
            if (rvRes.ok) pdfHtmlContent = await rvRes.text();
        } catch(e) { console.warn(`[BULK-PDF] ${d.no}:`, e.message); }

        status.textContent = `✉️ Gönderiliyor: ${d.no} (${idx}/${validDocs.length})`;
        try {
            const subject = `Belge Bildirimi — ${d.no}${d.cari !== '---' ? ` (${d.cari})` : ''}`;
            const body = `Sayın ${d.cari},\n\n${d.no} nolu belgeniz hazırlanmıştır. Ekte PDF olarak gönderilmiştir.\n\nSaygılarımızla.`;
            const mailBody = {
                to: d.email, subject, body,
                docData: d.docData,
                pdfHtml: pdfHtmlContent,
                pdfFilename: `${d.no}.pdf`
            };
            if (method === 'outlook') mailBody.fromAccount = outlookAccount;

            const r = await fetch(`${CONFIG.backendUrl}/api/send-email${method === 'outlook' ? '-outlook' : ''}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
                body: JSON.stringify(mailBody)
            });
            const result = await r.json();
            if (result.success) sent++;
            else { failed++; console.error(`[BULK-MAIL] ${d.no} hata:`, result.error); }
        } catch (e) {
            failed++;
            console.error(`[BULK-MAIL] ${d.no}:`, e.message);
        }
    }

    btn.disabled = false;
    btn.innerHTML = '<i class="fa-solid fa-paper-plane"></i> Toplu Gönder';
    status.textContent = `✅ ${sent} gönderildi${failed > 0 ? ` | ❌ ${failed} başarısız` : ''}`;

    if (sent > 0) {
        setTimeout(() => {
            closeBulkMailModal();
            clearSelection();
            alert(`✅ ${sent} e-posta (PDF ekiyle) başarıyla gönderildi.${failed > 0 ? `\n❌ ${failed} belge gönderilemedi.` : ''}`);
        }, 1200);
    }
}

// QUICK ACTIONS FROM PREVIEW FOOTER
document.getElementById('btnSendSms').addEventListener('click', () => {
    if (currentData && currentData.header) {
        sendQuickSms(currentData.header.No);
    }
});

document.getElementById('btnSendEmail').addEventListener('click', () => {
    if (currentData && currentData.header) {
        sendQuickEmail(currentData.header.No);
    }
});

// --- [ AUTO MAIL LOGIC ] ---
function autoLog(msg, color = '#94a3b8') {
    const log = document.getElementById('autoMailLog');
    if (!log) return;
    const time = new Date().toLocaleTimeString();
    log.innerHTML += `<div style="color:${color}; margin-bottom:4px;">[${time}] ${msg}</div>`;
    log.scrollTop = log.scrollHeight;
}

// ---- AUTO MAIL ROBOT ----
async function loadAutoMailConfig() {
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/auto-mail-config`, {
            headers: { 'x-api-key': CONFIG.apiKey }
        });
        const cfg = await res.json();
        
        const enabledCheck = document.getElementById('autoMailEnabled');
        const dateInput = document.getElementById('autoMailStartDate');
        if (enabledCheck) enabledCheck.checked = !!cfg.enabled;
        if (dateInput) dateInput.value = cfg.startDate || new Date().toISOString().split('T')[0];
        
        const selectedTypes = cfg.types || [];
        document.querySelectorAll('.auto-mail-type-check').forEach(ch => {
            ch.checked = selectedTypes.includes(ch.value);
        });
        
        autoLog(`Ayarlar yüklendi. Robot Durumu: ${cfg.enabled ? 'AKTİF' : 'PASİF'}`, cfg.enabled ? '#10b981' : '#64748b');
    } catch (e) {
        console.error('[AUTO MAIL] Yükleme hatası:', e);
    }
}

async function saveAutoMailConfig() {
    const enabled = document.getElementById('autoMailEnabled').checked;
    const startDate = document.getElementById('autoMailStartDate').value;
    const types = Array.from(document.querySelectorAll('.auto-mail-type-check:checked')).map(ch => ch.value);
    
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/save-auto-mail-config`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
            body: JSON.stringify({ enabled, startDate, types })
        });
        const data = await res.json();
        if (data.success) {
            autoLog('✅ Ayarlar başarıyla kaydedildi.', '#10b981');
            alert('Robot ayarları kaydedildi.');
        } else {
            throw new Error(data.error);
        }
    } catch (e) {
        autoLog(`❌ Kaydetme hatası: ${e.message}`, '#ef4444');
        alert('Ayarlar kaydedilemedi: ' + e.message);
    }
}

document.getElementById('btnSaveAutoMail').addEventListener('click', saveAutoMailConfig);
document.getElementById('btnStartAutoMail').addEventListener('click', startManualScan);

function autoLog(msg, color = '#94a3b8') {
    const log = document.getElementById('autoMailLog');
    if (!log) return;
    const time = new Date().toLocaleTimeString();
    log.innerHTML += `<div style="color:${color}; margin-bottom:4px;">[${time}] ${msg}</div>`;
    log.scrollTop = log.scrollHeight;
}

async function startManualScan() {
    const btn = document.getElementById('btnStartAutoMail');
    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> TARANIYOR...';
    
    autoLog('Manuel tarama başlatıldı...', '#3498db');
    
    const types = Array.from(document.querySelectorAll('.auto-mail-type-check:checked')).map(ch => ch.value);
    if (types.length === 0) {
        autoLog('Uyarı: Hiçbir belge türü seçilmedi!', '#f59e0b');
        btn.disabled = false;
        btn.innerHTML = '<i class="fa-solid fa-play"></i> TARAMAYI VE GÖNDERİMİ BAŞLAT';
        return;
    }

    const dateFrom = document.getElementById('autoMailStartDate').value || new Date().toISOString().split('T')[0];
    const dateTo = new Date().toISOString().split('T')[0];

    for (const type of types) {
        try {
            const res = await fetch(`${CONFIG.backendUrl}/api/list-documents`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
                body: JSON.stringify({ type, dateFrom, dateTo, dbConfig: getStoredDbConfig() })
            });
            const data = await res.json();
            const docs = (data.documents || []);

            autoLog(`${type.toUpperCase()}: ${docs.length} belge bulundu.`);

            for (const doc of docs) {
                autoLog(`Gönderiliyor: ${doc.No}...`, '#3498db');
                
                const fRes = await fetch(`${CONFIG.backendUrl}/api/fetch-document`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
                    body: JSON.stringify({ type, ficheNo: doc.No, dbConfig: getStoredDbConfig() })
                });
                const fData = await fRes.json();
                const email = fData.header?.AliciEmail;

                if (email && email.includes('@')) {
                    const rvRes = await fetch(`${CONFIG.backendUrl}/api/render-view`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
                        body: JSON.stringify({ data: fData, type })
                    });
                    const pdfHtml = await rvRes.text();

                    const sRes = await fetch(`${CONFIG.backendUrl}/api/send-email`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
                        body: JSON.stringify({ 
                            to: email, 
                            subject: `${type.toUpperCase()} - ${doc.No}`, 
                            body: `Sayın yetkili,\n\n${doc.No} nolu belgeniz ekte sunulmuştur.`,
                            docData: fData,
                            pdfHtml: pdfHtml,
                            pdfFilename: `${doc.No}.html`
                        })
                    });
                    const sData = await sRes.json();
                    if (sData.success) autoLog(`✅ ${doc.No} gönderildi.`, '#10b981');
                    else autoLog(`❌ ${doc.No} hata: ${sData.error}`, '#ef4444');
                } else {
                    autoLog(`⚠️ ${doc.No}: E-posta adresi bulunamadı.`, '#f59e0b');
                }
            }
        } catch(e) {
            autoLog(`❌ ${type.toUpperCase()} hatası: ${e.message}`, '#ef4444');
        }
    }

    autoLog('Manuel işlem tamamlandı.', '#10b981');
    btn.disabled = false;
    btn.innerHTML = '<i class="fa-solid fa-play"></i> TARAMAYI VE GÖNDERİMİ BAŞLAT';
}

console.log('Logo Viewer initialized.');

// ---- OUTLOOK HELPERS ----
document.getElementById('emailMethod').addEventListener('change', (e) => {
    const val = e.target.value;
    toggleEmailMethodUI(val);
    if (val === 'outlook') loadOutlookAccounts();
});

function toggleEmailMethodUI(method) {
    if (method === 'outlook') {
        document.getElementById('smtpSettingsGroup').classList.add('hidden');
        document.getElementById('outlookSettingsGroup').classList.remove('hidden');
    } else {
        document.getElementById('smtpSettingsGroup').classList.remove('hidden');
        document.getElementById('outlookSettingsGroup').classList.add('hidden');
    }
}

async function loadOutlookAccounts(selectedAcc = '') {
    const sel = document.getElementById('outlookAccount');
    if (!sel) return;
    
    // Yükleme durumu
    const originalSelected = selectedAcc || sel.value;
    sel.innerHTML = '<option value="">Taranıyor...</option>';
    
    try {
        const res = await fetch(`${CONFIG.backendUrl}/api/get-outlook-accounts`, {
            headers: { 'x-api-key': CONFIG.apiKey }
        });
        if (!res.ok) throw new Error('Masaüstü bağlantısı başarısız!');
        
        const data = await res.json();
        
        if (data.success && data.accounts && data.accounts.length > 0) {
            sel.innerHTML = data.accounts.map(acc => 
                `<option value="${acc}" ${acc === originalSelected ? 'selected' : ''}>${acc}</option>`
            ).join('');
        } else {
            console.warn('[OUTLOOK] Hata Detayı:', data.error);
            const errMsg = data.error || 'Aktif hesap bulunamadı';
            
            if (errMsg.toLowerCase().includes('yetkisiz')) {
                sel.innerHTML = `<option value="">⚠️ YETKİ SORUNU (Güvenlik Engeli)</option>`;
            } else {
                sel.innerHTML = `<option value="">⚠️ ${errMsg}</option>`;
            }
        }
    } catch (e) {
        console.error('[OUTLOOK]', e);
        sel.innerHTML = '<option value="">❌ Backend Erişim Hatası</option>';
    }
}

// ---- TOPLU GÖNDERİM DOĞRULAMA MODÜLÜ ----
let currentBulkMode = ''; // 'sms' veya 'email'
let bulkItems = []; // { no, cari, email, tel, type }

async function startBulkProcess(mode) {
    if (selectedDocs.size === 0) {
        alert("Lütfen en az bir belge seçin!");
        return;
    }
    
    currentBulkMode = mode;
    const modal = document.getElementById('bulkValidationModal');
    const gridBody = document.getElementById('validationGridBody');
    
    // Modalı göster ve loading durumuna al
    modal.classList.add('show');
    gridBody.innerHTML = '<tr><td colspan="4" style="text-align:center; padding:50px;"><i class="fas fa-circle-notch fa-spin fa-2x" style="color:var(--accent-light); margin-bottom:15px;"></i><br>Belge verileri hazırlanıyor, lütfen bekleyin...</td></tr>';
    
    document.getElementById('valStatTotal').innerText = 'Hazırlanıyor...';
    document.getElementById('valStatOk').innerText = '0 Geçerli';
    document.getElementById('valStatErr').innerText = '0 Hatalı';
    document.getElementById('btnFinalBulkSend').disabled = true;

    bulkItems = [];
    let count = 0;
    const total = selectedDocs.size;

    for (const no of selectedDocs) {
        try {
            const res = await fetch(`${CONFIG.backendUrl}/api/fetch-document`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'x-api-key': CONFIG.apiKey },
                body: JSON.stringify({ type: currentType, ficheNo: no, dbConfig: getStoredDbConfig() })
            });
            const doc = await res.json();
            if (doc.header) {
                bulkItems.push({
                    no: doc.header.No,
                    cari: doc.header.CariUnvan,
                    email: doc.header.AliciEmail || '',
                    tel: doc.header.AliciTel || '',
                    type: currentType
                });
            }
        } catch (e) { console.error('Veri çekme hatası:', e); }
        count++;
        document.getElementById('valStatTotal').innerText = `Hazırlanıyor: ${count}/${total}`;
    }
    
    renderValidationGrid();
}

function renderValidationGrid() {
    const gridBody = document.getElementById('validationGridBody');
    document.getElementById('valStatTotal').innerText = `Toplam: ${bulkItems.length}`;
    
    gridBody.innerHTML = bulkItems.map((item, index) => `
        <tr data-index="${index}">
            <td style="font-family:monospace; color:var(--accent-light); font-weight:700;">${item.no}</td>
            <td style="font-size:0.75rem; max-width:200px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;" title="${item.cari}">${item.cari}</td>
            <td contenteditable="true" class="val-email" onblur="updateBulkItem(${index}, 'email', this.innerText)">${item.email}</td>
            <td contenteditable="true" class="val-tel" onblur="updateBulkItem(${index}, 'tel', this.innerText)">${item.tel}</td>
        </tr>
    `).join('');
    
    validateGrid();
}

function updateBulkItem(index, field, value) {
    bulkItems[index][field] = value.trim();
    validateGrid();
}

function validateGrid() {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const telRegex = /^[0-9]{10,14}$/;
    
    let okCount = 0;
    let errCount = 0;
    
    const rows = document.querySelectorAll('#validationGridBody tr');
    rows.forEach((row, idx) => {
        const item = bulkItems[idx];
        if (!item) return;
        const emailCell = row.querySelector('.val-email');
        const telCell = row.querySelector('.val-tel');
        
        let rowHasError = false;
        
        if (currentBulkMode === 'email') {
            if (!emailRegex.test(item.email)) {
                emailCell.classList.add('error-cell');
                rowHasError = true;
            } else {
                emailCell.classList.remove('error-cell');
            }
        }
        
        if (currentBulkMode === 'sms') {
            const cleanTel = item.tel.replace(/\D/g, '');
            if (!telRegex.test(cleanTel)) {
                telCell.classList.add('error-cell');
                rowHasError = true;
            } else {
                telCell.classList.remove('error-cell');
            }
        }

        if (rowHasError) errCount++; else okCount++;
    });
    
    document.getElementById('valStatOk').innerText = `${okCount} Geçerli`;
    document.getElementById('valStatErr').innerText = `${errCount} Hatalı`;
    document.getElementById('btnFinalBulkSend').disabled = (errCount > 0 || bulkItems.length === 0);
}

document.getElementById('btnFinalBulkSend').onclick = async () => {
    const btn = document.getElementById('btnFinalBulkSend');
    const originalHtml = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> İşleniyor...';

    let success = 0;
    let fail = 0;

    let emailCfg = {};
    if (currentBulkMode === 'email') {
        try {
            const res = await fetch(`${CONFIG.backendUrl}/api/get-config`, { headers: { 'x-api-key': CONFIG.apiKey } });
            const data = await res.json();
            emailCfg = data.notificationSettings?.email || {};
        } catch (e) { console.error('Config hatası:', e); }
    }

    for (const item of bulkItems) {
        try {
            if (currentBulkMode === 'sms') {
                const cleanPhone = item.tel.replace(/\D/g,'');
                const phone = cleanPhone.startsWith('0') ? cleanPhone.substring(1) : cleanPhone;
                const message = `Sayıın ${item.cari}, ${item.no} nolu belgeniz oluşturulmuştur.`;
                const res = await fetch(`${CONFIG.backendUrl}/api/send-sms`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ phone, message })
                });
                if ((await res.json()).success) success++; else fail++;
            } else {
                const method = emailCfg.method || 'smtp';
                const endpoint = method === 'outlook' ? '/api/send-email-outlook' : '/api/send-email';
                const subject = `${CONFIG.companyName || 'Logo Viewer'} - Belge Bildirimi (${item.no})`;
                const body = `Sayın ${item.cari},\n\n${item.no} nolu belgeniz ekte sunulmuştur.`;
                const mailBody = { to: item.email, subject, body };
                if (method === 'outlook') mailBody.fromAccount = emailCfg.outlookAccount;

                const mailRes = await fetch(`${CONFIG.backendUrl}${endpoint}`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(mailBody) });
                if ((await mailRes.json()).success) success++; else fail++;
            }
        } catch (e) { fail++; }
    }

    alert(`Toplu İşlem Tamamlandı.\nBaşarılı: ${success}\nHatalı: ${fail}`);
    document.getElementById('bulkValidationModal').classList.remove('show');
    btn.disabled = false;
    btn.innerHTML = originalHtml;
    clearSelection();
};
