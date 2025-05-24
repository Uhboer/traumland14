#!/bin/bash

# Получаем абсолютный путь к скрипту
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
ROOT_DIR="$SCRIPT_DIR/.."

# Пути относительно корня проекта
RESOURCES_DIR="$ROOT_DIR/Resources"
OUTPUT_FILE="$RESOURCES_DIR/buildInfo.yml"

# Создаем папку Resources если ее нет
mkdir -p "$RESOURCES_DIR"

# Получаем информацию о коммите
COMMIT_HASH=$(git -C "$ROOT_DIR" rev-parse --short HEAD 2>/dev/null || echo "unknown")

# Получаем дату коммита в UTC и парсим компоненты
COMMIT_DATE=$(git -C "$ROOT_DIR" log -1 --format=%cd --date=format-local:%Y-%m-%d 2>/dev/null || echo "1970-01-01")

# Разбираем дату на компоненты (убираем ведущие нули)
IFS='-' read -r YEAR MONTH DAY <<< "$COMMIT_DATE"
MONTH=$((10#$MONTH))
DAY=$((10#$DAY))

# Записываем в YAML-файл
cat > "$OUTPUT_FILE" <<EOL
year: $YEAR
month: $MONTH
day: $DAY
commit: "$COMMIT_HASH"
EOL

echo "Build info generated at: $OUTPUT_FILE"
echo "Commit: $COMMIT_HASH, Date: $YEAR-$MONTH-$DAY"
