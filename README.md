# Tiny Rogues Training Dummy

A simple training dummy and DPS meter for Tiny Rogues.

Unofficial community mod for Tiny Rogues. Not affiliated with RubyDev.

[**Download the latest release**](https://github.com/tannerbyers/tinyrogues-training-dummy/releases/latest)

![Training Dummy demo](docs/assets/training-dummy-demo.gif)

## What it does

- Adds an immortal training dummy to the initial floor-start/bonfire context
- Shows the current DPS result and the previous benchmark result
- Works with normal attacks, crits, DOTs, procs, companions, and other damage effects
- Appears again after a room is completed, while staying out of active combat
- Supports normal weapon and equipment changes while the dummy is present
- Does not mark the run as cheated

Hit the dummy to start a test. The current benchmark finalizes after the configured quiet period, and the completed result remains visible until the next test begins.

After finalization, the dummy is replaced with a fresh prefab instance so lingering effects do not carry into the next benchmark. Effects created before the replacement may still be subject to the game's own projectile or status behavior; the mod does not perform global projectile cleanup.

Manual aiming may be required; native controller auto-aim toward the dummy is not currently supported.

## Install

Requires **BepInEx 6 for Unity IL2CPP**.

1. Download the latest release.
2. Extract it into your Tiny Rogues game folder.
3. Launch the game normally.

The mod file should be located at:

`BepInEx/plugins/TinyRoguesTrainingDummy/TinyRogues.TrainingDummy.dll`

## Configuration

Optional settings are created after the first launch at:

`BepInEx/config/tanner.tinyrogues.trainingdummy.cfg`

The defaults are intended for normal use.

## Compatibility

- Tiny Rogues **0.2.8.6**
- Windows
- Steam Deck / Proton

Game updates may require a new version of the mod.

## Uninstall

Delete:

`BepInEx/plugins/TinyRoguesTrainingDummy/`

## Roadmap

See [ROADMAP.md](ROADMAP.md) for planned improvements.

## Contributing

Bug reports, focused feature proposals, documentation, and runtime validation are welcome. Start with [CONTRIBUTING.md](CONTRIBUTING.md), then check the issues marked [`good first issue`](https://github.com/tannerbyers/tinyrogues-training-dummy/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22) or [`help wanted`](https://github.com/tannerbyers/tinyrogues-training-dummy/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22).

## License

MIT
