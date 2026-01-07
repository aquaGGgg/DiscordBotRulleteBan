FROM node:20-alpine AS dev
WORKDIR /app

COPY package*.json ./
RUN npm install
RUN apk add --no-cache ffmpeg
RUN apk add --no-cache ffmpeg python3 make g++ \
  && npm ci \
  && apk del python3 make g++


COPY . .
EXPOSE 3000

CMD ["npm","run","dev"]


FROM node:20-alpine AS prod
WORKDIR /app

COPY package*.json ./
RUN npm install --omit=dev

COPY . .
EXPOSE 3000

CMD ["node","src/index.js"]
