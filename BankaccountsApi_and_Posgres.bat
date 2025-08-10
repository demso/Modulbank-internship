@echo off
echo Starting Docker postgres container...

docker-compose up -d bankaccounts.db --force-recreate

timeout /t 1 /nobreak >nul

echo Starting BankaccountsAPI...

start "" dotnet run --project BankAccounts.Api --launch-profile http

start "" dotnet run --project BankAccounts.Identity --launch-profile http

timeout /t 5 /nobreak >nul

start http://localhost:80