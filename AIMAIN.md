## Current code checkpoint — trumpet prototype committed, NOT TESTED (2026-11-08 evening)

GitHub commit `3654088` updates ONLY `mod/lingamsdigestivesim/src/Simulation/PlayerGasStateModSystem.cs`. It adds admin-only `/gas sound` (dispatches vanilla built-in `game:sounds/voice/trumpet` from player position, with randomized pitch 0.86–1.14, volume 0.85, 20-block attenuation range; **does not consume gas**) and `/gas fart` (only when gas>0, dispatches built-in trumpet and **consumes 60% of currently stored gas**, purely provisional manual test; deeper pitch at >=100% pressure). Empty gas is silent. Uses server `World.PlaySoundAt(AssetLocation, Entity, null, pitch, range, volume)` and leaves digestion, assets, and other systems unchanged. Sound dispatch is not proof a player actually hears audio; runtime audition pending. **Not yet pulled, built, installed, or tested by the user.** Next user step: `git -C "$HOME\Ventage-Story" pull --ff-only`; then compile in Debug with existing .NET 10 command, back up installed working accumulator DLL before copying; test `/gas sound` first, verify gas unchanged, only then test `/gas fart`. One Windows PowerShell command per response. The last verified running test world had 121% gas with 0 active food sources. Preserve previous accumulator DLL for rollback.

---

## Sound manipulation design — Vintage Story 1.22.7 built-in API (2026-11-08)

**User reminder:** Vintage Story already has native sound manipulation; do not treat the built-in trumpet as one immutable recording. Official API 1.22.7 supports `IWorldAccessor.PlaySoundAt` with explicit or randomized **pitch**, **volume** and positional **range**, and `SoundAttributes` supports random pitch/volume. On client, `IClientWorldAccessor.LoadSound(SoundParams)` and `ILoadedSound` expose ongoing `SetPitch`, `SetVolume`, `SetPosition`, `SetLowPassfiltering`, `SetReverb`, fades, and playback controls. Keep the first test simple: use `game:sounds/voice/trumpet` (existing file `assets/game/sounds/voice/trumpet.ogg`) with pitch/volume/range variants; prefer server positional `PlaySoundAt` for shared player audibility, consider client-controlled advanced effects only later with explicit networking/testing. Potentially vary perceived size, distance, intensity using these parameters, not duplicate audio assets. **No sound code built or tested yet**. API references: https://apidocs.vintagestory.at/api/Vintagestory.API.Common.IWorldAccessor.html , https://apidocs.vintagestory.at/api/Vintagestory.API.Client.ILoadedSound.html , https://apidocs.vintagestory.at/api/Vintagestory.API.Client.SoundParams.html . Exact sound asset location still needs runtime verification.

---

## Existing vanilla instrument sounds for first release test — 2026-11-08

User reminds us that **Vintage Story 1.22.7 already ships instrument audio samples** under `assets/game/sounds/voice/`: `accordion.ogg`, `altoflute.ogg`, `clarinet.ogg`, `harmonica.ogg`, `oboe.ogg`, `sax.ogg`, `trumpet.ogg`, `tuba.ogg`. Do not invent an external audio-recording requirement! For the **next manual release/sound-only test**, the user explicitly selected **the vanilla trumpet**: target asset location `game:sounds/voice/trumpet` (runtime play API still must be tested). Provisional category mapping from prior discussion: squeakers=altoflute/clarinet/oboe; loose=harmonica/tuba; loud=sax/trumpet/accordion. These category mappings are suggestions, not final sounds. This is a **sound selection**, not a compiled feature; gas release/sound code not yet written or verified. Ensure server authoritative gas subtraction only on real release, but sound-only admin diagnostics never subtract gas.

---

## VERIFIED: no gas after finished digestion — 2026-11-08 evening PST

**USER-VERIFIED FINAL ACCUMULATOR CHECK:** After a two-food active digestion test finished at **121.00%** gas with **zero pending or active contributions**, the user entered `/time add 2` (two further game hours) in the same Creative world and reported `/gas status` was **unchanged**. The completed food sources did not produce new gas after their retirement; accumulated gas remained stored, including the amount over 100% normal-full. Combined with earlier direct-food EAT-PROBE tests, pending meal closure and save/restart, partial digestion/restart, and full production, this is a **successful narrow player-direct-food accumulator validation**. Not covered: repeated-bite reset edge case, exact food-to-gas attribution, meals/liquids, animal production, player switching/multiplayer, mod disable/re-enable world safety. **Next gameplay milestone:** actual gas-consuming manual release plus simple audible placeholder fart sound and separate non-consuming sound diagnostics, followed by intentional crouch behavior; ensure no subtraction unless a genuine release occurs. Preserve one-step PowerShell workflow and tested DLL backup.

