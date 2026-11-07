# AIMAIN.md — AI Entry Point

> **For any ChatGPT or coding assistant continuing this project: read this file first.** The user should only need to say: "Read AIMAIN.md in my Ventage-Story GitHub repo and continue where we left off."

## Project identification

- Repository: `wesley947/Ventage-Story` (repository spelling is intentional; do not rename it without asking).
- Project: a Vintage Story digestive-gas simulation mod, working title **Cabbage Carnage**. The **public name is NOT final**.
- Target: Vintage Story 1.22.x, C#/.NET 10.
- Current stage (2026-11-06): planning the architecture; no compiled mod or established internal mod ID yet.
- Keep the permanent internal mod ID, namespaces, serialized attribute keys, and source comments neutral. Do not hard-code a provisional display name into them.

## On every new chat

1. Open this repository via the GitHub connector. Check the default branch and real file contents, rather than trusting an old summary.
2. Read `AGENTS.md` if present: coding and collaboration rules.
3. Read `docs/PROJECT_MEMORY.md` if present: current architecture, technical context, and major requirements.
4. Read `docs/DECISIONS.md` if present: decisions and their rationales, including reversals.
5. Read `docs/ROADMAP.md` if present: completed work, next milestone, blockers, and deferred features.
6. Read `docs/LORE_INDEX.md` if present when writing, integrating, or reviewing lore.
7. Inspect the actual relevant source and data files before making changes. If any referenced document doesn't exist, SAY SO and propose creating it; never invent its contents.
8. Summarize the current state and the **single next practical step**. Do not make the user reconstruct the project from memory.

## Documentation map

| File | Purpose |
| --- | --- |
| `AIMAIN.md` | Permanent AI entry point and documentation map; keep short and trustworthy. |
| `AGENTS.md` | Instructions for AI collaborators: coding style, test workflow, small edits, one PowerShell command at a time. |
| `docs/PROJECT_MEMORY.md` | Durable project specification, game/API findings, architecture, persistence, food and species model. |
| `docs/DECISIONS.md` | Dated decisions, especially those changing earlier design assumptions. |
| `docs/ROADMAP.md` | What is done, what is next, blockers, and feature-creep backlog. |
| `docs/LORE_INDEX.md` | Great Silence chronology, writers, discovery types, journal sections, planned and implemented lore entries. |
| `docs/SOUND_CREDITS.md` | Eventually record sound sources, creators, licenses, and edits. |

**These files are planned; check GitHub for actual existence before claiming they are present.**

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

## Immediate next step at repository bootstrap

Create the remaining documentation files with the latest accurately agreed specification, then create a tiny compilable, neutral-named mod shell and verify it loads in Vintage Story 1.22.x. Research actual feeding and attribute APIs before implementing digestion. Follow the user's preference for **one PowerShell command at a time**, waiting for results before supplying the next command.

Do not mistake a design mockup, hypothetical number, or untested snippet for an implemented feature.
