FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5010
EXPOSE 5011

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Admin/Back/Api/GesFer.Admin.Api.csproj", "Admin/Back/Api/"]
COPY ["src/Admin/Back/application/GesFer.Admin.Application.csproj", "Admin/Back/application/"]
COPY ["src/Admin/Back/domain/GesFer.Admin.Domain.csproj", "Admin/Back/domain/"]
COPY ["src/Admin/Back/Infrastructure/GesFer.Admin.Infra.csproj", "Admin/Back/Infrastructure/"]
COPY ["src/Product/Back/Infrastructure/GesFer.Infrastructure.csproj", "Product/Back/Infrastructure/"]
COPY ["src/Product/Back/domain/GesFer.Domain.csproj", "Product/Back/domain/"]
COPY ["src/Shared/Back/GesFer.Shared.Back.Domain.csproj", "Shared/Back/"]
RUN dotnet restore "Admin/Back/Api/GesFer.Admin.Api.csproj"
# Copy only the project source folders to avoid unexpected context layout
COPY src/Admin/Back/ ./Admin/Back/
COPY src/Product/Back/ ./Product/Back/
COPY src/Shared/Back/ ./Shared/Back/
WORKDIR "/src/Admin/Back/Api"
RUN dotnet build "GesFer.Admin.Api.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/Admin/Back/Api"
RUN dotnet publish "GesFer.Admin.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
# Install curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GesFer.Admin.Api.dll"]
