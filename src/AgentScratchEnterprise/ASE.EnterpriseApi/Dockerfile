FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ASE.EnterpriseApi/", "ASE.EnterpriseApi/"]
COPY ["ASE.Libraries/", "ASE.Libraries/"]
RUN dotnet restore "ASE.EnterpriseApi/ASE.EnterpriseApi.csproj"
COPY . .
WORKDIR "/src/ASE.EnterpriseApi"
RUN dotnet build "ASE.EnterpriseApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ASE.EnterpriseApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
# Azure AI Search configuration - override at runtime (keyless via DefaultAzureCredential)
ENV Search__Environment=AZURE
ENV Search__AzureSearchEndpoint=
ENV Search__AzureSearchIndexName=
ENV Search__AzureSearchSemanticConfig=
ENV Search__KnowledgeBaseName=
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ASE.EnterpriseApi.dll"]
