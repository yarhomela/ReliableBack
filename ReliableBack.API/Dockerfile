FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ReliableBack.API/ReliableBack.API.csproj",           "ReliableBack.API/"]
COPY ["ReliableBack.Application/ReliableBack.Application.csproj", "ReliableBack.Application/"]
COPY ["ReliableBack.Infrastructure/ReliableBack.Infrastructure.csproj", "ReliableBack.Infrastructure/"]
COPY ["ReliableBack.Domain/ReliableBack.Domain.csproj",     "ReliableBack.Domain/"]

RUN dotnet restore "ReliableBack.API/ReliableBack.API.csproj"

COPY . .
WORKDIR "/src/ReliableBack.API"
RUN dotnet build "ReliableBack.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReliableBack.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReliableBack.API.dll"]