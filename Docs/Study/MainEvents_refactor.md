# Refactor study: `MainEvents` (week 2, Sunday: SOLID)

Original script: [MainEvents_before.cs.txt](MainEvents_before.cs.txt) — a 346-line "events" script from the VR game *Walker*, written by me before using any AI tools. It is stored as `.txt` so it is not compiled here; it depends on Walker classes (`SplineFollower`, `BatteryMeter`, …).

## What was wrong (before)

- **One class, many jobs (SRP):** picks avatar hands, tracks player speed, and runs five story moments (rock, mantis, bridge, trunk, horse), plus voice lines, subtitles, camera switches, animation and movement switching.
- **Copy-paste instead of data:** `BridgeTrigger1`–`7` are almost identical; the 14 bridge animators are separate fields (`BridgeAnime1`…`14`) and are reset by 28 near-identical lines.
- **Repeated patterns:** "set clip, `Play()`, set subtitle text" and "enable/disable player + Polaris + movement + battery" appear many times.
- **Depends on concrete classes (DIP):** the events script holds `SplineFollower`, `rigidSplineMovement`, `BatteryMeter` directly, so any change to how the player moves touches story code.
- **Style issues:** public fields everywhere, `== true`, unused `using` lines.

## Planned split (after)

| New class | One job | Replaces |
|---|---|---|
| `NarrationPlayer` | `Say(clip, text)`: voice line + subtitle | the repeated clip/Play/subtitle lines |
| `PlayerMovementSwitch` | `SetEnabled(bool)` for player, Polaris, movement, battery | the repeated enable/disable blocks |
| `BridgeCollapse` | collapse a segment, respawn, reset | `BridgeTrigger1`–`7`, two reset coroutines |
| `MantisEncounter` | the mantis sequence | `MantisTrigger`, `MantisTrigger2`, three coroutines |
| `HandsSelector` | male/female hands at start | `Start()` |
| `MainEvents` (small) | connect scene triggers to the classes above | what remains |

## Bridge: before and after

Before: seven methods like

```csharp
public void BridgeTrigger2()
{
    if (speed < BridgeCrossMin || speed > BridgeCrossMax)
    {
        BridgeAnime3.SetTrigger("T3");
        Break2.Play();
    }
}
```

After: the differences become data edited in the Inspector, and the code exists once.

```csharp
[System.Serializable]
public class BridgeSegment
{
    public Animator[] Animators;
    public string[] TriggerNames;
    public AudioSource[] BreakSounds;
}

[SerializeField] private BridgeSegment[] _segments;

public void CollapseSegment(int index)
{
    var segment = _segments[index];
    for (var i = 0; i < segment.Animators.Length; i++)
        segment.Animators[i].SetTrigger(segment.TriggerNames[i]);

    for (var i = 0; i < segment.BreakSounds.Length; i++)
        segment.BreakSounds[i].Play();
}
```

Seven methods become one; adding a segment needs no new code (pattern P7, data-driven configuration).

## Dependency inversion

`BridgeCollapse` only needs the player's **speed** (a value) and a way to freeze and restore movement. It receives a small `PlayerMovementSwitch` instead of `SplineFollower`, `rigidSplineMovement` and `BatteryMeter`. If the movement system changes, only `PlayerMovementSwitch` changes, and the bridge code stays as it is.

## Takeaways

1. Look for jobs that have nothing in common and split them.
2. Repeated code with small differences means the differences are data.
3. Depend on a small promise (or a tiny class), not on every concrete thing underneath.
