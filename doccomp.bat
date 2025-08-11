docker-compose up -d --build --force-recreate

timeout /t 3 /nobreak >nul

start http://localhost:80