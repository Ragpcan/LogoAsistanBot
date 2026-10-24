// LOGO & ZİRVE CRM BOT MANAGEMENT PANEL - APP.JS
const API_BASE = '/api';

// --- NULL SAFE HELPERS ---
function getEl(id) { return document.getElementById(id); }
function getVal(id, def = '') { const el = getEl(id); return el ? el.value.trim() : def; }
function setVal(id, val) { const el = getEl(id); if(el) el.value = (val !== null && val !== undefined) ? val : ''; }
function setCheck(id, chk) { const el = getEl(id); if(el) el.checked = !!chk; }
function setText(id, txt) { const el = getEl(id); if(el) el.textContent = (txt !== null && txt !== undefined) ? txt : ''; }

// Global Auth Header
function getAuthHeaders() {
  const token = localStorage.getItem('logoCrmToken') || sessionStorage.getItem('logoCrmToken');
  return {
    'Content-Type': 'application/json',
    'Authorization': token ? 'Bearer ' + token : ''
  };
}

// Global Tab Switching
function switchTab(tabId, btnElement) {
  document.querySelectorAll('.tab-content').forEach(el => el.classList.remove('active'));
  document.querySelectorAll('.tab-btn').forEach(el => el.classList.remove('active'));

  const targetTab = getEl(tabId);
  if (targetTab) targetTab.classList.add('active');
  if (btnElement) btnElement.classList.add('active');

  if (tabId === 'tab-salesmen') {
    loadSalesmenData();
    loadWebUsers();
  } else if (tabId === 'tab-mailsms') {
    loadMailSmsSettings();
  } else if (tabId === 'tab-reports') {
    loadReports();
    loadMainReportHistory();
  } else if (tabId === 'tab-settings') {
    loadSettings();
  }
}
window.switchTab = switchTab;

// BOT STATUS MANAGEMENT
async function checkBotStatus() {
  try {
    const res = await fetch(API_BASE + '/bot/status');
    if (!res.ok) return;
    const data = await res.json();
    const isRunning = data.isRunning === true || data.status === 'Running';

    const badge = getEl('botStatusBadge');
    const statusText = getEl('botStatusText');
    const btnStart = getEl('btnQuickStart');
    const btnStop = getEl('btnQuickStop');

    if (badge) badge.className = 'status-badge ' + (isRunning ? 'running' : 'stopped');
    if (statusText) statusText.textContent = isRunning ? 'Çalışıyor' : 'Durduruldu';
    setText('dashBotStatusText', isRunning ? '🟢 Çalışıyor' : '🔴 Durduruldu');
    setText('dashBotUsername', data.botUsername ? '@' + data.botUsername : '@LogoZirveCrmBot');

    if (btnStart) btnStart.style.display = isRunning ? 'none' : 'inline-flex';
    if (btnStop) btnStop.style.display = isRunning ? 'inline-flex' : 'none';
  } catch (err) {
    console.error('Bot durum kontrolü hatası:', err);
  }
}

async function startBot() {
  try {
    const res = await fetch(API_BASE + '/bot/start', { method: 'POST' });
    const data = await res.json();
    alert(data.message || 'Bot başlatıldı');
    checkBotStatus();
  } catch (err) {
    alert('Bot başlatma hatası: ' + err);
  }
}
window.startBot = startBot;

async function stopBot() {
  try {
    const res = await fetch(API_BASE + '/bot/stop', { method: 'POST' });
    const data = await res.json();
    alert(data.message || 'Bot durduruldu');
    checkBotStatus();
  } catch (err) {
    alert('Bot durdurma hatası: ' + err);
  }
}
window.stopBot = stopBot;

function openLogoErpPortal() {
  window.open('/erp-portal.html', '_blank');
}
window.openLogoErpPortal = openLogoErpPortal;

function logout() {
  sessionStorage.removeItem('logoCrmAuth');
  localStorage.removeItem('logoCrmToken');
  window.location.href = '/login.html';
}
window.logout = logout;

// ADMIN SETTINGS & SYSTEM TEST
async function loadSettings() {
  try {
    const res = await fetch(API_BASE + '/settings', { headers: getAuthHeaders() });
    if (!res.ok) return;

    const data = await res.json();
    setVal('cfgBotToken', data.botToken);
    setVal('cfgServer', data.server || 'localhost');
    setVal('cfgDatabase', data.database || 'LOGO_CRM_DB');
    setVal('cfgUserId', data.userId || 'sa');
    setVal('cfgPassword', data.password);

    const targetEl = getEl('cfgTargetSystem');
    if (targetEl) targetEl.value = data.targetSystem || 'ERP';

    setVal('cfgFirmNo', data.firmNo || '001');
    setVal('cfgPeriodNo', data.periodNo || '01');
    setVal('cfgWarehouseNo', data.warehouseNo || '0');
    setVal('cfgLicenseExpirationDate', data.licenseExpirationDate ? data.licenseExpirationDate.split('T')[0] : '');
    setVal('cfgDocumentYear', data.documentYear || new Date().getFullYear());
    setVal('cfgInvoiceFichePrefix', data.invoiceFichePrefix || 'FAT');
    setVal('cfgDispatchFichePrefix', data.dispatchFichePrefix || 'IRS');
    setVal('cfgOrderFichePrefix', data.orderFichePrefix || 'SIP');
    setVal('cfgAiApiKey', data.aiApiKey || '');

    const aiProviderEl = getEl('cfgAiProvider');
    if (aiProviderEl) aiProviderEl.value = data.aiProvider || 'Groq';
    setVal('cfgAiModel', data.aiModel || 'llama-3.3-70b-versatile');
    const aiEnabledEl = getEl('cfgAiIsEnabled');
    if (aiEnabledEl) aiEnabledEl.checked = data.aiIsEnabled !== false;

    const targetSys = data.targetSystem || 'ERP';
    const sysText = targetSys === 'ERP' 
      ? `Logo ERP / Tiger (Firma: ${data.firmNo || '001'}, Dönem: ${data.periodNo || '01'}, Ambar: ${data.warehouseNo || '0'})` 
      : 'Logo CRM (MT_Proposals)';

    setText('dashTargetSystem', sysText);
    setText('dashDbName', data.database ? `${data.server || 'localhost'} / ${data.database}` : '-');
  } catch (err) {
    console.error('Settings load error:', err);
  }
}
window.loadSettings = loadSettings;

