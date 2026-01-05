# DiscordBotRulleteBan — Development Environment (WSL + Docker)

Проект состоит из сервисов:
- `postgres` — PostgreSQL
- `backend` — ASP.NET Minimal API
- `ds-bot` — Node.js (discord.js)
- `admin-panel` — Vue (Vite)

Каждый сервис поднимается **вручную**, по отдельности. Все команды `docker compose` выполнять **из корня проекта**, где находится `docker-compose.yml`.

---

## 📋 Содержание
- [Требования](#требования)
- [Клонирование проекта](#клонирование-проекта)
- [Переменные окружения](#переменные-окружения)
- [Запуск сервисов](#запуск-сервисов)
- [Полезные команды](#полезные-команды)
- [Доступные адреса и порты](#доступные-адреса-и-порты)
- [Вход в контейнеры](#вход-в-контейнеры)
- [Рекомендации для разработчика ds-bot](#рекомендации-для-разработчика-ds-bot)
- [Git + WSL (CRLF/LF)](#git--wsl-crlflf)
- [Быстрый старт (чек-лист)](#быстрый-старт-чек-лист)

---

## Требования

### Windows
- Docker Desktop
- WSL2 (Ubuntu)

В Docker Desktop обязательно включить:
- `Settings → Resources → WSL Integration`
- Интеграцию для вашей WSL-дистрибуции

### WSL
- git
- (рекомендуется для разработки бота) Node.js 20+

Проверка Docker:
```bash
docker --version
docker compose version
Клонирование проекта
Пример для диска G::

bash
cd /mnt/g/Project
git clone <REPO_URL>
cd DiscordBotRulleteBan
Переменные окружения
В репозитории хранится только .env.example. Создать локальный .env:

bash
cp .env.example .env
Открыть .env и при необходимости заполнить:

DISCORD_TOKEN

Файл .env не коммитится.

Запуск сервисов
1) Postgres
bash
docker compose up -d postgres
Проверка:

bash
docker compose ps
docker compose logs -n 50 postgres
2) Backend (ASP.NET)
bash
docker compose up -d backend
Проверка:

bash
curl -4 http://127.0.0.1:8080/
Логи:

bash
docker compose logs -f backend
Backend должен таргетить .NET 8.0:

xml
<TargetFramework>net8.0</TargetFramework>
3) Admin panel (Vue + Vite)
bash
docker compose up -d admin-panel
Открыть в браузере: http://localhost:5173

Логи:

bash
docker compose logs -f admin-panel
4) Discord bot
bash
docker compose up -d ds-bot
Логи:

bash
docker compose logs -f ds-bot
Полезные команды
Статус сервисов: docker compose ps

Остановить сервис: docker compose stop ds-bot

Перезапустить сервис: docker compose restart ds-bot

Пересобрать сервис: docker compose up -d --build ds-bot

Остановить всё: docker compose down

Полный сброс (включая volumes): docker compose down -v

Доступные адреса и порты
Сервис	Снаружи	В docker-сети
Postgres	localhost:5432	postgres:5432
Backend	http://localhost:8080	http://backend:8080
Admin panel	http://localhost:5173	-
Discord bot	Порт наружу не используется	-
Проверка работы Discord bot — через логи.

Вход в контейнеры
bash
# Backend
docker compose exec backend bash

# Admin panel
docker compose exec admin-panel sh

# Bot
docker compose exec ds-bot sh

# Postgres
docker compose exec postgres sh
Рекомендации для разработчика ds-bot
Для комфортной разработки рекомендуется установить Node.js 20 в WSL:

bash
sudo apt update
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs
node -v
npm -v
Git + WSL (CRLF/LF)
В WSL выполнить один раз:

bash
git config --global core.autocrlf input
Быстрый старт (чек-лист)
bash
cd /mnt/g/Project/DiscordBotRulleteBan
cp .env.example .env

docker compose up -d postgres
docker compose up -d backend
docker compose up -d admin-panel
docker compose up -d ds-bot

docker compose ps
