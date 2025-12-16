#Docker packages your entire application, including the runtime (like .NET), 
#dependencies, configuration files, and the application code, 
#into a single portable container image.

#This container can then run anywhere on your laptop,
#a cloud server, or even a CI/CD pipeline without 
#worrying about environment mismatches

#Dockerfile = How to build the app (compile .NET code, install dependencies, etc.)

#docker-compose.yml = How to run the app + DB together, 
#on the same virtual network with linked environment variables and volumes.


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]
