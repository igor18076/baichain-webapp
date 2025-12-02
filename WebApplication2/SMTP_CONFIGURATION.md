# Настройка SMTP

## Для разработки (Development)

Используйте **User Secrets** для хранения чувствительных данных:

```bash
dotnet user-secrets set "Smtp:Host" "smtp.example.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:UseStartTls" "true"
dotnet user-secrets set "Smtp:Username" "your-username"
dotnet user-secrets set "Smtp:Password" "your-password"
dotnet user-secrets set "Smtp:FromAddress" "order@baichein.ru"
dotnet user-secrets set "Smtp:FromDisplayName" "BaiChain команда"
```

Или через Visual Studio:
- Правой кнопкой на проект → Manage User Secrets
- Добавьте настройки в формате JSON:

```json
{
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UseStartTls": true,
    "Username": "your-username",
    "Password": "your-password",
    "FromAddress": "order@baichein.ru",
    "FromDisplayName": "BaiChain команда"
  }
}
```

## Для продакшена (Production)

Используйте **переменные окружения**:

### Windows (IIS / App Service)
```powershell
$env:Smtp__Host="smtp.example.com"
$env:Smtp__Port="587"
$env:Smtp__UseStartTls="true"
$env:Smtp__Username="your-username"
$env:Smtp__Password="your-password"
$env:Smtp__FromAddress="order@baichein.ru"
$env:Smtp__FromDisplayName="BaiChain команда"
```

### Linux / Docker
```bash
export Smtp__Host="smtp.example.com"
export Smtp__Port="587"
export Smtp__UseStartTls="true"
export Smtp__Username="your-username"
export Smtp__Password="your-password"
export Smtp__FromAddress="order@baichein.ru"
export Smtp__FromDisplayName="BaiChain команда"
```

### Docker Compose
```yaml
environment:
  - Smtp__Host=smtp.example.com
  - Smtp__Port=587
  - Smtp__UseStartTls=true
  - Smtp__Username=your-username
  - Smtp__Password=your-password
  - Smtp__FromAddress=order@baichein.ru
  - Smtp__FromDisplayName=BaiChain команда
```

### Azure App Service
В портале Azure → Configuration → Application settings:
- `Smtp__Host` = `smtp.example.com`
- `Smtp__Port` = `587`
- `Smtp__UseStartTls` = `true`
- `Smtp__Username` = `your-username`
- `Smtp__Password` = `your-password` (пометить как секрет)
- `Smtp__FromAddress` = `order@baichein.ru`
- `Smtp__FromDisplayName` = `BaiChain команда`

## Пример настройки для популярных провайдеров

### Gmail (требует App Password)
```
Host: smtp.gmail.com
Port: 587
UseStartTls: true
Username: your-email@gmail.com
Password: [App Password из настроек Google]
```

### Yandex
```
Host: smtp.yandex.ru
Port: 465 (SSL) или 587 (STARTTLS)
UseStartTls: true (для 587) или false (для 465)
Username: your-email@yandex.ru
Password: [пароль приложения]
```

### Mail.ru
```
Host: smtp.mail.ru
Port: 587
UseStartTls: true
Username: your-email@mail.ru
Password: [пароль приложения]
```

## Важно

⚠️ **НИКОГДА** не коммитьте пароли в репозиторий!
- Используйте User Secrets для разработки
- Используйте переменные окружения для продакшена
- Используйте Azure Key Vault или аналоги для критичных данных

