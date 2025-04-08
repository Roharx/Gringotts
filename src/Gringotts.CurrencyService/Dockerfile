# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the csproj file and restore dependencies first (caching)
COPY ["src/Gringotts.CurrencyService/Gringotts.CurrencyService.csproj", "src/Gringotts.CurrencyService/"]
RUN dotnet restore "src/Gringotts.CurrencyService/Gringotts.CurrencyService.csproj"

# Copy the rest of the source code and publish
COPY . .
WORKDIR "/src/src/Gringotts.CurrencyService"
RUN dotnet publish "Gringotts.CurrencyService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gringotts.CurrencyService.dll"]
