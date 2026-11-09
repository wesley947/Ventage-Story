# AIMAIN.md — AI Entry Point

> **For any ChatGPT or coding assistant continuing this project: read this file first.** The user should only need to say: "Read AIMAIN.md in my Ventage-Story GitHub repo and continue where we left off."

## Project identification

- Repository: `wesley947/Ventage-Story` (repository spelling is intentional; do not rename it without asking).
- Project: a Vintage Story digestive-gas simulation mod, with **Lingam's** as the intended public naming/creator brand. **Public display title remains undecided.** Previous names included *Cabbage Carnage* and *Ventage Story*.
- Target: Vintage Story 1.22.x, C#/.NET 10.
- Current stage (2026-11-08): local template compiled and loaded; the JSON food registry **successfully compiled, packaged, and loaded 195 profiles on both server and client**. The debug native item-code audit succeeded and identified **158 matching native codes for 122 profiles**. New optional C# code-binding loader and its JSON asset **compiled and loaded successfully on both server and client** (158 bindings), confirmed in logs Nov 8 16:10. **STOP / CRASH HOTFIX 2026-11-08 16:36:** The debug eating probe (custom server-only `CollectibleBehavior` appended to registered item types) led to a client `ItemTypeNet.ReadItemTypePacket` null-key exception when the **correct DLL** was installed; VSPT disabled. GitHub `main` **now removes** its attachment call and deletes the unsafe probe source, preserving all 195 profiles + 158 bindings. **Next:** user must `git pull`, rebuild, install and confirm world loads without crash before attempting a different, safer food-eating interception. Do NOT reintroduce direct post-registration modification of `CollectibleBehaviors`. No gas reservoir yet. **Approved stable internal Mod ID: `lingamsdigestivesim`** (subject to verifying no conflict before distribution). The repository still uses `Ventage-Story` until explicitly renamed.
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

**Documentation status (2026-11-08):** All files listed in this table have been created and committed to GitHub. The actual C# mod shell and playable features have **not** been implemented. Recheck the repository before assuming this remains current.

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

## Current checkpoint (2026-11-08)

The local GitHub repo was successfully cloned under `$HOME\\Ventage-Story`. User has **not yet confirmed navigating into it**, but the clone command reported success. Research of game-owned voice samples and sound APIs has advanced, and the user has declared a minimum multiplayer acceptance goal: unmodded nearby players hear the modded player's fart. Only-client-modded/unmodified-server delivery is **unverified and potentially unsupported**; defer hands-on multiplayer test to when the user's son visits around Thanksgiving. Continue local template bootstrap now. The immediate next one-command PowerShell instruction is `git -C "$HOME\\Ventage-Story" status --short --branch`, then dry-run generation if clean.

## Immediate next step at repository bootstrap

Official `VintageStory.Mod.Templates@1.22.0` is installed, `dotnet new vsmod --help` reviewed, and the game API DLL was found in the development PC's expected location. Next: inspect local clone/working directory and dry-run the `vsmod` template using stable Mod ID `lingamsdigestivesim`, while leaving the public display title changeable. Then create a tiny compilable mod shell and verify it loads in Vintage Story 1.22.x. Research actual feeding and attribute APIs before implementing digestion. Follow the user's preference for **one PowerShell command at a time**, waiting for results before supplying the next command.

Do not mistake a design mockup, hypothetical number, or untested snippet for an implemented feature.
