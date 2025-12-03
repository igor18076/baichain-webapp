# Скрипт диагностики Docker контейнера
# Использование: .\docker-check.ps1

Write-Host "=== Диагностика Docker контейнера ===" -ForegroundColor Green
Write-Host ""

# Проверка запущенных контейнеров
Write-Host "[1] Проверка контейнеров..." -ForegroundColor Yellow
$containers = docker ps -a --filter "name=baichain-webapp" --format "{{.Names}}\t{{.Status}}\t{{.Ports}}"
if ($containers) {
    Write-Host $containers -ForegroundColor Green
} else {
    Write-Host "Контейнер не найден!" -ForegroundColor Red
    Write-Host "Запустите: docker-compose up -d" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Проверка логов
Write-Host "[2] Последние 20 строк логов..." -ForegroundColor Yellow
docker logs --tail 20 baichain-webapp 2>&1

Write-Host ""

# Проверка переменных окружения
Write-Host "[3] Переменные окружения ASP.NET Core..." -ForegroundColor Yellow
docker exec baichain-webapp env 2>&1 | Select-String "ASPNETCORE" | ForEach-Object {
    Write-Host $_ -ForegroundColor Cyan
}

Write-Host ""

# Проверка портов
Write-Host "[4] Проброшенные порты..." -ForegroundColor Yellow
$ports = docker port baichain-webapp 2>&1
if ($ports) {
    Write-Host $ports -ForegroundColor Green
} else {
    Write-Host "Порты не проброшены!" -ForegroundColor Red
}

Write-Host ""

# Проверка health check
Write-Host "[5] Проверка health endpoint..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -TimeoutSec 5 -UseBasicParsing
    Write-Host "✓ Health check успешен: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "✗ Health check не прошел: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Проверка главной страницы
Write-Host "[6] Проверка главной страницы..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:8080" -TimeoutSec 5 -UseBasicParsing
    Write-Host "✓ Главная страница доступна: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "✗ Главная страница недоступна: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Проверка занятости порта
Write-Host "[7] Проверка занятости порта 8080..." -ForegroundColor Yellow
$portCheck = netstat -ano | Select-String ":8080"
if ($portCheck) {
    Write-Host "Порт 8080 используется:" -ForegroundColor Yellow
    Write-Host $portCheck -ForegroundColor Cyan
} else {
    Write-Host "Порт 8080 свободен" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Диагностика завершена ===" -ForegroundColor Green
Write-Host ""
Write-Host "Полезные команды:" -ForegroundColor Yellow
Write-Host "  docker logs -f baichain-webapp          # Логи в реальном времени"
Write-Host "  docker exec -it baichain-webapp bash    # Войти в контейнер"
Write-Host "  docker-compose restart                  # Перезапустить контейнер"
Write-Host "  docker-compose logs -f webapp           # Логи через docker-compose"