function onAiProviderChange() {
  const provider = getEl('cfgAiProvider')?.value || 'Groq';
  const modelInput = getEl('cfgAiModel');
  const keyInput = getEl('cfgAiApiKey');
  if (!modelInput) return;

  if (provider === 'Gemini') {
    if (!modelInput.value || modelInput.value.startsWith('llama') || modelInput.value.startsWith('gpt') || modelInput.value.startsWith('deepseek')) {
      modelInput.value = 'gemini-3.6-flash';
    }
    modelInput.placeholder = 'Örn: gemini-3.6-flash veya gemini-1.5-flash';
    if (keyInput) keyInput.placeholder = 'Örn: AIzaSy...';
  } else if (provider === 'Groq') {
    if (!modelInput.value || modelInput.value.startsWith('gemini') || modelInput.value.startsWith('gpt') || modelInput.value.startsWith('deepseek')) {
      modelInput.value = 'llama-3.3-70b-versatile';
    }
    modelInput.placeholder = 'Örn: llama-3.3-70b-versatile';
    if (keyInput) keyInput.placeholder = 'Örn: gsk_...';
  } else if (provider === 'OpenAI') {
    if (!modelInput.value || modelInput.value.startsWith('gemini') || modelInput.value.startsWith('llama')) {
      modelInput.value = 'gpt-4o-mini';
    }
    modelInput.placeholder = 'Örn: gpt-4o-mini veya gpt-4o';
    if (keyInput) keyInput.placeholder = 'Örn: sk-proj-...';
  } else if (provider === 'DeepSeek') {
    if (!modelInput.value || modelInput.value.startsWith('gemini')) {
      modelInput.value = 'deepseek-chat';
    }
    modelInput.placeholder = 'Örn: deepseek-chat';
    if (keyInput) keyInput.placeholder = 'Örn: sk-...';
  }
}
window.onAiProviderChange = onAiProviderChange;

// MAIL & SMS SETTINGS
async function loadMailSmsSettings() {
  try {
    const res = await fetch(API_BASE + '/settings/mailsms');
    if (!res.ok) return;

    const data = await res.json();
    if (data.mailSettings) {
      setVal('cfgMailHost', data.mailSettings.host);
      setVal('cfgMailPort', data.mailSettings.port || 587);
      setVal('cfgMailUser', data.mailSettings.user);
      setVal('cfgMailPass', data.mailSettings.pass);
      setCheck('cfgMailEnableSsl', data.mailSettings.enableSsl !== false);
      setCheck('cfgMailAutoSend', data.mailSettings.autoSend === true);
    }
    if (data.smsSettings) {
      setVal('cfgSmsUser', data.smsSettings.user);
      setVal('cfgSmsPass', data.smsSettings.pass);
      setVal('cfgSmsHeader', data.smsSettings.header);
      setCheck('cfgSmsAutoSend', data.smsSettings.autoSend === true);
    }
    if (data.autoSendConfig) {
      setVal('cfgAutoSendStartDate', data.autoSendConfig.startDate);
      setVal('cfgAutoSendIntervalMinutes', data.autoSendConfig.intervalMinutes || 15);
      setCheck('cfgAutoSendFaturalar', data.autoSendConfig.sendFaturalar !== false);
      setCheck('cfgAutoSendIrsaliyeler', data.autoSendConfig.sendIrsaliyeler !== false);
      setCheck('cfgAutoSendSiparisler', data.autoSendConfig.sendSiparisler === true);
    }
  } catch (err) {
    console.error('Mail/SMS Ayarları Yükleme Hatası:', err);
  }
}
window.loadMailSmsSettings = loadMailSmsSettings;

async function saveMailSmsSettings() {
  const alertBox = getEl('mailSmsAlert');
  if (alertBox) alertBox.style.display = 'none';

  const payload = {
    MailHost: getVal('cfgMailHost'),
    MailPort: parseInt(getVal('cfgMailPort', '587')) || 587,
    MailUser: getVal('cfgMailUser'),
    MailPass: getVal('cfgMailPass'),
    MailEnableSsl: getEl('cfgMailEnableSsl') ? getEl('cfgMailEnableSsl').checked : true,
    MailAutoSend: getEl('cfgMailAutoSend') ? getEl('cfgMailAutoSend').checked : false,
    SmsUser: getVal('cfgSmsUser'),
    SmsPass: getVal('cfgSmsPass'),
    SmsHeader: getVal('cfgSmsHeader'),
    SmsAutoSend: getEl('cfgSmsAutoSend') ? getEl('cfgSmsAutoSend').checked : false,
    AutoSendStartDate: getVal('cfgAutoSendStartDate'),
    AutoSendIntervalMinutes: parseInt(getVal('cfgAutoSendIntervalMinutes', '15')) || 15,
    AutoSendFaturalar: getEl('cfgAutoSendFaturalar') ? getEl('cfgAutoSendFaturalar').checked : true,
    AutoSendIrsaliyeler: getEl('cfgAutoSendIrsaliyeler') ? getEl('cfgAutoSendIrsaliyeler').checked : true,
    AutoSendSiparisler: getEl('cfgAutoSendSiparisler') ? getEl('cfgAutoSendSiparisler').checked : false
  };

  try {
    const res = await fetch(API_BASE + '/settings/mailsms', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json();
    if (alertBox) {
      alertBox.style.display = 'block';
      alertBox.className = 'alert ' + (data.isSuccess ? 'alert-success' : 'alert-danger');
      alertBox.innerHTML = (data.isSuccess ? '✅ ' : '❌ ') + data.message;
    } else {
      alert((data.isSuccess ? '✅ BAŞARILI: ' : '❌ HATA: ') + data.message);
    }
  } catch (err) {
    if (alertBox) {
      alertBox.style.display = 'block';
      alertBox.className = 'alert alert-danger';
      alertBox.innerHTML = '❌ Kaydetme hatası: ' + err;
    } else {
      alert('❌ Kaydetme hatası: ' + err);
    }
  }
}
window.saveMailSmsSettings = saveMailSmsSettings;

async function testMailConnection() {
  const recipient = prompt('Test mailinin gönderileceği e-posta adresini giriniz:', getVal('cfgMailUser'));
  if (!recipient) return;

  try {
    const res = await fetch(API_BASE + '/settings/test-mail', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({
        Host: getVal('cfgMailHost'),
        Port: parseInt(getVal('cfgMailPort', '587')) || 587,
        User: getVal('cfgMailUser'),
        Pass: getVal('cfgMailPass'),
        EnableSsl: getEl('cfgMailEnableSsl') ? getEl('cfgMailEnableSsl').checked : true,
        Recipient: recipient
      })
    });
    const text = await res.text();
    let data;
    try { data = JSON.parse(text); } catch { data = { isSuccess: res.ok, message: text || res.statusText }; }
    const success = data.isSuccess || data.success;
    alert((success ? '✅ BAŞARILI: ' : '❌ HATA: ') + (data.message || text));
  } catch (err) {
    alert('❌ Test Mail Hatası: ' + err);
  }
}
window.testMailConnection = testMailConnection;

