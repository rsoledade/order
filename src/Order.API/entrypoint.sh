#!/bin/sh
set -e

MIGRATIONS_DIR="/app/Data/Migrations"

# Apenas aplica migrations existentes; não tenta criar migrations no container de produção
if [ -z "$(ls -A $MIGRATIONS_DIR 2>/dev/null)" ]; then
  echo "Nenhuma migration encontrada. É necessário criar migrations localmente antes de subir o container."
  echo "Utilize o comando: dotnet ef migrations add InitialCreate --project src/Order.Infrastructure/Order.Infrastructure.csproj 
		--startup-project src/Order.API/Order.API.csproj --output-dir Data/Migrations"
else
  echo "Migrations já existem. Aplicando migrations pendentes (se houver)..."
  dotnet ef database update --project /app/Order.Infrastructure.csproj --startup-project /app/Order.API.csproj
fi

# Inicia a aplicação
dotnet Order.API.dll
