#!/usr/bin/env bash
set -euo pipefail

ssh deck \
  'tail -F ~/tinyrogues/BepInEx/LogOutput.log' \
  | grep --line-buffered -E \
  'Tiny Rogues Training Dummy|\[SAFETY\]|\[UI-FONT\]|\[UI\]|\[DUMMY\]|\[BONFIRE\]|\[DEV\]|Error|Exception'
