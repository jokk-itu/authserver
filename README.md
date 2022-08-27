# Authserver

Supporting the Authorization Code flow for OAuth 2.0 and OpenId Connect v1.

## Pipeline runs

[![CI](https://github.com/jokk-itu/authserver/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/jokk-itu/authserver/actions/workflows/build.yml)

## Documentation

Take a look in the Wiki section of the repository.

## How to run

The repository contains a testsystem, consisting of a webapp using OpenId Connect and a protected resource (API).
The repository is setup to use docker-compose for all programs, therefore to run all programs run the compose file.
```
docker-compose up -d
```

Now proceed to the [WebApp](http://localhost:5002/home/secret)
This will try to fetch data from a secured enpoint in the protected resource.
Since a user has not been authenticated, a challenge is received instead.
You will then be redirected to the authorize endpoint of the OP.
Use the following user:
```
Username: jokk
Password: Password12!
```

If you would like to register your own user, then proceed to the [Register endpoint](http://localhost:5000/connect/v1/account/register).

If the credentials are correct, you will be redirected back to the secret page, and the secret endpoint of the protected resource will return a secret.
