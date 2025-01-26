# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Server/music-manager-starter.Server.csproj", "Server/"]
COPY ["Client/music-manager-starter.Client.csproj", "Client/"]
COPY ["Shared/music-manager-starter.Shared.csproj", "Shared/"]
COPY ["Data/music-manager-start.Data.csproj", "Data/"]
RUN dotnet restore "Server/music-manager-starter.Server.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish the application
RUN dotnet publish "Server/music-manager-starter.Server.csproj" -c Release -o /app/publish

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /app
COPY --from=build /app/publish .

# Create directory for uploads
RUN mkdir -p /app/wwwroot/uploads && chmod 777 /app/wwwroot/uploads

# Expose the port the app will run on
ENV PORT=8080
ENV ASPNETCORE_URLS=http://+:${PORT}

# Configure the health check endpoint
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:${PORT}/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "music-manager-starter.Server.dll"]
