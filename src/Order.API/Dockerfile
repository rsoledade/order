FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Order.API/Order.API.csproj", "src/Order.API/"]
COPY ["src/Order.Application/Order.Application.csproj", "src/Order.Application/"]
COPY ["src/Order.Domain/Order.Domain.csproj", "src/Order.Domain/"]
COPY ["src/Order.Infrastructure/Order.Infrastructure.csproj", "src/Order.Infrastructure/"]
RUN dotnet restore "src/Order.API/Order.API.csproj"
COPY . .
WORKDIR "/src/src/Order.API"
RUN dotnet build "Order.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Order.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.API.dll"]
