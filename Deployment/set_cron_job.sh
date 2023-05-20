#! bin/sh

Run docker-compose run for configurationapp
to rotate Keys

docker compose run --rm configurationapp rotate

Run docker-compose run for certbot
to rotate certificates

docker compose run --rm certbot renew