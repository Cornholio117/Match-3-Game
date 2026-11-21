# CODE_TOUR_WEEK4.md — A guided tour of Week 4 (core gameplay)

Written 2026-11-21 after walking through the real code with practice questions. Read it before interviews. The goal is not
to memorise code but to be able to **supervise AI-written code**: say what each piece is for, spot a harmful change,
and prove a test is real.

## The big picture (pattern P1: model and view)

```
finger -> BoardInputHandler -> BoardController -> BoardGame.TrySwap -> TurnResolver
                                      |                                   |
                                      |                         list of BoardStep (the "diary")
                                      v                                   |
                                 BoardView  <---------- plays the diary as animation
```

The logic resolves the whole turn instantly and records what happened. The view replays it. Logic never calls a view.

## Part 1 — `BoardStep` (the diary of a turn)
- Kinds: Clear, Fall, Spawn, Create. Each entry holds a kind, a colour, `From` and `To` (a fall is the only step where they differ).
- `readonly struct`: a value type stored inside the list (no garbage per entry) and not editable after it is written.
- Private constructor plus `Clear/Fall/Spawn/Create` factory methods: no nonsense entries.
- **Supervise:** `struct` -> `class` allocates per step (GC Alloc in the Profiler, breaks R5.1); public fields let anyone rewrite history (breaks R2.2).

## Part 2 — `BoardModel` (the grid in memory)
- Two sheets over one grid: `_cells` (colour) and `_kinds` (special sticker). A gem is both. `None = 0` so a new array is an empty board.
- Guards throw on off-board positions: fail loudly at the real bug.
- `Swap` must move **both** sheets. If only colours move, the bonus sticks to the wrong gem.
- `Fill` goes bottom-left to top-right and bans a colour when the two cells behind it (left, below) already match. Seeded random = same board.
- **Known gap (not a bug today):** `Fill` does not reset `_kinds`. It becomes a bug if `Fill` is reused for a "no valid moves" reshuffle: old stickers would survive as ghost specials. Write the failing test first when that feature is built (R6.4).
- **Supervise:** removing the `_kinds` lines from `Swap` gives a bonus that triggers on the wrong gem, not a crash (the board holds numbers, not references).

## Part 3 — `MatchFinder` (the referee)
- `Find` scans every row left to right and every column bottom to top, tracking the current run. It reads **one step past the end as `None`** so a run touching the edge closes itself.
- Rows and columns are scanned separately, so a gem in an L/T/+ belongs to two runs.
- `IsPartOfMatch` counts same-colour neighbours outward from one cell: cheap, used by `SwapRules` (swap, ask, swap back).
- The results list is passed in and reused (no allocation per call).
- **Supervise:** `i <= length` -> `i < length` silently misses runs at the right/top edge (middle runs still work). Removing the `None` guard in `IsPartOfMatch` lets three empty cells count as a match.

## Part 4 — `TurnResolver.Resolve` (the heart)
```
loop: Find runs -> (none? return) -> (limit? return) -> PlanCreations -> CollectMatchedCells
      -> FireSpecials (chain reaction) -> ClearCells -> ApplyCreations -> Gravity -> Refill -> next wave
```
- One pass = one wave; after the refill new runs make a cascade. Stops when `Find` finds nothing.
- New special gems are planned **before** clearing so they are protected. `FireSpecials` needs the cleared set.
- Only wave 0 knows the player's swap cells; cascades use a fixed fallback cell.
- Creation priority (decided 2026-11-21): run of 5+ -> shockwave; L/T/+ of shorter runs -> diagonal; exactly 4 -> line.
- The cascade limit (50) is a safety net: the loop runs on the main thread, so without it a bad refill would freeze the phone.
- **Supervise:** refill before gravity puts new gems into holes in place (no falling, tests fail). Removing the limit: ask the AI to *prove* termination or keep the guard.

