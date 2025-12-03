# Скрипт для проверки контейнеров, созданных Visual Studio
Write-Host "=== Проверка Docker контейнеров ===" -ForegroundColor Green
Write-Host ""

# Все контейнеры
Write-Host "[1] Все контейнеры (включая остановленные):" -ForegroundColor Yellow
docker ps -a --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | Select-String -Pattern "webapplication|baichain|CONTAINER"

Write-Host ""

# Запущенные контейнеры
Write-Host "[2] Запущенные контейнеры:" -ForegroundColor Yellow
$running = docker ps --format "{{.Names}}\t{{.Status}}\t{{.Ports}}"
if ($running) {
    Write-Host $running -ForegroundColor Green
} else {
    Write-Host "Нет запущенных контейнеров" -ForegroundColor Red
}

Write-Host ""

# Проверка портов
Write-Host "[3] Контейнеры, использующие порт 8080:" -ForegroundColor Yellow
$containers = docker ps -a --format "{{.Names}}"
foreach ($container in $containers) {
    $ports = docker port $container 2>&1
    if ($ports -match "8080") {
        Write-Host "$container :" -ForegroundColor Cyan
        Write-Host "  $ports" -ForegroundColor Green
    }
}

Write-Host ""

# Логи последнего контейнера
Write-Host "[4] Поиск контейнеров с webapplication в имени:" -ForegroundColor Yellow
$webappContainers = docker ps -a --filter "name=webapplication" --format "{{.Names}}"
if ($webappContainers) {
    foreach ($container in $webappContainers) {
        Write-Host "Контейнер: $container" -ForegroundColor Cyan
        Write-Host "Последние 10 строк логов:" -ForegroundColor Yellow
        docker logs --tail 10 $container 2>&1
        Write-Host ""
    }
} else {
    Write-Host "Контейнеры с именем 'webapplication' не найдены" -ForegroundColor Yellow
}

Write-Host ""

# Проверка образа
Write-Host "[5] Образы webapplication:" -ForegroundColor Yellow
docker images | Select-String -Pattern "webapplication|REPOSITORY"

Write-Host ""
Write-Host "=== Рекомендации ===" -ForegroundColor Green
Write-Host "1. Если контейнер не запущен, проверьте логи выше"
Write-Host "2. Если порт не проброшен, проверьте настройки в launchSettings.json"
Write-Host "3. Для запуска через docker-compose используйте: docker-compose up -d"
Write-Host "4. Для остановки всех контейнеров: docker stop \$(docker ps -q)"

