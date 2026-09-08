#!/bin/sh
set -e

PORT="${PORT:-8080}"
export ASPNETCORE_URLS="http://+:${PORT}"

exec dotnet LoginApi.dll
