#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

dotnet build -c Release

VERSION="$(
    grep 'public const string Version' src/Plugin.cs \
    | sed -E 's/.*"([^"]+)".*/\1/'
)"

NAME="TinyRoguesTrainingDummy-$VERSION"
STAGE="dist/$NAME"

mkdir -p \
  "$STAGE/BepInEx/plugins/TinyRoguesTrainingDummy"

cp \
  bin/Release/net6.0/TinyRogues.TrainingDummy.dll \
  "$STAGE/BepInEx/plugins/TinyRoguesTrainingDummy/"

cp README.md "$STAGE/"
cp CHANGELOG.md "$STAGE/"
cp LICENSE "$STAGE/"

cd dist

TEMP_ZIP="$ROOT/dist/$NAME.zip.$$"

zip -qr \
  "$TEMP_ZIP" \
  "$NAME/LICENSE" \
  "$NAME/BepInEx/plugins/TinyRoguesTrainingDummy/TinyRogues.TrainingDummy.dll" \
  "$NAME/CHANGELOG.md" \
  "$NAME/README.md"

mv -f "$TEMP_ZIP" "$ROOT/dist/$NAME.zip"

echo
echo "Created: dist/$NAME.zip"
echo
unzip -l "$NAME.zip"
