#!/bin/bash

# Скрипт для сборки и публикации образа в Docker Hub
# Использование: ./docker-push.sh [version] [username]

set -e

# Цвета для вывода
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Параметры
VERSION=${1:-latest}
DOCKERHUB_USERNAME=${2:-${DOCKERHUB_USERNAME}}

# Проверка наличия Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}Ошибка: Docker не установлен${NC}"
    exit 1
fi

# Проверка username
if [ -z "$DOCKERHUB_USERNAME" ]; then
    echo -e "${YELLOW}Введите ваш Docker Hub username:${NC}"
    read DOCKERHUB_USERNAME
fi

if [ -z "$DOCKERHUB_USERNAME" ]; then
    echo -e "${RED}Ошибка: Docker Hub username обязателен${NC}"
    exit 1
fi

IMAGE_NAME="${DOCKERHUB_USERNAME}/webapplication2"
FULL_TAG="${IMAGE_NAME}:${VERSION}"

echo -e "${GREEN}=== Сборка и публикация Docker образа ===${NC}"
echo -e "Username: ${DOCKERHUB_USERNAME}"
echo -e "Image: ${FULL_TAG}"
echo ""

# Проверка входа в Docker Hub
if ! docker info | grep -q "Username"; then
    echo -e "${YELLOW}Требуется вход в Docker Hub...${NC}"
    docker login
fi

# Переход в директорию проекта
cd "$(dirname "$0")/WebApplication2" || exit 1

# Сборка образа
echo -e "${GREEN}[1/3] Сборка образа...${NC}"
docker build \
    --build-arg DOTNET_VERSION=10.0 \
    --build-arg PROJECT_NAME=WebApplication2 \
    -t "${FULL_TAG}" \
    -t "${IMAGE_NAME}:latest" \
    .

# Проверка успешности сборки
if [ $? -ne 0 ]; then
    echo -e "${RED}Ошибка при сборке образа${NC}"
    exit 1
fi

echo -e "${GREEN}[2/3] Образ успешно собран${NC}"

# Публикация образа
echo -e "${GREEN}[3/3] Публикация образа в Docker Hub...${NC}"
docker push "${FULL_TAG}"

if [ "$VERSION" != "latest" ]; then
    docker push "${IMAGE_NAME}:latest"
fi

echo ""
echo -e "${GREEN}✓ Образ успешно опубликован!${NC}"
echo -e "  ${FULL_TAG}"
if [ "$VERSION" != "latest" ]; then
    echo -e "  ${IMAGE_NAME}:latest"
fi
echo ""
echo -e "Использование:"
echo -e "  docker pull ${FULL_TAG}"
echo -e "  docker run -d -p 8080:8080 ${FULL_TAG}"

