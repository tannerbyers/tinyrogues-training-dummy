## What changed?

<!-- Describe the user-visible change and link the issue it addresses. -->

Closes #

## Validation

- [ ] `dotnet build -c Release`
- [ ] `git diff --check`
- [ ] `./scripts/package-all.sh` when packaging or release behavior changed
- [ ] Manual Tiny Rogues test completed, including game version and platform below

Runtime version/platform:

## Checklist

- [ ] The change preserves the dummy's exclusion from `EnemyManager.aliveEnemies`.
- [ ] No generated binaries, interop files, personal logs, or local helpers are included.
- [ ] User-facing documentation was updated if needed.
