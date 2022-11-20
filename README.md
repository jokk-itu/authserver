# Authserver

Supporting the Authorization Code flow for OAuth 2.1 and OpenId Connect 1.0

## Pipeline runs

[![CI](https://github.com/jokk-itu/authserver/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/jokk-itu/authserver/actions/workflows/build.yml)

## Documentation

Take a look in the Wiki section of the repository.

## How to run

The project relies on a database. It can either be SQL Server or SQLite.
This can be configured using the appsettings.json file.

If using SQL Server, then a docker-compose.yml file can be used.

All projects use Kestrel as server, and can be started using <code>dotnet run</code>.