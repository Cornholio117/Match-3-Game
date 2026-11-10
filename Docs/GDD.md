# Game Design Doc: Match-3 (working title)

One page. Decisions made in week 3 (Tuesday). Numbers are starting values; all of them live in config assets and are meant to be tuned.

## Pitch
A fast, satisfying 2D match-3 for Android phones in the spirit of Bejeweled: swap, match, cascade. Short levels, limited moves, one booster and one special piece. The goal is a game that feels good in a 30-second video, with clean, tested architecture behind it.

**Platform:** Android first (iOS-ready code), portrait, one-handed play. **Art:** gem sprites provided by the author (7 designs, 5 will be used).

## Core loop
1. Swap two neighbouring gems.
2. If a row or column of 3+ matches, they clear; gems above fall, new ones fill in; new matches clear in a cascade.
3. Score goes up, moves go down. Reach the level goal before moves run out.
4. Win: earn stars and coins, spend coins on a booster, go to the next level.

## Board and pieces
- 8×8 board. 4 colours in the early levels, the 5th colour arrives later (difficulty ramp).
- Invalid swaps (no match) are rejected. The board never starts with matches and is reshuffled if no move is possible.

## Special piece and combo
- **Line gem:** a 4-match creates a gem that, when matched or swapped, clears its **row** (horizontal 4-match) or **column** (vertical 4-match). A small arrow shows its direction.
- **Combo:** swapping two line gems together clears a row **and** a column at once.

## Booster
- **Hammer (plus shape):** tap a gem to clear it and its 4 neighbours. Costs 50 coins; at most one use per level. Size and price are config values.

## Levels and goals
- 20 levels, data-driven (`LevelConfig` ScriptableObject per level: moves, target, goal type).
- Goal types: reach a target **score**, or **collect N pieces** of a colour.
- Ramp: fewer moves and higher targets over time; levels 1 to 5 use 4 colours, the rest use 5. Starting guess: ~25 moves early, ~15 moves late.

## Progression and economy
| Part | Rule |
|---|---|
| Stars | 1 star at the target score, 2 at 1.5× the target, 3 at 2× |
| Coins earned | 10 per star (10 to 30 per level) |
| Hammer | 50 coins |
| Meta | level-select map, stars and coins saved between sessions |

## Core risks and how the design handles them
| Problem | Prevention |
|---|---|
| Matches on the starting board | Seeded fill that avoids starting matches, tested |
| No valid moves | Check after each turn; reshuffle if none |
| Randomness you can't repeat | Seeded `IRandom`, replayable levels |
| Logic tangled with animation | Model resolves instantly and returns steps; view plays them; input blocked while animating |
| Endless cascades | Cascade limit and tests over many seeded boards |
| Difficulty you can't tune | All numbers in config assets |
| Mid-level state can't be saved | Board and score are plain data in the model |
| Stutter on phones | Pooling, zero per-frame allocations, measured in `PERF_LOG.md` |

## Scope cut list (not in v1)
- Timed mode in the style of Bejeweled Blitz (stretch goal only).
- More special pieces, such as a 5-match gem.
- Multiplayer, accounts, server backend, localization.
- Real-money purchases (a coin pack is a possible later idea).

## Open items
- Choose which 5 of the 7 gem designs to use, and whether to use the glow versions.
- Confirm the art's licence before publishing to Google Play.
