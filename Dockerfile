FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln .
COPY src/RuneGames.Domain/RuneGames.Domain.csproj src/RuneGames.Domain/
COPY src/RuneGames.Application/RuneGames.Application.csproj src/RuneGames.Application/
COPY src/RuneGames.Infrastructure/RuneGames.Infrastructure.csproj src/RuneGames.Infrastructure/
COPY src/RuneGames.API/RuneGames.API.csproj src/RuneGames.API/

RUN dotnet restore

COPY . .
RUN dotnet publish src/RuneGames.API/RuneGames.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RuneGames.API.dll"]
