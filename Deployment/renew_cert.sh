#! /bin/sh
# Verify TXT record here: https://mxtoolbox.com/SuperTool.aspx?action=txt%3aauthserver.dk&run=toolpage

docker compose run --rm certbot certonly \
    --manual \
    --preferred-challenges=dns \
    --email joachim@kelsen.nu \
    --server https://acme-v02.api.letsencrypt.org/directory \
    --agree-tos \
    -d "*.authserver.dk"