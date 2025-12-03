# Диагностика проблем с Docker контейнером

## Проблема: Приложение не открывается на localhost:8080

### Шаг 1: Проверка статуса контейнера

```bash
docker ps -a
```

Убедитесь, что контейнер запущен и не завершился с ошибкой.

### Шаг 2: Просмотр логов контейнера

```bash
docker logs baichain-webapp
```

Или если используете docker-compose:
```bash
docker-compose logs webapp
```

### Шаг 3: Проверка портов

Убедитесь, что порт 8080 не занят другим приложением:

**Windows:**
```powershell
netstat -ano | findstr :8080
```

**Linux/Mac:**
```bash
lsof -i :8080
# или
netstat -tuln | grep 8080
```

### Шаг 4: Проверка работы внутри контейнера

```bash
# Войти в контейнер
docker exec -it baichain-webapp /bin/bash

# Проверить, что приложение слушает порт
curl http://localhost:8080/health

# Или проверить процессы
ps aux | grep dotnet
```

### Шаг 5: Проверка переменных окружения

```bash
docker exec baichain-webapp env | grep ASPNETCORE
```

Должны быть:
- `ASPNETCORE_URLS=http://+:8080`
- `ASPNETCORE_ENVIRONMENT=Production`

### Шаг 6: Проверка сетевых настроек

```bash
# Проверить, что порт проброшен
docker port baichain-webapp

# Должно показать: 8080/tcp -> 0.0.0.0:8080
```

### Шаг 7: Тестирование подключения

```bash
# Из контейнера
docker exec baichain-webapp curl http://localhost:8080/health

# С хоста
curl http://localhost:8080/health
```

## Частые проблемы и решения

### Проблема 1: Контейнер сразу завершается

**Причина:** Ошибка при запуске приложения

**Решение:**
```bash
# Посмотреть логи
docker logs baichain-webapp

# Запустить в интерактивном режиме для отладки
docker run -it --rm -p 8080:8080 baichain-webapp
```

### Проблема 2: Порт уже занят

**Решение:**
```bash
# Изменить порт в docker-compose.yml
ports:
  - "8081:8080"  # Использовать 8081 на хосте

# Или остановить процесс, занимающий порт
```

### Проблема 3: Приложение не слушает на всех интерфейсах

**Проверка:**
```bash
docker exec baichain-webapp netstat -tuln
```

Должно быть: `0.0.0.0:8080` или `:::8080`, а не `127.0.0.1:8080`

**Решение:** Убедитесь, что `ASPNETCORE_URLS=http://+:8080` (с `+`)

### Проблема 4: Firewall блокирует подключение

**Windows:**
- Проверьте настройки Windows Firewall
- Разрешите входящие подключения на порт 8080

**Linux:**
```bash
sudo ufw allow 8080/tcp
```

### Проблема 5: Docker Desktop не пробрасывает порты

**Решение:**
- Перезапустите Docker Desktop
- Проверьте настройки сетевых ресурсов в Docker Desktop

## Команды для быстрой диагностики

```bash
# Полная диагностика
docker ps -a
docker logs baichain-webapp --tail 50
docker exec baichain-webapp env | grep ASPNETCORE
docker port baichain-webapp
curl -v http://localhost:8080/health
```

## Пересборка и перезапуск

Если ничего не помогло, попробуйте пересобрать:

```bash
# Остановить и удалить контейнер
docker-compose down

# Очистить старые образы
docker-compose build --no-cache

# Запустить заново
docker-compose up -d

# Смотреть логи в реальном времени
docker-compose logs -f webapp
```

## Альтернативный способ запуска

Если docker-compose не работает, попробуйте напрямую:

```bash
cd WebApplication2

# Сборка
docker build -t webapplication2:latest .

# Запуск
docker run -d \
  --name webapplication2 \
  -p 8080:8080 \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  webapplication2:latest

# Логи
docker logs -f webapplication2
```

