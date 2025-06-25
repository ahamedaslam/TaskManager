#Docker packages your entire application, including the runtime (like .NET), 
#dependencies, configuration files, and the application code, 
#into a single portable container image.

#This container can then run anywhere on your laptop,
#a cloud server, or even a CI/CD pipeline without 
#worrying about environment mismatches

#Dockerfile = How to build the app (compile .NET code, install dependencies, etc.)

#docker-compose.yml = How to run the app + DB together, 
#on the same virtual network with linked environment variables and volumes.

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY TaskManager.API/ .

RUN dotnet restore TaskManager.csproj
RUN dotnet publish TaskManager.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "TaskManager.dll"]
