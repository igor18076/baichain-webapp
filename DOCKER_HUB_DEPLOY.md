# Инструкция по публикации образа в Docker Hub

## Предварительные требования

1. **Аккаунт на Docker Hub**
   - Зарегистрируйтесь на [hub.docker.com](https://hub.docker.com)
   - Подтвердите email адрес

2. **Установленный Docker**
   - Убедитесь, что Docker Desktop или Docker Engine установлен и запущен
   - Проверьте версию: `docker --version`

## Пошаговая инструкция

### Шаг 1: Вход в Docker Hub

```bash
docker login
```

Введите ваш username и password (или Personal Access Token) от Docker Hub.

**Альтернатива:** Использование Personal Access Token (рекомендуется для CI/CD):
```bash
echo "YOUR_TOKEN" | docker login --username YOUR_USERNAME --password-stdin
```

### Шаг 2: Переход в директорию проекта

```bash
cd WebApplication2
```

### Шаг 3: Сборка образа

Соберите образ с правильным тегом для Docker Hub:

```bash
docker build -t YOUR_DOCKERHUB_USERNAME/webapplication2:latest .
```

**Пример:**
```bash
docker build -t myusername/webapplication2:latest .
```

**С дополнительными параметрами:**
```bash
docker build \
  --build-arg DOTNET_VERSION=8.0 \
  --build-arg PROJECT_NAME=WebApplication2 \
  -t YOUR_DOCKERHUB_USERNAME/webapplication2:latest \
  -t YOUR_DOCKERHUB_USERNAME/webapplication2:v1.0.0 \
  .
```

### Шаг 4: Проверка образа

Убедитесь, что образ создан:

```bash
docker images | grep webapplication2
```

### Шаг 5: Тестирование образа локально (опционально)

Перед пушем протестируйте образ:

```bash
docker run -d -p 8080:8080 --name webapplication2-test YOUR_DOCKERHUB_USERNAME/webapplication2:latest
```

Проверьте работу:
- Откройте браузер: `http://localhost:8080`
- Проверьте health check: `http://localhost:8080/health`

Остановите контейнер:
```bash
docker stop webapplication2-test
docker rm webapplication2-test
```

### Шаг 6: Публикация образа в Docker Hub

```bash
docker push YOUR_DOCKERHUB_USERNAME/webapplication2:latest
```

**Если создали несколько тегов:**
```bash
docker push YOUR_DOCKERHUB_USERNAME/webapplication2:latest
docker push YOUR_DOCKERHUB_USERNAME/webapplication2:v1.0.0
```

### Шаг 7: Проверка на Docker Hub

1. Откройте [hub.docker.com](https://hub.docker.com)
2. Перейдите в ваш профиль → Repositories
3. Найдите `webapplication2`
4. Убедитесь, что образ опубликован

## Полный пример команд

```bash
# 1. Вход
docker login

# 2. Сборка
docker build -t myusername/webapplication2:latest .

# 3. Публикация
docker push myusername/webapplication2:latest
```

## Использование опубликованного образа

После публикации любой может использовать ваш образ:

```bash
docker pull YOUR_DOCKERHUB_USERNAME/webapplication2:latest
docker run -d -p 8080:8080 YOUR_DOCKERHUB_USERNAME/webapplication2:latest
```

## Рекомендации по версионированию

Используйте семантическое версионирование:

```bash
# Сборка с версией
docker build -t myusername/webapplication2:v1.0.0 -t myusername/webapplication2:latest .

# Публикация обеих версий
docker push myusername/webapplication2:v1.0.0
docker push myusername/webapplication2:latest
```

## Автоматизация через GitHub Actions (опционально)

Создайте `.github/workflows/docker-publish.yml`:

```yaml
name: Docker Build and Push

on:
  push:
    tags:
      - 'v*'

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Login to Docker Hub
        uses: docker/login-action@v2
        with:
          username: ${{ secrets.DOCKERHUB_USERNAME }}
          password: ${{ secrets.DOCKERHUB_TOKEN }}
      
      - name: Build and push
        uses: docker/build-push-action@v4
        with:
          context: ./WebApplication2
          push: true
          tags: |
            ${{ secrets.DOCKERHUB_USERNAME }}/webapplication2:latest
            ${{ secrets.DOCKERHUB_USERNAME }}/webapplication2:${{ github.ref_name }}
```

## Устранение проблем

### Ошибка: "denied: requested access to the resource is denied"
- Проверьте, что вы вошли в Docker Hub: `docker login`
- Убедитесь, что имя образа начинается с вашего username

### Ошибка: "unauthorized: authentication required"
- Выполните `docker logout` и затем `docker login` снова
- Проверьте правильность username и password

### Медленная загрузка
- Используйте Docker Hub в непиковые часы
- Рассмотрите использование Docker Hub Pro для увеличения лимитов

## Безопасность

1. **Не храните пароли в скриптах** - используйте переменные окружения
2. **Используйте Personal Access Tokens** вместо паролей в CI/CD
3. **Сканируйте образы на уязвимости** через Docker Hub Security Scanning
4. **Не пушите образы с секретами** - используйте secrets management

