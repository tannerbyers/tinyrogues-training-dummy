# Contributing

Thanks for helping improve Tiny Rogues Training Dummy. This is a small community mod, so focused changes and clear runtime evidence are especially useful.

## Before you start

- For a bug, include the Tiny Rogues version, BepInEx version, platform, reproduction steps, and relevant `BepInEx/LogOutput.log` lines.
- For a feature or larger change, open an issue first so the scope can be agreed on.
- Check existing issues before opening a duplicate.
- Issues labelled `good first issue` are suitable for a first contribution. `help wanted` means implementation help is welcome.

## Local development

Requirements:

- .NET SDK with the version required by `TinyRogues.TrainingDummy.csproj`
- BepInEx 6 Unity IL2CPP interop assemblies for the local build
- Tiny Rogues 0.2.8.6 for runtime testing

The project references local BepInEx and Tiny Rogues IL2CPP interop assemblies under `lib/`. They are intentionally ignored and must be obtained from your own local installation. Do not commit or redistribute proprietary game assemblies. The repository does not currently have build CI because those references cannot be supplied legally in a clean public workflow.

Build and validate with:

```sh
dotnet clean
dotnet build -c Release
git diff --check
./scripts/package-all.sh
```

Do not commit `bin/`, `obj/`, `dist/`, reverse-engineering files, or local game/development helpers. The package script produces release archives under `dist/` for local inspection.

For the runtime map and invariants, see [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md). The minimum manual smoke test is listed there.

## Project constraints

The training dummy is deliberately not registered in `EnemyManager.aliveEnemies`. This keeps room completion, combat state, and weapon/equipment inventory behavior normal. Changes must preserve that isolation. Do not restore auto-aim by adding it to the enemy manager.

The dummy is manually targetable and damageable. Native controller auto-aim is deferred. Do not add the dummy to the enemy manager or introduce broad targeting patches without an issue and runtime evidence.

## Pull requests

- Keep pull requests focused and explain the user-visible behavior changed.
- Include the commands you ran and, for gameplay changes, a short manual test report with the game version and platform.
- Update README, CHANGELOG, or ROADMAP when user-facing behavior or release plans change.
- Do not include game binaries, interop dumps, personal logs, or unrelated formatting changes.

Maintainers may ask for a smaller change or additional runtime evidence before merging.
