#!/usr/bin/env bash
#   Use this script to test if a given TCP host/port are available.
#
#   The MIT License (MIT)
#
#   Copyright (c) 2016 Sean T. Allen
#
#
#   Permission is hereby granted, free of charge, to any person obtaining a copy
#   of this software and associated documentation files (the "Software"), to deal
#   in the Software without restriction, including without limitation the rights
#   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
#   copies of the Software, and to permit persons to whom the Software is
#   furnished to do so, subject to the following conditions:
#
#   The above copyright notice and this permission notice shall be included in all
#   copies or substantial portions of the Software.
#
#   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
#   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
#   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
#   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
#   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
#   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
#   SOFTWARE.

set -e

TIMEOUT=15
QUIET=0
HOST=
PORT=
CHILD=

usage() {
  cat << USAGE  >&2
Usage:
  ./wait-for-it.sh host:port [-t timeout] [-- command args]
  -h HOST | --host=HOST       Host or IP under test
  -p PORT | --port=PORT       TCP port under test
  -t TIMEOUT  | --timeout=TIMEOUT
                              Timeout in seconds, zero for no timeout
  -- COMMAND ARGS              Execute command with args after the test finishes

Examples:
  ./wait-for-it.sh google.com:80 -- echo "Google is up"
  ./wait-for-it.sh 192.168.1.1:22 -t 10
USAGE
  exit 1
}

die() {
  echo "$*" >&2
  exit 1
}

parse_args() {
  while [[ $# -gt 0 ]]; do
    case "$1" in
      *:* )
      hostport=(${1//:/ })
      HOST=${hostport[0]}
      PORT=${hostport[1]}
      shift 1
      ;;
      -h)
      HOST="$2"
      if [[ $2 == *:* ]]; then
        hostport=(${2//:/ })
        HOST=${hostport[0]}
        PORT=${hostport[1]}
      fi
      shift 2
      ;;
      --host=*)
      HOST="${1#*=}"
      shift 1
      ;;
      -p)
      PORT="$2"
      shift 2
      ;;
      --port=*)
      PORT="${1#*=}"
      shift 1
      ;;
      -t)
      TIMEOUT="$2"
      shift 2
      ;;
      --timeout=*)
      TIMEOUT="${1#*=}"
      shift 1
      ;;
      -q)
      QUIET=1
      shift 1
      ;;
      --quiet)
      QUIET=1
      shift 1
      ;;
      --)
      shift
      CHILD="$@"
      break
      ;;
      --help)
      usage
      ;;
      *)
      echo "Unknown argument: $1" >&2
      usage
      ;;
    esac
  done

  if [[ -z "$HOST" ]] || [[ -z "$PORT" ]]; then
    echo "Error: you need to provide a host and port to test." >&2
    usage
  fi
}

echo "Waiting for $HOST:$PORT"

wait_for() {
  if [[ $TIMEOUT -gt 0 ]]; then
    end_time=$(( $(date +%s) + TIMEOUT ))
  fi
  while true; do
    if [[ $QUIET -ne 1 ]]; then
      echo "\$(date +%T) - checking $HOST:$PORT..."
    fi
    if nc -z "$HOST" "$PORT"; then
      echo "\$(date +%T) - $HOST:$PORT is available"
      break
    fi
    if [[ $TIMEOUT -gt 0 ]] && [[ $(date +%s) -ge $end_time ]]; then
      echo "Timeout after waiting $TIMEOUT seconds for $HOST:$PORT" >&2
      exit 1
    fi
    sleep 1
  done
}

# parse arguments
parse_args "$@"

# wait
wait_for

# execute optional command
if [[ -n "$CHILD" ]]; then
  exec $CHILD
fi
