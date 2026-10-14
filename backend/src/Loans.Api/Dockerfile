# Build context: repository root
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props global.json ./
COPY backend/src/Loans.Domain/Loans.Domain.csproj backend/src/Loans.Domain/
COPY backend/src/Loans.Application/Loans.Application.csproj backend/src/Loans.Application/
COPY backend/src/Loans.Infrastructure/Loans.Infrastructure.csproj backend/src/Loans.Infrastructure/
COPY backend/src/Loans.Api/Loans.Api.csproj backend/src/Loans.Api/
RUN dotnet restore backend/src/Loans.Api/Loans.Api.csproj
COPY backend/src/ backend/src/
RUN dotnet publish backend/src/Loans.Api/Loans.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Loans.Api.dll"]
