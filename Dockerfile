# The one image that runs on staging and production (ADR-0004): ASP.NET Core serves the API
# and the built SPA, and the EF Core migration bundle sits next to the app (ADR-0009).
#
#   docker build --build-arg APP_VERSION=<short sha> -t musialize .
#
# Both the app and the bundle read ConnectionStrings__Database from the environment.
#   App:        docker run -e ConnectionStrings__Database=… -p 8080:8080 musialize
#   Migrations: docker run --rm -e ConnectionStrings__Database=… musialize ./efbundle
#
# Three stages: "web" builds the SPA, "api" builds the API and the migration bundle, and the
# last stage copies only their output into the runtime image. Node.js, the .NET SDK and the
# source code never reach the image that runs.
#
# Each stage copies the dependency files first and installs from them, then copies the source.
# Docker rebuilds a layer only when its inputs change, so most commits, which change source
# but not dependencies, skip the slow install and restore steps.

# ── Stage 1: build the SPA ──────────────────────────────────────────────────────────────

FROM node:26-slim AS web
WORKDIR /src
# Husky's Git hooks aren't needed in a build.
ENV HUSKY=0

# Dependencies: only the package files, so this layer stays cached until they change.
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY web/package.json web/
# Node.js 25+ ships without Corepack, so install the pnpm version from packageManager.
RUN npm install --global "$(node -p "require('./package.json').packageManager")"
RUN pnpm install --frozen-lockfile --filter @musialize/web

# Source: build the SPA into web/dist.
COPY web/ web/
RUN pnpm --filter @musialize/web build

# ── Stage 2: build the API and the migration bundle ─────────────────────────────────────

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS api
WORKDIR /src

# Dependencies: restore NuGet packages and the dotnet-ef tool from the project files only.
# .editorconfig too: it tells the analyzers to skip the generated EF Core migrations.
COPY global.json dotnet-tools.json .editorconfig ./
COPY backend/Directory.Build.props backend/Directory.Packages.props backend/
COPY backend/src/Musialize.Domain/Musialize.Domain.csproj backend/src/Musialize.Domain/
COPY backend/src/Musialize.Application/Musialize.Application.csproj backend/src/Musialize.Application/
COPY backend/src/Musialize.Infrastructure/Musialize.Infrastructure.csproj backend/src/Musialize.Infrastructure/
COPY backend/src/Musialize.Api/Musialize.Api.csproj backend/src/Musialize.Api/
RUN dotnet tool restore && dotnet restore backend/src/Musialize.Api

# Source, plus the SPA from stage 1 in wwwroot. It must be there before publishing, because
# publish writes the static assets manifest that lets the app serve the files compressed.
COPY backend/src/ backend/src/
COPY --from=web /src/web/dist/ backend/src/Musialize.Api/wwwroot/

# The short commit SHA that /health reports.
ARG APP_VERSION=dev
RUN dotnet publish backend/src/Musialize.Api --no-restore --output /app -p:AppVersion=$APP_VERSION

# The migration bundle: one executable that applies the pending migrations.
RUN dotnet ef migrations bundle --project backend/src/Musialize.Infrastructure \
    --startup-project backend/src/Musialize.Api --configuration Release --output /app/efbundle

# ── Stage 3: the image that runs ────────────────────────────────────────────────────────

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble

# Dokploy's health check runs curl inside the container.
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# The app and efbundle from stage 2, in the working directory /app.
WORKDIR /app
COPY --from=api /app/ ./

# The app already defaults to Production, but the migration bundle defaults to Development and
# would then read appsettings.Development.json, falling back to local credentials when
# ConnectionStrings__Database is missing. Production makes it fail with a clear error instead.
ENV ASPNETCORE_ENVIRONMENT=Production

# The non-root user that comes with the image. ASP.NET Core listens on 8080 by default.
USER $APP_UID
EXPOSE 8080
CMD ["dotnet", "Musialize.Api.dll"]
