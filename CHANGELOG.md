# Changelog

## 1.0.4

### Release cleanup

- Removed obsolete Thunderstore packaging and references.
- Finalized the GitHub release package and documentation.
- Preserved the known limitation that native automatic targeting may not target the dummy.

## 1.0.3

### Improved

- Clearer Windows and Steam Deck / Proton installation and troubleshooting guidance
- Improved contributor and developer documentation

### Changed

- Removed an internal development helper from the production plugin
- General runtime cleanup with no intentional change to the core training-dummy workflow

### Validation

- Steam Deck regression smoke test passed after the cleanup
- Benchmarking, room lifecycle, post-combat spawning, equipment swapping, and clean-target reset behavior remain intact

## 1.0.2

- Improved release and package readiness
- Added contributor and community documentation
- Clarified supported platforms and installation paths
- Preserved the finalized training-dummy benchmark workflow

## 1.0.1

- Added clean dummy replacement between completed DPS benchmarks
- Added safe post-combat dummy spawning for gear comparisons
- Fixed the training dummy persisting after leaving a floor starting room
- Preserved benchmark history across target replacement and room transitions
- Verified final damage tracking, combat-state behavior, inventory behavior, and target defensive properties
- Native controller auto-aim and some automatic targeting may not target the dummy; manual aiming may be required

## 1.0.0

- Added an immortal training dummy to each floor's starting area
- Added DPS, total damage, test duration, and peak hit tracking
- Added configurable display, timing, and placement options
- Added Windows and Steam Deck / Proton support
