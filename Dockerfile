FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_ENVIRONMENT=Production

VOLUME /root/.aspnet/DataProtection-Keys

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Presentation/GmbhSystem.Api/GmbhSystem.Api.csproj", "src/Presentation/GmbhSystem.Api/"]
COPY ["src/Core/GmbhSystem.Application/GmbhSystem.Application.csproj", "src/Core/GmbhSystem.Application/"]
COPY ["src/Core/GmbhSystem.Domain/GmbhSystem.Domain.csproj", "src/Core/GmbhSystem.Domain/"]
COPY ["src/Infrastructure/GmbhSystem.Infrastructure/GmbhSystem.Infrastructure.csproj", "src/Infrastructure/GmbhSystem.Infrastructure/"]
COPY ["src/Infrastructure/GmbhSystem.Persistence/GmbhSystem.Persistence.csproj", "src/Infrastructure/GmbhSystem.Persistence/"]
RUN dotnet restore "src/Presentation/GmbhSystem.Api/GmbhSystem.Api.csproj"
COPY . .
WORKDIR "/src/src/Presentation/GmbhSystem.Api"
RUN dotnet build "./GmbhSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GmbhSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "GmbhSystem.Api.dll"]