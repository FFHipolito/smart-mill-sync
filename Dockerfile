FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/SmartMillSync.Domain/SmartMillSync.Domain.csproj", "src/SmartMillSync.Domain/"]
COPY ["src/SmartMillSync.Shared/SmartMillSync.Shared.csproj", "src/SmartMillSync.Shared/"]
COPY ["src/SmartMillSync.Application/SmartMillSync.Application.csproj", "src/SmartMillSync.Application/"]
COPY ["src/SmartMillSync.Infrastructure/SmartMillSync.Infrastructure.csproj", "src/SmartMillSync.Infrastructure/"]
COPY ["src/SmartMillSync.Api/SmartMillSync.Api.csproj", "src/SmartMillSync.Api/"]

RUN dotnet restore "src/SmartMillSync.Api/SmartMillSync.Api.csproj"

COPY . .
WORKDIR "/src/src/SmartMillSync.Api"
RUN dotnet build "SmartMillSync.Api.csproj" -c Release -o /app/build
RUN dotnet publish "SmartMillSync.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartMillSync.Api.dll"]
