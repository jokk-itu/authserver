#! bin/sh

docker compose down
rm Identity.db
touch Identity.db
docker compose pull
docker compose run --rm configurationapp migration
docker compose run --rm configurationapp scope
docker compose run --rm configurationapp resource
docker compose run --rm configurationapp client
docker compose run --rm configurationapp rotate
docker compose up -d nginx authorizationserver webapp bffapp