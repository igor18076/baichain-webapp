# Инструкция по деплою

## Быстрый деплой на Linux сервер

### 1. Подготовка сервера

```bash
# Обновление системы
sudo apt update && sudo apt upgrade -y

# Установка Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# Установка Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Установка Nginx
sudo apt install nginx -y
```

### 2. Клонирование и настройка проекта

```bash
# Создание директории
sudo mkdir -p /opt/baichain
cd /opt/baichain

# Клонирование репозитория (или копирование файлов)
git clone <repository-url> .
# ИЛИ
# Скопируйте файлы проекта вручную

# Создание .env файла
nano .env
```

Содержимое `.env`:
```
SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP_USE_STARTTLS=true
SMTP_USERNAME=your-username
SMTP_PASSWORD=your-password
SMTP_FROM_ADDRESS=order@baichein.ru
SMTP_FROM_DISPLAY_NAME=BaiChain команда
```

### 3. Сборка и запуск

```bash
# Сборка образа
docker-compose build

# Запуск
docker-compose up -d

# Проверка статуса
docker-compose ps
docker-compose logs -f
```

### 4. Настройка Nginx

Создайте файл `/etc/nginx/sites-available/baichain`:

```nginx
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;

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
```

Активируйте:
```bash
sudo ln -s /etc/nginx/sites-available/baichain /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### 5. Настройка SSL (Let's Encrypt)

```bash
# Установка Certbot
sudo apt install certbot python3-certbot-nginx -y

# Получение сертификата
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Проверка автообновления
sudo certbot renew --dry-run
```

### 6. Автозапуск

Создайте `/etc/systemd/system/baichain.service`:

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

Активируйте:
```bash
sudo systemctl daemon-reload
sudo systemctl enable baichain
sudo systemctl start baichain
```

## Обновление приложения

```bash
cd /opt/baichain
git pull  # или обновите файлы вручную
docker-compose build
docker-compose up -d
docker-compose logs -f
```

## Резервное копирование

Рекомендуется регулярно делать бэкапы:
- `.env` файл
- `app_data/` директория (логи контактов)
- Конфигурация Nginx

```bash
# Создание бэкапа
tar -czf backup-$(date +%Y%m%d).tar.gz .env app_data/ /etc/nginx/sites-available/baichain
```

