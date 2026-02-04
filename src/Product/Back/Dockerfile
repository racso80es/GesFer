FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Product/Back/Api/GesFer.Api.csproj", "Product/Back/Api/"]
COPY ["src/Product/Back/application/GesFer.Application.csproj", "Product/Back/application/"]
COPY ["src/Product/Back/domain/GesFer.Domain.csproj", "Product/Back/domain/"]
COPY ["src/Product/Back/Infrastructure/GesFer.Infrastructure.csproj", "Product/Back/Infrastructure/"]
COPY ["src/Shared/Back/GesFer.Shared.Back.Domain.csproj", "Shared/Back/"]
RUN dotnet restore "Product/Back/Api/GesFer.Api.csproj"
# Copy only the project source folders to avoid unexpected context layout
COPY src/Product/Back/ ./Product/Back/
COPY src/Shared/Back/ ./Shared/Back/
WORKDIR "/src/Product/Back/Api"
RUN dotnet build "GesFer.Api.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/Product/Back/Api"
RUN dotnet publish "GesFer.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
# Install curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GesFer.Api.dll"]
