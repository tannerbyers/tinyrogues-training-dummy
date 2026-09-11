#!/bin/bash
set -e

cd "$(dirname "$0")/.."

echo "Building..."
dotnet build -c Release

echo "Deploying..."
rsync -av \
  ./bin/Release/net6.0/TinyRogues.TrainingDummy.dll \
  deck:/home/deck/tinyrogues/BepInEx/plugins/TrainingDummy/

echo
echo "Deployed."
