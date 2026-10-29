# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity incremental/idle game ("Energy Idler", company `Hypnagogia`), Unity **6000.3 LTS**, URP 2D, WebGL target published to itch.io. Game code lives in `Assets/_Code` (namespace `FattestInc.*`, compiled into the default `Assembly-CSharp` / `Assembly-CSharp-Editor`); shared framework code lives in `Assets/_Code_Utils` (asmdef `Hypnagogia.Utils`).

## Working with the Editor (Unity CLI)

The project has `com.unity.pipeline` installed, so the Unity CLI (`unity`, installed to `%LOCALAPPDATA%\Unity\bin`) can drive a **running** Editor. Prefer it over reading `Editor.log` or asking the user to click things.

```bash
unity pipeline list                                   # is the Editor running / Pipeline server reachable?
unity command console status --result-only            # compile-failure flag + error/warning counts
unity command console get --level error --result-only # console errors
unity command script recompile                        # then poll: unity command script recompile_status
unity command test run --mode EditMode --result-only  # all EditMode tests
unity command test run --mode EditMode --filter FattestInc.Tests.BuyMultipleTests --result-only  # one class or test (full name)
```

- `unity command` with no args lists all command groups (scene, prefab, asset, build, editor.playmode, settings, …).
- If the Pipeline server is unreachable, the Editor is compiling/reloading or stuck in Safe Mode. Unity 6.x writes its log to `%LOCALAPPDATA%\Unity\Editor\Editor.log`, not the project's `Logs/`.
- Batch-mode Unity can't open the project while the Editor has it open. Don't hand-edit `ProjectSettings/*.asset` while the Editor is running; Unity will overwrite it from memory. Use the CLI `settings` commands, or close the Editor first.

## Tests

EditMode NUnit tests live in `Assets/_Code/Editor/Tests` (namespace `FattestInc.Tests`) and mainly cover factory cost/value/duration curves and multi-buy math. Mocks use **Moq**, installed through NuGetForUnity (`Assets/packages.config`, restored into `Assets/Packages/`). If NuGet hasn't restored yet after a fresh clone or upgrade, the tests fail to compile on `using Moq`.

## Architecture

### Dependency injection (Extenject fork) and the `H*` building blocks

Everything is wired through Zenject (a fork: `karolwieczorek/Extenject`). `Hypnagogia.Utils` adds conventions on top:

- **`HSystem`**: plain C# logic class; `SystemStart()`/`SystemStop()` map to `IInitializable`/`IDisposable`. Bind with `Container.BindSystems(typeof(...))` (non-lazy singletons). Systems may also implement `ITickable`.
- **`HDataStore`**: `MonoBehaviour` holding runtime state, usually exposed as `Observable<T>` plus C# events. Bind with `Container.BindDataStores(...)`; spawned under a `DataStores` GameObject.
- **`HReferencer`**: `ScriptableObject` holding asset references (prefabs, data lists). Every referencer asset under `Assets/_Data/Referencers` is auto-collected (Odin `AssetList`) by `_ReferencersInstaller.asset` and bound by type.
- **Helpers**: stateless services such as `BuyMultipleHelper`, `SaveHelper` and `UnlockingHelper`, bound `AsSingle`.
- Inject fields with `[HInject]` (not `[Inject]`).

Installer layers:
- `Assets/Resources/ProjectContext.prefab` → `ProjectInstaller` (global data stores, audio, save/scene helpers), `SignalBusInstaller`, `ReferencersInstaller`.
- `BootstrapperInstaller` (scene `scn_Bootstrapper`) → `ResourceFactoriesInitializationSystem`, which loads the save and seeds factories.
- `GameplayInstaller` (scene `scn_Gameplay`) → `ResourceFactoriesSystem` (ticks factories, unlock/show logic), `EconomySystem` (recomputes per-second income), `SaveSystem`, `ChangelogGateSystem`.

Play starts from `scn_Bootstrapper`; `Bootstrapper` calls `ScenesLoaderHelper.LoadGameScene()` (UniTask + `ZenjectSceneLoader`, scenes referenced via Eflatun `SceneReference`).

### Economy model

- Factories are ScriptableObjects implementing `IFactoryData` (`IFactoryInfo` + `IFactoryLevelsData`). There are two implementations:
  - `FactoryLevelsData`: per-level table imported from Google Sheets (`GoogleSpreadsheetScriptableObject`, Odin buttons to fetch).
  - `FormulaFactoryData`: cost = `baseCost * costMultiplier^(level-1)`, linear output, fixed duration.
  `FactoriesReferencer` holds the active list (`Assets/_Data/Factories`, `Assets/_Data/FormulaFactories`).
- `FactoryType.Clicker` produces per click; `Idle` produces `valuePerTick` every `duration` seconds (`ResourceFactory.Tick`).
- `EconomyDataStore` owns the currency (`ulong`), per-second income, and the `ResourceFactory` runtime objects keyed by `FactoryId`. Upgrades raise `FactoryUpgradedEvent`, which drives income recalculation, unlock checks and autosave.
- Unlock/visibility rules come from `UnlockingFactoriesData` / `ProgressionLevelsData` (`Progression/API`, via `UnlockingHelper`).
- Saves are JSON (`SaveData`) wrapped with version and encryption info and stored in `PlayerPrefs` (`SaveStorageUtils`).

### Simulation / auto-balancer

`Simulation/API/EconomySimulator` is a pure, engine-free second-by-second simulation over `IFactoryData` and unlock rules (greedy buying). `SimulationRunner` feeds it from the referencers, and the `Simulation*ChartView` classes render the results with **XCharts** (scene `scn_Simulation_Testing`, `SimulationWindow`). Keep balance math in `API` classes so tests and the simulator can use it without a scene.

### UI

Windows are prefabs listed in `WindowsReferencer`, lazily instantiated through the DI container by `WindowManager.GetWindow<T>()`. Window types derive from `WindowBase` / `WindowWithParametersBase`. Tweens use LitMotion; the inspector tooling uses Odin attributes (only `Sirenix.OdinInspector.Attributes.dll` is in the repo).

### Folder convention

Feature folders are split into `API` (data, interfaces, helpers, data stores; safe to reference) and `Implementation` (systems/MonoBehaviours wired by installers).

## Builds

WebGL builds and itch.io uploads run from the Editor menu defined in `Assets/_Code/Editor/Builds/ItchUploader.cs` (uses `butler`; path configured in the "Itch.io butler" preferences). `WebGLPythonServer` serves local builds.

## Package gotchas

- Stay on Unity 6000.3 LTS. Unity 6000.6 broke both XCharts (an `EntityId`→`int` conversion became an error) and `com.unity.uiextensions` 3.0.0 (new `ILayoutElement.maxWidth/maxHeight` members).
- XCharts is pinned to a git commit in `Packages/manifest.json`. Keep the `dUI_TextMeshPro` scripting define **off**: TMP is built into uGUI in Unity 6 and the XCharts asmdef has no TMP reference, so enabling the define breaks compilation. The charts use legacy uGUI text.
- `.sln`, `.slnx`, `.csproj` and `.vscode/` are generated/ignored.

## Code style

4-space indent, opening brace on the same line, private fields without prefixes or `_`, local functions after an early `return`, `ulong` for currency. Match the surrounding file.