async function testSmsConnection() {
  const phone = prompt('Test SMS gönderilecek telefon numarasını giriniz (Örn: 05321234567):');
  if (!phone) return;

  try {
    const res = await fetch(API_BASE + '/settings/test-sms', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ Phone: phone, PhoneNumber: phone })
    });
    const text = await res.text();
    let data;
    try { data = JSON.parse(text); } catch { data = { isSuccess: res.ok, message: text || res.statusText }; }
    const success = data.isSuccess || data.success;
    alert((success ? '✅ BAŞARILI: ' : '❌ HATA: ') + (data.message || text));
  } catch (err) {
    alert('❌ Test SMS Hatası: ' + err);
  }
}
window.testSmsConnection = testSmsConnection;

// TELEGRAM USERS & PLASIYER DATA
async function loadSalesmenData() {
  const tbody = getEl('salesmenTableBody');
  if (!tbody) return;

  try {
    const res = await fetch(API_BASE + '/salesmen/telegram-users');
    if (!res.ok) return;
    const users = await res.json();

    if (!Array.isArray(users) || users.length === 0) {
      tbody.innerHTML = '<tr><td colspan="6" style="padding:1rem; text-align:center; color:#94a3b8;">Kayıtlı Telegram kullanıcısı bulunamadı.</td></tr>';
      return;
    }

    tbody.innerHTML = users.map(u => {
      const isPending = !u.isApproved || u.status === 'Pending';
      const statusBadge = isPending 
        ? '<span class="status-badge stopped" style="font-size:0.75rem; background:#fef3c7; color:#b45309; padding:2px 8px; border-radius:4px;">⏳ ONAY BEKLİYOR</span>'
        : `<span class="status-badge running" style="font-size:0.75rem;">${u.allowedFirmCyphCode || 'TÜMÜ (ONAYLI)'}</span>`;

      return `
      <tr style="border-bottom:1px solid #f1f5f9; ${isPending ? 'background-color:#fffbe0;' : ''}">
        <td style="padding:0.75rem; font-weight:700; color:#475569;">${u.chatId}</td>
        <td style="padding:0.75rem;">
          <div style="font-weight:700; color:#0f172a;">${u.fullName || 'İsimsiz'}</div>
          <div style="font-size:0.75rem; color:#64748b;">@${u.username || '-'}</div>
        </td>
        <td style="padding:0.75rem;">${statusBadge}</td>
        <td style="padding:0.75rem;"><span class="status-badge running" style="font-size:0.75rem;">${u.allowedItemCyphCode || 'TÜMÜ (ONAYLI)'}</span></td>
        <td style="padding:0.75rem;"><span class="status-badge running" style="font-size:0.75rem;">${u.allowedOrderCyphCode || 'TÜMÜ (ONAYLI)'}</span></td>
        <td style="padding:0.75rem;">
          ${isPending ? `<button class="btn btn-success btn-sm" onclick="changeUserStatus(${u.chatId}, 'Approved')" style="padding:0.3rem 0.6rem; font-size:0.78rem; background-color:#16a34a; border-color:#16a34a; color:#fff; font-weight:bold; margin-right:4px;">✅ Onayla (Erişim Ver)</button>` : ''}
          <button class="btn btn-secondary btn-sm" onclick="approveUser(${u.chatId})" style="padding:0.3rem 0.6rem; font-size:0.78rem;">Yetkiler</button>
          <button class="btn btn-danger btn-sm" onclick="blockUser(${u.chatId})" style="padding:0.3rem 0.6rem; font-size:0.78rem;">Engelle</button>
        </td>
      </tr>
      `;
    }).join('');
  } catch (err) {
    tbody.innerHTML = '<tr><td colspan="6" style="padding:1rem; text-align:center; color:#ef4444;">Yükleme hatası: ' + err + '</td></tr>';
  }
}
window.loadSalesmenData = loadSalesmenData;

async function changeUserStatus(chatId, status) {
  try {
    const res = await fetch(API_BASE + '/salesmen/update-status', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ ChatId: chatId, Status: status })
    });
    if (res.ok) {
      alert(status === 'Approved' ? '✅ Kullanıcı onaylandı ve erişim verildi!' : 'İşlem tamamlandı.');
      loadSalesmenData();
    } else {
      alert('❌ Durum güncellenemedi.');
    }
  } catch (err) {
    alert('❌ Hata: ' + err);
  }
}
window.changeUserStatus = changeUserStatus;

async function loadWebUsers() {
  const tbody = getEl('webUsersTableBody');
  if (!tbody) return;

  try {
    const res = await fetch(API_BASE + '/webusers');
    if (!res.ok) return;
    const users = await res.json();

    if (!Array.isArray(users) || users.length === 0) {
      tbody.innerHTML = '<tr><td colspan="5" style="padding:1rem; text-align:center; color:#94a3b8;">Kayıtlı web paneli kullanıcısı bulunamadı.</td></tr>';
      return;
    }

    tbody.innerHTML = users.map(u => `
      <tr style="border-bottom:1px solid #f1f5f9;">
        <td style="padding:0.75rem; font-weight:700; color:#2563eb;">${u.username}</td>
        <td style="padding:0.75rem; font-weight:600;">${u.fullName || '-'}</td>
        <td style="padding:0.75rem;"><span class="status-badge running" style="font-size:0.75rem;">${u.role || 'Admin'}</span></td>
        <td style="padding:0.75rem; font-size:0.78rem; color:#64748b;">Canlı Panel, Telegram Yetkileri, Mail/SMS, XSLT, Admin Ayarları</td>
        <td style="padding:0.75rem;">
          <button class="btn btn-secondary btn-sm" style="padding:0.3rem 0.6rem; font-size:0.78rem;">Düzenle</button>
          <button class="btn btn-danger btn-sm" style="padding:0.3rem 0.6rem; font-size:0.78rem;">Sil</button>
        </td>
      </tr>
    `).join('');
  } catch (err) {
    tbody.innerHTML = '<tr><td colspan="5" style="padding:1rem; text-align:center; color:#ef4444;">Yükleme hatası: ' + err + '</td></tr>';
  }
}
window.loadWebUsers = loadWebUsers;

// INIT ON DOM LOADED
document.addEventListener('DOMContentLoaded', () => {
  checkBotStatus();
  loadSettings();
  loadSalesmenData();
  loadWebUsers();
  setInterval(checkBotStatus, 15000);
});


// TELEGRAM USER PERMISSIONS MODAL HANDLERS
let currentTelegramUsersList = [];

