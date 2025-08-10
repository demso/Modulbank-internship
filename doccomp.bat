docker-compose up -d --force-recreate

timeout /t 1 /nobreak >nul

start http://localhost:80