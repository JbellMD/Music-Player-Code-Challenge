# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /source

# Install Node.js and npm
RUN apk add --update nodejs npm

# Copy everything
COPY . .

# Install npm dependencies and build client assets
WORKDIR /source
RUN npm install
RUN mkdir -p Client/wwwroot/css
RUN npm run buildcss:release

# Restore dependencies and build
RUN dotnet restore "Server/music-manager-starter.Server.csproj"
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