async function approveUser(chatId) {
  openTelegramPermissionsModal(chatId);
}
window.approveUser = approveUser;

async function openTelegramPermissionsModal(chatId) {
  try {
    const res = await fetch(API_BASE + '/salesmen/telegram-users');
    if (!res.ok) return;
    const users = await res.json();
    currentTelegramUsersList = users;
    const u = users.find(x => x.chatId === chatId);

    if (!u) {
      alert('Kullanıcı bilgisi bulunamadı!');
      return;
    }

    setVal('permChatId', u.chatId);
    setText('permModalTitle', '🔑 Telegram Kullanıcı Yetkileri: ' + (u.fullName || 'Chat ID: ' + u.chatId));

    setCheck('chkPermApproved', u.isApproved !== false);
    setCheck('chkPermBlocked', u.isBlocked === true);

    setVal('txtPermFirmCyphCode', u.allowedFirmCyphCode || '');
    setVal('txtPermItemCyphCode', u.allowedItemCyphCode || '');
    setVal('txtPermOrderCyphCode', u.allowedOrderCyphCode || '');

    setCheck('chkPermCanViewAllFirms', u.canViewAllFirms !== false);
    setCheck('chkPermCanViewAllItems', u.canViewAllItems !== false);
    setCheck('chkPermCanViewAllOrders', u.canViewAllOrders !== false);

    setCheck('chkPermCanCreateSalesOrder', u.canCreateSalesOrder !== false);
    setCheck('chkPermCanViewSalesOrders', u.canViewSalesOrders !== false);
    setCheck('chkPermCanCreateDispatch', u.canCreateDispatch !== false);
    setCheck('chkPermCanViewSalesDispatches', u.canViewSalesDispatches !== false);
    setCheck('chkPermCanCreateSalesInvoice', u.canCreateSalesInvoice === true);
    setCheck('chkPermCanViewSalesInvoices', u.canViewSalesInvoices !== false);

    setCheck('chkPermCanViewPurchaseOrders', u.canViewPurchaseOrders === true);
    setCheck('chkPermCanViewPurchaseDispatches', u.canViewPurchaseDispatches === true);
    setCheck('chkPermCanViewPurchaseInvoices', u.canViewPurchaseInvoices === true);
    setCheck('chkPermCanSendEBelge', u.canSendEBelge === true);
    setCheck('chkPermCanCreateCollection', u.canCreateCollection !== false);
    setCheck('chkPermCanViewSalesmanTarget', u.canViewSalesmanTarget !== false);
    setCheck('chkPermCanSendSms', u.canSendSms !== false);
    setCheck('chkPermCanSendEmail', u.canSendEmail !== false);

    const modal = getEl('telegramUserPermissionsModal');
    if (modal) modal.style.display = 'flex';
  } catch (err) {
    alert('Yetki bilgileri yüklenemedi: ' + err);
  }
}
window.openTelegramPermissionsModal = openTelegramPermissionsModal;

function closeTelegramPermissionsModal() {
  const modal = getEl('telegramUserPermissionsModal');
  if (modal) modal.style.display = 'none';
}
window.closeTelegramPermissionsModal = closeTelegramPermissionsModal;

async function saveTelegramPermissionsModal() {
  const chatId = parseInt(getVal('permChatId'));
  if (!chatId) return;

  const isApproved = getEl('chkPermApproved') ? getEl('chkPermApproved').checked : true;
  const isBlocked = getEl('chkPermBlocked') ? getEl('chkPermBlocked').checked : false;

  const payload = {
    ChatId: chatId,
    IsApproved: isApproved,
    IsBlocked: isBlocked,
    AllowedFirmCyphCode: getVal('txtPermFirmCyphCode'),
    AllowedItemCyphCode: getVal('txtPermItemCyphCode'),
    AllowedOrderCyphCode: getVal('txtPermOrderCyphCode'),
    CanViewAllFirms: getEl('chkPermCanViewAllFirms') ? getEl('chkPermCanViewAllFirms').checked : true,
    CanViewAllItems: getEl('chkPermCanViewAllItems') ? getEl('chkPermCanViewAllItems').checked : true,
    CanViewAllOrders: getEl('chkPermCanViewAllOrders') ? getEl('chkPermCanViewAllOrders').checked : true,
    CanCreateSalesOrder: getEl('chkPermCanCreateSalesOrder') ? getEl('chkPermCanCreateSalesOrder').checked : true,
    CanViewSalesOrders: getEl('chkPermCanViewSalesOrders') ? getEl('chkPermCanViewSalesOrders').checked : true,
    CanCreateDispatch: getEl('chkPermCanCreateDispatch') ? getEl('chkPermCanCreateDispatch').checked : true,
    CanViewSalesDispatches: getEl('chkPermCanViewSalesDispatches') ? getEl('chkPermCanViewSalesDispatches').checked : true,
    CanCreateSalesInvoice: getEl('chkPermCanCreateSalesInvoice') ? getEl('chkPermCanCreateSalesInvoice').checked : false,
    CanViewSalesInvoices: getEl('chkPermCanViewSalesInvoices') ? getEl('chkPermCanViewSalesInvoices').checked : true,
    CanViewPurchaseOrders: getEl('chkPermCanViewPurchaseOrders') ? getEl('chkPermCanViewPurchaseOrders').checked : false,
    CanViewPurchaseDispatches: getEl('chkPermCanViewPurchaseDispatches') ? getEl('chkPermCanViewPurchaseDispatches').checked : false,
    CanViewPurchaseInvoices: getEl('chkPermCanViewPurchaseInvoices') ? getEl('chkPermCanViewPurchaseInvoices').checked : false,
    CanSendEBelge: getEl('chkPermCanSendEBelge') ? getEl('chkPermCanSendEBelge').checked : false,
    CanCreateCollection: getEl('chkPermCanCreateCollection') ? getEl('chkPermCanCreateCollection').checked : true,
    CanViewSalesmanTarget: getEl('chkPermCanViewSalesmanTarget') ? getEl('chkPermCanViewSalesmanTarget').checked : true,
    CanSendSms: getEl('chkPermCanSendSms') ? getEl('chkPermCanSendSms').checked : true,
    CanSendEmail: getEl('chkPermCanSendEmail') ? getEl('chkPermCanSendEmail').checked : true
  };

  try {
    const res = await fetch(API_BASE + '/salesmen/update-permissions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json();
    alert((data.isSuccess ? '✅ BAŞARILI: ' : '❌ HATA: ') + data.message);
    closeTelegramPermissionsModal();
    loadSalesmenData();
  } catch (err) {
    alert('❌ Yetkiler kaydedilirken hata oluştu: ' + err);
  }
}
window.saveTelegramPermissionsModal = saveTelegramPermissionsModal;

// XSLT MANAGEMENT HELPERS
function loadXsltFromFile(inputEl, targetTextAreaId) {
  if (!inputEl || !inputEl.files || inputEl.files.length === 0) return;
  const file = inputEl.files[0];
  const reader = new FileReader();
  reader.onload = function (e) {
    const textarea = document.getElementById(targetTextAreaId);
    if (textarea) {
      textarea.value = e.target.result;
      alert(`📄 '${file.name}' dosyası metin kutusuna yüklendi! Lütfen 'Kaydet & Uygula' butonuna basarak sunucuya kaydediniz.`);
    }
  };
  reader.readAsText(file);
}
window.loadXsltFromFile = loadXsltFromFile;

async function fetchXsltFromServer(type, targetTextAreaId) {
  try {
    const res = await fetch(`${API_BASE}/settings/xslt?type=${type}`);
    if (!res.ok) return;
    const data = await res.json();
    if (data.isSuccess && data.content) {
      const textarea = document.getElementById(targetTextAreaId);
      if (textarea) textarea.value = data.content;
    }
  } catch (err) {
    console.error('XSLT fetch error:', err);
  }
}
window.fetchXsltFromServer = fetchXsltFromServer;

async function saveXsltToServer(type, sourceTextAreaId) {
  const textarea = document.getElementById(sourceTextAreaId);
  if (!textarea) return;
  const content = textarea.value;
  if (!content || !content.trim()) {
    alert('❌ XSLT içeriği boş olamaz!');
    return;
  }

  try {
    const res = await fetch(`${API_BASE}/settings/xslt`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ type: type, content: content })
    });
    const data = await res.json();
    alert((data.isSuccess ? '✅ ' : '❌ ') + data.message);
  } catch (err) {
    alert('❌ XSLT kaydedilirken hata oluştu: ' + err);
  }
}
window.saveXsltToServer = saveXsltToServer;

