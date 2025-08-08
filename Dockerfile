# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies first (cache)
COPY *.csproj ./
RUN dotnet restore

# Copy all source files and build
COPY . ./
RUN dotnet publish -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app

# Copy published app from build stage
COPY --from=build /app/out ./

# Copy appsettings.json for fallback config
COPY appsettings.json ./

# Create directories for mounted volumes
RUN mkdir -p /watched /logs

# Create directories and set environment variables if you want defaults
ENV WATCH_DIR=/watched
ENV LOG_FILE=/logs/audit-log.json

# Run the app, passing in environment variables as arguments if needed
ENTRYPOINT ["dotnet", "FileIntegrityWatcher.dll"]