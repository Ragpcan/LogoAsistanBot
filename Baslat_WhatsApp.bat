@echo off
chcp 65001 > nul
title Logo Asistan Bot - Tokensiz WhatsApp Web Köprüsü
color 0A
echo ===============================================================================
echo            🤖 LOGO ASİSTAN BOT - TOKENSIZ WHATSAPP WEB KÖPRÜSÜ
echo ===============================================================================
echo.
cd /d "%~dp0WhatsAppBridge"

echo [BILGI] WhatsApp Web Köprüsü başlatılıyor (Port 5001)...
echo [BILGI] Portaldan (http://localhost:5000/erp-portal.html) QR kodunu taratabilirsiniz.
echo.
node server.js
pause
