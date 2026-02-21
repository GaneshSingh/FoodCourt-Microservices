# --- ItemService Dockerfile (.NET 9.0) ---

# 1. Use the .NET 9 Kitchen (SDK) to build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY ItemService.csproj ./
RUN dotnet restore

# Copy the rest and build
COPY . ./
RUN dotnet publish -c Release -o /app/out

# 2. Use the .NET 9 Runtime to run (Lighter image)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Tell the robot to listen on port 8080 inside the box
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "ItemService.dll"]