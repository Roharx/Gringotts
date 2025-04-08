FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Gringotts.LedgerService/Gringotts.LedgerService.csproj", "src/Gringotts.LedgerService/"]
RUN dotnet restore "src/Gringotts.LedgerService/Gringotts.LedgerService.csproj"
COPY . .
WORKDIR "/src/src/Gringotts.LedgerService"
RUN dotnet build "Gringotts.LedgerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gringotts.LedgerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gringotts.LedgerService.dll"]
