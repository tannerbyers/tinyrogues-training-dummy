#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

rm -rf bin obj
dotnet build -c Release

VERSION="$(
    grep 'public const string Version' src/Plugin.cs \
    | sed -E 's/.*"([^"]+)".*/\1/'
)"

NAME="TinyRoguesTrainingDummy-$VERSION"
STAGE="dist/$NAME"

rm -rf "$STAGE"
mkdir -p \
  "$STAGE/BepInEx/plugins/TinyRoguesTrainingDummy"

cp \
  bin/Release/net6.0/TinyRogues.TrainingDummy.dll \
  "$STAGE/BepInEx/plugins/TinyRoguesTrainingDummy/"

cp README.md "$STAGE/"
cp CHANGELOG.md "$STAGE/"
cp LICENSE "$STAGE/"

cd dist

rm -f "$NAME.zip"

zip -qr \
  "$NAME.zip" \
  "$NAME"

echo
echo "Created: dist/$NAME.zip"
echo
unzip -l "$NAME.zip"
