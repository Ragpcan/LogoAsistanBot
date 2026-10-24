const express = require('express');
const cors = require('cors');
const qrcode = require('qrcode');
const pino = require('pino');
const path = require('path');
const fs = require('fs');
const http = require('http');

const {
    default: makeWASocket,
    useMultiFileAuthState,
    DisconnectReason,
    fetchLatestBaileysVersion
} = require('@whiskeysockets/baileys');

const PORT = 5001;
const ERP_API_BASE = 'http://localhost:5000/api';
const AUTH_DIR = path.join(__dirname, 'auth_session');

if (!fs.existsSync(AUTH_DIR)) {
    fs.mkdirSync(AUTH_DIR, { recursive: true });
}

const app = express();
app.use(cors());
app.use(express.json());

let sock = null;
let currentQrDataUrl = null;
let isConnected = false;
let userPhone = null;

// ==========================================
// BAILEYS WHATSAPP SOCKET ENGINE
// ==========================================
async function initWhatsApp() {
    try {
        const { state, saveCreds } = await useMultiFileAuthState(AUTH_DIR);
        const { version } = await fetchLatestBaileysVersion();

        const logger = pino({ level: 'silent' });

        sock = makeWASocket({
            version,
            logger,
            auth: state,
            printQRInTerminal: false,
            browser: ['Logo Asistan Bot', 'Chrome', '124.0.0.0']
        });

        sock.ev.on('creds.update', saveCreds);

        sock.ev.on('connection.update', async (update) => {
            const { connection, lastDisconnect, qr } = update;

            if (qr) {
                try {
                    currentQrDataUrl = await qrcode.toDataURL(qr);
                    isConnected = false;
                    console.log(' [WhatsApp Bridge] Yeni QR kod üretildi. Portaldan okutabilirsiniz.');
                } catch (err) {
                    console.error('QR kod üretme hatası:', err);
                }
            }

            if (connection === 'close') {
                const shouldReconnect = (lastDisconnect?.error?.output?.statusCode !== DisconnectReason.loggedOut);
                isConnected = false;
                userPhone = null;
                console.log(` [WhatsApp Bridge] Bağlantı kapandı. Yeniden bağlanılıyor mu: ${shouldReconnect}`);

                if (shouldReconnect) {
                    setTimeout(() => initWhatsApp(), 4000);
                } else {
                    currentQrDataUrl = null;
                    console.log(' [WhatsApp Bridge] Oturum sonlandırıldı (Logged out).');
                }
            } else if (connection === 'open') {
                isConnected = true;
                currentQrDataUrl = null;
                userPhone = sock.user?.id ? sock.user.id.split(':')[0] : 'Aktif';
                console.log(` [WhatsApp Bridge] Bağlantı BAŞARILI! Bağlı numara: +${userPhone}`);
            }
        });

        // ==========================================
        // GELEN MESAJLARI DİNLE VE OTO CEVAP VER
        // ==========================================
        sock.ev.on('messages.upsert', async (m) => {
            try {
                if (m.type !== 'notify') return;
                const msg = m.messages[0];
                if (!msg || msg.key.fromMe) return;

                const remoteJid = msg.key.remoteJid;
                if (!remoteJid || remoteJid.endsWith('@g.us')) return; // Grupları yoksay

                const senderPhone = remoteJid.split('@')[0];
                const text = (msg.message?.conversation || msg.message?.extendedTextMessage?.text || '').trim();
                if (!text) return;

                console.log(` [WhatsApp Bridge] Mesaj geldi: +${senderPhone} -> "${text}"`);

                const lower = text.toLowerCase();

                // 1. Bakiye / Vade / Fatura Sorguları
                if (lower.includes('bakiye') || lower.includes('vade') || lower.includes('ekstre') || lower.includes('fatura') || lower.includes('borc') || lower.includes('borç')) {
                    const reply = await queryCustomerInvoices(senderPhone);
                    await sock.sendMessage(remoteJid, { text: reply });
                }
                // 2. Selamlama / Yardım
                else if (lower.includes('merhaba') || lower.includes('selam') || lower.includes('yardım') || lower.includes('bilgi')) {
                    const welcomeMsg = `🤖 *Logo Asistan Bot WhatsApp Hattı*\n\nMerhaba! Muhasebe ve bakiye sorgulama asistanına hoş geldiniz.\n\n📌 *Kullanabileceğiniz Komutlar:*\n• *bakiye* veya *vade*: Açık fatura ve son ödeme tarihlerini listeler.\n• *ekstre*: Güncel hesap durumunuzu gösterir.\n\nYetkili personelimizle görüşmek için lütfen mesajınızı yazınız.`;
                    await sock.sendMessage(remoteJid, { text: welcomeMsg });
                }
            } catch (err) {
                console.error(' [WhatsApp Bridge] Gelen mesaj işleme hatası:', err.message);
            }
        });

    } catch (err) {
        console.error(' [WhatsApp Bridge] Başlatma hatası:', err);
        setTimeout(() => initWhatsApp(), 5000);
    }
}

