# Incremental Implementation Plan — Ventage Story

**Last updated:** 2026-11-08
**Stage:** Architecture plan only. **No C# source, compiled DLL, asset package, or successful in-game test yet.**

## Ground rules
- Continue following [AIMAIN.md](../AIMAIN.md), [AGENTS.md](../AGENTS.md), [PROJECT_MEMORY.md](PROJECT_MEMORY.md) and [DECISIONS.md](DECISIONS.md).
- The internal `modid` is a long-lived technical identifier: **user approved `lingamsdigestivesim` on 2026-11-08**. Public display title is still open, with creator-brand preference **Lingam's ...**. Preserve the distinction between ID, repo name and display title. Verify no public collision before release.
- Target Vintage Story 1.22.x and .NET 10. Use the **current official VintageStory.Mod.Templates** when available; the wiki's older tutorial says it was last verified for 1.19.8, so inspect generated files rather than copy an old template blindly.
- **One Windows PowerShell command at a time.** First establish SDK/template availability, then inspect generation commands; do not overwrite existing folders.
- Work on a separate dev world and use non-destructive feature flags where necessary. Never assume a planned feature is present until built and observed in game.
- Keep core mechanics separate from game API adapters. No speculative Harmony patches or unverified references to 1.22 assemblies.

## First action on development machine
1. **Completed:** installed `VintageStory.Mod.Templates@1.22.0`; `vsmod` and `vsmoddll` are available and `dotnet new vsmod --help` was inspected.
2. **Completed:** verified `%APPDATA%\\Vintagestory\\VintagestoryAPI.dll` exists. Exact full VS installation/version/reference configuration still needs confirmation when generated.
3. **Next:** inspect local clone location and working tree; ensure template output goes to a new folder and will not overwrite GitHub documentation.
4. Run a **dry-run** of the `vsmod` template, using internal Mod ID `lingamsdigestivesim`, while keeping the display title changeable. Then generate and inspect `.csproj`, `modinfo.json`, `assets/<modid>`, `ModSystem` and build/run/packaging targets.
5. Compile and load in a test world before building custom gameplay code.

Official setup guide: https://wiki.vintagestory.at/Modding:Setting_up_your_Development_Environment
Official examples (beware version drift): https://github.com/anegostudios/vsmodexamples

## Digestion/time model checkpoint — 2026-11-08

**Updated user scope:** stomach = progressive **time-buffer/transit phase**, **no first-release burping/upper-gas sounds or mechanics**. Keep it extensible. One food consumption event creates its **own portion- and ingredient-aware digestive contribution**; multiple meals overlap. Use `api.World.Calendar.TotalHours` as the world-time source and calculate elapsed game hours, so custom day lengths/speeds are honored automatically (default 48 real min/day). Don't tie to vanilla satiety loss or reset when another meal is eaten. Gut fermentation after stomach+extra lag gradually adds gas. Suggested gastric 2–4 game h and overall fermentation onset 5–8+ game h are **illustrative placeholders**, not approved balancing. Test time speed, sleep/time skips, unload, additive meal overlap, and no backlogged sound replay. See `PROJECT_MEMORY.md` and D39–D40.

**Additional rolling-queue acceptance cases (2026-11-08):** First install at positive vanilla satiety but no known food history (proposed no fake historical meals); 16 rapidly eaten carrots may group into equivalent counted batch; mixed ingredients are independently timestamped; satiety cap must not suppress actually consumed food; gastric transfer is gradual; intestinal gas accrues over per-food finite production windows; per-food expiry removes source **without deleting accrued intestinal gas**; distinct fart cooldowns; time skips and saved/reloaded partially processed meals preserve correct future potential. User's idea is conceptual—do not build a giant per-item list or lock numeric timing prematurely.

## Intended logical responsibilities (folders can evolve with the template)
- `src/Core/`: minimal `ModSystem`, service registration/lifecycle, feature flags, logging, native config.
- `src/Simulation/`: **pure C#** logic: digestion contributions, gas generation over game time, upper/lower reservoirs, cooldown decisions and release events. Keep unit-testable without needing the game to run.
- `src/Entities/`: adapter for attached persistent state, life/death/unload callbacks, species selection and runtime tick. The precise game API hooks must be verified before implementation.
- `src/Foods/`: food/ingredient/portion resolver and data-driven profiles. Handle raw/cooked/meal item contents where the installed game exposes them. Neutral/relief foods are open research.
- `src/Creatures/`: data-driven species registry, optional third-party profiles, export and debug inspection.
- `src/Audio/`: later, audible sound category selection and client positional playback / concurrency controls.
- `src/Lore/`: later, journal discovery and loot hook; favor existing vanilla readable item types with embedded per-ItemStack `title` and `text`, pending verified persistence test.
- `assets/<modid>/`: JSON profiles, translations, later audio and lore data. Need not pre-create every folder.
- `tests/`: deterministic unit tests for gas arithmetic and meal overlap if convenient (no world-file dependence).

**Avoid giant empty frameworks.** Each module should be introduced as its first working requirement needs it.

## Workable implementation checkpoints

| Stage | Implement | Evidence required before next stage |
| --- | --- | --- |
| 0 — bootstrap | Official .NET 10 project shell, neutral mod ID, correct `modinfo.json`, clean startup logger | DLL compiles, mod listed in Manager, test world loads with no error; reproducible build instructions committed |
| 1 — diagnostics/config | JSON config with version and safe defaults; a single status/debug command | Config round trip and command work with a clean restart |
| 2 — entity persistence | Attach a tiny versioned per-entity state containing zero upper/lower gas to **one** goat (after verifying API) | Save, unload, restart and inspect same entity state; no vanilla behavior changes |
| 3 — deterministic digestion | Reusable pure code for meal contributions, elapsed game time, separate reservoirs and release consumption | Unit tests cover 0 gas, two overlapping meals, long time skip, caps, save/reload and no lost future potential |
| 4 — intake and food | Start with one consumed test food, then multiple ingredients, recipes and partial servings | Observe actual intake amounts without suppressing vanilla feeding or nutrition |
| 5 — ordinary release | Rare reservoir-dependent fart/belch decisions; baseline gas; cooldowns; admin inspection | An eligible animal can release gas, empty cannot, no forced events, and offscreen time skips never replay old sounds |
| 6 — audio and species | Shared temporary sounds, positional hearing, classification, modded animal profiles and creature exporter | Verify with 1 then ~40 goats in survival and creative, no endless noise; exported profiles are privacy-safe |
| 7 — first-release lore | **Hello World** vanilla written book/scroll with embedded text; rare bonus loot, journal behavior, story entries | Save/remove-mod/trade tests pass on a **copied test world**; vanilla lore chances not reduced |
| 8 — release hardening | Polish audio, credits, test matrix, README, ModDB ZIP and source release | Build, restart, multiplayer, add/remove mod and assets validated; documentation committed |

**Optional/later:** hunger stomach gurgles, fertilizer/dung, advanced sound libraries, bear hind-leg posture influence (~1s delayed optional opportunity), naturally emerging animal startle reactions. No scripted goat mayhem or separate cascade feature.

## One-turn next practical step
**Template already installed and Mod ID approved.** Next inspect the user's local clone/current working directory before generating any files. Then dry-run `vsmod` with verified parameters and a new safe output path. Continue with exactly one PowerShell command at a time.

## Documentation checkpoints
On successful code milestones or major decisions, update this document and `ROADMAP.md`, and relevant `PROJECT_MEMORY.md`/`DECISIONS.md` entries. Commit to GitHub and verify before reporting the project stage as complete.
