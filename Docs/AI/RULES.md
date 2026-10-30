# RULES.md — Coding, structure, and performance rules

Rules are numbered so they can be referenced in reviews (e.g. "breaks R4.2").
"MUST" = always. "SHOULD" = default, deviate only with a stated reason.

---

## R1. Architecture

- R1.1 MUST keep **game logic in plain C#** (no `MonoBehaviour`, no `UnityEngine` dependency where possible).
  Board, matching, scoring, level goals, economy, and save data live in `Game.Core`.
- R1.2 MUST keep **MonoBehaviours thin**: they read input, forward it to logic, and render state (views).
- R1.3 MUST separate **Input → Logic → Output (view, audio, VFX)**. Logic never calls a view directly;
  it raises events or returns results that views apply.
- R1.4 SHOULD prefer composition over inheritance. Max one level of inheritance for gameplay classes.
- R1.5 MUST give each class one responsibility. If a class passes ~300 lines, propose a split.
- R1.6 MUST wire dependencies explicitly (constructor parameters for plain C#, `[SerializeField]` or an
  installer/bootstrap for MonoBehaviours). See PATTERNS.md P6.
- R1.7 MUST NOT use singletons with static `Instance` for gameplay. One `Bootstrap` composition root creates services.

## R2. C# style

- R2.1 Naming:
  - Classes, structs, enums, methods, properties, events, constants: `PascalCase`
  - Interfaces: `IPascalCase`
  - Private and serialized fields: `_camelCase`
  - Locals and parameters: `camelCase`
  - Events: past tense or noun (`MatchFound`, `MovesChanged`). Handlers: `Handle` + event (`HandleMatchFound`).
- R2.2 MUST use `[SerializeField] private` instead of public fields. Expose read-only properties when needed.
- R2.3 MUST put every script in a namespace that mirrors its folder: `Game.Core.Board`, `Game.Runtime.UI`, …
- R2.4 MUST mark classes `sealed` unless designed for inheritance.
- R2.5 SHOULD use `readonly` for fields assigned once.
- R2.6 No `#region` blocks unless a file has a real reason (generated code, very long editor tool).
- R2.7 Comments explain *why*, not *what*. Public APIs of Core get a one-line `///` summary.
- R2.8 MUST NOT leave `TODO` without a short description of what is missing.

## R3. Folder structure (STRICT)

All project-owned content lives under **one root folder**. Third-party SDKs create their own root folders
(`Plugins`, `GoogleMobileAds`, `ExternalDependencyManager`, …) — leave those where they are; never move them.

```
Assets/
└── _Project/
    ├── Animations/
    ├── Audio/
    ├── Art/              (sprites, atlases, fonts — imported, not authored)
    ├── Materials/
    ├── Prefabs/
    ├── Scenes/
    ├── ScriptableObjects/  (config + level data assets)
    ├── Settings/         (URP assets, Input Actions)
    ├── UI/               (UI prefabs, UI sprites)
    ├── VFX/              (particle prefabs, shaders, shader graphs)
    ├── AddressableContent/  (anything loaded via Addressables)
    └── Scripts/
        ├── Core/         → asmdef Game.Core      (pure C#, no UnityEngine refs where possible)
        ├── Runtime/      → asmdef Game.Runtime   (MonoBehaviours, views, services)
        └── Editor/       → asmdef Game.Editor    (editor tools only, Editor platform)
Assets/Tests/
    ├── EditMode/         → asmdef Game.Tests.EditMode (references Game.Core)
    └── PlayMode/         → asmdef Game.Tests.PlayMode (references Game.Runtime)
```

- R3.1 MUST NOT create new folders under `Assets/` root. New sub-folders inside `_Project/` are allowed
  if they fit an existing category — state the reason.
- R3.2 Scripts MUST go under `_Project/Scripts/<Core|Runtime|Editor>/<Feature>/`.
- R3.3 Asset names are `PascalCase`. Prefabs and scripts have no prefix. Other assets use a type prefix:
  `SPR_` sprite, `MAT_` material, `SO_` ScriptableObject asset, `SFX_` sound effect, `MUS_` music, `VFX_` particle prefab.

## R4. Scene hierarchy (STRICT)

Every scene MUST follow this hierarchy. Separator objects are empty GameObjects named exactly as shown.

```
==========General==========
    Bootstrap               (composition root — scenes after Boot may omit it)
==========Camera==========
    Main Camera
==========Board==========
    BoardView
    PiecePool
==========VFX==========
    VfxPool
==========Audio==========
    MusicSource
    SfxPool
==========UI==========
    HUD Canvas              (dynamic: score, moves)
    Popup Canvas            (popups)
    Static Canvas           (backgrounds that never change)
    EventSystem
```

