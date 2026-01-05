# Discord Bot (ds-bot) — Development Guide

Этот документ предназначен для разработчика, который пишет **Discord-бота** (`ds-bot`).

Бот запускается **в Docker**, код редактируется локально в WSL.

---

## Требования

### Обязательно

* Docker Desktop
* WSL2 (Ubuntu)
* git

В Docker Desktop должна быть включена:

* `Settings → Resources → WSL Integration`
* интеграция с вашей WSL-дистрибуцией

---

## Рекомендуется (для удобной разработки)

Установить Node.js 20 в WSL:

```bash
sudo apt update
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs
node -v
npm -v
```

Это нужно для:

* автодополнения в IDE
* eslint / prettier
* локального запуска и отладки

---

## Клонирование проекта

```bash
cd /mnt/g/Project
git clone <REPO_URL>
cd DiscordBotRulleteBan
```

---

## Переменные окружения

Создать локальный `.env` из шаблона:

```bash
cp .env.example .env
```

Минимум, что нужно заполнить:

* `DISCORD_TOKEN`

Файл `.env` **не коммитится**.

---

## Запуск зависимостей

Перед запуском бота должны быть запущены:

```bash
docker compose up -d postgres
docker compose up -d backend
```

---

## Запуск Discord-бота

```bash
docker compose up -d ds-bot
```

Логи бота:

```bash
docker compose logs -f ds-bot
```

---

## Проверка, что бот жив

Бот не имеет внешнего порта.
Проверка осуществляется по логам:

```bash
docker compose logs -f ds-bot
```

Если в логах есть:

* успешный запуск
* логин в Discord (если токен задан)

→ бот работает.

---

## Взаимодействие с backend

Изнутри контейнера бот обращается к backend по адресу:

```
http://backend:8080
```

Переменная окружения:

```
BOT_BACKEND_BASE_URL=http://backend:8080
```

---

## Работа с кодом

Код бота находится в:

```
NodeJSDSBot/
```

Типовой dev-скрипт:

```json
"scripts": {
  "dev": "node src/index.js"
}
```

При изменении кода:

* перезапуск:

```bash
docker compose restart ds-bot
```

* если менялись зависимости:

```bash
docker compose up -d --build ds-bot
```

---

## Вход в контейнер бота

```bash
docker compose exec ds-bot sh
```

---

## Типовые проблемы

### Контейнер сразу падает

```bash
docker compose logs -n 200 ds-bot
```

Частые причины:

* не задан `DISCORD_TOKEN`
* ошибка в коде бота
* отсутствует `package.json`

---

## Git + WSL

В WSL выполнить один раз:

```bash
git config --global core.autocrlf input
```

`node_modules/` **не коммитится** — зависимости ставятся внутри Docker.

---

## Быстрый старт (чек-лист)

```bash
cd /mnt/g/Project/DiscordBotRulleteBan
cp .env.example .env
# заполнить DISCORD_TOKEN

docker compose up -d postgres
docker compose up -d backend
docker compose up -d ds-bot

docker compose logs -f ds-bot
```
