FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj files for all layers to leverage Docker layer caching
COPY ["SnapShop.API/SnapShop.API.csproj", "SnapShop.API/"]
COPY ["SnapShop.Application/SnapShop.Application.csproj", "SnapShop.Application/"]
COPY ["SnapShop.Core/SnapShop.Core.csproj", "SnapShop.Core/"]
COPY ["SnapShop.Infrastructure/SnapShop.Infrastructure.csproj", "SnapShop.Infrastructure/"]

RUN dotnet restore "SnapShop.API/SnapShop.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/SnapShop.API"
RUN dotnet publish "SnapShop.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "SnapShop.API.dll"]
