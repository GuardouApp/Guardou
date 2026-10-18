# syntax=docker/dockerfile:1

ARG FLUTTER_VERSION=3.47.7

FROM --platform=$BUILDPLATFORM debian:bookworm-slim AS flutter-sdk
ARG FLUTTER_VERSION
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl git unzip xz-utils \
    && rm -rf /var/lib/apt/lists/*
RUN git clone --depth 1 --branch ${FLUTTER_VERSION} https://github.com/flutter/flutter.git /opt/flutter
ENV PATH="/opt/flutter/bin:/opt/flutter/bin/cache/dart-sdk/bin:${PATH}"
RUN flutter config --no-analytics --no-cli-animations \
    && flutter precache --web

FROM flutter-sdk AS frontend
WORKDIR /src/frontend
COPY frontend/pubspec.yaml frontend/pubspec.lock ./
RUN flutter pub get
COPY api/openapi.json /src/api/openapi.json
COPY frontend/ ./
RUN dart run swagger_parser \
    && dart run build_runner build \
    && flutter build web --release

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS server
WORKDIR /src/server
COPY server/nuget.config server/Directory.Build.props server/Directory.Packages.props server/.editorconfig ./
COPY server/Guardou.Domain/Guardou.Domain.csproj Guardou.Domain/
COPY server/Guardou.Application/Guardou.Application.csproj Guardou.Application/
COPY server/Guardou.Infrastructure/Guardou.Infrastructure.csproj Guardou.Infrastructure/
COPY server/Guardou.Api/Guardou.Api.csproj Guardou.Api/
COPY server/Guardou.Cli/Guardou.Cli.csproj Guardou.Cli/
RUN dotnet restore Guardou.Cli/Guardou.Cli.csproj
COPY server/ ./
RUN dotnet publish Guardou.Cli/Guardou.Cli.csproj -c Release -o /app --no-restore \
    -p:UseAppHost=false -p:OpenApiGenerateDocuments=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
RUN addgroup -g 1000 guardou \
    && adduser -D -u 1000 -G guardou -h /home/guardou guardou \
    && mkdir -p /data \
    && chown guardou:guardou /data \
    && printf '#!/bin/sh\nexec dotnet /app/guardou.dll "$@"\n' > /usr/local/bin/guardou \
    && chmod 755 /usr/local/bin/guardou
WORKDIR /app
COPY --from=server /app ./
COPY --from=frontend /src/frontend/build/web ./wwwroot/
ENV ASPNETCORE_HTTP_PORTS=8080
USER 1000:1000
VOLUME /data
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1:8080/health || exit 1
ENTRYPOINT ["dotnet", "/app/guardou.dll"]
CMD ["run"]
