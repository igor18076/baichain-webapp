# Быстрый старт с Docker

## Запуск приложения

### Вариант 1: Через docker-compose (рекомендуется)

```bash
# Из корневой директории проекта
docker-compose up -d

# Просмотр логов
docker-compose logs -f webapp
```

### Вариант 2: Прямой запуск Docker

```bash
cd WebApplication2

# Сборка образа
docker build -t webapplication2:latest .

# Запуск контейнера
docker run -d \
  --name baichain-webapp \
  -p 8080:8080 \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  webapplication2:latest

# Просмотр логов
docker logs -f baichain-webapp
```

## Проверка работы

1. **Откройте в браузере:**
   - Главная страница: http://localhost:8080
   - Health check: http://localhost:8080/health

2. **Или используйте диагностический скрипт:**
   ```powershell
   .\docker-check.ps1
   ```

## Остановка

```bash
# Остановить контейнер
docker-compose down

# Или для прямого запуска
docker stop baichain-webapp
docker rm baichain-webapp
```

## Решение проблем

Если приложение не открывается:

1. **Проверьте логи:**
   ```bash
   docker logs baichain-webapp
   ```

2. **Проверьте статус контейнера:**
   ```bash
   docker ps -a
   ```

3. **Запустите диагностику:**
   ```powershell
   .\docker-check.ps1
   ```

4. **Пересоберите образ:**
   ```bash
   docker-compose build --no-cache
   docker-compose up -d
   ```

Подробная диагностика: см. `docker-debug.md`

