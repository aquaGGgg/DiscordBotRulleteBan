# DiscordBotRulleteBan — Development Environment (WSL + Docker)

Проект состоит из 4 сервисов, каждый поднимается **вручную**:

* `postgres` — база данных PostgreSQL
* `backend` — ASP.NET Minimal API
* `ds-bot` — Node.js (discord.js)
* `admin-panel` — Vue (Vite)

Все команды `docker compose` выполнять **из корня проекта**, где расположен `docker-compose.yml`.

---

## Требования

### Windows

* Docker Desktop
* WSL2 (Ubuntu или аналог)

В Docker Desktop необходимо включить:

* `Settings → Resources → WSL Integration`
* интеграцию с вашей WSL-дистрибуцией

### WSL

* git
* (рекомендуется) Node.js 20+ для разработки бота и фронта

Проверка Docker:

```bash
docker --version
docker compose version
```

---

## Клонирование проекта

Пример для диска `G:`:

```bash
cd /mnt/g/Project
git clone <REPO_URL>
cd DiscordBotRulleteBan
```

---

## Переменные окружения

В репозитории хранится только `.env.example`.

Создать локальный `.env`:

```bash
cp .env.example .env
```

Минимум, что требуется заполнить:

* `DISCORD_TOKEN` (если бот должен подключаться к Discord)

Файл `.env` **не коммитится**.

---

## Запуск сервисов

### 1) Postgres

```bash
docker compose up -d postgres
```

Проверка:

```bash
docker compose ps
docker compose logs -n 50 postgres
```

---

### 2) Backend (ASP.NET)

```bash
docker compose up -d backend
```

Проверка:

```bash
curl -4 http://127.0.0.1:8080/
```

Логи:

```bash
docker compose logs -f backend
```

> Backend должен таргетить `.NET 8.0`

```xml
<TargetFramework>net8.0</TargetFramework>
```

---

### 3) Admin panel (Vue + Vite)

```bash
docker compose up -d admin-panel
```

Открыть в браузере:

```
http://localhost:5173
```

Логи:

```bash
docker compose logs -f admin-panel
```

---

### 4) Discord bot (ds-bot)

```bash
docker compose up -d ds-bot
```

Логи:

```bash
docker compose logs -f ds-bot
```

---

## Полезные команды

Статус сервисов:

```bash
docker compose ps
```

Остановить сервис:

```bash
docker compose stop ds-bot
```

Перезапустить сервис:

```bash
docker compose restart ds-bot
```

Пересобрать сервис (после изменения зависимостей):

```bash
docker compose up -d --build ds-bot
```

Остановить всё:

```bash
docker compose down
```

Полный сброс (включая volumes):

```bash
docker compose down -v
```

---

## Порты и адреса

### Postgres

* Снаружи (Windows / WSL): `localhost:5432`
* Внутри docker-сети: `postgres:5432`

### Backend

* Снаружи: `http://localhost:8080`
* Внутри docker-сети: `http://backend:8080`

### Admin panel

* `http://localhost:5173`

### Discord bot

* Внешний порт не используется
* Проверка работы — через логи

---

## Вход в контейнеры

Backend:

```bash
docker compose exec backend bash
```

Admin panel:

```bash
docker compose exec admin-panel sh
```

Discord bot:

```bash
docker compose exec ds-bot sh
```

Postgres:

```bash
docker compose exec postgres sh
```

---

## Рекомендации по разработке

### Node.js (для ds-bot и admin-panel)

Для удобной локальной разработки рекомендуется установить Node.js 20 в WSL:

```bash
sudo apt update
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs
node -v
npm -v
```

### Git + WSL (CRLF/LF)

В WSL выполнить один раз:

```bash
git config --global core.autocrlf input
```

`node_modules/`, `bin/`, `obj/` **не коммитятся** — зависимости и сборки происходят внутри Docker.

---

## Типовые проблемы

### Контейнер стартует и сразу падает

Смотреть логи:

```bash
docker compose logs -n 200 <service>
```

### Ошибка target framework (.NET)

Backend должен использовать `.NET 8.0`:

```xml
<TargetFramework>net8.0</TargetFramework>
```

### Docker не найден в WSL

Проверить, что включена WSL Integration в Docker Desktop.

---

## Быстрый старт (чек-лист)

```bash
cd /mnt/g/Project/DiscordBotRulleteBan
cp .env.example .env

# запуск сервисов
docker compose up -d postgres
docker compose up -d backend
docker compose up -d admin-panel
docker compose up -d ds-bot

# проверка
docker compose ps
```
