#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

rm -rf bin obj
dotnet build -c Release

./scripts/deploy-to-deck.sh
./scripts/restart-game-on-deck.sh
