@echo off
chcp 65001 > nul
title GitHub'a Kod Yukleme - Logo Asistan Bot
color 0A

echo ======================================================================
echo           LOGO ASISTAN BOT - GITHUB'A YUKLEME ARACI
echo ======================================================================
echo.
echo Depo Adresi: https://github.com/Ragpcan/LogoAsistanBot.git
echo Dal (Branch): main
echo.
echo Lutfen bekleyin, kodlar GitHub'a gonderiliyor...
echo Ekrana GitHub giris penceresi geldiginde 'Sign in with your browser'
echo secenegine tiklayip onaylayin.
echo.
echo ======================================================================
echo.

cd /d "%~dp0"
git push -u origin main

echo.
if %ERRORLEVEL% equ 0 (
    color 0A
    echo ======================================================================
    echo [BASARILI] Kodlariniz GitHub'a basariyla yuklendi!
    echo Deponuzu gormek icin: https://github.com/Ragpcan/LogoAsistanBot
    echo ======================================================================
) else (
    color 0C
    echo.
    echo [HATA] Yukleme sirasinda bir sorun olustu.
)

echo.
pause
