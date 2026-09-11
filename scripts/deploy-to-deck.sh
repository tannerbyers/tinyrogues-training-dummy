#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DLL="$ROOT/bin/Release/net6.0/TinyRogues.TrainingDummy.dll"
REMOTE_DIR='$HOME/tinyrogues/BepInEx/plugins/TinyRoguesTrainingDummy'

if [ ! -f "$DLL" ]; then
    echo "Release DLL not found. Run dotnet build -c Release first."
    exit 1
fi

# Remove old copies so BepInEx can never load the plugin twice.
ssh deck '
find "$HOME/tinyrogues/BepInEx/plugins" \
  -type f \
  -name "TinyRogues.TrainingDummy.dll" \
  -delete

mkdir -p \
  "$HOME/tinyrogues/BepInEx/plugins/TinyRoguesTrainingDummy"
'

scp "$DLL" \
  'deck:~/tinyrogues/BepInEx/plugins/TinyRoguesTrainingDummy/TinyRogues.TrainingDummy.dll'

echo "Deployed TinyRogues.TrainingDummy.dll"
