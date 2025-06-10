# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Order.API/Order.API.csproj", "src/Order.API/"]
COPY ["src/Order.Application/Order.Application.csproj", "src/Order.Application/"]
COPY ["src/Order.Domain/Order.Domain.csproj", "src/Order.Domain/"]
COPY ["src/Order.Infrastructure/Order.Infrastructure.csproj", "src/Order.Infrastructure/"]
RUN dotnet restore "./src/Order.API/Order.API.csproj"
COPY . .
WORKDIR "/src/src/Order.API"
RUN dotnet build "./Order.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Order.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app

# Copia o script de entrypoint
COPY src/Order.API/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

COPY --from=publish /app/publish .

USER $APP_UID

# O restante permanece igual, pois o entrypoint.sh já não tentará criar migrations
ENTRYPOINT ["/app/entrypoint.sh"]