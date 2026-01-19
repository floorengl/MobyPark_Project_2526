FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .

# Expose HTTP and HTTPS ports, but HTTPS only if enabled
EXPOSE 80
EXPOSE 5001

# Set ASPNETCORE_URLS and certs based on USE_HTTPS
ARG USE_HTTPS=false
ENV USE_HTTPS=${USE_HTTPS}
RUN if [ "$USE_HTTPS" = "true" ]; then \
		cp /app/https/devcert.pfx /https/devcert.pfx; \
		echo "ASPNETCORE_URLS=https://+:5001;http://+:80" >> /app/.env; \
		echo "ASPNETCORE_Kestrel__Certificates__Default__Path=/https/devcert.pfx" >> /app/.env; \
		echo "ASPNETCORE_Kestrel__Certificates__Default__Password=devpassword123" >> /app/.env; \
	else \
		echo "ASPNETCORE_URLS=http://+:80" >> /app/.env; \
	fi

ENTRYPOINT ["dotnet", "MobyPark-api.dll"]
