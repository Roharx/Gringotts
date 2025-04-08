# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Gringotts.TransactionPublisher/Gringotts.TransactionPublisher.csproj", "src/Gringotts.TransactionPublisher/"]
RUN dotnet restore "src/Gringotts.TransactionPublisher/Gringotts.TransactionPublisher.csproj"
COPY . .
WORKDIR "/src/src/Gringotts.TransactionPublisher"
RUN dotnet publish "Gringotts.TransactionPublisher.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gringotts.TransactionPublisher.dll"]
