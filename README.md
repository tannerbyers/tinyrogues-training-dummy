# Tiny Rogues Training Dummy

Tiny Rogues Training Dummy adds a safe training target for comparing builds during a run.

**Core loop:** finish a fight → change gear → test it on the dummy → compare DPS → continue.

[Download the latest release](https://github.com/tannerbyers/tinyrogues-training-dummy/releases/latest) · [Thunderstore package](https://thunderstore.io/c/tiny-rogues/p/Tanner_Byers/TrainingDummy/)

![Training Dummy demo](docs/assets/training-dummy-demo.gif)

## Features

- DPS measurement with a compact in-game display
- Previous completed result shown as `LAST`
- Fresh target between benchmarks for clean comparisons
- Dummy available after combat for gear testing
- Normal weapon and equipment swapping while it is present
- No cheat-state activation
- Windows and Steam Deck / Proton support

The dummy is stationary. Manual aiming works normally, but Tiny Rogues' native controller auto-aim does not target it.

## How a benchmark works

1. Hit the dummy — `DPS` begins updating.
2. Stop dealing damage for about 3 seconds — the benchmark completes.
3. The completed result becomes `LAST` when the next test begins.
4. The old dummy is replaced with a fresh one.

The refresh clears lingering DOTs, debuffs, and effects so the next comparison starts on a clean target.

The dummy appears in the starting/bonfire room and after a room is completed. It is removed when you leave the room and does not appear during active combat.

## Installation

### Windows

The normal release ZIP is already laid out for the game directory:

1. Install **BepInEx 6 for Unity IL2CPP** for Tiny Rogues.
2. Download `TinyRoguesTrainingDummy-1.0.3.zip` from [Releases](https://github.com/tannerbyers/tinyrogues-training-dummy/releases).
3. Extract the ZIP into the Tiny Rogues game folder, merging its `BepInEx` folder.

The final path must be:

```text
Tiny Rogues/
└── BepInEx/plugins/TinyRoguesTrainingDummy/TinyRogues.TrainingDummy.dll
```

Launch the game once, then check `Tiny Rogues/BepInEx/LogOutput.log` for:

```text
Tiny Rogues Training Dummy 1.0.3 loaded
```

You can also install the Thunderstore package with Gale, r2modman, or another compatible manager.

### Steam Deck / Proton

Use the same release ZIP and preserve the same `BepInEx/plugins/TinyRoguesTrainingDummy/` layout inside the Proton prefix used by Tiny Rogues. This repository's supported setup uses `~/tinyrogues/`, so its log is:

```text
~/tinyrogues/BepInEx/LogOutput.log
```

Confirm the startup line above after launching the game. A Thunderstore-compatible manager is also supported when it manages the same Proton game prefix.

## Troubleshooting

### Mod does not load

Check `BepInEx/LogOutput.log` and search for `Tiny Rogues Training Dummy`. Confirm BepInEx itself loaded, the DLL is under `BepInEx/plugins/TinyRoguesTrainingDummy/`, and you are using BepInEx 6 Unity IL2CPP.

### Dummy does not appear

Confirm Tiny Rogues is **0.2.8.6**, BepInEx loaded, and the plugin startup line is present. The dummy appears in a starting/bonfire room or after a completed room, not during active combat. Check the log for `[DUMMY]`, `[BONFIRE]`, or spawn warnings.

### Wrong install location

Do not leave the ZIP's top-level folder nested inside another folder. The DLL should be exactly:

```text
BepInEx/plugins/TinyRoguesTrainingDummy/TinyRogues.TrainingDummy.dll
```

### Finding logs on Steam Deck

For the supported repository setup, inspect `~/tinyrogues/BepInEx/LogOutput.log` in the Proton environment. If using a manager, use the profile's BepInEx folder for that same game prefix.

## Compatibility

- Tiny Rogues **0.2.8.6**
- BepInEx 6 Unity IL2CPP
- Windows
- Steam Deck / Proton

Game updates may require a new mod release.

## Configuration and uninstall

Optional settings are created after the first launch at `BepInEx/config/tanner.tinyrogues.trainingdummy.cfg`. Defaults are intended for normal use.

To uninstall, delete `BepInEx/plugins/TinyRoguesTrainingDummy/`.

## Contributing and support

See [SUPPORT.md](SUPPORT.md) for bug reports and [CONTRIBUTING.md](CONTRIBUTING.md) for local setup and development guidance. This is an unofficial community mod and is not affiliated with RubyDev.

## License

MIT
