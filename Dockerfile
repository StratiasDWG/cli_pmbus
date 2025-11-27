# PMBus Master CLI - Multi-stage Docker Build
# This Dockerfile creates a containerized build environment for the PMBus Master CLI
# without requiring .NET SDK installation on the host machine.

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy project files
COPY *.csproj ./
COPY Tests/*.csproj ./Tests/

# Restore dependencies
RUN dotnet restore PmbusMasterCLI.csproj
RUN dotnet restore PmbusMasterCLI.Tests.csproj

# Copy all source files
COPY . .

# Build the application
RUN dotnet build PmbusMasterCLI.csproj -c Release --no-restore

# Run tests
RUN dotnet test PmbusMasterCLI.Tests.csproj -c Release --no-build --verbosity normal

# Publish the application
RUN dotnet publish PmbusMasterCLI.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime (optional, for running in container)
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Note: This image cannot directly access USB devices (NI-8451) without special Docker configuration
# This is primarily for building and testing the CLI binary
ENTRYPOINT ["dotnet", "pmbus-cli.dll"]

# Stage 3: Build artifacts extraction
FROM scratch AS export
COPY --from=build /app/publish /
