FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy only the API csproj (not the solution or tests)
COPY *.csproj ./
RUN dotnet restore

# now copy the rest of the API project
COPY . .
RUN dotnet publish -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "MobyPark-api.dll"]
