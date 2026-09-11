# Roadmap

This roadmap tracks improvements that extend the training dummy as a reliable build-benchmarking tool without turning it into a large analytics suite.

Tiny Rogues is undergoing a major rewrite, so work should favor empirical measurement and comparison over systems likely to become obsolete.

## P0 — Measurement Correctness

### Audit combat-state behavior

Determine whether attacking the dummy correctly activates mechanics that depend on:

- combat beginning
- EnemyManager.IsInCombat
- EnemyManager.CheckIsInCombat
- TemporaryEvents.CombatStarted
- while-in-combat effects
- start-of-combat effects

Do not simply force combat state globally. First identify which mechanics depend on combat state and determine the minimum safe behavior required for accurate benchmarking.

### Audit dummy defensive properties

Verify the spawned target dummy's:

- armor
- weakness state/type
- resistance state/type
- creature types
- boss state

Because DPS uses final damage received, hidden defensive properties would directly alter reported DPS.

Desired default:

- neutral armor
- no weakness
- no resistance
- not a boss

## 1.1 — Better Benchmark Sessions

### Preserve completed measurements

Change the measurement lifecycle to:

READY -> MEASURING -> RESULT

When the player stops attacking or the configured window ends:

- finalize the measurement
- keep the result visible
- do not erase it automatically

The next attack starts a new test.

### Previous-test comparison

Store the previous completed test.

Example:

TRAINING DUMMY
624 DPS
PREV 592 · +5.4%

### Clarify metric

Describe the result as:

single-target final-damage DPS

Known limitations:

- AoE coverage
- chains requiring multiple enemies
- kill-triggered effects
- boss-only effects
- enemy-type-specific effects

## 1.2 — Damage Source Breakdown

Capture rich damage metadata around Actor.InflictDamage and continue using AfterDamageTaken for the final resolved damage.

Relevant information:

- Id
- DamageOrigin
- OriginKey
- DamageDealer
- Tags
- damage types
- DOT markers
- companion markers
- crit/lucky/crushing/etc.

Initial categories:

- Direct
- DOT
- Companion
- Triggered

Do not infer attribution from GameObject names.

## 1.3 — Testing Immediately After Loot

Investigate optional:

SpawnAfterRoomClear = false

When enabled:

- wait until combat is fully finished
- spawn after room completion
- never interfere with rewards
- remove on room transition
- prefer reward/weapon rooms if reliably identifiable

Keep disabled by default.

## Later — Multi-Target Benchmark

Potential controlled 3–5 target test for:

- combined DPS
- average damage per target
- AoE/chain performance

Do not prioritize before the major Tiny Rogues rewrite.

## Post-Rewrite

Re-evaluate:

- boss target profiles
- resistance profiles
- weakness profiles
- enemy-type profiles
- multi-target benchmarks
- named damage-source attribution

## Out of Scope For Now

- theoretical DPS calculator
- weapon database
- run-history database
- persistent analytics
- complex settings UI
- resistance editor
- boss simulator
- attack-pattern visualizer
- charts/graphs