---

## Verified completion and 100% overflow — 2026-11-08 evening PST

**IN-GAME PASS (user screenshot):** After the restored partly digested two-food session reached **69.47% gas with 2 active sources**, user entered `/time add 8`. Game reported eight game hours added. `/gas status` then reported **121.00% (121.00 / 100.00 units)** and **"No pending or active foods."** Delta was **+51.53 units** after those 8 hours; total meal gain from pre-meal 15.40% was **+105.60 gas units**. This confirms current build can finish overlapping active contributions, prune completed records, and preserve gas above the 100% normal-full soft threshold (no silent clamping). No gas release, sound, or animal behavior is present yet. Next precise test: advance 2 more game hours *without eating* and verify gas stays at 121.00%, pending and active remain zero (ensure finished sources do not keep producing). Then evaluate when to implement the player release/sound gameplay. The exact food types/quantities behind the two contributions are not individually verified.

---

## Verified continued digestion after reload — 2026-11-08 evening PST

**USER-VERIFIED:** After the previous partial-digestion save/exit/restart test at **46.28% gas** with **two active contributions**, the user entered `/time add 2` in the same Creative test world. Vintage Story reported two game hours added, and `/gas status` then displayed **69.47% (69.47 / 100 units)** with **two active contributions, zero pending**. Delta = **+23.19 gas units**. Thus saved, partly completed digestion remains live and continues accumulating gas after a restart; contributions were not stalled. The exact per-food split is unverified. **Next test:** allow enough additional game hours for both contributions to complete, verify they are removed and gas does not increase further without eating; test overflow over 100% if reached naturally, as gas must not be silently clamped. Prior EAT-PROBE successful intake and initial/zero/pending/completed meal persistence remain established. One command per turn.

---

## Verified partial-digestion restart — 2026-11-08 evening PST

**PASS — real in-game screenshot:** With pending cabbage previously transferred to active digestion, the user used Vintage Story's `/time add 7` (integer hours; `7:00` is rejected), then `/gas status` showed **45.18% (45.18 / 100)** and **2 active food contributions, 0 pending**. User saved and fully restarted the game, then status showed **46.28% (46.28 / 100)** and **the same 2 active food contributions**. Mod retained the existing reservoir and live food progress across a full restart, with a small +1.10 gas delta plausibly attributable to natural game-time progression; no obvious replay/double-credit spike. This is a *partial-digestion* restart smoke test, separate from previously verified fully finished meal persistence and pending-window persistence. Do **not** infer exact cumulative accounting across restart solely from these two readings. The user had eaten multiple cabbages and two contributions are visible; exact per-food quantities, ingredient identity, and source of the 1.10 delta were not isolated. **Next test:** in the dedicated creative world, advance a modest further increment of calendar hours while 2 foods remain active, check gas increases appropriately and completion eventually prunes sources; optionally verify double-credit invariants with diagnostic breakdown later. One PowerShell or in-game command at a time.

---

## Verified pending meal save/restart — 2026-11-08 20:10 PST

**USER-VERIFIED IN-GAME:** Starting with previously earned **15.40% gas**, user ate **one cabbage** and immediately saved/exited Vintage Story, then fully restarted the same Creative test world. `/gas status` after restart showed **Gas: 15.40%** and **Pending: 1 food type(s), 16s remaining; active: 0 food contribution(s)**. Thus an incomplete 30-real-second player eating session is preserved across save/restart along with the accumulated reservoir; it is not prematurely activated and the saved remaining countdown is not reset to 30. This is separate from the earlier confirmed complete-meal restart test. Does not yet verify exact offline countdown pause semantics or active partial-production persistence. Next: stop eating, allow more than 16 real seconds with game active, then `/gas status` should show 0 pending and 1 active cabbage contribution; subsequently sleep for a partial-production test.

---

## Verified completion-state restart persistence — 2026-11-08/09

**USER VERIFIED:** Following successful completion of the two-food digestion test producing **15.40% gas**, the player saved the test world, completely exited and restarted Vintage Story, and reported that `/gas status` remained as expected (15.40% and no pending/active foods). This confirms retention of **completed digestion gas** after restart without observed replay in this test. It does **not** yet verify restart of a pending 30-second meal or a **partially produced active** food contribution, nor does it prove repeat-bite timer reset, cross-player behavior or animals. **Next test:** eat a known bound single cabbage (`game:vegetable-cabbage`, F005, 4.8 game-hour delay plus 9 game-hour linear production), close the 30s window, then advance ~7–9 game-hours via bed sleep to create partially produced gas before save/exit/restart. First confirm cabbage ingestion is observed. Compare status and gas before/after restart to rule out double-credit. Preserve one-step instructions.

