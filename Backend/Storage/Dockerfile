# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Storage.slnx ./
COPY Storage.Api/Storage.Api.csproj Storage.Api/
COPY Storage.Data/Storage.Data.csproj Storage.Data/
COPY Storage.Domain/Storage.Domain.csproj Storage.Domain/
COPY Storage.Services/Storage.Services.csproj Storage.Services/

RUN dotnet restore Storage.Api/Storage.Api.csproj

COPY . .

RUN dotnet publish Storage.Api/Storage.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "Storage.Api.dll"]
