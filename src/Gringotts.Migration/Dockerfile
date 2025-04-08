# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Gringotts.Migration/Gringotts.Migration.csproj", "src/Gringotts.Migration/"]
RUN dotnet restore "src/Gringotts.Migration/Gringotts.Migration.csproj"
COPY . .
WORKDIR "/src/src/Gringotts.Migration"
RUN dotnet publish "Gringotts.Migration.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gringotts.Migration.dll"]