document.addEventListener('DOMContentLoaded', () => {
  setTimeout(() => {
    fetchXsltFromServer('irsaliye', 'xsltCodeEditorMain');
    fetchXsltFromServer('fatura', 'xsltEFaturaEditor');
    fetchXsltFromServer('arsiv', 'xsltEArsivEditor');
  }, 500);
});



async function saveSettings() {
  const resultEl = getEl('saveSettingsResult');
  if (resultEl) { resultEl.style.display = 'none'; }

  const payload = {
    server:               getVal('cfgServer'),
    database:             getVal('cfgDatabase'),
    userId:               getVal('cfgUserId'),
    password:             getVal('cfgPassword'),
    trustServerCertificate: true,
    botToken:             getVal('cfgBotToken'),
    targetSystem:         getEl('cfgTargetSystem') ? getEl('cfgTargetSystem').value : 'ERP',
    firmNo:               getVal('cfgFirmNo'),
    periodNo:             getVal('cfgPeriodNo'),
    warehouseNo:          getVal('cfgWarehouseNo'),
    documentYear:         getVal('cfgDocumentYear'),
    invoiceFichePrefix:   getVal('cfgInvoiceFichePrefix'),
    dispatchFichePrefix:  getVal('cfgDispatchFichePrefix'),
    orderFichePrefix:     getVal('cfgOrderFichePrefix'),
    aiApiKey:             getVal('cfgAiApiKey'),
    aiProvider:           getEl('cfgAiProvider') ? getEl('cfgAiProvider').value : 'Groq',
    aiModel:              getVal('cfgAiModel'),
    aiIsEnabled:          getEl('cfgAiIsEnabled') ? getEl('cfgAiIsEnabled').checked : true
  };

  try {
    const res = await fetch(API_BASE + '/settings', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(payload)
    });
    const data = await res.json();

    if (resultEl) {
      resultEl.style.display = 'block';
      resultEl.style.background = data.isSuccess ? '#dcfce7' : '#fee2e2';
      resultEl.style.color = data.isSuccess ? '#166534' : '#991b1b';
      resultEl.style.border = `1px solid ${data.isSuccess ? '#86efac' : '#fca5a5'}`;
      resultEl.style.padding = '0.75rem 1rem';
      resultEl.style.borderRadius = '8px';
      resultEl.textContent = (data.isSuccess ? '✅ ' : '❌ ') + data.message;
    } else {
      alert((data.isSuccess ? '✅ ' : '❌ ') + data.message);
    }
  } catch (err) {
    if (resultEl) {
      resultEl.style.display = 'block';
      resultEl.style.background = '#fee2e2';
      resultEl.style.color = '#991b1b';
      resultEl.textContent = '❌ Ayarlar kaydedilemedi: ' + err;
    } else {
      alert('❌ Ayarlar kaydedilemedi: ' + err);
    }
  }
}
window.saveSettings = saveSettings;

async function testDbConnection() {
  const resultEl = getEl('testDbResult');
  if (resultEl) {
    resultEl.style.display = 'block';
    resultEl.style.background = '#eff6ff';
    resultEl.style.color = '#1d4ed8';
    resultEl.style.border = '1px solid #93c5fd';
    resultEl.style.padding = '0.75rem 1rem';
    resultEl.style.borderRadius = '8px';
    resultEl.textContent = '⏳ Bağlantı test ediliyor...';
  }

  const payload = {
    server:   getVal('cfgServer'),
    database: getVal('cfgDatabase'),
    userId:   getVal('cfgUserId'),
    password: getVal('cfgPassword'),
    trustServerCertificate: true,
    targetSystem: getEl('cfgTargetSystem') ? getEl('cfgTargetSystem').value : 'ERP',
    firmNo:   getVal('cfgFirmNo')
  };

  try {
    const res = await fetch(API_BASE + '/settings/test-db', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(payload)
    });
    const data = await res.json();

    if (resultEl) {
      resultEl.style.background = data.isSuccess ? '#dcfce7' : '#fee2e2';
      resultEl.style.color     = data.isSuccess ? '#166534' : '#991b1b';
      resultEl.style.border    = `1px solid ${data.isSuccess ? '#86efac' : '#fca5a5'}`;
      resultEl.style.whiteSpace = 'pre-line';
      resultEl.textContent = (data.isSuccess ? '✅ ' : '❌ ') + data.message;
    }
  } catch (err) {
    if (resultEl) {
      resultEl.style.background = '#fee2e2';
      resultEl.style.color = '#991b1b';
      resultEl.textContent = '❌ Test başarısız: ' + err;
    }
  }
}
window.testDbConnection = testDbConnection;

