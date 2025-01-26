# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
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
RUN dotnet publish "Server/music-manager-starter.Server.csproj" -c Release -o /app/publish --runtime linux-x64 --self-contained true

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal-amd64
WORKDIR /app
COPY --from=build /app/publish .

# Create directory for uploads
RUN mkdir -p /app/wwwroot/uploads && chmod 777 /app/wwwroot/uploads

# Expose the port the app will run on
EXPOSE 8080

# Configure the health check endpoint
ENV ASPNETCORE_URLS=http://+:8080
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["./music-manager-starter.Server"]
