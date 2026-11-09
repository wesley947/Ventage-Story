# Incremental implementation and test plan

**Last reconciled:** 2026-11-08, 21:15 PST. This supersedes earlier, now-obsolete template bootstrap, burping and two-reservoir stage proposals. Original exploration is preserved in [PROJECT_MEMORY.md](PROJECT_MEMORY.md), [DECISIONS.md](DECISIONS.md), and Git history. **Start from [AIMAIN.md](../AIMAIN.md).**

## Verified working baseline: Vintage Story 1.22.7 / .NET 10 Debug

The user has compiled, installed, hash-compared, and exercised the mod in a separate Creative test world.

- **Registry and intake:** 195 food profiles, 13 composite descriptions, 6 exclusions, 158 audited game-code mappings; DEBUG-only server Harmony probe verified three direct foods (violet web cap, rye, spelt) and subsequent pending food intake. The old server-only injected unregistered collectible behavior crashed client deserialization and **must never be restored**.
- **Persistence:** namespaced version-1 per-player gas reservoir initialized 15–20%; stored 63% and valid 0% across restarts. A pending cabbage meal survived restart with 16s countdown remaining, and two unfinished active contributions survived a restart.
- **Production:** 30-real-second player inactivity window, per-food *delay-only* stomach timing, game-hour linear gas generation into one reservoir. Sleeping and `/time add` produced first 15.40%; a later meal continued 45.18→46.28→69.47→121.00%, removed finished sources, kept uncapped gas, and remained at 121% after +2 more hours without eating.
- **Audio/release:** server positional built-in `game:sounds/voice/trumpet` heard by user, random initial pitch audible; `/gas sound` leaves gas unchanged. `/gas fart` consumed 72.60 gas units from 121%, leaving independently verified 48.40%. This is a **test-only 60% fractional release**, not final tiers or crouch behavior.

**Unverified:** a proper Release-build intake path, all prepared meal/liquid types, animal integration, multiplayer recipient hearing, install/remove/reinstall compatibility, varied finished fart audio library, lore and public package.

## Immediate next slice — safe non-consuming audio experimentation

1. Review [AUDIO_API_AUDIT.md](AUDIO_API_AUDIT.md) and source files before coding.
2. Preserve the **known-good accumulated-gas + trumpet DLL** in a separate backup. Inspect the user's local `git status` before staging/creating files.
3. Build a **small client-side sound harness** (possibly a standalone Fart Lab mod) that loads one of the eight *existing game-owned* instrument .ogg sounds and allows hands-on playback, stop, duration, pitch and volume changes through `ILoadedSound`. **Audio preview uses no gas, ever.**
4. Incrementally add four-node time-indexed pitch automation, separate ADSR volume, tremolo, subtle pitch wobble and optional short end squeak/pop **after** verifying live API behavior.
5. Save user-approved good examples as **data-only, versioned JSON presets**, with no copied game audio. Consider standalone Fart Lab authoring tool and matching JSON playback support in the gameplay mod, without requiring both mods installed. The separate-mod approach is a **proposal, not finalized**. [Proposed preset format](FART_PRESET_FORMAT.md).
6. Compare advanced client-side envelopes against what unmodified multiplayer listeners actually hear via server `PlaySoundAt`. Do not presume server can send arbitrary continuous pitch envelopes without a compatible client.
7. Curate and audition a varied sound library as **D60 first-public-release requirement**. Neither a plain trumpet-only package nor an experimental Fart Lab window constitutes release readiness.

## Core gameplay continuation (after/alongside audio work)

- Move successful-intake collection out of DEBUG-only test code into a production-safe, reversible adapter after strict server/client testing.
- Make `/gas fart` a real server-authoritative release event with amounts based on current reservoir; ensure positive release only when gas available and near-empty forced release expels every remaining unit as a short squeak/pop.
- Implement player crouch/hold/stand, high pressure (~125%) opportunities, debounced movement triggers, meaningful release cooldowns and no sound backlogs after time skips/reload.
- Persist legitimate mixed foods, multiple servings and overlapping meals correctly; current two-food and cabbage smoke tests do **not** validate precise attribution.
- Implement separate species-aware terrestrial animal intakes and baseline accumulation (exclude flying creatures/birds). Evaluate 40-goat stress case without constant sound spam, and species/modded-entity profile exports.
- Optional pressure indicator on Character Stats `C` later, after core behavior and audio are established.

## First public version: acceptance gates

1. **Good varied fart library is mandatory (D60)** and must be auditioned/approved by the user before **0.1.0 or 1.0.0**. Longer low rumblers and brief gravelly blasts must both be possible; high squeakers finite; gas amount/duration/flow only mildly correlated with overlap (D59).
2. **World safety (D55):** On a **copy of a throwaway world**, test install, save, disable/remove mod, reload and play vanilla, save again, reinstall and verify original inventory, world and gas behavior survive sensibly.
3. **Release configuration:** Build with the actual included source and packaging files; verify complete food detection outside Debug-only `#if DEBUG` path. Confirm cross-client and mod/unmodded listener compatibility to the extent supported.
4. **First-release lore (D16):** A small set of appropriate Great Silence in-world collectible writings; never overwrite vanilla lore or claim untested persistence. Test trading and mod-uninstall safety on a world copy.
5. Sound credits/license review and suitable versioned ModDB/GitHub release packaging.
6. **Reproducible GitHub checkout:** The user's generated local `.csproj`, `modinfo.json` and starter template/build scripts remain **untracked by GitHub**. Review/commit only after examining the real local files; do not overwrite or regenerate them.

## Architectural invariants

- **D44:** The player 30-second REAL-time intake countdown **resets** after each actual bite; it never cumulatively adds 30 seconds. Session closes at final timeout and starts food-specific game-hour delay/production.
- **D53:** Stomach is only a delay. **One intestinal reservoir**; no stomach fluid compartment, burps, upper-gas storage, or progressive gastric-transfer mechanics.
- **D46:** `storedGasAfter = storedGasBefore + actualNewProduction − actualReleased`. Sources finish when they have generated their potential; their gas persists. No time-based disappearance, cap at 100%, or fake releases.
- **D55:** Vanilla files, registrations, recipes and inventory remain untouched. A narrowly scoped reversible server Harmony patch is only a diagnostic starting point, not proof all servers/worlds are safe.
- **Sound separation:** Audio preview belongs to the audition tool; gas subtraction belongs to a real, authoritative server release. JSON presets are parameter descriptions, not audio recordings.
- **Workflow:** One Windows PowerShell command per user turn, inspect output, install only after backup and hash verification; commit GitHub source/docs and verify before declaring a test complete. Maintain experimental branches separate from known-good main after a branch checkpoint.

## Relevant files

[AIMAIN.md](../AIMAIN.md) · [ROADMAP.md](ROADMAP.md) · [DECISIONS.md](DECISIONS.md) · [AUDIO_API_AUDIT.md](AUDIO_API_AUDIT.md) · [FART_PRESET_FORMAT.md](FART_PRESET_FORMAT.md) · [FOOD_REGISTRY_INTEGRATION.md](FOOD_REGISTRY_INTEGRATION.md) · [LORE_INDEX.md](LORE_INDEX.md)