async function testTelegramBotConnection() {
  const resultEl = getEl('testTelegramResult');
  if (resultEl) {
    resultEl.style.display = 'block';
    resultEl.style.background = '#eff6ff';
    resultEl.style.color = '#1d4ed8';
    resultEl.style.border = '1px solid #93c5fd';
    resultEl.style.padding = '0.75rem 1rem';
    resultEl.style.borderRadius = '8px';
    resultEl.textContent = '⏳ Telegram bağlantısı test ediliyor...';
  }

  const payload = { botToken: getVal('cfgBotToken') };

  try {
    const res = await fetch(API_BASE + '/settings/test-telegram', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(payload)
    });
    const data = await res.json();

    if (resultEl) {
      resultEl.style.background = data.isSuccess ? '#dcfce7' : '#fee2e2';
      resultEl.style.color     = data.isSuccess ? '#166534' : '#991b1b';
      resultEl.style.border    = `1px solid ${data.isSuccess ? '#86efac' : '#fca5a5'}`;
      resultEl.style.whiteSpace = 'pre-line';
      resultEl.textContent = (data.isSuccess ? '✅ ' : '❌ ') + data.message;
    }
  } catch (err) {
    if (resultEl) {
      resultEl.style.background = '#fee2e2';
      resultEl.style.color = '#991b1b';
      resultEl.textContent = '❌ Test başarısız: ' + err;
    }
  }
}
window.testTelegramBotConnection = testTelegramBotConnection;

function onAiApiKeyInput() {
  const key = (getVal('cfgAiApiKey') || '').trim();
  const providerEl = getEl('cfgAiProvider');
  const modelEl = getEl('cfgAiModel');

  if (key.startsWith('AQ.') || key.startsWith('AIza')) {
    if (providerEl) providerEl.value = 'Gemini';
    if (modelEl && (!modelEl.value || modelEl.value.includes('llama') || modelEl.value.includes('gpt'))) {
      modelEl.value = 'gemini-3.6-flash';
    }
  } else if (key.startsWith('gsk_')) {
    if (providerEl) providerEl.value = 'Groq';
    if (modelEl && (!modelEl.value || modelEl.value.includes('gemini') || modelEl.value.includes('gpt'))) {
      modelEl.value = 'llama-3.3-70b-versatile';
    }
  } else if (key.startsWith('sk-')) {
    if (providerEl && providerEl.value === 'Gemini') providerEl.value = 'OpenAI';
    if (modelEl && (!modelEl.value || modelEl.value.includes('gemini') || modelEl.value.includes('llama'))) {
      modelEl.value = 'gpt-4o-mini';
    }
  }
}
window.onAiApiKeyInput = onAiApiKeyInput;

function onAiProviderChange() {
  const provider = getEl('cfgAiProvider') ? getEl('cfgAiProvider').value : 'Gemini';
  const modelEl = getEl('cfgAiModel');
  if (!modelEl) return;

  if (provider === 'Gemini') modelEl.value = 'gemini-3.6-flash';
  else if (provider === 'Groq') modelEl.value = 'llama-3.3-70b-versatile';
  else if (provider === 'OpenAI') modelEl.value = 'gpt-4o-mini';
  else if (provider === 'DeepSeek') modelEl.value = 'deepseek-chat';
}
window.onAiProviderChange = onAiProviderChange;

async function testAiTokenConnection() {
  const resultEl = getEl('testAiResult');
  let key = (getVal('cfgAiApiKey') || '').trim();
  let provider = getEl('cfgAiProvider') ? getEl('cfgAiProvider').value : 'Gemini';
  let model = getVal('cfgAiModel') || '';

  if (key.startsWith('AQ.') || key.startsWith('AIza')) {
    provider = 'Gemini';
    if (!model || model.includes('gpt') || model.includes('llama')) model = 'gemini-3.6-flash';
  } else if (key.startsWith('gsk_')) {
    provider = 'Groq';
    if (!model || model.includes('gpt') || model.includes('gemini')) model = 'llama-3.3-70b-versatile';
  } else if (key.startsWith('sk-')) {
    if (provider === 'Gemini') provider = 'OpenAI';
    if (!model || model.includes('gemini') || model.includes('llama')) model = 'gpt-4o-mini';
  }

  if (resultEl) {
    resultEl.style.display = 'block';
    resultEl.style.background = '#f3e8ff';
    resultEl.style.color = '#6b21a8';
    resultEl.style.border = '1px solid #d8b4fe';
    resultEl.style.padding = '0.75rem 1rem';
    resultEl.style.borderRadius = '8px';
    resultEl.textContent = `⏳ Yapay Zeka (${provider} - ${model}) test ediliyor...`;
  }

  const payload = {
    apiKey: key,
    provider: provider,
    model: model
  };

  try {
    const res = await fetch(API_BASE + '/settings/test-ai', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(payload)
    });
    const data = await res.json();

    if (resultEl) {
      resultEl.style.background = data.isSuccess ? '#dcfce7' : '#fee2e2';
      resultEl.style.color     = data.isSuccess ? '#166534' : '#991b1b';
      resultEl.style.border    = `1px solid ${data.isSuccess ? '#86efac' : '#fca5a5'}`;
      resultEl.style.whiteSpace = 'pre-line';
      resultEl.textContent = (data.isSuccess ? '✅ ' : '❌ ') + data.message;
    }
  } catch (err) {
    if (resultEl) {
      resultEl.style.background = '#fee2e2';
      resultEl.style.color = '#991b1b';
      resultEl.textContent = '❌ AI Test başarısız: ' + err;
    }
  }
}
window.testAiTokenConnection = testAiTokenConnection;

function togglePasswordVisibility(inputId) {
  const input = getEl(inputId);
  if (!input) return;
  if (input.type === 'password') {
    input.type = 'text';
  } else {
    input.type = 'password';
  }
}
window.togglePasswordVisibility = togglePasswordVisibility;

// ==========================================
// AKILLI RAPORLAR & SQL YÖNETİMİ
// ==========================================
let allCustomReports = [];
let currentReportCategoryFilter = 'ALL';

async function loadReports() {
  const container = getEl('reportsListContainer');
  if (!container) return;
  container.innerHTML = '<div style="grid-column:1/-1; text-align:center; padding:2rem; color:#64748b;"><i class="fa-solid fa-spinner fa-spin fa-2x"></i><br>Raporlar yükleniyor...</div>';

  try {
    const res = await fetch(API_BASE + '/reports', { credentials: 'omit' });
    if (!res.ok) throw new Error('Rapor listesi alınamadı: ' + res.statusText);
    allCustomReports = await res.json();
    renderReportsList();
  } catch (err) {
    container.innerHTML = `<div style="grid-column:1/-1; color:#ef4444; padding:1.5rem; text-align:center;">❌ Raporlar yüklenemedi: ${err.message}</div>`;
  }
}
window.loadReports = loadReports;

