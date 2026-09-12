# Architecture

## Overview

The plugin creates Tiny Rogues' own target-dummy prefab at safe lifecycle points, keeps that object outside normal enemy bookkeeping, and listens to the game's final damage event to measure damage received by that exact instance. `DpsOverlay` attaches a small world-space display to the current dummy.

## Important files

- `src/Plugin.cs` — plugin lifecycle, configuration, dummy spawn/cleanup, Harmony patches, exact-instance damage capture, and benchmark state.
- `src/DpsOverlay.cs` — native-font world-space `TRAINING DUMMY`, `DPS`, and `LAST` display and cleanup.
- `scripts/package-all.sh` — builds and validates both the normal release and Thunderstore packages.
- `scripts/package-release.sh` — creates the game-folder release ZIP.
- `scripts/package-thunderstore.sh` — creates the manager-ready package.
- `scripts/validate-thunderstore.sh` — checks package contents and version metadata.

## Dummy lifecycle

```text
run/floor start → dummy spawn
leave room → cleanup
active combat → no dummy
AfterCompletedRoom → post-combat dummy
benchmark completes → old dummy destroyed → fresh dummy spawned
```

The first floor uses the player spawn; later floors use the active bonfire. A completed room queues a post-combat spawn only after room completion has settled.

## Enemy isolation

This is a critical invariant: the training dummy must never be added to `EnemyManager.aliveEnemies`. `OnRegisterEnemy` captures the exact spawned dummy and prevents vanilla registration.

Adding it to `aliveEnemies` previously caused weapon drop/equip restrictions, incorrect combat state, room-state contamination, and possible progression issues. Do not restore targeting or auto-aim by adding the dummy to `aliveEnemies`.

## Damage capture

`TemporaryEvents.AfterDamageTaken` supplies the final damage event. The handler counts an event only when `damageReceiver` is the exact tracked dummy instance, avoiding unrelated room damage. This is intended to cover final resolved damage from direct and secondary sources; unusual mechanics still require runtime validation in issue #4.

## Benchmark state

Active measurement (`_totalDamage`, timestamps, and event count) is separate from completed history (`_completedDps` and `_previousDps`). `ClearActiveMeasurement()` clears only the in-progress test. Full run reset also clears completed history. When a test finalizes after the idle period, the result is retained, then the dummy is replaced; the next hit moves the prior completed result to `LAST` and starts fresh active state.

## UI

`DpsOverlay` reuses Tiny Rogues' world-text prefab and intentionally presents only `TRAINING DUMMY`, `DPS`, and `LAST`. It follows the tracked dummy and destroys its label when the dummy is gone.

## Packaging

Run:

```sh
./scripts/package-all.sh
```

This generates `dist/TinyRoguesTrainingDummy-<version>.zip` and `dist/thunderstore/TrainingDummy-<version>.zip`. Packages contain the DLL and player-facing metadata, not local interop assemblies.

## Testing

Minimum runtime smoke test:

1. Start a run: dummy appears.
2. Hit it: DPS updates.
3. Stop: benchmark completes and a fresh dummy appears.
4. Start another test: `LAST` remains.
5. Leave room: dummy disappears.
6. Enter combat: no dummy.
7. Complete combat: dummy appears.
8. Swap, drop, and equip a weapon: it works.
9. Attack the dummy again: it works.
10. Progress normally.

## Compatibility checklist

After a Tiny Rogues update, verify these integration points first: `EnemyManager.OnRegisterEnemy`, `DungeonGenerator.GoToNextRoom`, the bonfire lifecycle, `TemporaryEvents.AfterCompletedRoom`, `TemporaryEvents.AfterDamageTaken`, cheat-state APIs, and target-dummy prefab behavior. Confirm the dummy remains outside `aliveEnemies` and the run is not marked cheated.

## Known architectural limitations

- Native controller auto-aim currently does not target the dummy.
- Full unusual damage-path validation remains tracked in issue #4.
- Multi-target mode is deferred.
