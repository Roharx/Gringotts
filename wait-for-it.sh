#!/usr/bin/env sh

HOST="$1"
PORT="$2"
TIMEOUT="${3:-15}"
shift 3

if [ -z "$HOST" ] || [ -z "$PORT" ] || [ "$1" != -- ]; then
  echo "Usage: $0 HOST PORT [TIMEOUT] -- COMMAND ARGS..." >&2
  exit 1
fi
shift 1

end=$(( $(date +%s) + TIMEOUT ))
while :; do
  if (echo >"/dev/tcp/${HOST}/${PORT}") >/dev/null 2>&1; then
    break
  fi

  now=$(date +%s)
  if [ "$now" -ge "$end" ]; then
    echo "Timeout after ${TIMEOUT}s waiting for ${HOST}:${PORT}" >&2
    exit 1
  fi

  sleep 1
done

# Execute the remainder of the command
exec "$@"