function filterReportsCategory(category, btnEl) {
  currentReportCategoryFilter = category;
  if (btnEl && btnEl.parentElement) {
    btnEl.parentElement.querySelectorAll('.btn').forEach(b => b.classList.remove('active'));
    btnEl.classList.add('active');
  }
  renderReportsList();
}
window.filterReportsCategory = filterReportsCategory;

function renderReportsList() {
  const container = getEl('reportsListContainer');
  if (!container) return;

  const filtered = currentReportCategoryFilter === 'ALL'
    ? allCustomReports
    : allCustomReports.filter(r => (r.category || '').toLowerCase() === currentReportCategoryFilter.toLowerCase());

  if (filtered.length === 0) {
    container.innerHTML = `<div style="grid-column:1/-1; background:#ffffff; border-radius:12px; padding:3rem; text-align:center; color:#64748b; border:1px dashed #cbd5e1;">
      <i class="fa-solid fa-folder-open fa-3x" style="color:#94a3b8; margin-bottom:1rem;"></i>
      <h3 style="font-size:1.1rem; font-weight:700; color:#334155;">Bu kategoride tanımlı rapor bulunamadı.</h3>
      <p style="font-size:0.85rem; color:#64748b; margin-top:0.4rem;">Sağ üstteki <strong>'Yeni SQL Raporu Ekle'</strong> butonuna basarak ilk raporunuzu tanımlayabilirsiniz.</p>
    </div>`;
    return;
  }

  let html = '';
  filtered.forEach(r => {
    const badgeColor = r.isActive ? '#10b981' : '#94a3b8';
    const firmamBadge = r.showInFirmamMenu ? '<span style="font-size:0.75rem; background:#ede9fe; color:#6b21a8; padding:0.2rem 0.5rem; border-radius:12px; font-weight:700;">🏢 firmam</span>' : '';
    
    html += `
      <div class="card" style="margin-bottom:0; display:flex; flex-direction:column; justify-content:space-between; border: 1px solid #e2e8f0; border-left: 4px solid #7c3aed; transition: transform 0.2s, box-shadow 0.2s;">
        <div>
          <div style="display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:0.6rem;">
            <div style="font-size:1.05rem; font-weight:800; color:#0f172a;">
              ${r.title || 'Başlıksız Rapor'}
            </div>
            <div style="display:flex; gap:0.3rem; align-items:center;">
              ${firmamBadge}
              <span style="font-size:0.75rem; background:#f1f5f9; color:#475569; padding:0.2rem 0.5rem; border-radius:12px; font-weight:600;">${r.category}</span>
            </div>
          </div>

          <p style="font-size:0.84rem; color:#64748b; margin-bottom:0.75rem; line-height:1.4;">
            ${r.description || 'Açıklama belirtilmemiş.'}
          </p>

          <div style="background:#0f172a; color:#38bdf8; padding:0.6rem; border-radius:6px; font-family:monospace; font-size:0.75rem; max-height:80px; overflow:hidden; text-overflow:ellipsis; white-space:pre-wrap; margin-bottom:0.75rem;">
            ${r.sqlQuery ? r.sqlQuery.substring(0, 120) + (r.sqlQuery.length > 120 ? '...' : '') : 'SQL belirtilmemiş'}
          </div>

          <div style="font-size:0.8rem; color:#475569; margin-bottom:0.5rem;">
            <strong>Telegram Butonu:</strong> <code>${r.telegramButtonText || r.title}</code>
          </div>
        </div>

        <div style="display:flex; justify-content:space-between; align-items:center; border-top:1px solid #f1f5f9; padding-top:0.75rem; margin-top:0.5rem;">
          <div>
            <span style="display:inline-block; width:8px; height:8px; border-radius:50%; background:${badgeColor}; margin-right:4px;"></span>
            <span style="font-size:0.8rem; font-weight:600; color:${badgeColor};">${r.isActive ? 'Aktif' : 'Pasif'}</span>
          </div>
          <div style="display:flex; gap:0.4rem;">
            <button class="btn btn-secondary btn-sm" onclick="editReport('${r.id}')" title="Düzenle" style="padding:0.4rem 0.8rem; font-size:0.8rem;">
              <i class="fa-pen-to-square"></i> Düzenle
            </button>
            <button class="btn btn-danger btn-sm" onclick="deleteReport('${r.id}')" title="Sil" style="padding:0.4rem 0.8rem; font-size:0.8rem;">
              <i class="fa-trash"></i>
            </button>
          </div>
        </div>
      </div>
    `;
  });

  container.innerHTML = html;
}

function openAddReportModal() {
  setText('reportModalTitle', '<i class="fa-solid fa-code" style="color:#7c3aed;"></i> Yeni SQL Raporu Tanımla');
  setVal('rptId', '');
  setVal('rptTitle', '');
  setVal('rptCategory', 'Kasa');
  setVal('rptButtonText', '');
  setVal('rptOrder', '1');
  setVal('rptDescription', '');
  setVal('rptSqlQuery', 'SELECT TOP 10 * FROM LG_{firmNo}_CLCARD');
  if (getEl('rptShowFirmam')) getEl('rptShowFirmam').checked = true;
  if (getEl('rptIsActive')) getEl('rptIsActive').checked = true;
  if (getEl('testSqlResultBox')) getEl('testSqlResultBox').style.display = 'none';

  getEl('reportModal').style.display = 'flex';
}
window.openAddReportModal = openAddReportModal;

function editReport(id) {
  const r = allCustomReports.find(x => x.id === id);
  if (!r) return;

  setText('reportModalTitle', '<i class="fa-solid fa-pen-to-square" style="color:#7c3aed;"></i> SQL Raporunu Düzenle');
  setVal('rptId', r.id || '');
  setVal('rptTitle', r.title || '');
  setVal('rptCategory', r.category || 'Genel');
  setVal('rptButtonText', r.telegramButtonText || '');
  setVal('rptOrder', r.displayOrder || '1');
  setVal('rptDescription', r.description || '');
  setVal('rptSqlQuery', r.sqlQuery || '');
  if (getEl('rptShowFirmam')) getEl('rptShowFirmam').checked = (r.showInFirmamMenu !== false);
  if (getEl('rptIsActive')) getEl('rptIsActive').checked = (r.isActive !== false);
  if (getEl('testSqlResultBox')) getEl('testSqlResultBox').style.display = 'none';

  getEl('reportModal').style.display = 'flex';
}
window.editReport = editReport;

