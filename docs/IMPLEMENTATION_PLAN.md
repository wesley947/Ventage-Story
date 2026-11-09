## Latest implementation checkpoint — 2026-11-08 17:08 PST

The older 'no code built' / template bootstrap and two-reservoir stages below are historical and **NOT current instructions**. Actual verified implementation: Vintage Story mod shell and JSON registry load 195 food profiles, 13 composite rules, 6 exclusions, and 158 native bindings. DEBUG server-side Harmony EAT-PROBE successfully logged three real direct food consumptions (violet web cap F093, rye F055, spelt F058), each one native unit with satiety increase. Nothing yet persists intake, processes elapsed-game-time gas production, stores intestinal gas, releases gas or plays sounds. Next test: persist/reload one tiny namespaced entity attribute. D53 disallows stomach compartments, upper-gas reservoirs, burps and progressive gastric transfer; stomach is just delay, followed by gradual intestinal gas production into one conserved persistent reservoir. D44 uses sliding 30-second **inactivity** sessions with meal timestamp at closure. The old stage table can provide test ideas but is not an implementation specification where it conflicts with D44/D53/D55. Avoid touching vanilla item registration; previous injected behavior caused a crash.

---

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

## Current accepted digestion simplification — no gastric transit simulation

**User clarification (2026-11-08) overrides older sections below:** Stomach means a **single configurable wait/delay** from completed meal timestamp until the ingredient can generate **intestinal gas**. Do **not** model progressive gastric emptying, a stomach/upper gas reservoir, burping, separate storage compartments or extra scaffolding for future burps. Each food in a finalized mixed-food session retains individual amount/profile, digestive-delay game-hours, gas potential and production-duration game-hours; once delay ends, its potential converts gradually into **one persistent intestinal reservoir**. Use `Calendar.TotalHours`; calculate *newly produced amount* from cumulative progress to avoid double counting on sleep/load; expire source contributions only after they finish generating. Independent animal baseline adds directly to reservoir, without imaginary food history. Flatulence is the sole way to subtract gas. **Stomach delay and intestinal production are two fields/intervals, not two simulation subsystems.** All old references to progressive stomach transit/upper/lower storage are superseded.

## Digestion/time model checkpoint — 2026-11-08

**Updated user scope:** stomach = progressive **time-buffer/transit phase**, **no first-release burping/upper-gas sounds or mechanics**. Keep it extensible. One food consumption event creates its **own portion- and ingredient-aware digestive contribution**; multiple meals overlap. Use `api.World.Calendar.TotalHours` as the world-time source and calculate elapsed game hours, so custom day lengths/speeds are honored automatically (default 48 real min/day). Don't tie to vanilla satiety loss or reset when another meal is eaten. Gut fermentation after stomach+extra lag gradually adds gas. Suggested gastric 2–4 game h and overall fermentation onset 5–8+ game h are **illustrative placeholders**, not approved balancing. Test time speed, sleep/time skips, unload, additive meal overlap, and no backlogged sound replay. See `PROJECT_MEMORY.md` and D39–D40.

**Revised intake-window design (2026-11-08):** User prefers a **sliding 30-real-second inactivity timer**: each successfully eaten bite refreshes the session deadline; after 30 seconds of inactivity, finalize a composite meal with a **stomach entry time at closure**, not first bite. Keep exact food identity and quantities, not an averaged profile. A long uninterrupted snacking session can delay earlier bites; an optional hard session cap was proposed by assistant but is **not agreed**. Preserve pending meal across save/restart and never lose consumed food. The older fixed-window proposal below is **superseded** unless user later reverses.

**Fixed ingestion-window test proposal:** User suggests a 30-second real-time session from first eaten item; new bites within interval accumulate distinct food identities/amounts, including alternating carrot and peanut, and are finalized as a compact batch at deadline. Track each actual bite's game-hour stamp immediately so finalization doesn't pause stomach transit. If 16 carrots exceed the window, multiple batches are correct. Test pending-window save/restart, food profile differences and cap satiety. Real-time window and game-hour physiological scheduling should be separate. **Tentative design, not yet implemented.**