---

## Verified first automatic gas generation — 2026-11-08/09 (user test)

**IN-GAME PASS (dedicated Creative test world, .NET 10 Debug build):** User fast-forward pulled through `cea83d1`, compiled successfully (no warnings or errors reported), backed up previous persistence DLL, copied new DLL and verified compiled/installed SHA256 hashes matched. Vintage Story loaded; existing gas state stayed at **0.00%**; `/gas status` showed `No pending or active foods.` After eating **two different recognized direct foods** (user mentioned spelt and cooked legumes among food eaten), status showed `Pending: 2 food type(s), 23s remaining; active: 0 food contribution(s)`. After 30s inactivity status showed `Pending: 0 ... active: 2 food contribution(s)` with gas still 0.00%. After **sleeping to advance the game calendar**, status showed **Gas: 15.40% (15.40 / 100.00 units)** and `No pending or active foods.` This verifies a full actual **food detection → meal closure → game-time gas accrual → completion** cycle in one test world. **NOT YET VERIFIED:** exact per-food quantity/contribution accounting, timer reset by a subsequent bite, partial-production steps, saved pending/active state after restart, idempotency after restart, disconnected-player behavior, animal digestion, release or sounds. Do not call those passed.

**NEXT TEST:** save and fully restart the world at 15.40%, then `/gas status` to verify no disappearance or duplicate production; subsequently test a new meal with partial digestion and restart. Keep prior gas-persistence and EAT-PROBE safe backups. One instruction at a time.

---

## Current engineering checkpoint — new digestive loop SOURCE ONLY (2026-11-08)

**NOT YET COMPILED, INSTALLED OR GAME-TESTED:** New server-only `mod/lingamsdigestivesim/src/Simulation/PlayerDigestionModSystem.cs` was committed, and the known-good `DirectFoodIntakeHarmonyProbe.cs` was minimally extended with a success-only notification event. `PlayerGasStateModSystem.cs` now displays pending/active food counts in `/gas status`. In DEBUG only, a completed directly observed vanilla food bite queues its actual verified profile, resets that player's countdown to **30 real seconds** (never adds 30), and pending food is stored in the existing versioned player entity subtree. On 30 seconds of eating inactivity, grouped individual foods become active at the current world game hour. Each active food has a delay/production duration and stored `ProducedSoFar`; server 1-second ticks calculate only the positive unaccounted delta into the **same** persistent single gas reservoir (never clamp at 100%). Saving, disconnecting and reconnecting preserve pending countdown and active progress; the timer does not count disconnected time. This behavior is a test candidate, NOT yet verified. Animals, cooked meals, liquids, release, sound and UI are still not implemented.

**Immediate next step:** user pulls GitHub `main`, compiles their existing untracked local project, shares warnings/errors. Do not deploy to main world until the build and dedicated creative-world test gates pass. Preserve existing EAT-PROBE backup and test one command at a time. Verify 30s reset, active meal closure, gas rise after game-time delay, restart at partial completion, and no duplicate gas before considering this milestone complete.

---
# AIMAIN.md — AI Entry Point

> **For any ChatGPT or coding assistant continuing this project: read this file first.** The user should only need to say: "Read AIMAIN.md in my Ventage-Story GitHub repo and continue where we left off."

## Project identification

- Repository: `wesley947/Ventage-Story` (repository spelling is intentional; do not rename it without asking).
- Project: a Vintage Story digestive-gas simulation mod, with **Lingam's** as the intended public naming/creator brand. **Public display title remains undecided.** Previous names included *Cabbage Carnage* and *Ventage Story*.
- Target: Vintage Story 1.22.x, C#/.NET 10.
- Current stage (2026-11-08 evening PST): **PLAYER GAS PERSISTENCE VERIFIED IN GAME.** New server-only `src/Simulation/PlayerGasStateModSystem.cs` compiled successfully and installed with matching DLL hashes; the user's creative test world loaded. `/gas status` initialized a first-time player at 15.19% (within approved 15–20% range), repeated status did not reroll, `/gas set 63` survived a complete save/exit/restart at exactly 63.00%, and `/gas set 0` also survived a complete restart at exactly 0.00% (no accidental reinitialization). Version 1 per-player server entity Attributes and admin-only `/gas status`/`/gas set` work for this tested player. This does NOT yet verify multiple players, animals, uninstall/reinstall compatibility, active meal persistence, digestion, releases, sounds or saved world copies. DEBUG direct-food EAT-PROBE still verified for violet web cap F093, rye F055 and spelt F058. Next: a minimal real gas-release and sound test, then crouch behavior; food-dependent replenishment comes afterward. Repo is `wesley947/Ventage-Story`, internal Mod ID `lingamsdigestivesim`.
- Keep permanent technical identifiers based on approved `lingamsdigestivesim`, not on any provisional public display name. The public name may change without changing saved-data namespaces or item codes.

