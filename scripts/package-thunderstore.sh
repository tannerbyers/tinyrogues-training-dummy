#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

rm -rf bin obj

dotnet build -c Release

VERSION="$(grep 'public const string Version' src/Plugin.cs | sed -E 's/.*"([^"]+)".*/\1/')"

NAME="TrainingDummy-$VERSION"
OUT="dist/thunderstore"
STAGE="$OUT/$NAME"

rm -rf "$STAGE"
mkdir -p "$STAGE/BepInEx/plugins/TinyRoguesTrainingDummy"

cp thunderstore/manifest.json "$STAGE/manifest.json"
cp thunderstore/README.md "$STAGE/README.md"
cp CHANGELOG.md "$STAGE/CHANGELOG.md"
cp docs/assets/icon.png "$STAGE/icon.png"

cp   bin/Release/net6.0/TinyRogues.TrainingDummy.dll   "$STAGE/BepInEx/plugins/TinyRoguesTrainingDummy/"

rm -f "$OUT/$NAME.zip"

(
  cd "$STAGE"
  zip -qr "../$NAME.zip" .
)

echo
echo "Created: $OUT/$NAME.zip"
echo
unzip -l "$OUT/$NAME.zip"
