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
