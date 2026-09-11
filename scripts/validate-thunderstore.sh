#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

VERSION="$(
  grep 'public const string Version' src/Plugin.cs \
  | sed -E 's/.*"([^"]+)".*/\1/'
)"

ZIP="dist/thunderstore/TrainingDummy-$VERSION.zip"

if [ ! -f "$ZIP" ]; then
  echo "ERROR: Missing package: $ZIP"
  exit 1
fi

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

unzip -q "$ZIP" -d "$TMP"

required=(
  "manifest.json"
  "README.md"
  "CHANGELOG.md"
  "icon.png"
  "BepInEx/plugins/TinyRoguesTrainingDummy/TinyRogues.TrainingDummy.dll"
)

for path in "${required[@]}"; do
  if [ ! -f "$TMP/$path" ]; then
    echo "ERROR: Missing $path"
    exit 1
  fi
done

bad="$(
  find "$TMP" -type f \
    | grep -E 'Assembly-CSharp\.dll|GameAssembly\.dll|/interop/|/lib/|/obj/|/bin/' \
    || true
)"

if [ -n "$bad" ]; then
  echo "ERROR: Package contains files that should not be distributed:"
  echo "$bad"
  exit 1
fi

python3 - "$TMP/manifest.json" <<'PY'
import json
import sys
from pathlib import Path

p = Path(sys.argv[1])
data = json.loads(p.read_text())

required = [
    "name",
    "version_number",
    "website_url",
    "description",
    "dependencies",
]

missing = [field for field in required if field not in data]

if missing:
    raise SystemExit(
        "ERROR: Missing manifest fields: " + ", ".join(missing)
    )

print("Manifest JSON: OK")
PY

echo "Package contents: OK"
echo "Thunderstore package validation passed."
