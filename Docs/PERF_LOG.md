# PERF_LOG.md — Profiling and optimization log

All measurements on the **test device** unless stated. Development build, Profiler attached, same level each time.

**Test device:** <model> · Android <version> · <RAM> GB · <GPU>
**Test scene:** <level / scenario, e.g. "Level 5, 20 moves with cascades">

## Budgets

| Metric | Budget |
|---|---|
| Frame time (CPU main thread) | ≤ 16.6 ms (60 fps) |
| GC Alloc per frame (gameplay) | 0 B |
| Draw calls / batches (gameplay) | ≤ 50 |
| Memory (total used) | ≤ <set in week 8> MB |
| Cold start to main menu | ≤ <set in week 8> s |
| AAB download size | ≤ <set in week 8> MB |

## Measurements

| Date | Change | Metric | Before | After | Notes |
|---|---|---|---|---|---|
| | Baseline | Frame time | | | |
| | Baseline | GC Alloc / frame | | | |
| | Baseline | Batches | | | |
| | Baseline | Memory | | | |
| | Baseline | Cold start | | | |
| | Baseline | Build size | | | |