## On every new chat

1. Open this repository via the GitHub connector. Check the default branch and real file contents, rather than trusting an old summary.
2. Read `AGENTS.md` if present: coding and collaboration rules.
3. Read `docs/PROJECT_MEMORY.md` if present: current architecture, technical context, and major requirements.
4. Read `docs/DECISIONS.md` if present: decisions and their rationales, including reversals.
5. Read `docs/ROADMAP.md` if present: completed work, next milestone, blockers, and deferred features. Consult `docs/IMPLEMENTATION_PLAN.md` when generating or extending code.
6. Read `docs/CREATURE_EXPORT_SPEC.md` when working on modded animals, diagnostics, data exports, or community support requests.
7. Read `docs/LORE_INDEX.md` when writing, integrating, or reviewing lore; read `docs/SOUND_CREDITS.md` before including third-party recordings.
8. For food work, read `docs/FOOD_CODE_AUDIT_REVIEW.md`, `docs/FOOD_REGISTRY_INTEGRATION.md` and `docs/FOOD_FRAMEWORK.md`, then inspect `mod/lingamsdigestivesim/assets/lingamsdigestivesim/config/foodprofiles.json` and the CSV master in `data/`.
9. Inspect actual relevant source/data files before changes. If any referenced document is missing, say so; never invent its contents.
10. Summarize current state and the **single next practical step**. Do not make the user reconstruct the project from memory.

## Documentation map

| File | Purpose |
| --- | --- |
| `AIMAIN.md` | Permanent AI entry point and documentation map; keep short and trustworthy. |
| `AGENTS.md` | Instructions for AI collaborators: coding style, test workflow, small edits, one PowerShell command at a time. |
| `docs/PROJECT_MEMORY.md` | Durable project specification, game/API findings, architecture, persistence, food and species model. |
| `docs/DECISIONS.md` | Dated decisions, especially those changing earlier design assumptions. |
| `docs/ROADMAP.md` | What is done, what is next, blockers, and feature-creep backlog. |
| `docs/IMPLEMENTATION_PLAN.md` | Step-by-step construction and test gates for the first working .NET 10 code-mod skeleton and later features. |
| `docs/LORE_INDEX.md` | Great Silence chronology, writers, discovery types, journal sections, planned and implemented lore entries. |
| `docs/CREATURE_EXPORT_SPEC.md` | Design for opt-in, privacy-safe export of modded creature metadata to support GitHub issue requests without chasing animals. |
| `docs/SOUND_CREDITS.md` | Eventually record sound sources, creators, licenses, and edits. |

**Documentation status (2026-11-08):** All files listed in this table have been created and committed to GitHub. The C# mod shell, food catalog, direct-food diagnostic and **basic persistent player gas reservoir with admin testing commands** have been implemented and tested. Food-driven digestion, live gas generation, releases, animal reservoirs and audio are not yet implemented.

## Current essential design decisions (bootstrap summary)

- Use Vintage Story's per-entity persistent attribute system rather than an external database. Verify 1.22 APIs before implementation.
- Keep gas **production, accumulation in a single intestinal reservoir, and actual release** distinct (D53; no burps or upper reservoir).
- Multiple meals overlap additively. A new meal never resets existing meal progress or the single gas reservoir. Store only active contributions and account for remaining potential before merging/removing records.
- Catalog all vanilla raw, cooked, and prepared foods; gas potential/delay/duration depend on food and species. Low spontaneous background gas is allowed for eligible animals even without a tracked trigger food.
- Support terrestrial animals and players; exclude flying animals/birds and nonbiological creatures initially. Species profiles should support optional modded animal entity codes without hard dependencies. The complete vanilla catalog is pending.
- Movement, jumping, and fear may increase the **chance of releasing already stored gas**, but fear releases should remain rare; quiet animals can accumulate for larger releases. No gas means no release.
- Allow at most one delayed, optional post-death release from remaining gas; final details require testing.
- Prevent a 30–40 goat enclosure from becoming an audio cacophony with cooldowns and herd audio limits, without corrupting the actual gas simulation.
- Start with common placeholder fart sounds (no burping). Add tagged sound types and species-specific libraries near release or later. Record contributor licensing/credit.
- Build ordinary native config first; investigate optional in-game settings UI. Simulation settings are server-authoritative; personal audio settings may be client-specific.
- Provide remote diagnostics for loaded creatures without chasing or targeting them, plus an anonymized species/behavior JSON export for animal-support requests.
- Creative mode is convenient for calm-animal testing; survival is needed for realistic fear-flight behavior.
- Maintain the lore as organized files. **Some Great Silence lore must be present in the FIRST public release**; later expand into more scrolls, books, resonator recordings, and possibly tapestry fragments.
- The Great Silence: elite old-world society considered flatulence vulgar and funded suppression treatments. During plague/Rot evacuation, treatment became compulsory for privileged underground refuge admission. Restricted subterranean diets and suppression persisted. With return to surface foods, bodily gas and initial fears returned; people learned it was harmless and rediscovered laughter. Historical writings should clearly but artfully reference the subject, and fit Vintage Story canon where confirmed.

