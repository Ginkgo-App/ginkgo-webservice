# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source
# copy everything and build app
COPY . .
WORKDIR /source
RUN cd WebMvcPluginUser/WebMvcPluginUser && dotnet publish -c release -o ../../APICore/APICore/Plugins
RUN cd WebMvcPluginTour/WebMvcPluginTour && dotnet publish -c release -o ../../APICore/APICore/Plugins
RUN cd WebMvcPluginPlace/WebMvcPluginPlace && dotnet publish -c release -o ../../APICore/APICore/Plugins
RUN cd WebMvcPluginChat/WebMvcPluginChat && dotnet publish -c release -o ../../APICore/APICore/Plugins
RUN cd APICore/APICore && dotnet publish -c release -o /app
# final stage/image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /app
COPY --from=build /app ./
CMD ASPNETCORE_URLS=http://*:$PORT dotnet APICore.dll
