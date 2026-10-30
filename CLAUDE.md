# CLAUDE.md — Mobile 2D Match-3 (Unity 6.3 LTS)

Read this file fully at the start of every session. Then read the four files imported below.
They are rules, not suggestions. If a request conflicts with them, say so and ask before proceeding.

@Docs/AI/RULES.md
@Docs/AI/PATTERNS.md
@Docs/AI/EXAMPLES.md
@Docs/AI/WORKFLOW.md

---

## 1. What this project is

- A small **2D match-3 mobile game** for Android (iOS-ready code, Android builds first).
- Purpose: a **portfolio project** that proves production-quality Unity skills to mobile game studios
  (clean C# architecture, unit tests, uGUI, Addressables, SDK integration, measured optimization, store release).
- Code quality and clarity matter more than feature count. Every system may be shown in a job interview.

## 2. Tech stack (fixed — do not change without asking)

| Area | Choice |
|---|---|
| Engine | Unity 6.3 LTS |
| Render pipeline | URP, 2D Renderer (Universal 2D template) |
| Language | C# (Unity 6 default .NET / C# version) |
| Input | Input System package, EnhancedTouch for touch |
| UI | uGUI + TextMeshPro (not UI Toolkit for runtime UI) |
| Async | `Awaitable` / `async` (Unity 6 built-in). Coroutines only for simple visual sequences |
| Content | Addressables |
| Tests | Unity Test Framework (EditMode for logic, PlayMode for integration) |
| Source control | Git + Git LFS, hosted on GitHub (public repo) |
| AI tooling | Claude Code + Unity CLI + Unity's official Claude Code plugin (`/unity:` skills) |

Do **not** add third-party packages (DOTween, UniTask, VContainer, Zenject, Odin, etc.) without asking first.
SDKs planned for later weeks (analytics, ads mediation, IAP, crash reporting, remote config) are added only when the task says so.

## 3. Game scope (keep it small)

- Board: 8×8 grid, 5 basic piece colors.
- Core: swap → match (3+ in row/column) → clear → gravity → refill → cascade.
- One special piece (line clear from a 4-match) + one combo.
- Levels: data-driven, target ~20 levels. Goals = score or collect N pieces; limited moves.
- Meta: coins, stars per level, level-select map, one purchasable booster.
- Out of scope unless asked: multiplayer, accounts, server backend, localization, 3D, DOTS/ECS.

## 4. Current phase

Update this line when the phase changes:

**Current week: 1 — C# foundations.** The project skeleton (folders, asmdefs, Git, Boot scene) is set up now;
gameplay code starts in week 4. Until then, tasks are study exercises and setup only.

Weekly focus:
1–2 C# + patterns · 3 setup · 4 core gameplay · 5 loop + uGUI · 6 save/economy/Addressables ·
7 SDKs · 8 optimization · 9 ship to Google Play · 10 portfolio · 11 interview prep · 12 apply.

## 5. How to work with me (the developer)

1. I am an experienced Unity/XR developer and technical artist moving into mobile. Explain *why* for any
   non-obvious mobile-specific decision in one or two sentences — I want to learn, not just receive code.
2. Before a change that touches more than 3 files, post a short plan (files + one line each) and wait for OK.
3. Keep changes small and focused. One feature or fix per change.
4. Never delete assets, scenes, or prefabs without asking.
5. Never commit or push unless I ask. When I ask, follow the commit rules in WORKFLOW.md.
6. If you are unsure whether an API exists in Unity 6.3, check the docs or the `/unity:` skills — do not guess.
7. When a task is done, finish with: what changed (files), how it was verified, and anything left open.
