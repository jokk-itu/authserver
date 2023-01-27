# Authserver

Supporting OAuth 2.1 and OpenId Connect 1.0

The following grant types are supported:
- Authorization Code
- Refresh Token
- Client Credentials

## Pipeline runs

[![CI](https://github.com/jokk-itu/authserver/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/jokk-itu/authserver/actions/workflows/build.yml)

## Documentation

Take a look in the Wiki section of the repository.

## How to run

The project relies on a database. It can either be SQL Server or SQLite.
This can be configured using the appsettings.json file.

If using SQL Server, then a docker-compose.yml file can be used.

All projects use Kestrel as server, and can be started using <code>dotnet run</code>.

## How to add data

There exist three scripts to create entities for scopes, resources and clients in Tools folder.
<b>Beware that the scopes script must be run first.</b>
<b>Remember to set the new clientid and clientsecret for each client.</b>

Create users by directing to the register endpoint, which is <b>connect/register</b>

## Clients

There exist multiple clients, each support different scenarios.

### WebApp

Supporting the authorization code grant type and the refresh token grant type on a confidential web app.

### Svelte.BFF

Supporting the authorization code grant type and the refresh token grant type on a confidential api,
which supports the backend for frontend pattern on the frontend app created using svelte.

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