#!/bin/sh
if [ "$(id -u)" = "0" ]; then
    exec su-exec guardou dotnet /app/guardou.dll "$@"
fi
exec dotnet /app/guardou.dll "$@"
