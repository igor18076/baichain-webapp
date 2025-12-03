# Скрипт PowerShell для сборки и публикации образа в Docker Hub
# Использование: .\docker-push.ps1 [version] [username]

param(
    [string]$Version = "latest",
    [string]$DockerHubUsername = $env:DOCKERHUB_USERNAME
)

$ErrorActionPreference = "Stop"

# Функция для вывода цветного текста
function Write-ColorOutput($ForegroundColor, $Message) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    Write-Output $Message
    $host.UI.RawUI.ForegroundColor = $fc
}

# Проверка наличия Docker
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-ColorOutput "Red" "Ошибка: Docker не установлен"
    exit 1
}

# Проверка username
if ([string]::IsNullOrEmpty($DockerHubUsername)) {
    $DockerHubUsername = Read-Host "Введите ваш Docker Hub username"
}

if ([string]::IsNullOrEmpty($DockerHubUsername)) {
    Write-ColorOutput "Red" "Ошибка: Docker Hub username обязателен"
    exit 1
}

$ImageName = "${DockerHubUsername}/webapplication2"
$FullTag = "${ImageName}:${Version}"

Write-ColorOutput "Green" "=== Сборка и публикация Docker образа ==="
Write-Output "Username: $DockerHubUsername"
Write-Output "Image: $FullTag"
Write-Output ""

# Проверка входа в Docker Hub
try {
    docker info | Out-Null
} catch {
    Write-ColorOutput "Yellow" "Требуется вход в Docker Hub..."
    docker login
}

# Переход в директорию проекта
$ProjectDir = Join-Path $PSScriptRoot "WebApplication2"
if (-not (Test-Path $ProjectDir)) {
    Write-ColorOutput "Red" "Ошибка: Директория проекта не найдена: $ProjectDir"
    exit 1
}

Push-Location $ProjectDir

try {
    # Сборка образа
    Write-ColorOutput "Green" "[1/3] Сборка образа..."
    docker build `
        --build-arg DOTNET_VERSION=10.0 `
        --build-arg PROJECT_NAME=WebApplication2 `
        -t $FullTag `
        -t "${ImageName}:latest" `
        .

    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "Red" "Ошибка при сборке образа"
        exit 1
    }

    Write-ColorOutput "Green" "[2/3] Образ успешно собран"

    # Публикация образа
    Write-ColorOutput "Green" "[3/3] Публикация образа в Docker Hub..."
    docker push $FullTag

    if ($Version -ne "latest") {
        docker push "${ImageName}:latest"
    }

    Write-Output ""
    Write-ColorOutput "Green" "✓ Образ успешно опубликован!"
    Write-Output "  $FullTag"
    if ($Version -ne "latest") {
        Write-Output "  ${ImageName}:latest"
    }
    Write-Output ""
    Write-Output "Использование:"
    Write-Output "  docker pull $FullTag"
    Write-Output "  docker run -d -p 8080:8080 $FullTag"
} finally {
    Pop-Location
}

