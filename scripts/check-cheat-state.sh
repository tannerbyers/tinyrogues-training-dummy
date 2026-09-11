#!/usr/bin/env bash
set -euo pipefail

ssh deck '
mkdir -p "$HOME/tinyrogues/BepInEx/config"

tmp="$HOME/tinyrogues/BepInEx/config/TinyRogues.TrainingDummy.dev-command.txt.tmp"
dst="$HOME/tinyrogues/BepInEx/config/TinyRogues.TrainingDummy.dev-command.txt"

printf "%s\n" "cheat-state" > "$tmp"
mv "$tmp" "$dst"
'