function closeReportModal() {
  if (getEl('reportModal')) getEl('reportModal').style.display = 'none';
}
window.closeReportModal = closeReportModal;

async function saveReportModal() {
  const payload = {
    id: getVal('rptId'),
    title: getVal('rptTitle'),
    category: getVal('rptCategory'),
    telegramButtonText: getVal('rptButtonText'),
    displayOrder: parseInt(getVal('rptOrder') || '0', 10),
    description: getVal('rptDescription'),
    sqlQuery: getVal('rptSqlQuery'),
    showInFirmamMenu: getEl('rptShowFirmam') ? getEl('rptShowFirmam').checked : true,
    isActive: getEl('rptIsActive') ? getEl('rptIsActive').checked : true
  };

  if (!payload.title || !payload.title.trim()) {
    alert('❌ Lütfen rapor başlığı giriniz.');
    return;
  }
  if (!payload.sqlQuery || !payload.sqlQuery.trim()) {
    alert('❌ Lütfen T-SQL sorgusu giriniz.');
    return;
  }

  try {
    const res = await fetch(API_BASE + '/reports', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(payload)
    });
    const data = await res.json();
    if (data.isSuccess) {
      alert('✅ ' + data.message);
      closeReportModal();
      loadReports();
    } else {
      alert('❌ ' + data.message);
    }
  } catch (err) {
    alert('❌ Rapor kaydedilirken hata oluştu: ' + err);
  }
}
window.saveReportModal = saveReportModal;

async function deleteReport(id) {
  if (!confirm('Bu raporu silmek istediğinize emin misiniz?')) return;

  try {
    const res = await fetch(API_BASE + '/reports/' + id, {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    const data = await res.json();
    if (data.isSuccess) {
      alert('✅ ' + data.message);
      loadReports();
    } else {
      alert('❌ ' + data.message);
    }
  } catch (err) {
    alert('❌ Rapor silinirken hata oluştu: ' + err);
  }
}
window.deleteReport = deleteReport;

async function loadMainReportHistory() {
  const tbody = getEl('mainReportHistoryTableBody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="8" style="text-align:center; padding:1.5rem; color:#94a3b8;"><i class="fa-solid fa-spinner fa-spin"></i> Tarihçe yükleniyor...</td></tr>';

  try {
    const res = await fetch(API_BASE + '/reports/history', { headers: getAuthHeaders() });
    if (!res.ok) throw new Error('Geçmiş yüklenemedi.');
    const history = await res.json();

    if (!history || history.length === 0) {
      tbody.innerHTML = '<tr><td colspan="8" style="text-align:center; padding:1.5rem; color:#94a3b8;">Henüz çalıştırılmış bir rapor veya AI sorgusu bulunmuyor.</td></tr>';
      return;
    }

    tbody.innerHTML = history.map(item => {
      const dt = new Date(item.executedAt);
      const dtStr = dt.toLocaleDateString('tr-TR') + ' ' + dt.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
      const statusBadge = item.isSuccess 
        ? '<span style="color:#16a34a; font-weight:700;">✅ Başarılı</span>'
        : '<span style="color:#ef4444; font-weight:700;">❌ Hata</span>';

      return `
        <tr>
          <td><code style="font-size:0.8rem;">${dtStr}</code></td>
          <td><strong>${item.reportTitle}</strong></td>
          <td><span style="padding:0.15rem 0.5rem; background:#f1f5f9; border-radius:12px; font-size:0.75rem; font-weight:700;">${item.category || 'Genel'}</span></td>
          <td><code>${item.executedBy || 'Sistem'}</code></td>
          <td>${item.durationMs || 0} ms</td>
          <td><strong>${item.rowCount || 0}</strong></td>
          <td>${statusBadge}</td>
          <td>
            <button class="btn btn-secondary btn-sm" style="padding:0.15rem 0.4rem; font-size:0.75rem;" onclick="alert('SQL:\\n' + ${JSON.stringify(item.sqlQuery || '-')} + '\\n\\nSonuç:\\n' + ${JSON.stringify(item.resultPreviewJson || item.errorMessage || 'Veri yok')})">
              <i class="fa-solid fa-eye"></i> Detay
            </button>
          </td>
        </tr>
      `;
    }).join('');
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="8" style="text-align:center; padding:1.5rem; color:#ef4444;">❌ Hata: ${err.message}</td></tr>`;
  }
}
window.loadMainReportHistory = loadMainReportHistory;

async function clearMainReportHistory() {
  if (!confirm('Tüm rapor ve yapay zeka sorgu geçmişini temizlemek istediğinize emin misiniz?')) return;

  try {
    const res = await fetch(API_BASE + '/reports/history', {
      method: 'DELETE',
      headers: getAuthHeaders()
    });
    const data = await res.json();
    alert('✅ ' + (data.message || 'Geçmiş temizlendi.'));
    loadMainReportHistory();
  } catch (err) {
    alert('❌ Geçmiş temizlenirken hata: ' + err.message);
  }
}
async function testCustomReportSql() {
  const box = getEl('testSqlResultBox');
  const sql = getVal('rptSqlQuery');
  if (!box || !sql || !sql.trim()) {
    alert('❌ Lütfen test edilecek bir SQL sorgusu giriniz.');
    return;
  }

  box.style.display = 'block';
  box.innerHTML = '<span style="color:#2563eb;"><i class="fa-solid fa-spinner fa-spin"></i> SQL sorgusu veritabanında test ediliyor...</span>';

  try {
    const res = await fetch(API_BASE + '/reports/test-sql', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({
        sqlQuery: sql,
        firmNo: getVal('cfgFirmNo') || '001',
        periodNo: getVal('cfgPeriodNo') || '01'
      })
    });
    const data = await res.json();
    if (data.isSuccess) {
      box.innerHTML = `<span style="color:#16a34a; font-weight:700;">✅ SQL Test Başarılı! (${data.durationMs || 0} ms, ${data.rowCount || 0} satır)</span>\n\n<strong style="color:#475569;">Çözülen SQL:</strong>\n${data.resolvedSql}\n\n<strong style="color:#475569;">Sonuç (JSON):</strong>\n${data.resultJson}`;
    } else {
      box.innerHTML = `<span style="color:#dc2626; font-weight:700;">❌ Test Hatası:</span>\n${data.message}\n\n<strong style="color:#475569;">Çözülen SQL:</strong>\n${data.resolvedSql || sql}`;
    }
  } catch (err) {
    box.innerHTML = `<span style="color:#dc2626; font-weight:700;">❌ İstek Hatası:</span>\n${err}`;
  }
}
window.testCustomReportSql = testCustomReportSql;



