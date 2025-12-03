# ============================================
# Multi-stage Dockerfile для ASP.NET Core
# Оптимизирован для production с лучшими практиками 2025
# ============================================

# ARG для параметризации сборки
ARG DOTNET_VERSION=10.0
ARG PROJECT_NAME=WebApplication2

# ============================================
# Stage 1: Restore dependencies
# Оптимизация кеширования через отдельный этап
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS restore
ARG PROJECT_NAME

WORKDIR /src

# Копируем только файлы проектов для максимального кеширования
# При сборке из директории проекта файл находится в корне контекста
COPY ["${PROJECT_NAME}.csproj", "./"]

# Восстанавливаем зависимости
RUN dotnet restore "${PROJECT_NAME}.csproj"

# ============================================
# Stage 2: Build and Publish
# Сборка и публикация в Release режиме
# ============================================
FROM restore AS build
ARG PROJECT_NAME

# Копируем остальные файлы проекта
COPY . .

WORKDIR /src

# Публикация проекта (включает сборку и восстановление)
RUN dotnet publish "${PROJECT_NAME}.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ============================================
# Stage 3: Runtime (Final)
# Минимальный runtime образ без SDK
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
ARG PROJECT_NAME

# Установка curl для health check (минимальный размер, очистка кеша)
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl ca-certificates && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# Создание non-root пользователя для безопасности
# Используем существующего пользователя или создаем нового
RUN if id -u 1000 >/dev/null 2>&1; then \
        EXISTING_USER=$(id -nu 1000); \
        groupadd -r appuser 2>/dev/null || true; \
        usermod -g appuser $EXISTING_USER 2>/dev/null || true; \
    else \
        groupadd -r appuser && \
        useradd -r -g appuser -u 1000 appuser; \
    fi && \
    mkdir -p /app && \
    chown -R 1000:1000 /app

WORKDIR /app

# Копирование опубликованных файлов из этапа build
COPY --from=build --chown=1000:1000 /app/publish .

# Переключение на non-root пользователя (UID 1000)
USER 1000

# Настройка переменных окружения
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_ENVIRONMENT=Production

# Открытие порта
EXPOSE 8080

# Health check для мониторинга состояния приложения
# Проверяет HTTP endpoint /health (настроен в Program.cs)
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl --fail --silent http://localhost:8080/health || exit 1

# Точка входа приложения
# Примечание: имя DLL должно соответствовать PROJECT_NAME
ENTRYPOINT ["dotnet", "WebApplication2.dll"]
