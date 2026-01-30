FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Product/Back/Api/GesFer.Api.csproj", "src/Product/Back/Api/"]
COPY ["src/Product/Back/application/GesFer.Application.csproj", "src/Product/Back/application/"]
COPY ["src/Product/Back/domain/GesFer.Domain.csproj", "src/Product/Back/domain/"]
COPY ["src/Product/Back/Infrastructure/GesFer.Infrastructure.csproj", "src/Product/Back/Infrastructure/"]
COPY ["src/Shared/Back/GesFer.Shared.Back.Domain.csproj", "src/Shared/Back/"]
RUN dotnet restore "src/Product/Back/Api/GesFer.Api.csproj"
COPY . .
WORKDIR "/src/src/Product/Back/Api"
RUN dotnet build "GesFer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GesFer.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GesFer.Api.dll"]
