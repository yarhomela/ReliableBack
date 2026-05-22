FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ReliableBack.Worker/ReliableBack.Worker.csproj",         "ReliableBack.Worker/"]
COPY ["ReliableBack.Application/ReliableBack.Application.csproj", "ReliableBack.Application/"]
COPY ["ReliableBack.Infrastructure/ReliableBack.Infrastructure.csproj", "ReliableBack.Infrastructure/"]
COPY ["ReliableBack.Domain/ReliableBack.Domain.csproj",         "ReliableBack.Domain/"]

RUN dotnet restore "ReliableBack.Worker/ReliableBack.Worker.csproj"

COPY . .
WORKDIR "/src/ReliableBack.Worker"
RUN dotnet build "ReliableBack.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReliableBack.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReliableBack.Worker.dll"]