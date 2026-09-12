# Tiny Rogues Training Dummy

Adds a safe training target and DPS meter for comparing builds during a Tiny Rogues run.

**Core loop:** finish a fight → change gear → test it on the dummy → compare DPS → continue.

## Features

- DPS
- Previous completed result through `LAST`
- Fresh target between benchmarks
- Supports normal attacks, crits, DOTs, procs, and companions
- Reappears after completed combat rooms
- Supports normal weapon and equipment changes while present
- Does not mark the run as cheated
- Configurable timing, placement, and display scale

Hit the dummy to start a benchmark. Stop dealing damage for about 3 seconds to complete it; the result becomes `LAST` when the next test begins. The dummy is then replaced with a fresh target so lingering DOTs, debuffs, and effects do not carry into the next comparison.

The stationary dummy supports manual aiming. Tiny Rogues' native controller auto-aim does not target it.

## Installation

Install through Gale, r2modman, or another Thunderstore-compatible mod manager.

For manual installation, use the normal GitHub release ZIP and extract it into the Tiny Rogues game folder. The DLL must end up at `BepInEx/plugins/TinyRoguesTrainingDummy/TinyRogues.TrainingDummy.dll`.

## Compatibility

- Tiny Rogues 0.2.8.6
- BepInEx 6 Unity IL2CPP
- Windows
- Steam Deck / Proton

Unofficial community mod for Tiny Rogues. Not affiliated with RubyDev.

Report issues on GitHub:
https://github.com/tannerbyers/tinyrogues-training-dummy/issues
