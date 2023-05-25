#! bin/sh

docker compose down authorizationserver webapp
rm Identity.db
touch Identity.db
docker compose run --rm configurationapp migration
docker compose run --rm configurationapp scope
docker compose run --rm configurationapp resource
docker compose run --rm configurationapp rotate
docker compose pull
docker compose up -d authorizationserver webapp