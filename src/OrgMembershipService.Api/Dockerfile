FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/OrgMembershipService.Api/OrgMembershipService.Api.csproj", "src/OrgMembershipService.Api/"]
COPY ["src/OrgMembershipService.UseCases/OrgMembershipService.UseCases.csproj", "src/OrgMembershipService.UseCases/"]
COPY ["src/OrgMembershipService.Domain/OrgMembershipService.Domain.csproj", "src/OrgMembershipService.Domain/"]
COPY ["src/OrgMembershipService.Infrastructure/OrgMembershipService.Infrastructure.csproj", "src/OrgMembershipService.Infrastructure/"]
RUN dotnet restore "src/OrgMembershipService.Api/OrgMembershipService.Api.csproj"
COPY . .
WORKDIR "/src/src/OrgMembershipService.Api"
RUN dotnet build "./OrgMembershipService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OrgMembershipService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrgMembershipService.Api.dll"]
