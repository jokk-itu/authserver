#! /bin/sh

curl -X 'POST' \
  'http://localhost:18001/oauth2/v1/Token/token' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Basic dGVzdDoyQkI4MEQ1MzdCMURBM0UzOEJEMzAzNjFBQTg1NTY4NkJERTBFQUNENzE2MkZFRjZBMjVGRTk3QkY1MjdBMjVC'  \
  -d '{
  "grant_type": "authorization_code",
  "code": "CfDJ8KFrmWKh/95Hl3GGy0fkz9ux9J/Uqwe0EG23J4iGwxDeAtoYHXNqYDH0Ia+y9tG04czpgAGEcMdLpzmQkWQiZLwyj9/0ec+LOUkBBBFACEW6b9BCgMaKik2PVks856QRLTu4UBqBwz977ft8rdqbkMS8GyQuZdbfSuMOSFw0j6BOkzqKRFP5EN0tAGqW79LCQ3URlLSl+VEG4eQ8cY1Su4EnLmEsKEsZCsRmLz7NRPHw",
  "redirect_uri": "http://localhost:5002/callback"
}'