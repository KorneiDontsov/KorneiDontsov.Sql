# invoke from root

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY ./KorneiDontsov.Sql.sln .
COPY ./src/ ./src/
COPY ./examples/KorneiDontsov.Sql.Postgres.Example.Dto/ ./examples/KorneiDontsov.Sql.Postgres.Example.Dto/
COPY ./examples/KorneiDontsov.Sql.Postgres.Example/ ./examples/KorneiDontsov.Sql.Postgres.Example/

RUN dotnet publish ./examples/KorneiDontsov.Sql.Postgres.Example/KorneiDontsov.Sql.Postgres.Example.csproj \
	-c Release \
	-o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "KorneiDontsov.Sql.Postgres.Example.dll"]
