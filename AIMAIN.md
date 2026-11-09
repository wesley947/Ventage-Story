# AIMAIN.md — AI Entry Point

> **For any ChatGPT or coding assistant continuing this project: read this file first.** The user should only need to say: "Read AIMAIN.md in my Ventage-Story GitHub repo and continue where we left off."

## Project identification

- Repository: `wesley947/Ventage-Story` (repository spelling is intentional; do not rename it without asking).
- Project: a Vintage Story digestive-gas simulation mod, with **Lingam's** as the intended public naming/creator brand. **Public display title remains undecided.** Previous names included *Cabbage Carnage* and *Ventage Story*.
- Target: Vintage Story 1.22.x, C#/.NET 10.
- Current stage (2026-11-08 17:08 PST): 195 food profiles, 13 composite meal rules, 6 exclusions and 158 verified native item-code bindings load. The DEBUG server-only Harmony EAT-PROBE has now PASSED actual consumption tests: violet web cap (F093), rye grain (F055), and spelt grain (F058), each logged as one native unit, with user-observed satiety increases. This verifies those three direct food paths, not meals, liquids or animal intake. No gas generated. Earlier server-only collectible-behavior injection caused startup crash; it was removed and must not return. No persistent meal state, running digestion, gas accumulator, release mechanic or audio yet. Next engineering gate: a tiny namespaced per-entity persistence save/reload test. Approved Mod ID lingamsdigestivesim; repo spelling Ventage-Story intentional.
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

**Documentation status (2026-11-08):** All files listed in this table have been created and committed to GitHub. The C# mod shell, food catalog and direct-food diagnostic have been implemented and tested. The digestive simulation, gas reservoir, releases and audio are not yet implemented.

## Current essential design decisions (bootstrap summary)

- Use Vintage Story's per-entity persistent attribute system rather than an external database. Verify 1.22 APIs before implementation.
- Keep gas **production, accumulation in upper/lower reservoirs, and release** distinct.
- Multiple meals overlap additively. A new meal never resets existing meal progress or either gas reservoir. Store only active contributions and account for remaining potential before merging/removing records.
- Catalog all vanilla raw, cooked, and prepared foods; gas potential/delay/duration depend on food and species. Low spontaneous background gas is allowed for eligible animals even without a tracked trigger food.
- Support terrestrial animals and players; exclude flying animals/birds and nonbiological creatures initially. Species profiles should support optional modded animal entity codes without hard dependencies. The complete vanilla catalog is pending.
- Movement, jumping, and fear may increase the **chance of releasing already stored gas**, but fear releases should remain rare; quiet animals can accumulate for larger releases. No gas means no release.
- Allow at most one delayed, optional post-death release from remaining gas; final details require testing.
- Prevent a 30–40 goat enclosure from becoming an audio cacophony with cooldowns and herd audio limits, without corrupting the actual gas simulation.
- Start with common placeholder fart/belch sounds. Add tagged sound types and species-specific libraries near release or later. Record contributor licensing/credit.
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

## Current checkpoint (2026-11-08, after EAT-PROBE testing)

User verified three completed direct-food events, each with a one-unit log and increased satiety: violet web cap F093, rye grain F055, and spelt grain F058. Supersedes all earlier references to the Harmony observer being untested. The previous server-only injected CollectibleBehavior caused a startup crash and must never be restored. The successful DEBUG Harmony observer is read-only and creates no gas.

**Next engineering gate:** verify saved and reloaded versioned, namespaced per-entity attributes in a test world before adding the meal queue, 30-second player intake sessions, or gas accumulation. Give one PowerShell command at a time.

## Current authoritative digestion model

Decision D53 supersedes old two-reservoir, burping and progressive gastric-transfer concepts. The stomach is only a food-specific **game-time delay**; production then occurs gradually over a finite duration into **one persistent intestinal reservoir**. Players use a sliding **30 real-second inactivity timer**, reset to 30 seconds at each successful bite, and timestamp the combined meal when the window closes. Distinct food identities and contributions remain independent; new food never resets older meals. Animals use observed feeding plus sparse species-specific background gas, without player-style meal batching. Stored gas is conserved; **only actual fart events subtract it**. Normal full pressure is 100%, not a hard clamp, with overpressure release behavior targeted around 125%. No burping, upper-gas reservoir, modeled stomach compartment or future burp scaffolding. Unapproved numerical examples are not binding.

See docs/DECISIONS.md D44-D55, docs/PROJECT_MEMORY.md's latest override notes, and docs/ROADMAP.md for constraints and next actions.
