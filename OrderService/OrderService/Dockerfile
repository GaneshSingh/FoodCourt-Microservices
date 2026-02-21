# --- OrderService Dockerfile (.NET 8.0) ---

# 1. Use the .NET 8 Kitchen (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY OrderService.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/out

# 2. Use the .NET 8 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Tell the robot to listen on port 8080 inside the box
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "OrderService.dll"]