FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /src
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore ./BackendDSBot.csproj
RUN dotnet publish ./BackendDSBot.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS prod
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENTRYPOINT ["dotnet","BackendDSBot.dll"]
