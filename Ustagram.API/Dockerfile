FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Ustagram.API/Ustagram.API.csproj", "Ustagram.API/"]
COPY ["Ustagram.Application/Ustagram.Application.csproj", "Ustagram.Application/"]
COPY ["Ustagram.Domain/Ustagram.Domain.csproj", "Ustagram.Domain/"]
COPY ["Ustagram.Infrastructure/Ustagram.Infrastructure.csproj", "Ustagram.Infrastructure/"]
RUN dotnet restore "Ustagram.API/Ustagram.API.csproj"
COPY . .
WORKDIR "/src/Ustagram.API"
RUN dotnet build "Ustagram.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Ustagram.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ustagram.API.dll"]
