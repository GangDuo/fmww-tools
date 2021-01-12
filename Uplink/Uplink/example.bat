@echo off

del log.txt
type nul > log.txt

CALL :InvokeUplink "C:\000.csv"

GOTO end

REM -------------------------------------------------
REM サブルーチン
REM -------------------------------------------------
:InvokeUplink
Uplink.exe -i -s:%1 >> log.txt
type log.txt
exit /b

:end
SETLOCAL
ENDLOCAL
PAUSE
