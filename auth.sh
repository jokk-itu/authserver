#! /bin/sh

curl -X 'POST' \
  'http://localhost:18001/oauth2/v1/Token/token' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Basic dGVzdDoyQkI4MEQ1MzdCMURBM0UzOEJEMzAzNjFBQTg1NTY4NkJERTBFQUNENzE2MkZFRjZBMjVGRTk3QkY1MjdBMjVC'  \
  -d '{
  "grant_type": "authorization_code",
  "code": "CfDJ8KFrmWKh/95Hl3GGy0fkz9v4YCRBPVP57LS8473zr5Ff362w+FCGvBQLAdro8byW4vDerJMUhxPoOHvKv9njOc/MgStsH27eW0uE+VDmEfVD5kHHW39x46I/DcHyUAT+U6GTCHaadNWc+njnAa4JKJSuzTUbwksw9MtgJLf19fS6OXQdozjfDLnzcQQ8CZTMtCqUT9m9CoGFgjOn1MiVBVw=",
  "redirect_uri": "http://localhost:5002/callback"
}'
