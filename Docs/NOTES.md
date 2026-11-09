# NOTES.md — Study notes

Short notes in plain words, one entry per topic. Newest at the bottom. Review these before interviews.

---

## Unity lifecycle and ScriptableObjects (Week 3, Sun)
- Order: `Awake` (set yourself up; once per object) → `OnEnable` (subscribe) → `Start` (use other objects; all `Awake`s have run) → `Update` (only if something changes every frame) → `OnDisable` (unsubscribe) → `OnDestroy`.
- `OnEnable`/`OnDisable` run on every activate/deactivate (so pooled objects hit them many times); `Awake`/`OnDestroy` run once. Every `+=` needs its `-=`.
- ScriptableObject = an asset file of data, edited in the Inspector. One class (`LevelConfig`), many assets (`SO_Level_01` … `SO_Level_20`). Private serialized fields with read-only properties; config is read-only at runtime, state lives in models (`new MovesModel(config.Moves)`). Naming: `SO_` prefix, folder `_Project/ScriptableObjects` (R3.3, P7).
- Code: `Game.Runtime.Config.LevelConfig`; assets `SO_Level_01` (20 moves, 1000 target) and `SO_Level_02` (18 moves, 1500 target). Checked: `MovesModel(level2.Moves).Remaining == 18`.

## Week 2 review: things to re-check (Fri)
- Factory: the board only says `Create(color, position)`. It must not know about the prefab, the pool, the sprite lookup or the reset.
- MVP constructor: the presenter is handed a model and a view (`IMovesView`), listens to the model, and calls `ShowMoves` on the view. `Bootstrap` creates and hands them in.
- Pool numbers (Editor): 2.0 KB and 0.55 ms with `Instantiate`/`Destroy` vs 0 B and 0.25 ms with `ObjectPool`.

## MVP and dependency injection (Week 2, Thu)
- **MVP:** Model = data and rules, no UI (`MovesModel`). View = only displays, no logic (a label with `ShowMoves`). Presenter = plain C# that listens to the model's events and tells the view what to show. Subscribe in `Enable`, unsubscribe in `Disable`.
- The presenter takes an interface (`IMovesView`), so a test can hand it a fake view and check what it was asked to show; no scene needed.
- Formatting belongs to the view, but a change like "5 / 20" needs the total, so the interface and presenter gain a parameter too.
- **DI:** dependencies are handed in (constructor), not fetched. **Service locator:** the class reaches into a global registry; needs are hidden and tests need global setup. Project rule: DI only, no locator, no static singleton (P6, R1.7).
- `Bootstrap` (top of the Boot scene) creates the model, view and presenter and passes them in.

**Two-minute explanation (practise out loud):**
"I'd use MVP. The model holds the data and rules with no UI, like `MovesModel`. The view only displays things, like a label with a `ShowMoves` method. The presenter is plain C# that listens to the model's events and tells the view what to show, so I can test it with a fake view. The presenter doesn't create or look up its model and view; they're handed in through its constructor, which is dependency injection, and a single Bootstrap creates everything and passes it in. I avoid a service locator because the dependencies would be hidden and tests would need global setup."

## Pool and Factory (Week 2, Wed)
- Unity's `ObjectPool<T>` (`UnityEngine.Pool`) takes four callbacks: `createFunc` (a `Func`, makes a new one), `actionOnGet`, `actionOnRelease`, `actionOnDestroy` (`Action`s), plus `defaultCapacity` and `maxSize` (spare ones beyond it are destroyed). Use `Get()` / `Release()`.
- Reset all state when an object goes back (our `OnDespawned`, P4) so the shelf only holds clean ones.
- Factory = one class owns the creation details; callers say `Create(color, position)` and `Recycle(piece)`. Behind an interface (`IPieceFactory`) so tests can use a fake; sprites come from a config, so a new gem type changes config, not the factory. The board knows nothing about pooling.
- Measured (Editor, 50 objects/frame): `Instantiate`/`Destroy` 2.0 KB and 0.55 ms → pool 0 B and 0.25 ms (see PERF_LOG.md). Compare the same Profiler row in both modes, not different rows.

