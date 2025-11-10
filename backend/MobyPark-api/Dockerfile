# backend/MobyPark-api/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MobyPark_Project_2526.sln ./
COPY backend/MobyPark-api/*.csproj backend/MobyPark-api/
RUN dotnet restore backend/MobyPark-api/MobyPark-api.csproj   # <-- change here

COPY . .
RUN dotnet publish backend/MobyPark-api/MobyPark-api.csproj -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "MobyPark-api.dll"]
