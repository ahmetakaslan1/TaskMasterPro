# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY TaskMasterPro.slnx ./
COPY src/TaskMasterPro.Domain/TaskMasterPro.Domain.csproj src/TaskMasterPro.Domain/
COPY src/TaskMasterPro.Application/TaskMasterPro.Application.csproj src/TaskMasterPro.Application/
COPY src/TaskMasterPro.Infrastructure/TaskMasterPro.Infrastructure.csproj src/TaskMasterPro.Infrastructure/
COPY src/TaskMasterPro.API/TaskMasterPro.API.csproj src/TaskMasterPro.API/
COPY tests/TaskMasterPro.UnitTests/TaskMasterPro.UnitTests.csproj tests/TaskMasterPro.UnitTests/
COPY tests/TaskMasterPro.IntegrationTests/TaskMasterPro.IntegrationTests.csproj tests/TaskMasterPro.IntegrationTests/

# Restore dependencies
RUN dotnet restore TaskMasterPro.slnx

# Copy all files
COPY . .

# Build and publish API
WORKDIR /src/src/TaskMasterPro.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy the published output from build stage
COPY --from=build /app/publish .

# Expose ports
EXPOSE 8080

# Configure environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Entry point
ENTRYPOINT ["dotnet", "TaskMasterPro.API.dll"]
