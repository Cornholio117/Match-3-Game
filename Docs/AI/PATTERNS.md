# PATTERNS.md — Approved patterns for this project

Use these patterns by default. Each one says **when** to use it, the **structure**, and the **rules**.
Code templates for most of them are in EXAMPLES.md.

---

## P1. Model–View split (core of the whole game)

**When:** every gameplay system (board, pieces, score, goals, moves).

**Structure:**
```
Game.Core (plain C#)            Game.Runtime (MonoBehaviour)
BoardModel  ── events/results ─▶ BoardView ── animates ─▶ PieceView (pooled)
     ▲                                │
     └──────── commands ◀── BoardInputHandler ◀── touch input
```

**Rules:**
- The model is the single source of truth. Views never store gameplay state.
- The model returns *what happened* (e.g. a list of `BoardStep`: Swap, Clear, Fall, Spawn); the view plays those
  steps as animations, in order. This keeps logic instant and testable, and animation separate.
- Input is blocked while the view is animating (one `IsBusy` flag owned by the controller, not the view).

## P2. Game-flow state machine

**When:** app and level flow (Boot → MainMenu → LevelSelect → Playing → Win/Lose → …), and board turn phases
(WaitingForInput → Resolving → Animating).

**Structure:** `IState { Enter(); Exit(); }`, a `StateMachine` that holds the current state and calls `Exit`/`Enter`.
States receive what they need through their constructor.

**Rules:**
- Class-based states, not the Animator.
- One state machine per concern (app flow vs board turn). Do not merge them.
- Log transitions through `GameLog` when `_debugMode` is on.

## P3. Events for decoupling

**When:** one system must react to another without a direct reference (UI reacting to score, audio reacting to matches).

**Structure:** C# events on the model or service (`public event Action<int> ScoreChanged;`).
Use a central `EventBus` only for truly global, cross-feature events (level won, currency changed).

**Rules:**
- Subscribe in `OnEnable`, unsubscribe in `OnDisable`. Always.
- Event payloads are small structs or primitives — no allocations per raise in gameplay.
- Do not use UnityEvents for code-to-code communication (fine for designer-wired button clicks).

## P4. Object pooling (replaces "Spawner + Pooling")

**When:** pieces, VFX, floating score texts, audio one-shots — anything created and destroyed repeatedly.

**Structure:** `UnityEngine.Pool.ObjectPool<T>` wrapped in a small typed pool component (`PiecePool`, `VfxPool`).
Pooled objects implement `IPoolable { OnSpawned(); OnDespawned(); }` to reset their state.

**Rules:**
- Never `Instantiate`/`Destroy` these at runtime after warm-up. Pre-warm in a loading step.
- Reset *all* visual state on despawn (scale, color, alpha, active tweens, particle state).
- Pool size is configured in the Inspector, with a tooltip explaining the expected maximum.

## P5. Screens and popups (MVP for UI)

**When:** every UI screen and popup.

**Structure:**
```
ScreenView (MonoBehaviour: references to buttons/texts, exposes events + Show/Hide/Set methods)
   ▲   │ events (PlayClicked)
   │   ▼
ScreenPresenter (plain C#: subscribes to view + model, decides what to show)
   │
   ▼
Model / services (LevelProgress, Wallet, …)
```
A `PopupManager` owns a stack of popups (open, close, back button on Android).

**Rules:**
- Views have no logic beyond formatting and animation.
- Presenters are testable without a scene.
- Android back button closes the top popup; on the root screen it asks to quit.

## P6. Services and composition root

**When:** anything shared across scenes (save, audio, analytics, ads, IAP, remote config, scene loading).

**Structure:** one `Bootstrap` MonoBehaviour in the Boot scene creates services as plain C# objects behind
interfaces (`ISaveService`, `IAnalyticsService`) and passes them to whoever needs them.
Services that must survive scene loads live on one `DontDestroyOnLoad` root created by Bootstrap.

**Rules:**
- Code depends on the interface, never the concrete SDK class. This lets SDKs be swapped and tests use fakes.
- Each SDK gets a `Null` implementation (`NullAnalyticsService`) used in the Editor and tests until the real one is added.
- No service locator lookups in gameplay code. Pass dependencies in.

## P7. Data-driven configuration

**When:** level definitions, piece types, economy values, tuning numbers.

**Structure:** `ScriptableObject` assets in `_Project/ScriptableObjects/`. Level data may also be JSON
(needed later for remote content updates via Addressables).

**Rules:**
- No gameplay numbers hard-coded in scripts. Put them in a config asset.
- Config assets are read-only at runtime. Runtime state lives in models.

## P8. Addressables loading

**When:** level art, level data packs, large audio, anything that may update remotely.

**Rules:**
- Every `LoadAssetAsync` MUST have a matching `Release` (track handles in the owning class, release in `OnDestroy`/on unload).
- Wrap Addressables in an `IAssetLoader` service so the rest of the code does not depend on the API.
- Show loading state in UI; never block the main thread waiting on a handle.

## P9. Save system

**When:** progress, currency, settings.

**Rules:**
- One serializable `SaveData` class with a `Version` int. Migrations upgrade old versions step by step.
- Write to a temp file, then replace the real file (prevents corruption on crash).
- Save on `OnApplicationPause(true)` and `OnApplicationQuit`, plus after important changes (level complete, purchase).
- If loading fails → keep a backup, log the error, start from defaults. Never crash on bad save data.

## P10. Debug gizmos (kept from the XR project)

**When:** anything spatial.

**Rules:**
- Always behind `_debugMode`.
- Colors per R8.3.
- Draw grid cells and indices for the board, swipe start/end for input, spawn columns for refill.
