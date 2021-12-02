# FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal AS base
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/server/", "src/server/"]
COPY [".paket/", ".paket/"]
COPY [".config/", ".config/"]
COPY ["csse_covid_19_data/csse_covid_19_daily_reports/", "csse_covid_19_data/csse_covid_19_daily_reports/"]
COPY ["paket.dependencies", "paket.lock", "./"]

RUN dotnet tool restore
WORKDIR /src/src/server
# RUN dotnet restore "server.fsproj"
# RUN dotnet build "server.fsproj" -c Release -o /app/build

# FROM build AS publish
# RUN dotnet publish -c Release -r alpine-x64 "server.fsproj" --self-contained true /p:PublishTrimmed=true /p:PublishSingleFile=true -o /app/publish
RUN dotnet publish -c Release -r alpine-x64 "server.fsproj" --self-contained true /p:PublishTrimmed=true -o /app/publish

FROM base AS final
WORKDIR /app/publish
COPY ["csse_covid_19_data/csse_covid_19_daily_reports/", "/csse_covid_19_data/csse_covid_19_daily_reports/"]
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "server.dll"]