## State machine (Week 2, Tue)
- The game is in exactly one state at a time (Boot, Menu, Level, Win), so impossible combinations of bools can't happen.
- `IState` = `Enter()` + `Exit()`; `StateMachine.ChangeState(next)` = guard (same state → do nothing), `Current?.Exit()`, set `Current`, `Current.Enter()`. Leave first, then arrive. The first call has no old state to exit.
- The machine only knows `IState` (DIP), so adding a `ShopState` never touches it. Class-based, not the Animator; one machine per concern (P2).
- Code: `Game.Core.Flow` (`IState`, `StateMachine`); tests use a `LoggingState` that records Enter/Exit to check the order.

## Observer and Command (Week 2, Mon)
- Observer = events / the notice board: one thing announces, many react, the announcer doesn't know them (`MovesChanged`, `EventBus`).
- Command = an action as an object that knows how to `Execute()` and `Undo()` and remembers its own values (`AddScoreCommand(score, points)`). Keep the history in a `Stack`: newest undone first.
- Other uses: input queue, replays, tutorials, booster use. Real match-3 undo must restore the board state (cascades change many cells), not just "swap again".
- `private readonly` = hidden from outside + set once in the constructor, so a command can't lose track of what it did.

## SOLID (Week 2, Sun)
- **S** Single Responsibility: one class, one job (`BoardModel` finds matches; input, view and saving live elsewhere).
- **O** Open/Closed: add behaviour without editing old code (a new booster = a new class).
- **L** Liskov Substitution: a child must work wherever its parent is expected (every booster activates the same way).
- **I** Interface Segregation: small interfaces (`IPoolable` has two methods), not one giant one.
- **D** Dependency Inversion: depend on a promise, not the concrete thing (`BoardModel` receives `IRandom`); `Bootstrap` hands in the real tools.
- Refactor example: old `MainEvents.cs` split into narration, movement switch, bridge, mantis, hands (see `Docs/Study/MainEvents_refactor.md`). Repeated code with small differences = data in a list.

## Week 1 review: things to re-check (Fri)
- Array vs List: the 8x8 board is an array (`board[x, y]`, fixed size); a `List` is for a changing count. `GridPosition` is a cell's address, not its contents.
- Interface = a list of method names a class promises (no code, many per class). Abstract class = a parent with real code plus blanks (one parent).
- Garbage fixes: `SetText("Score: {0}", score)` instead of string concatenation; `GameLog.Info` instead of `Debug.Log`.
- Async: the waiter holds a `token`; the boss holds a `source` and calls `source.Cancel()`.

## Value vs reference types (Week 1, Sun)
- `struct` = photocopy: `b = a` makes an independent copy. `Vector3`, `GridPosition`.
- `class` = two names for one object: `b = a` points to the same object. `Transform`, `GameObject`.
- `b = new ...` always makes a new, separate object, even for classes.
- Structs create no GC garbage, which is why board positions are structs.
- Why `transform.position.x = 5` is a compile error: reading `position` returns a copy.

## Boxing (Week 1, Sun)
- Boxing = a plain value (`int`, struct) gets wrapped in a heap object. That object is garbage the GC must clean up.
- Happens when a value goes into an `object` variable: `object o = score;`, `Debug.Log(coins)`, `ArrayList`.
- One call is fine. Inside code that runs every frame it adds up and causes stutters.
- Avoid with generics (`List<int>`), `IEquatable<T>` on structs, and `GameLog` instead of `Debug.Log`.

## Garbage collector and hidden allocations (Week 1, Mon)
- Heap = the counter where objects live. GC = the cleaner that pauses the game to clear unused ones. At 60 fps a frame has about 16 ms, so a pause is a visible stutter.
- Hidden garbage sources: string concatenation, `new` in `Update`, boxing, lambdas that capture variables, LINQ, `foreach` over interface-typed collections.
- Fixes: create once and reuse (`Clear()`), use a plain `for` loop, use `TMP_Text.SetText("{0}", n)`, update UI on events instead of every frame.
- Lab results: new List 88 B → 0 B; LINQ 284 B → 0 B. String snippet unresolved in the Editor (98 B); re-measure on device in week 8 (see PERF_LOG.md).
- `for` vs `foreach`: `for` shows the index and never allocates; `foreach` is shorter but allocates when the variable's type is an interface (`IEnumerable<T>`). Prefer `for` in hot paths.
- Editor numbers are for finding problems; device numbers go into the log.

## Collections cheat sheet (Week 1, Tue)

