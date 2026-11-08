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

### Week 2 pool lab (`PoolLab`, Unity Editor Play mode, Profiler CPU Hierarchy, `PoolLab.Update` row, average per frame)

50 objects spawned and removed every frame from a simple quad prefab. Editor numbers, not device numbers; `Destroy` finishes later in the frame, so the whole-frame saving for the non-pool mode is likely larger than the `Update` row shows. Re-measure with the real gem prefab on the test device in week 8.

| Date | Change | Metric | Before | After | Notes |
|---|---|---|---|---|---|
| 2026-11-08 | `Instantiate`/`Destroy` → `ObjectPool<GameObject>` | GC Alloc / frame | 2.0 KB | 0 B | |
| 2026-11-08 | `Instantiate`/`Destroy` → `ObjectPool<GameObject>` | `PoolLab.Update` time | 0.55 ms | 0.25 ms | remaining cost is mostly `SetActive` on get/release |

### Week 1 study lab (`AllocationLab`, Unity Editor Play mode, Profiler GC Alloc column)

These are Editor numbers, not device numbers. Re-measure on the test device in week 8.

| Date | Change | Metric | Before | After | Notes |
|---|---|---|---|---|---|
| 2026-10-31 | Reuse one `List<int>` and `Clear()` instead of `new List<int>()` per call | GC Alloc / frame | 88 B | 0 B | |
| 2026-10-31 | Plain `for` loop with a counter instead of LINQ `Where().ToList()` with a captured variable | GC Alloc / frame | 284 B | 0 B | |
| 2026-10-31 | `TMP_Text.SetText("Frame: {0}", n)` instead of `text = "Frame: " + n` | GC Alloc / frame | 74 B (no label) | 98 B (unexplained) | Direct `SetText` calls measured about 0 B each; the 98 B in `AllocationLab.Update` is not explained. Re-check on device in week 8. |
