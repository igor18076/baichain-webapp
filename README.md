# BaiChain - Веб-приложение

Веб-приложение компании BaiChain для представления услуг, обработки заявок и контактов.

## 📋 Содержание

- [Технологический стек](#технологический-стек)
- [Требования](#требования)
- [Быстрый старт](#быстрый-старт)
- [Разработка](#разработка)
- [Конфигурация](#конфигурация)
- [Деплой на Linux с Docker](#деплой-на-linux-с-docker)
- [Настройка SSL сертификатов](#настройка-ssl-сертификатов)
- [Архитектура проекта](#архитектура-проекта)
- [Безопасность](#безопасность)
- [Мониторинг и логирование](#мониторинг-и-логирование)
- [Устранение неполадок](#устранение-неполадок)

## 🛠 Технологический стек

- **.NET 10.0** - основная платформа
- **ASP.NET Core MVC** - веб-фреймворк
- **MailKit** - отправка email через SMTP
- **Polly** - политики повторных попыток
- **Docker** - контейнеризация
- **Bootstrap 5** - CSS фреймворк
- **jQuery** - JavaScript библиотека

## 📦 Требования

### Для разработки:
- .NET 10.0 SDK
- Visual Studio 2022 или VS Code
- Git

### Для продакшена:
- Linux сервер (Ubuntu 20.04+ / Debian 11+)
- Docker и Docker Compose
- Nginx (для reverse proxy и SSL)
- Domain name с настроенным DNS

## 🚀 Быстрый старт

### Клонирование репозитория

```bash
git clone <repository-url>
cd WebApplication2Test
```

### Локальный запуск

```bash
cd WebApplication2
dotnet restore
dotnet run
```

Приложение будет доступно по адресу: `http://localhost:5151`

## 💻 Разработка

### Настройка окружения разработки

1. **Установите User Secrets для SMTP:**

```bash
cd WebApplication2
dotnet user-secrets set "Smtp:Host" "smtp.example.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:UseStartTls" "true"
dotnet user-secrets set "Smtp:Username" "your-username"
dotnet user-secrets set "Smtp:Password" "your-password"
dotnet user-secrets set "Smtp:FromAddress" "order@baichein.ru"
dotnet user-secrets set "Smtp:FromDisplayName" "BaiChain команда"
```

Или через Visual Studio: ПКМ на проект → Manage User Secrets

2. **Запустите приложение:**

```bash
dotnet run --environment Development
```

### Структура проекта

```
WebApplication2/
├── Controllers/          # Контроллеры MVC
│   ├── HomeController.cs # Главная страница и форма обратной связи
│   └── ErrorController.cs # Обработка ошибок
├── Models/               # Модели данных
│   ├── ContactFormViewModel.cs
│   └── ErrorViewModel.cs
├── Services/             # Бизнес-логика
│   ├── BackgroundEmailQueue.cs # Очередь email
│   ├── MailKitEmailSender.cs   # Отправка email
│   ├── QueuedEmailSender.cs    # Фоновый сервис отправки
│   └── SmtpOptions.cs          # Настройки SMTP
├── Views/                # Razor представления
│   ├── Home/
│   ├── Error/
│   └── Shared/
├── wwwroot/             # Статические файлы
│   ├── css/
│   ├── js/
│   └── src/
├── Program.cs           # Точка входа
├── Dockerfile           # Docker образ
└── appsettings.json     # Конфигурация
```

## ⚙️ Конфигурация

### Настройки приложения

Основные настройки находятся в `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Smtp": {
    "Host": "",
    "Port": 587,
    "UseStartTls": true,
    "Username": "",
    "Password": "",
    "FromAddress": "order@baichein.ru",
    "FromDisplayName": "BaiChain команда"
  }
}
```

⚠️ **Важно:** Пароли и чувствительные данные НЕ должны храниться в `appsettings.json`!

### Переменные окружения

Приложение поддерживает конфигурацию через переменные окружения с двойным подчёркиванием:

```bash
Smtp__Host=smtp.example.com
Smtp__Port=587
Smtp__Password=your-password
```

Подробнее см. [SMTP_CONFIGURATION.md](WebApplication2/SMTP_CONFIGURATION.md)

## 🐳 Деплой на Linux с Docker

### Предварительные требования

1. Установите Docker и Docker Compose на Linux сервере:

```bash
# Ubuntu/Debian
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# Установка Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

2. Настройте DNS записи для вашего домена:
   - A запись: `@` → IP адрес сервера
   - A запись: `www` → IP адрес сервера

### Шаг 1: Подготовка файлов

Создайте на сервере директорию проекта:

```bash
mkdir -p /opt/baichain
cd /opt/baichain
```

Скопируйте следующие файлы на сервер:
- `WebApplication2/Dockerfile`
- `WebApplication2/WebApplication2.csproj`
- Всю директорию `WebApplication2/` (кроме `bin/` и `obj/`)

### Шаг 2: Создание docker-compose.yml

Создайте файл `docker-compose.yml` в корне проекта:

```yaml
version: '3.8'

services:
  webapp:
    build:
      context: ./WebApplication2
      dockerfile: Dockerfile
    container_name: baichain-webapp
    restart: unless-stopped
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - Smtp__Host=${SMTP_HOST}
      - Smtp__Port=${SMTP_PORT:-587}
      - Smtp__UseStartTls=${SMTP_USE_STARTTLS:-true}
      - Smtp__Username=${SMTP_USERNAME}
      - Smtp__Password=${SMTP_PASSWORD}
      - Smtp__FromAddress=${SMTP_FROM_ADDRESS:-order@baichein.ru}
      - Smtp__FromDisplayName=${SMTP_FROM_DISPLAY_NAME:-BaiChain команда}
    volumes:
      - ./logs:/app/logs
      - ./app_data:/app/App_Data
    networks:
      - baichain-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

networks:
  baichain-network:
    driver: bridge
```

### Шаг 3: Создание .env файла

Создайте файл `.env` в той же директории:

```bash
SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP_USE_STARTTLS=true
SMTP_USERNAME=your-username
SMTP_PASSWORD=your-password
SMTP_FROM_ADDRESS=order@baichein.ru
SMTP_FROM_DISPLAY_NAME=BaiChain команда
```

⚠️ **Безопасность:** Добавьте `.env` в `.gitignore` и не коммитьте его в репозиторий!

### Шаг 4: Сборка и запуск

```bash
# Сборка образа
docker-compose build

# Запуск контейнера
docker-compose up -d

# Просмотр логов
docker-compose logs -f webapp

# Проверка статуса
docker-compose ps
```

### Шаг 5: Настройка Nginx как Reverse Proxy

Установите Nginx:

```bash
sudo apt update
sudo apt install nginx
```

Создайте конфигурацию `/etc/nginx/sites-available/baichain`:

```nginx
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;

    # Редирект на HTTPS (после настройки SSL)
    # return 301 https://$server_name$request_uri;

    # Временно для HTTP (до настройки SSL)
    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # Увеличение таймаутов для больших запросов
    proxy_connect_timeout 60s;
    proxy_send_timeout 60s;
    proxy_read_timeout 60s;
}
```

Активируйте конфигурацию:

```bash
sudo ln -s /etc/nginx/sites-available/baichain /etc/nginx/sites-enabled/
sudo nginx -t  # Проверка конфигурации
sudo systemctl reload nginx
```

### Шаг 6: Автозапуск при перезагрузке

Создайте systemd сервис `/etc/systemd/system/baichain.service`:

```ini
[Unit]
Description=BaiChain Web Application
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/opt/baichain
ExecStart=/usr/local/bin/docker-compose up -d
ExecStop=/usr/local/bin/docker-compose down
TimeoutStartSec=0

[Install]
WantedBy=multi-user.target
```

Активируйте сервис:

```bash
sudo systemctl daemon-reload
sudo systemctl enable baichain
sudo systemctl start baichain
```

## 🔒 Настройка SSL сертификатов

### Вариант 1: Let's Encrypt (бесплатный, рекомендуется)

1. **Установите Certbot:**

```bash
sudo apt update
sudo apt install certbot python3-certbot-nginx
```

2. **Получите сертификат:**

```bash
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com
```

Certbot автоматически:
- Получит сертификат
- Настроит Nginx для использования HTTPS
- Настроит автоматическое обновление

3. **Проверьте автообновление:**

```bash
sudo certbot renew --dry-run
```

Сертификат будет автоматически обновляться каждые 90 дней.

### Вариант 2: Ручная установка сертификата

Если у вас уже есть SSL сертификат:

1. **Скопируйте сертификаты на сервер:**

```bash
sudo mkdir -p /etc/nginx/ssl
sudo cp your-certificate.crt /etc/nginx/ssl/
sudo cp your-private-key.key /etc/nginx/ssl/
sudo chmod 600 /etc/nginx/ssl/your-private-key.key
```

2. **Обновите конфигурацию Nginx:**

```nginx
server {
    listen 443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;

    ssl_certificate /etc/nginx/ssl/your-certificate.crt;
    ssl_certificate_key /etc/nginx/ssl/your-private-key.key;

    # Современные настройки SSL
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;

    # HSTS
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}

# Редирект HTTP на HTTPS
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;
    return 301 https://$server_name$request_uri;
}
```

3. **Перезагрузите Nginx:**

```bash
sudo nginx -t
sudo systemctl reload nginx
```

### Проверка SSL

Проверьте конфигурацию SSL:

```bash
# Проверка сертификата
openssl s_client -connect yourdomain.com:443 -servername yourdomain.com

# Онлайн проверка
# https://www.ssllabs.com/ssltest/analyze.html?d=yourdomain.com
```

## 🏗 Архитектура проекта

### Основные компоненты

1. **HomeController** - обработка главной страницы и формы обратной связи
2. **BackgroundEmailQueue** - очередь сообщений для асинхронной отправки
3. **MailKitEmailSender** - отправка email через SMTP с retry политикой
4. **QueuedEmailSender** - фоновый сервис для обработки очереди email

### Поток обработки заявки

```
Пользователь → Форма обратной связи
    ↓
HomeController.Index (POST)
    ↓
Валидация данных
    ↓
Создание MimeMessage
    ↓
Добавление в очередь (BackgroundEmailQueue)
    ↓
Возврат успешного ответа пользователю
    ↓
[Фоновый процесс]
QueuedEmailSender
    ↓
MailKitEmailSender (с retry)
    ↓
SMTP сервер
```

### Обработка ошибок

- Все необработанные исключения → `/Error/500`
- HTTP статус коды (404, 403 и т.д.) → `/Error/{код}`
- Логирование через ILogger
- Файловое логирование контактов в `App_Data/contact.log`

## 🔐 Безопасность

### Реализованные меры

- ✅ HTTPS редирект в продакшене
- ✅ HSTS заголовки
- ✅ Anti-forgery токены для форм
- ✅ Валидация входных данных
- ✅ Безопасное хранение секретов (User Secrets / переменные окружения)
- ✅ Защита от XSS через Razor
- ✅ Content Security Policy (через Nginx)

### Рекомендации

1. **Регулярно обновляйте зависимости:**
```bash
dotnet list package --outdated
dotnet add package <package-name> --version <latest>
```

2. **Используйте сильные пароли для SMTP**
3. **Ограничьте доступ к `/App_Data/contact.log`** (содержит персональные данные)
4. **Настройте файрвол:**
```bash
sudo ufw allow 22/tcp   # SSH
sudo ufw allow 80/tcp   # HTTP
sudo ufw allow 443/tcp  # HTTPS
sudo ufw enable
```

## 📊 Мониторинг и логирование

### Health Check

Приложение предоставляет endpoint для проверки здоровья:

```bash
curl http://localhost:8080/health
```

### Логирование

- **Структурированное логирование** через ILogger
- **Файловые логи** контактов в `App_Data/contact.log`
- **Docker логи:**
```bash
docker-compose logs -f webapp
```

### Мониторинг

Рекомендуется настроить:
- Мониторинг доступности (UptimeRobot, Pingdom)
- Алерты на ошибки (Sentry, Application Insights)
- Мониторинг ресурсов сервера (Prometheus + Grafana)

## 🐛 Устранение неполадок

### Проблема: Приложение не запускается

```bash
# Проверьте логи
docker-compose logs webapp

# Проверьте конфигурацию
docker-compose config

# Пересоберите образ
docker-compose build --no-cache
docker-compose up -d
```

### Проблема: Email не отправляются

1. Проверьте настройки SMTP в `.env`
2. Проверьте логи:
```bash
docker-compose logs webapp | grep -i smtp
```
3. Убедитесь, что SMTP сервер доступен:
```bash
telnet smtp.example.com 587
```

### Проблема: 502 Bad Gateway

1. Проверьте, что контейнер запущен:
```bash
docker-compose ps
```

2. Проверьте логи Nginx:
```bash
sudo tail -f /var/log/nginx/error.log
```

3. Проверьте, что приложение слушает на порту 8080:
```bash
docker-compose exec webapp netstat -tlnp
```

### Проблема: SSL сертификат не работает

1. Проверьте права доступа к файлам:
```bash
sudo ls -la /etc/nginx/ssl/
```

2. Проверьте конфигурацию Nginx:
```bash
sudo nginx -t
```

3. Проверьте срок действия сертификата:
```bash
sudo certbot certificates
```

## 📝 Полезные команды

### Docker

```bash
# Перезапуск приложения
docker-compose restart webapp

# Остановка
docker-compose down

# Обновление приложения
git pull
docker-compose build
docker-compose up -d

# Очистка неиспользуемых образов
docker system prune -a
```

### Nginx

```bash
# Проверка конфигурации
sudo nginx -t

# Перезагрузка
sudo systemctl reload nginx

# Просмотр логов
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

### Система

```bash
# Проверка использования ресурсов
docker stats

# Проверка места на диске
df -h
du -sh /opt/baichain

# Проверка сетевых подключений
netstat -tlnp
```

## 📚 Дополнительная документация

- [Настройка SMTP](WebApplication2/SMTP_CONFIGURATION.md)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Docker Documentation](https://docs.docker.com/)
- [Nginx Documentation](https://nginx.org/en/docs/)

## 👥 Для разработчиков

### Добавление новой страницы

1. Создайте action в контроллере:
```csharp
public IActionResult NewPage()
{
    return View();
}
```

2. Создайте представление в `Views/Home/NewPage.cshtml`

3. Добавьте маршрут в навигацию (`_Layout.cshtml`)

### Добавление нового сервиса

1. Создайте интерфейс и реализацию в `Services/`
2. Зарегистрируйте в `Program.cs`:
```csharp
builder.Services.AddScoped<IMyService, MyService>();
```

### Стилизация

- Основные стили: `wwwroot/css/custom.css`
- Тематические стили: `wwwroot/css/custom-theme.css`
- Используйте CSS переменные для темной/светлой темы

## 📄 Лицензия

[Укажите лицензию проекта]

## 🤝 Контакты

- Email: order@baichein.ru
- Website: [your-domain.com]

---

**Последнее обновление:** 2024

