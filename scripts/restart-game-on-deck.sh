#!/usr/bin/env bash
set -euo pipefail

ssh deck 'python3 -' <<'PY'
from pathlib import Path
import os
import subprocess
import time

APPID = "2088570"
GAME_PATTERN = "Tiny Rogues.exe"

def running_game():
    return subprocess.run(
        ["pgrep", "-f", GAME_PATTERN],
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    ).returncode == 0

print("Stopping Tiny Rogues...")

subprocess.run(
    ["pkill", "-TERM", "-f", GAME_PATTERN],
    stdout=subprocess.DEVNULL,
    stderr=subprocess.DEVNULL,
)

for _ in range(30):
    if not running_game():
        break
    time.sleep(0.2)

if running_game():
    subprocess.run(
        ["pkill", "-KILL", "-f", GAME_PATTERN],
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )

time.sleep(1)

uid = os.getuid()
candidates = []

for proc in Path("/proc").iterdir():
    if not proc.name.isdigit():
        continue

    try:
        if proc.stat().st_uid != uid:
            continue

        raw_env = (proc / "environ").read_bytes().split(b"\0")

        env = {}

        for item in raw_env:
            if b"=" not in item:
                continue

            k, v = item.split(b"=", 1)

            env[k.decode(errors="replace")] = v.decode(errors="replace")

        if "DISPLAY" not in env and "WAYLAND_DISPLAY" not in env:
            continue

        cmd = (
            (proc / "cmdline")
            .read_bytes()
            .replace(b"\0", b" ")
            .decode(errors="replace")
        )

        score = 0

        if "steamwebhelper" in cmd:
            score += 100

        if "/steam" in cmd:
            score += 80

        if "gamescope" in cmd:
            score += 50

        if "DISPLAY" in env:
            score += 20

        if "DBUS_SESSION_BUS_ADDRESS" in env:
            score += 10

        candidates.append(
            (score, int(proc.name), cmd, env)
        )

    except Exception:
        pass

if not candidates:
    raise SystemExit(
        "ERROR: Could not find a process with Gaming Mode display environment"
    )

candidates.sort(reverse=True, key=lambda x: x[0])

score, pid, cmd, session_env = candidates[0]

print("Using graphical process:")
print("PID:", pid)
print("CMD:", cmd)
print("DISPLAY:", session_env.get("DISPLAY", "<missing>"))
print("WAYLAND_DISPLAY:", session_env.get("WAYLAND_DISPLAY", "<missing>"))
print(
    "DBUS_SESSION_BUS_ADDRESS:",
    session_env.get("DBUS_SESSION_BUS_ADDRESS", "<missing>")
)

env = os.environ.copy()
env.update(session_env)

env["HOME"] = "/home/deck"
env["USER"] = "deck"
env["LOGNAME"] = "deck"
env.setdefault("XDG_RUNTIME_DIR", f"/run/user/{uid}")

steam_bin = "/home/deck/.local/share/Steam/ubuntu12_32/steam"

log = open("/tmp/tinyrogues-launch.log", "wb")

print("Launching Tiny Rogues...")

subprocess.Popen(
    [steam_bin, "-applaunch", APPID],
    env=env,
    stdin=subprocess.DEVNULL,
    stdout=log,
    stderr=subprocess.STDOUT,
    start_new_session=True,
)

for _ in range(30):
    time.sleep(0.2)

    if running_game():
        print("Tiny Rogues launched successfully.")
        raise SystemExit(0)

print("First launch method failed; trying Steam URI...")

subprocess.Popen(
    [steam_bin, f"steam://rungameid/{APPID}"],
    env=env,
    stdin=subprocess.DEVNULL,
    stdout=log,
    stderr=subprocess.STDOUT,
    start_new_session=True,
)

for _ in range(30):
    time.sleep(0.2)

    if running_game():
        print("Tiny Rogues launched successfully.")
        raise SystemExit(0)

print("ERROR: Tiny Rogues did not relaunch.")
print("Check /tmp/tinyrogues-launch.log")
raise SystemExit(1)
PY
