FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ARG PORT=8080
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TISM_MQTT/TISM_MQTT.csproj", "TISM_MQTT/"]
RUN dotnet restore "./TISM_MQTT/TISM_MQTT.csproj"
COPY . .
WORKDIR "/src/TISM_MQTT"
RUN dotnet build "./TISM_MQTT.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TISM_MQTT.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TISM_MQTT.dll"]
