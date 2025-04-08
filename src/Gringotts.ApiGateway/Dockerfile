FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Gringotts.ApiGateway/Gringotts.ApiGateway.csproj", "src/Gringotts.ApiGateway/"]
RUN dotnet restore "src/Gringotts.ApiGateway/Gringotts.ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/Gringotts.ApiGateway"
RUN dotnet build "Gringotts.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gringotts.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gringotts.ApiGateway.dll"]
