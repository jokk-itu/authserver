# Authserver

Supporting OAuth 2.1 and OpenId Connect 1.0

The following grant types are supported:
- Authorization Code
- Refresh Token
- Client Credentials

## Pipeline runs

[![CI](https://github.com/jokk-itu/authserver/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/jokk-itu/authserver/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=jokk-itu_authserver&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=jokk-itu_authserver)

## Documentation

Take a look at [authserver.dk](https://www.authserver.dk).

## Test Environment

Take a look at a demo client at [webapp.authserver.dk](https://webapp.authserver.dk).
Take a look at a demo client a [bff.authserver.dk](https://bffapp.authserver.dk).
Take a look at the demo identity provider at [idp.authserver.dk](https://idp.authserver.dk).
Take a look at the demo protected resource at [weather.authserver.dk](https://weather.authserver.dk).

## How to run

The project relies on a SQLite database. It is constructed by running the ConfigurationApp commands.
Navigate to the Authorization/ConfigurationApp folder and run the ``` new_database.ps1 ``` script.
It will generate a new .sqlite database in the WebApp for AuthorizationServer with standard data.
Copy the file into the bin/Debug/net6.0 folder.
Run the Authorization/WebApp project by using ``` dotnet run ``` command.

## How to add data

The new_database.ps1 script will setup initial data in a new .sqlite database.
To dynamically create new data, the register endpoint can be used for clients.

## Clients

There exist multiple clients, each support different scenarios.

### WebApp

Supporting the authorization code grant type and the refresh token grant type on a confidential web app.

### Blazor.BFF

Supporting the authorization code grant type and the refresh token grant type on a confidential api,
which supports the backend for frontend pattern on the frontend app created using blazor webassembly.

### WorkerService

Supporting the client credetials gran type on a confidential worker. It is illustrated by querying the token endpoint,
and afterwards using the access token to query a protected resource for weather data.

## Resources

There exist multiple resources.

### Weather

Represents weather data.