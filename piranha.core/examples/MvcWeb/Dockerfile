# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the entire solution structure
COPY . /src/

# Restore dependencies
WORKDIR /src/examples/MvcWeb
RUN dotnet restore

# Build the application
RUN dotnet build -c Release -o /app/build -f net9.0

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false -f net9.0

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

# Install required dependencies for telemetry
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create directory for uploads if using local file storage
RUN mkdir -p /app/wwwroot/uploads

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "MvcWeb.dll"]