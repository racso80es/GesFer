FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5010
EXPOSE 5011

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Admin/Back/Api/GesFer.Admin.Api.csproj", "src/Admin/Back/Api/"]
COPY ["src/Admin/Back/application/GesFer.Admin.Application/GesFer.Admin.Application.csproj", "src/Admin/Back/application/GesFer.Admin.Application/"]
COPY ["src/Admin/Back/domain/GesFer.Admin.Domain/GesFer.Admin.Domain.csproj", "src/Admin/Back/domain/GesFer.Admin.Domain/"]
COPY ["src/Admin/Back/Infrastructure/GesFer.Admin.Infrastructure/GesFer.Admin.Infrastructure.csproj", "src/Admin/Back/Infrastructure/GesFer.Admin.Infrastructure/"]
COPY ["src/Product/Back/Infrastructure/GesFer.Infrastructure.csproj", "src/Product/Back/Infrastructure/"]
COPY ["src/Product/Back/domain/GesFer.Domain.csproj", "src/Product/Back/domain/"]
COPY ["src/Shared/Back/GesFer.Shared.Back.Domain.csproj", "src/Shared/Back/"]
RUN dotnet restore "src/Admin/Back/Api/GesFer.Admin.Api.csproj"
COPY . .
WORKDIR "/src/src/Admin/Back/Api"
RUN dotnet build "GesFer.Admin.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GesFer.Admin.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GesFer.Admin.Api.dll"]
