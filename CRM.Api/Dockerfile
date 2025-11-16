# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["CRM.Api/CRM.Api.csproj", "CRM.Api/"]
RUN dotnet restore "CRM.Api/CRM.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/CRM.Api"
RUN dotnet build "CRM.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CRM.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CRM.Api.dll"]

