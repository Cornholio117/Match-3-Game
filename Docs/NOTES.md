# NOTES.md — Study notes

Short notes in plain words, one entry per topic. Newest at the bottom. Review these before interviews.

---

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
