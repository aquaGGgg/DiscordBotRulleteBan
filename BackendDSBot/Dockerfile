# =========================
# DEV (dotnet watch)
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /src
EXPOSE 8080
# исходники примонтируются volume'ом, команду запуска задаёт docker-compose


# =========================
# BUILD (publish)
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BackendDSBot.csproj ./
RUN dotnet restore ./BackendDSBot.csproj

COPY . ./
RUN dotnet publish ./BackendDSBot.csproj -c Release -o /out --no-restore


# =========================
# PROD (runtime)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS prod
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

COPY --from=build /out ./
ENTRYPOINT ["dotnet","BackendDSBot.dll"]
