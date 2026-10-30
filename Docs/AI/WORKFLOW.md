# WORKFLOW.md — How Claude works in this project

Follow these steps in order for every task. They are written as a checklist on purpose.

---

## W1. Tools available

- **Unity CLI** (`unity`) — controls the open Editor through the Pipeline package (`com.unity.pipeline`).
  Check it is connected with `unity status` at the start of a session that needs the Editor.
- **Unity's Claude Code plugin** — `/unity:` skills (uGUI, sprite atlas, Input, Addressables, IAP, ads, URP 2D, …).
  Use the matching skill before writing code for that area, so the Unity 6.3 approach is used, not an older one.
- **Git** — local repo with LFS.

If you do not know the exact CLI command or flag, run `unity --help` / `unity <command> --help`
or read the CLI reference. **Never invent CLI commands or flags.**

## W2. Task loop

1. **Understand** — restate the task in one sentence. If something is ambiguous and expensive to redo, ask.
2. **Look first** — search the project for related code (R10). Note the files you will touch.
3. **Plan** — if more than 3 files change, post the plan and wait for OK (CLAUDE.md §5.2).
4. **Write tests first for Core logic** where practical (R6).
5. **Implement** in small steps.
6. **Verify** — run W3. Fix anything that fails before continuing.
7. **Report** — files changed, verification results, open items. Suggest a commit message (do not commit).

## W3. Verification (MUST run after every code change)

1. Let the Editor recompile; read the Console. Zero errors, zero new warnings.
2. Run EditMode tests. Run PlayMode tests if runtime code they cover changed.
3. If a scene or prefab changed: re-check the hierarchy against R4 and report differences.
4. If gameplay code changed: check the Profiler GC Alloc column in Play mode, or explain why it was not checked.
5. If a step could not be run (Editor closed, CLI not connected), say so plainly — never claim it passed.

## W4. Editing scenes, prefabs, and assets

- Prefer editing through the Editor (CLI / skills) over hand-editing `.unity`, `.prefab`, or `.asset` YAML.
- Hand-edit YAML only for trivial, well-understood changes, and say that you did.
- Never touch `.meta` GUIDs by hand. Never delete `.meta` files.
- Before large scene/prefab changes, remind me to commit first if the working tree has uncommitted work.

## W5. Git rules

- Never commit, push, or change branches unless asked.
- Commit message format (when asked):
  ```
  <type>(<area>): <short summary>

  <why, in 1–3 lines if needed>
  ```
  Types: `feat`, `fix`, `refactor`, `test`, `perf`, `chore`, `docs`. Areas: `board`, `ui`, `meta`, `save`, `sdk`, `build`, …
- Keep `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Build/`, `Builds/` out of Git (check `.gitignore`).
- Binary assets (`.png`, `.psd`, `.wav`, `.ogg`, `.mp3`, `.fbx`, `.ttf`, `.otf`) go through Git LFS (check `.gitattributes`).
- Project Settings: Asset Serialization = Force Text; Version Control = Visible Meta Files.

## W6. Docs to keep updated

| File | Update when |
|---|---|
| `Docs/DECISIONS.md` | A non-obvious technical choice is made (one short entry each) |
| `Docs/PERF_LOG.md` | Any profiling or optimization work (before/after numbers) |
| `CLAUDE.md` §4 | The current week/phase changes (I will tell you) |
| `README.md` | A feature milestone is complete (weeks 4–10) |

Propose doc updates in your report; make them when I agree.

## W7. When things go wrong

- Same error after 2 fix attempts → stop, explain what you tried and what you think the cause is, and ask.
- A Unity or package bug is suspected → say so, give the evidence, and suggest a workaround. Do not silently hack around it.
- Never disable tests, warnings, or checks to make something "pass".
