# Build context: repository root
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props global.json ./
COPY external-service/ExternalService.Mock/ExternalService.Mock.csproj external-service/ExternalService.Mock/
RUN dotnet restore external-service/ExternalService.Mock/ExternalService.Mock.csproj
COPY external-service/ external-service/
RUN dotnet publish external-service/ExternalService.Mock/ExternalService.Mock.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ExternalService.Mock.dll"]
