#!/bin/sh
set -e

if [ "$(id -u)" = "0" ]; then
    PUID="${PUID:-1000}"
    PGID="${PGID:-1000}"
    groupmod -o -g "$PGID" guardou
    usermod -o -u "$PUID" guardou
    find /data /db /home/guardou \( \! -user "$PUID" -o \! -group "$PGID" \) -exec chown -h "$PUID:$PGID" {} +
fi

exec guardou "$@"
