# Tiny Rogues Training Dummy

A lightweight training dummy mod for Tiny Rogues.

[**Download the latest release**](/releases/latest)

![Tiny Rogues Training Dummy showing live DPS, total damage, elapsed time, and peak damage](docs/assets/training-dummy-demo.gif)


The mod adds a targetable, immortal training dummy to each floor's starting area so builds can be tested without affecting room combat.

## Features

- Training dummy on every floor
- Normal weapon targeting, projectiles, DOTs and triggered damage
- Effective DPS measured from final damage actually received
- Total damage, measurement time and peak damage
- Native Tiny Rogues world-text presentation
- Configurable placement, timing and text scale
- Windows and Steam Deck / Proton support

## Supported version

- Tiny Rogues: **0.2.8.6**
- BepInEx 6 Unity IL2CPP x64

Tiny Rogues is undergoing a major rewrite. The next major game update is expected to require a new version of this mod.

## Safety

The mod does **not** invoke Tiny Rogues' `SpawnTargetDummy` cheat-console command.

Instead, it directly instantiates the game's existing target-dummy prefab and intercepts its enemy registration so it remains targetable without starting combat or locking doors.

The plugin also checks Tiny Rogues' `CheatConsole.HasCheated` state before and after spawning the dummy and logs an error if the state changes.

## Installation

BepInEx 6 for Unity IL2CPP must already be installed.

Extract the release ZIP into the Tiny Rogues game directory.

The DLL should end up at:

`BepInEx/plugins/TinyRoguesTrainingDummy/TinyRogues.TrainingDummy.dll`

Launch Tiny Rogues normally.

To verify loading, check:

`BepInEx/LogOutput.log`

for:

`Tiny Rogues Training Dummy ... loaded`

## Steam Deck

Install BepInEx for Tiny Rogues first, then extract this mod into the Tiny Rogues game directory in Desktop Mode.

The plugin layout is the same:

`BepInEx/plugins/TinyRoguesTrainingDummy/TinyRogues.TrainingDummy.dll`

The mod has been developed and tested under Steam Deck / Proton.

## Configuration

After the first launch, BepInEx creates:

`BepInEx/config/tanner.tinyrogues.trainingdummy.cfg`

Available options:

- `ShowOverlay`
- `TextScale`
- `DpsWindowSeconds`
- `IdleResetSeconds`
- `DummyOffsetX`
- `DummyOffsetY`
- `DebugLogging`

Defaults are intended for normal use.

## DPS behavior

A measurement begins on the first damage event.

The display reports:

- DPS
- total final damage
- elapsed measurement time
- peak single damage event

The measurement resets after the configured idle timeout or maximum measurement window.

Because the dummy records final damage received, conditional bonuses, critical hits, DOTs, procs and other resolved effects are naturally reflected in the result.

## Development

Developer/test scripts are available in `scripts/` in the source repository but are not included in release ZIPs.

Useful scripts include:

- `build-deploy-restart.sh`
- `deploy-to-deck.sh`
- `restart-game-on-deck.sh`
- `skip-to-next-floor.sh`
- `check-cheat-state.sh`
- `tail-mod-logs.sh`
- `package-release.sh`

## Building from source

The repository does not redistribute Tiny Rogues assemblies or generated IL2CPP interop assemblies.

A local Tiny Rogues/BepInEx installation is required to populate the development references under `lib/`.

## License

MIT
