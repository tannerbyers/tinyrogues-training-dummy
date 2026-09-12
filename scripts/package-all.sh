#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

./scripts/package-release.sh

VERSION="$(
  grep 'public const string Version' src/Plugin.cs \
  | sed -E 's/.*"([^"]+)".*/\1/'
)"

echo
echo "Release artifacts ready:"
echo "  dist/TinyRoguesTrainingDummy-$VERSION.zip"
