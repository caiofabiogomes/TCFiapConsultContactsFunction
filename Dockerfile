FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TCFiapConsultContactsFunction.csproj", "./"]

ARG ARG_SECRET_NUGET_PACKAGES
RUN dotnet nuget add source "https://nuget.pkg.github.com/caiofabiogomes/index.json" \
    --name github \
    --username caiofabiogomes \
    --password "${ARG_SECRET_NUGET_PACKAGES}" \
    --store-password-in-clear-text


RUN dotnet restore "TCFiapConsultContactsFunction.csproj"
COPY . .
RUN dotnet build "TCFiapConsultContactsFunction.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TCFiapConsultContactsFunction.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
