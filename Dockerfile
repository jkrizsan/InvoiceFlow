FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/InvoiceFlow.Domain/InvoiceFlow.Domain.csproj src/InvoiceFlow.Domain/
COPY src/InvoiceFlow.Application/InvoiceFlow.Application.csproj src/InvoiceFlow.Application/
COPY src/InvoiceFlow.Infrastructure/InvoiceFlow.Infrastructure.csproj src/InvoiceFlow.Infrastructure/
COPY src/InvoiceFlow.Api/InvoiceFlow.Api.csproj src/InvoiceFlow.Api/
RUN dotnet restore src/InvoiceFlow.Api/InvoiceFlow.Api.csproj

COPY . .
RUN dotnet publish src/InvoiceFlow.Api/InvoiceFlow.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "InvoiceFlow.Api.dll"]

