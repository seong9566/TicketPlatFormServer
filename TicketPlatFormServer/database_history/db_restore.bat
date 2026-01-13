@echo off
REM TicketPlatFormDB 복원 스크립트 (Windows)
REM 사용법: db_restore.bat [호스트] [포트] [사용자]

set HOST=%1
set PORT=%2
set USER=%3

if "%HOST%"=="" set HOST=localhost
if "%PORT%"=="" set PORT=3306
if "%USER%"=="" set USER=root

echo === TicketPlatFormDB 복원 ===
echo 호스트: %HOST%:%PORT%
echo 사용자: %USER%
echo.

set /p PASSWORD=MySQL 비밀번호 입력:

echo DB 복원 중...
mysql -h %HOST% -P %PORT% -u %USER% -p%PASSWORD% < TicketPlatFormDB_dump.sql

if %ERRORLEVEL% EQU 0 (
    echo 복원 완료!
) else (
    echo 복원 실패. 오류를 확인하세요.
    exit /b 1
)