## Documentation checkpoint rule

**Before moving to the next significant milestone**, and when an important design decision, API discovery, test result, bug, or story direction changes:

1. Remind the user that GitHub project documentation should be updated.
2. Update the affected Markdown file(s), using actual current state and a dated note where appropriate.
3. Commit those changes to GitHub. Confirm the commit succeeded before saying the project memory is saved.
4. Update the roadmap's `Next step` and `Last updated` so future chats can resume immediately.
5. Do not promise automatic/background updates; docs change only when a commit is actually performed.

Small iterative changes do not require an interruption on every message.

## Player gas persistence verified — 2026-11-08 evening PST

A server-only player gas persistence mod system was committed in `mod/lingamsdigestivesim/src/Simulation/PlayerGasStateModSystem.cs` (commit `a2295c4`), compiled using .NET 10 and installed with matching SHA256 hashes. In a dedicated creative test world: first `/gas status` returned **15.19%**, a second status returned the same; `/gas set 63` remained **63.00% after save/exit/restart**; `/gas set 0` remained **0.00% after save/exit/restart**, confirming that zero is not mistaken for missing mod data. Saved entity data uses a versioned `lingamsdigestivesim` subtree of server-side `Entity.Attributes` with `gasUnits`, `normalCapacityUnits`, and `lastUpdateTotalHours`. 100 units is a provisional player normal-full baseline. These tests confirm **one player's** state preservation only. Do not claim animal state, cross-player separation, uninstall safety, pending sessions, digestion or sounds are verified.

**Next engineering milestone:** generate an actual fart event that subtracts only existing gas and plays a basic sound, with debug sound-only testing separate from gas-consuming release. First test simple manual command, then controlled deliberate crouch and the three-tier large/medium/squeaker behavior. First-time entities should begin with 15–20% gas, but only when no prior mod state exists. Food-based generation and the 30-second intake session follow. Lore implementation deferred until the mechanics are working and fun; retain lore documents. Preserve one-command-at-a-time PowerShell workflow and the known-good EAT-PROBE.

## Current checkpoint (2026-11-08, after EAT-PROBE testing)

User verified three completed direct-food events, each with a one-unit log and increased satiety: violet web cap F093, rye grain F055, and spelt grain F058. Supersedes all earlier references to the Harmony observer being untested. The previous server-only injected CollectibleBehavior caused a startup crash and must never be restored. The successful DEBUG Harmony observer is read-only and creates no gas.

**Next engineering gate:** verify saved and reloaded versioned, namespaced per-entity attributes in a test world before adding the meal queue, 30-second player intake sessions, or gas accumulation. Give one PowerShell command at a time.

## Current authoritative digestion model

Decision D53 supersedes old two-reservoir, burping and progressive gastric-transfer concepts. The stomach is only a food-specific **game-time delay**; production then occurs gradually over a finite duration into **one persistent intestinal reservoir**. Players use a sliding **30 real-second inactivity timer**, reset to 30 seconds at each successful bite, and timestamp the combined meal when the window closes. Distinct food identities and contributions remain independent; new food never resets older meals. Animals use observed feeding plus sparse species-specific background gas, without player-style meal batching. Stored gas is conserved; **only actual fart events subtract it**. Normal full pressure is 100%, not a hard clamp, with overpressure release behavior targeted around 125%. No burping, upper-gas reservoir, modeled stomach compartment or future burp scaffolding. Unapproved numerical examples are not binding.

See docs/DECISIONS.md D44-D55, docs/PROJECT_MEMORY.md's latest override notes, and docs/ROADMAP.md for constraints and next actions.
