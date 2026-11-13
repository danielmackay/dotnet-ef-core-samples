#!/usr/bin/env bash
# Runs every console sample against the AppHost's SQL Server (127.0.0.1:1700)
# and reports each sample's exit code. Exits non-zero if any sample failed.
#
# Usage: ./run-samples.sh [Sample ...]    (no arguments = every sample)
#
# Samples that end with Console.ReadLine() get an empty stdin so they exit.
# Start the AppHost first (dotnet run --project AppHost) so SQL Server is up.

set -uo pipefail
cd "$(dirname "$0")"

# Projects that aren't runnable console samples.
skip=(Common AppHost)
# Web samples that never exit on their own: started, given time to seed, then stopped.
web=(BufferingAndStreaming)

contains() { local x; for x in "${@:2}"; do [[ "$x" == "$1" ]] && return 0; done; return 1; }

if [[ $# -gt 0 ]]; then
  samples=("$@")
else
  samples=()
  for proj in */*.csproj; do
    name="${proj%%/*}"
    contains "$name" "${skip[@]}" || samples+=("$name")
  done
fi

dotnet build EfCoreSamples.slnx -nologo -v q || { echo "Build failed"; exit 1; }

failed=0
results=()
for name in "${samples[@]}"; do
  log="$(mktemp -t "sample-$name")"
  if contains "$name" "${web[@]}"; then
    dotnet run --no-build --project "$name" </dev/null >"$log" 2>&1 &
    pid=$!
    sleep 20
    if kill -0 "$pid" 2>/dev/null; then
      pkill -P "$pid" 2>/dev/null; kill "$pid" 2>/dev/null; wait "$pid" 2>/dev/null
      code=0
    else
      wait "$pid"; code=$?
    fi
  else
    dotnet run --no-build --project "$name" </dev/null >"$log" 2>&1
    code=$?
  fi
  results+=("$(printf '%-30s %s' "$name" "$code")")
  if [[ $code -ne 0 ]]; then
    failed=1
    echo "---- $name failed (exit $code), last lines of output:"
    tail -20 "$log"
  fi
  rm -f "$log"
done

echo
echo "Sample                         Exit code"
printf '%s\n' "${results[@]}"
exit $failed
