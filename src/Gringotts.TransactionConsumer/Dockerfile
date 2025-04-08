# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Gringotts.TransactionConsumer/Gringotts.TransactionConsumer.csproj", "src/Gringotts.TransactionConsumer/"]
RUN dotnet restore "src/Gringotts.TransactionConsumer/Gringotts.TransactionConsumer.csproj"
COPY . .
WORKDIR "/src/src/Gringotts.TransactionConsumer"
RUN dotnet publish "Gringotts.TransactionConsumer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gringotts.TransactionConsumer.dll"]
