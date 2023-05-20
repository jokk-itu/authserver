#! bin/sh

docker compose down authserver webapp
rm Identity.db
docker compose run --rm configurationapp dotnet ConfiguratonApp.dll migration
docker compose run --rm configurationapp dotnet ConfiguratonApp.dll scope
docker compose run --rm configurationapp dotnet ConfiguratonApp.dll resource
docker compose run --rm configurationapp dotnet ConfiguratonApp.dll rotate
docker compose pull
docker compose up -d authserver webapp