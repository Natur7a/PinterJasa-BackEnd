FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/PinterJasa.API/PinterJasa.API.csproj ./src/PinterJasa.API/
RUN dotnet restore ./src/PinterJasa.API/PinterJasa.API.csproj

COPY src/ ./src/
RUN dotnet publish ./src/PinterJasa.API/PinterJasa.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PinterJasa.API.dll"]