- R4.1 Separator objects MUST: be at position (0,0,0), rotation 0, scale 1; have **no components**
  besides Transform; use the tag **`EditorOnly`** (so Unity strips them from builds — zero runtime cost).
- R4.2 Children go **next to** the separator in the hierarchy order shown, not as its children
  (separators are visual dividers; parenting under them would make objects EditorOnly too).
- R4.3 Within a section, group related objects under a normal parent GameObject (no `EditorOnly` tag).
- R4.4 If a section is missing → create it. If an object is in the wrong section → move it and report the move.
- R4.5 Unused sections may stay empty. Do not invent new top-level sections without asking.

## R5. Performance (mobile — CRITICAL)

Target device: a low/mid Android phone. Budget: **stable 60 fps, 0 B GC allocation per frame during gameplay.**

- R5.1 MUST NOT allocate per frame in gameplay: no LINQ, no string concatenation/formatting, no `new` of
  collections, no lambdas capturing variables, no boxing in hot paths.
- R5.2 MUST cache component references (`GetComponent` in `Awake`, never in `Update`).
- R5.3 MUST NOT use `Find`, `FindObjectOfType`, `FindFirstObjectByType`, `FindObjectsByType`, `GameObject.Find`,
  or `SendMessage` in runtime code. Editor tools may use them.
- R5.4 `Update()` is allowed only where something truly changes every frame (input polling, an active tween driver).
  Prefer events, `Awaitable`, or one central driver over many small `Update()` methods.
- R5.5 MUST pool anything spawned repeatedly (pieces, VFX, floating texts, audio one-shots). Use `UnityEngine.Pool.ObjectPool<T>`.
- R5.6 UI: split canvases by update frequency (static / dynamic / popup). Disable `Raycast Target` on
  non-interactive graphics. Avoid Layout Groups on elements that change every frame.
- R5.7 Sprites MUST be in a Sprite Atlas. Textures use ASTC compression on Android.
- R5.8 Logging in runtime code MUST use `GameLog` (EXAMPLES.md E5), which is compiled out of release builds.
- R5.9 Any optimization change MUST record before/after numbers in `Docs/PERF_LOG.md`.

## R6. Testing

- R6.1 Every Core class with logic MUST have EditMode tests. Test file: `<ClassName>Tests.cs`.
- R6.2 Test names: `Method_Condition_ExpectedResult` (e.g. `FindMatches_ThreeInRow_ReturnsOneMatch`).
- R6.3 Randomness MUST be injectable (seeded `System.Random` or an `IRandom` interface) so tests are deterministic.
- R6.4 A bug fix in Core MUST add a test that fails before the fix.
- R6.5 Do not mark a task done while tests fail.

## R7. Inspector design

- R7.1 Group fields with `[Header]`. Add `[Tooltip]` to anything whose unit or range is not obvious.
- R7.2 Use `[Min]`, `[Range]` to block invalid values.
- R7.3 Expose only what a designer would tune. Internal state stays private and unserialized.
- R7.4 Validate references in `OnValidate` (editor) or fail fast with a clear error in `Awake`.

## R8. Debugging

- R8.1 Debug visuals are optional via a `_debugMode` bool on the component (off by default).
- R8.2 Use Gizmos for anything spatial: grid cells, spawn points, touch areas, swipe direction.
- R8.3 Gizmo colors: green = valid/active, yellow = pending/selected, red = invalid/blocked, cyan = input.
- R8.4 Context-menu helpers (`[ContextMenu("Debug/…")]`) are welcome for testing from the Inspector.

## R9. Forbidden (unless I explicitly allow it in the task)

- Singletons with static `Instance` for gameplay systems
- `Find*` / `SendMessage` in runtime code
- Public fields
- `Resources` folder / `Resources.Load` (use Addressables or serialized references)
- `async void` except Unity event handlers — use `async Awaitable`
- Logic in views; views in logic
- Over-engineering: no abstraction layer for something used once

## R10. Before and after writing code

Before:
1. Search the project for an existing system that already does this (by name and by folder).
2. Follow the pattern already used in that area.
3. Choose the smallest solution that is still testable and extensible.

After — check each item and report any you could not satisfy:
1. Follows R1–R9.
2. Compiles with no warnings in the Console.
3. EditMode tests pass (and PlayMode tests if touched).
4. No new per-frame allocations (if gameplay code changed).
5. Inspector is clean and grouped.
6. Debug support added where relevant.