| Need | Container | Game example |
|---|---|---|
| Fixed count, access by position | Array | the 8x8 board: `board[x, y]` |
| Grows and shrinks, keeps order | `List<T>` | pieces cleared this turn |
| Each item once, fast "is it in there?" | `HashSet<T>` | cells already checked during a match search |
| Look up a value by a key | `Dictionary<K,V>` | piece colour → score or sprite |
| First in, first out (a line) | `Queue<T>` | pieces waiting to fall |
| Last in, first out (a pile) | `Stack<T>` | undo history |

- Searching a `List` one by one gets slower as it grows. `HashSet`/`Dictionary` go straight to the item, so they stay fast.
- A struct used as a set/dictionary key needs `IEquatable<T>` and `GetHashCode` (like `GridPosition`).
- Create collections once and reuse them with `Clear()` (no per-frame `new`).

## Delegates, generics and the event bus (Week 1, Wed)
- `Action` = a method with no result you can hand around (`new Ability(() => ...)`); `Action<int>` receives a value; `Func<int, bool>` answers a question (last type = the answer). Lambda `x => ...` = "takes x, does/gives ...".
- One method + a `Func` rule = many behaviours (booster `ClearWhere(cell => cell.Y == 3)`). Capturing lambdas allocate: fine for one-offs, not per-frame.
- Interface = "it can do this" (many allowed, no shared code). Abstract class = "it is this kind of thing" (one parent, shared code; `abstract` = blank each child must fill, `protected` = children may use, `override` = my version). `sealed` = nothing may inherit; our default (R2.4). Abstract + sealed is a compile error.
- Generics: `<T>` is a placeholder type (`List<T>`, `Pool<T>`). Constraints: `where T : new()`, `where T : class`, `where T : IPoolable` — the compiler then knows `T` can do that.
- Object pooling: never Instantiate/Destroy repeatedly; deactivate and reuse, and reset all state on return (OnSpawned/OnDespawned).
- Event bus: a notice board for global announcements only ("level won"). `Channel<T>` = listeners for one notice type; `EventBus` = `Dictionary<Type, object>` of channels. Publish with no listeners does nothing. For local things (a moves label) use a plain C# event instead.
- `TryGetValue(key, out var x)` is the safe dictionary lookup; the `out` variable name must not clash with a parameter name.

## Async vs coroutines (Week 1, Thu)
- Never wait with a plain pause: frames must keep rendering. Coroutine: `yield return new WaitForSeconds(2f)`. Async: `await Awaitable.WaitForSecondsAsync(2f, token)`.
- A coroutine belongs to its MonoBehaviour, so Unity stops it when the object is destroyed. An async method belongs to nobody, so it keeps running and then touches a destroyed object (`MissingReferenceException`) unless it listens to a stop signal.
- Stop signal = `CancellationToken`. In a MonoBehaviour use `destroyCancellationToken` (Unity raises it on destroy). In plain C# the caller passes a `token`; a test makes one with `CancellationTokenSource` and presses it with `source.Cancel()`.
- A cancelled wait throws `OperationCanceledException`; catch it quietly (expected), but never leave other empty catches (real bugs, e.g. a corrupted save → log and start fresh).
- Async can return a value (`Task<int>` / `Awaitable<T>`); coroutines need a callback. Avoid `async void` (R9): use `async Awaitable` and start with `_ = Method();`.
- Exercise: `FakeLevelLoader` in `AsyncLoaderTests.cs` (Task-based so it runs in EditMode; the game version uses Awaitable).

## Interfaces (Week 1, Wed — started early)
- An interface is a promise: "I have these methods". A class can promise many; it can only inherit from one parent.
- Lets tests swap real code for a fake (`IRandom` → a dice that always returns 3).

## Constructor, `readonly`, `=>` (Week 1)
- Constructor = the `Awake` of plain classes, but it takes parameters.
- `readonly` = a field that can only be set once, in the constructor. A safety lock.
- `=>` = one-line method; in a lambda it means "takes this, does that".

## Events (Week 1, Wed — started early)
- The owner shouts, listeners sign up with `+=` and sign off with `-=`.
- Always subscribe in `OnEnable` and unsubscribe in `OnDisable`, or destroyed objects stay on the list.
- C# `event` for code-to-code; `UnityEvent` only for designer-wired Inspector clicks.
