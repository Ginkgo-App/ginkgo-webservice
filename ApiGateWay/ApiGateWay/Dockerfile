# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /source
# copy everything and build app
COPY . .
WORKDIR /source
RUN dotnet publish -c release -o /app
# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
WORKDIR /app
COPY --from=build /app ./
CMD ASPNETCORE_URLS=http://*:$PORT dotnet ApiGateWay.dll