**Additional rolling-queue acceptance cases (2026-11-08):** First install at positive vanilla satiety but no known food history (proposed no fake historical meals); 16 rapidly eaten carrots may group into equivalent counted batch; mixed ingredients are independently timestamped; satiety cap must not suppress actually consumed food; gastric transfer is gradual; intestinal gas accrues over per-food finite production windows; per-food expiry removes source **without deleting accrued intestinal gas**; distinct fart cooldowns; time skips and saved/reloaded partially processed meals preserve correct future potential. User's idea is conceptual—do not build a giant per-item list or lock numeric timing prematurely.

**Player/animal split — 2026-11-08:** Players use sliding 30-real-second meal sessions; animal feeding contributions are attached when an actual food event can be identified, with no need for a per-animal bite-session timer. Vanilla penned animals never starve without eating, so add subtle **species-specific background gut-gas accrual** independent of any fictional food record, supporting rare natural releases even when no feeding is observed. Feeding event creates extra delayed, food-dependent potential; same shared reservoir/release engine for both species but differentiated profiles. Verify animal food-event metadata and compatibility with hunger/husbandry mods. Stress-test ~40 penned goats without a nonstop audio chorus; no backlog sounds on reconnect/unload. No burps in first release.

**Overpressure relief target (2026-11-08):** A species-adjusted **normal full** reservoir level is 100%, but not a hard clamp. Above 100% the probability of a significant natural release ramps steeply toward near certainty by about **125%**, and an overpressure fart releases **at least half of the currently stored gas**, potentially more. As a potential strict safety gate, at 125% schedule/force an *actual* relief event, not an invisible clamp; a normal event cooldown may yield to emergency pressure. Design probability in time-based evaluated opportunities (not FPS), ensure no release from zero, persist accumulation even during cooldown, and account for unloaded time without replaying many past sounds. Explicit curve and cap behavior TBD.

**Gas conservation — explicit 2026-11-08 requirement:** All produced gas is stored persistently for players and animals; no release or passage of time means no gas loss. Only an actual fart can subtract the specific released amount; release cooldowns must not affect production. A soft pressure 'capacity' threshold can influence actual releases, **not silently cap or discard stored gas**. Replace any previous proposal to absorb existing reservoir gas with at most reducing *future* generation. Test conservation at high pressure, while release is on cooldown, after food-production expiry, and across save/unload/time skips. Shared arithmetic/invariants belong in a small independently testable simulation layer.

**Natural and action-triggered fart roll checkpoint (2026-11-08):** Once a player's or animal's real reservoir is around a tunable ~25% full, permit incidental natural releases when idle/walking and additional **debounced** opportunity checks on meaningful actions (jump/land, crouch/stand, sprint/fear). Do not roll per frame or per animation step. Start with perhaps 15-second active simulation checks with species and pressure dependent probabilities; calibrate rate for 40-goat pen. Success subtracts only the chosen released amount and emits its event; failure subtracts nothing. Existing intentional crouch hold thresholds must not get duplicate random triggers. Release cooldown blocks some opportunities but never slows gas creation; emergency actual flatus near 125% can bypass it. Digestion remains in calendar hours; opportunity checks may use real elapsed active seconds. All numeric probabilities and sizes are experimental until user listening/game testing.

**Persistence/RNG performance checkpoint (2026-11-08):** New minimum functionality requires **reliable reload of player and animal gas amounts, digestive batches, last processed `Calendar.TotalHours`, and unfinished player 30-second intake window**, plus no repeated gas generation or lost gas after save/reload. API `Entity.Attributes` is permanently saved server-side by `Entity.ToBytes` (source verified, local gameplay verification pending), while `WatchedAttributes` also sync to clients. Prefer one namespaced versioned entity subtree for private state, never broadcasting complete food queues. Ordinary random fart opportunities: cheap per-actor 15–30 second active-time staggered checks, per-minute pressure-dependent probabilities with interval conversion, extra debounced movement events; no per-frame rolls. Proposed 1%/3%/6%/15%/35% per minute at 25/50/75/100/115% and emergency at 125% are **not approved**. Tests on ~40 goats, baseline and post-restart. Pause/offline timer behavior and crash-before-save limitations require documentation.

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