// Logo ERP API'sinden müşterinin faturalarını çekip metin formatlar
async function queryCustomerInvoices(phone) {
    try {
        // Türkiye telefon standardizasyonu
        let searchPhone = phone;
        if (searchPhone.startsWith('90')) searchPhone = searchPhone.substring(2);
        if (searchPhone.startsWith('0')) searchPhone = searchPhone.substring(1);

        const url = `${ERP_API_BASE}/reports/invoice-due-dates?search=${encodeURIComponent(searchPhone)}&firmNo=919&periodNo=01`;
        
        const res = await fetchJson(url);
        if (res && res.invoices && res.invoices.length > 0) {
            const invs = res.invoices;
            const stats = res.stats || {};
            const cariName = invs[0].cariName || 'Müşterimiz';

            let txt = `📄 *Sayın ${cariName},*\n\nLogo ERP sisteminde kayıtlı açık fatura ve vade durumunuz:\n\n`;

            invs.slice(0, 5).forEach((inv, i) => {
                const statusIcon = inv.daysRemaining < 0 ? '🔴' : (inv.daysRemaining === 0 ? '🟡' : '🟢');
                const daysText = inv.daysRemaining < 0 ? `${Math.abs(inv.daysRemaining)} gün gecikti` : (inv.daysRemaining === 0 ? 'Bugün' : `${inv.daysRemaining} gün kaldı`);
                txt += `${i + 1}. *Fatura:* ${inv.ficheno}\n`;
                txt += `   📅 *Vade:* ${inv.dueDate} (${statusIcon} ${daysText})\n`;
                txt += `   💰 *Tutar:* ${formatTL(inv.netTotal)} (Kalan: ${formatTL(inv.remainingAmount)})\n\n`;
            });

            if (invs.length > 5) {
                txt += `_...ve ${invs.length - 5} adet daha fatura bulunmaktadır._\n\n`;
            }

            txt += `📊 *Toplam Açık Borç:* ${formatTL(stats.totalRemaining || stats.totalDebt)}\n`;
            txt += `ℹ️ Sorularınız için muhasebe birimimizle irtibata geçebilirsiniz.`;
            return txt;
        } else {
            return `Sayın Müşterimiz,\nTelefon numaranıza (+${phone}) ait sistemde vadesi geçmiş veya açık fatura borcu bulunmamaktadır.\nİyi çalışmalar dileriz.`;
        }
    } catch (err) {
        return `Sayın Müşterimiz,\nBakiye sorgulanamadı (${err.message}). Lütfen daha sonra tekrar deneyiniz.`;
    }
}

function fetchJson(url) {
    return new Promise((resolve, reject) => {
        http.get(url, (res) => {
            let data = '';
            res.on('data', chunk => data += chunk);
            res.on('end', () => {
                try {
                    resolve(JSON.parse(data));
                } catch (e) {
                    reject(e);
                }
            });
        }).on('error', reject);
    });
}

function formatTL(num) {
    if (num === null || num === undefined) return '0,00 ₺';
    return Number(num).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ₺';
}

// ==========================================
// REST API ENDPOINTS (PORT 5001)
// ==========================================

// 1. Durum ve QR Kod Sorgulama
app.get('/status', (req, res) => {
    let status = 'disconnected';
    if (isConnected) status = 'connected';
    else if (currentQrDataUrl) status = 'qr_waiting';

    res.json({
        online: true,
        status: status,
        isConnected: isConnected,
        userPhone: userPhone,
        qr: currentQrDataUrl
    });
});

// 2. Mesaj Gönderim
app.post('/send', async (req, res) => {
    try {
        const { phone, message } = req.body;
        if (!phone || !message) {
            return res.status(400).json({ success: false, message: 'Telefon ve mesaj alanları zorunludur.' });
        }

        if (!isConnected || !sock) {
            return res.status(503).json({ success: false, message: 'WhatsApp köprüsü bağlı değil. Lütfen önce QR kodunu okutun.' });
        }

        let cleanPhone = String(phone).replace(/\D/g, '');
        if (cleanPhone.startsWith('0')) cleanPhone = '9' + cleanPhone;
        else if (cleanPhone.length === 10) cleanPhone = '90' + cleanPhone;

        const jid = `${cleanPhone}@s.whatsapp.net`;
        const result = await sock.sendMessage(jid, { text: message });

        res.json({
            success: true,
            messageId: result?.key?.id,
            to: cleanPhone,
            message: 'Mesaj WhatsApp Web köprüsü üzerinden iletildi.'
        });
    } catch (err) {
        console.error('Mesaj gönderme hatası:', err);
        res.status(500).json({ success: false, message: err.message });
    }
});

// 3. Oturumu Kapat / Sıfırla
app.post('/logout', async (req, res) => {
    try {
        if (sock) {
            await sock.logout();
        }
        if (fs.existsSync(AUTH_DIR)) {
            fs.rmSync(AUTH_DIR, { recursive: true, force: true });
        }
        isConnected = false;
        userPhone = null;
        currentQrDataUrl = null;
        setTimeout(() => initWhatsApp(), 1000);
        res.json({ success: true, message: 'Oturum kapatıldı, yeni QR kod üretiliyor.' });
    } catch (err) {
        res.status(500).json({ success: false, message: err.message });
    }
});

// ==========================================
// SERVER BAŞLAT
// ==========================================
app.listen(PORT, '0.0.0.0', () => {
    console.log(`====================================================`);
    console.log(` LOGO ASİSTAN BOT - TOKENSIZ WHATSAPP KÖPRÜSÜ`);
    console.log(` Port: http://localhost:${PORT}`);
    console.log(`====================================================`);
    initWhatsApp();
});
