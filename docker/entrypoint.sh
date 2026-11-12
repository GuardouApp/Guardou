#!/bin/sh
set -e

if [ "$(id -u)" = "0" ]; then
    PUID="${PUID:-1000}"
    PGID="${PGID:-1000}"
    groupmod -o -g "$PGID" guardou
    usermod -o -u "$PUID" guardou
    if [ "$(stat -c '%u:%g' /data)" != "$PUID:$PGID" ]; then
        find /data \( \! -user "$PUID" -o \! -group "$PGID" \) -exec chown -h "$PUID:$PGID" {} +
    fi
    find /home/guardou \( \! -user "$PUID" -o \! -group "$PGID" \) -exec chown -h "$PUID:$PGID" {} +
fi

exec guardou "$@"
