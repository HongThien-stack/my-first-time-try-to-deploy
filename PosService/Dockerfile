# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "src/PosService.API/PosService.API.csproj"
RUN dotnet build "src/PosService.API/PosService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/PosService.API/PosService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "PosService.API.dll"]