## Part 5 — `BoardController` (the traffic cop)
- `HandleSwapRequested` ignores input while `_isBusy` (the controller owns the flag, because it is game flow).
- `TrySwap` decides everything instantly and fills `_steps`; then the view plays the swap and the steps; a rejected swap plays a bounce.
- `_ = ResolveSwapAsync(...)`: start async work without `async void` (R9 forbids it).
- `try / catch / finally`: `finally { _isBusy = false; }` always unlocks the game. `OperationCanceledException` is expected on scene close; other exceptions are logged with `GameLog.Error`.
- **Supervise:** moving `_isBusy = false` out of `finally` means any exception leaves the game locked forever (looks frozen, board is fine). Playing the steps before `TrySwap` plays an empty/stale list: screen and model drift one turn apart. **Rule of thumb: if one line writes what another reads, their order is not free.**

## Part 6 — `BoardInputHandler` and `BoardView`
- Input: first finger only; `SwipeMath` (pure C#, tested) turns a drag into a direction; the swipe must be at least 0.3 of a gem; one swipe per touch (`_hasStart = false`); it only *reports* `SwapRequested`.
- Subscribe in `OnEnable`, unsubscribe in `OnDisable`. `TouchSimulation` is Editor-only.
- View: `PlayStepsAsync` groups neighbouring steps of the same kind and plays each group together. `_pieces[,]` holds **references to the gem objects on screen**, kept in sync by every step method. Pool gets/releases only; checks `destroyCancellationToken` by hand each frame.
- **Supervise:** `FloorToInt` instead of `RoundToInt` in `TryGetCell` makes the left and bottom half of every gem select the neighbour (gem centres sit on whole numbers). Playing falls in the wrong order overwrites an object reference in `_pieces`: one gem is moved twice, another is lost and stays on screen as a ghost, never cleared and never returned to the pool. The order of steps from `BoardGravity` (bottom gem first) is part of the contract.

## Part 7 — The tests
- Board pictures (`BoardBuilder.FromRows`, top row first, last line is y = 0), `ScriptedRandom` / `SeededRandom` for repeatable randomness, 100-seed stress tests for invariants, names like `Method_Condition_Expected`.
- Tests change **on purpose** when a rule changes, with a reason; never weaken a test to turn red green.
- **Proof a test is real:** break the code on purpose, watch the test fail, restore, watch it pass. This week: removing a diagonal (4 failures), forcing radius 1 (3), removing the edge slide (4), removing the 5-run rule (1).
- **Supervise:** `Assert.That(result, Is.Not.Null)` on a struct can never fail. Ask of every test: "what wrong answer would make this fail?" If none, it is decoration.

## The special gems built this week
| Gem | Made by | Fires as | Combos (centred on the cell the player moved to) |
|---|---|---|---|
| Line | exactly 4 in a row | its row or column | Line+Line = row + column |
| Diagonal | L, T or + of runs shorter than 5 | both diagonals through it | Line+Diagonal = row, column, both diagonals; Diagonal+Diagonal = an X through each swapped cell |
| Shockwave | 5 or more in a row (wins even inside a T) | a 5x5 square; near an edge the square slides inward so it is always 25 cells (radius is an Inspector setting) | Shock+Shock = whole board; Shock+Line = row and column 3 wide; Shock+Diagonal = both diagonals 3 wide |

Specials fire when **matched**, keep their colour, chain into other specials, and two specials may always be swapped (that is the combo).

## AI-supervision checklist (use on any AI diff)
1. **What does each changed line read and write?** If one writes what another reads, the order matters.
2. **Is the safety net still there?** Guards, `finally`, limits, null checks, cancellation.
3. **Does it allocate per frame?** `struct` to `class`, LINQ, string building, new lists in hot paths.
4. **Does it break a rule?** Public fields, `Find*`, `async void`, logic in a view, singletons (R1-R9).
5. **Does the model and the view still agree?** Anything that touches the board data must keep `_cells` and `_kinds` (and the view's `_pieces`) in step.
6. **Is there a test that would fail if this were wrong?** Break the code on purpose and see.
7. **Edge cases:** the board edge, empty cells, a run touching the edge, two things in one cell.

## Gaps worth rehearsing out loud
- Why the controller owns `_isBusy` and why `finally` matters.
- Why the diary (`BoardStep`) exists and why it must be written before it is read.
- Which line you would check first when "the bonus triggers on the wrong gem" (hint: `Swap` moves both sheets).
