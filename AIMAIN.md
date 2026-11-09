# AIMAIN.md — authoritative project handoff

> **Read this first in a new chat**, then inspect the real source and GitHub branch. User cue: "Read AIMAIN.md in my Ventage-Story GitHub repository and continue." This document states **what is true now**, not every past checkpoint. Historical tests and superseded experiments remain in [Project Memory](docs/PROJECT_MEMORY.md) and [Decisions](docs/DECISIONS.md).

**Reconciled checkpoint: 2026-11-08, 21:15 PST.**

## Identity and GitHub

- GitHub: `wesley947/Ventage-Story` — spelling is deliberate. Default branch: **main**. We audited the remote and confirmed **all earlier work was directly on main**. On 2026-11-08 we created a separate **`checkpoint/verified-accumulator-trumpet-2026-11-08`** branch as a stable snapshot of the working game-code and reconciled documentation. **Two remote branches now:** main + that checkpoint. No experimental feature branch or separate Fart Lab repo exists yet.
- Game: **Vintage Story 1.22.7**, C#/.NET 10. Stable internal ModID, asset namespace and persistent attribute key: **`lingamsdigestivesim`**. Do not rename with a public title.
- Public title undecided; *Cabbage Carnage* and *Ventage Story* are working names, with *Lingam's* preferred creator attribution.
- **Repository recovery gap:** GitHub contains C# code, assets, CSV, JSON generator and `Directory.Build.targets`, but the user's original **locally generated `.csproj`, `modinfo.json`, build scripts and template starter ModSystem are not tracked**. They work on the user's PC. Do not delete, replace or regenerate them; inspect `git status` first and commit an approved, sanitized package/build skeleton separately. A clean GitHub clone is not yet independently buildable.

## Current verified features (user's Creative test world)

| Area | Verified result | Not yet proven |
| --- | --- | --- |
| Food registry | Loaded **195** gas profiles, **13** composite descriptions, **6** exclusions, and **158** native item-code bindings (122 profiles), server/client. | Actual prepared mixed-meal, liquid, animal intake. |
| Direct eating | **DEBUG server-only** passive Harmony `tryEatStop` observer saw violet web cap F093, rye F055, spelt F058 and queued actual food into player meal simulation. | General Release build ingestion. **Never reintroduce** the failed injected unregistered collectible behavior, which crashed item decoding. |
| Stored player gas | First-time 15–20% (observed 15.19%); saved **63% and 0%** persisted across complete restarts. | Multiple players, animals, uninstall/re-enable. |
| Player meal window | Two separate pending food types moved into two active contributions after a **30 real-second inactivity** window. An unfinished cabbage window persisted across restart with **16s remaining**. | Repeated-bite reset edge cases, offline timing across all servers. |
| Game-hour accumulator | Sleep generated **15.40%** from food, preserved after restart. Partial digestion: **45.18% → 46.28% after restart → 69.47% after +2 game hours → 121.00% after +8 hours** with 0 active sources. Another +2 game hours caused **no further gas**; no 100% clamp. | Precise per-food accounting and untested ingestion types. |
| Audio | Built-in `game:sounds/voice/trumpet` audible with visibly different randomized pitch. `/gas sound` left gas at **121%**. | Rich live effects, client sound envelopes, multiplayer hearing. |
| Manual release | `/gas fart` expelled **72.60** from 121, leaving **48.40%**, confirmed by `/gas status`. | Automatic/crouch releases, finalized sounds and balancing. Current **60%-of-current** diagnostic causes endless tiny residues such as 0.08%, not final gameplay. |

**Evidence scope:** The user compiled and installed a Debug DLL in one Creative test world, verified matching installed/build hashes, and tested the features above. Do not claim broader release readiness. Source and data are in `mod/lingamsdigestivesim/`.

## Governing decisions

- **D44 + D53:** Successful player eating resets a **30-real-second inactivity countdown**, never adds to it. When the meal closes, each ingredient has its own *game-hour delay* followed by gradual production into **one** persistent intestinal reservoir. No modeled stomach compartment, upper gas or burps.
- **D46–D50:** Gas is conserved; **only a real release** removes stored gas. 100% normal-full is a soft threshold, not a hard cap. Animal background gas and feeding are *planned*, not implemented; birds/flying and nonbiological creatures excluded. No scripted herd frenzy.
- **D57–D59:** Pressure-sensitive big/medium/final squeaker releases planned. Very low forced release should be a near-instant squeak-pop clearing exactly 0; a low-pitched blast tends to expel more per second but rates/durations **overlap** with higher squeakers. Long bass rumbles and finite small squeakers should both be possible; no final constants approved.
- **D60:** **First public release blocker:** a **convincing, diverse, curated fart sound library** must exist before any initial `0.1.0` or `1.0.0` release; plain trumpet is an excellent proof, but not enough. Eight native voice files known: `accordion`, `altoflute`, `clarinet`, `harmonica`, `oboe`, `sax`, `trumpet`, `tuba`. Use native assets by location, do not redistribute game audio. See [Audio API Audit](docs/AUDIO_API_AUDIT.md).
- **D61 — SHARED PRESET FORMAT FIRST:** Define, validate and test a **versioned JSON fart library** that both the proposed Fart Lab editor *exports/previews* and Cabbage Carnage *imports/plays* without an installed editor. Each saved fart has a fixed gas cost or explicitly tagged special scalable mode; include **several** designs each at example tiers 75%, 50%, 25%, 10% of *normal-full capacity*. On a real release, randomly select **only affordable** presets and debit exactly their required gas once. Audio previews are always free; an empty reservoir is silent; a trace-gas forced squeak/pop empties the exact residue. The percent-of-normal-capacity definition is the proposed interpretation to confirm before schema freeze. **The standalone Fart Lab mod remains only a possibility.** [Preset format working draft](docs/FART_PRESET_FORMAT.md).
- **Fart Lab (exploratory architecture):** The user is considering a **separate Vintage Story mod** for authoring/auditioning sound presets. Its **versioned JSON sound-recipe collection** could be imported by the gameplay mod **without the editor installed**. Four pitch nodes, separate ADSR, tremolo, texture, randomness, ending flourish and named presets. JSON recipes are **not audio recordings**; a compatible renderer and shared format are needed. [Preset format proposal](docs/FART_PRESET_FORMAT.md). Neither the independent-mod split nor ordinary-player editor access is finalized. Rich client `ILoadedSound` envelopes are not automatically reproducible on unmodded multiplayer clients.
- **Safety D55:** Worlds must remain usable with/without the mod. Use server-owned namespaced state and reversible Harmony. Uninstall/re-enable and unmodded-client tests are outstanding. Never replace vanilla items/recipes or modify the game's files.
- **First public lore requirement D16:** some Great Silence lore collectibles must ship with the first public release, but **none are implemented yet**. The revised story (old-world elite social suppression, plague refuge, unfamiliar surface foods, first laughter) is in [Lore Index](docs/LORE_INDEX.md).

## Current next engineering task

**First agree on the shared D61 JSON preset format and validation fixtures**, then build a small client-side, developer-only sound audition harness to experimentally confirm `ILoadedSound` initial playback, smooth live pitch and volume adjustment, stop/fades and controllable duration. No gas consumption. Then audition samples, four pitch points/ADSR, tremolo, random gravel and quick end pops, curate a strong sound library. [Audio API Audit](docs/AUDIO_API_AUDIT.md) is the next specification. Avoid writing a giant GUI until the live audio controls work. Source code has **NOT** yet been added for Fart Lab.

**User workflow:** Always **one Windows PowerShell command per message**, wait for output. Before installing changed DLLs, back up working mod; build locally with the known .NET 10 command and compare hashes. GitHub commits are not local installation or proof of successful gameplay. Treat the user's locally generated template files as precious.

## Documentation map — the 15 audited Markdown files

| File | Purpose |
| --- | --- |
| [AGENTS.md](AGENTS.md) | Contributor workflow, safety, incremental tests. |
| [AIMAIN.md](AIMAIN.md) | **This authoritative current-state entry point.** |
| [docs/ROADMAP.md](docs/ROADMAP.md) | Next milestones, tested features, blockers. |
| [docs/DECISIONS.md](docs/DECISIONS.md) | Decisions D01–D60; later revisions override earlier ideas. |
| [docs/PROJECT_MEMORY.md](docs/PROJECT_MEMORY.md) | Full chronological research, experiments, failures and successful tests. |
| [docs/IMPLEMENTATION_PLAN.md](docs/IMPLEMENTATION_PLAN.md) | Engineering test gates and historical stage plans. |
| [docs/AUDIO_API_AUDIT.md](docs/AUDIO_API_AUDIT.md) | Exact supported/unproven audio controls and audition plan. |
| [docs/FART_PRESET_FORMAT.md](docs/FART_PRESET_FORMAT.md) | Proposed portable JSON sound recipe format for optional standalone Fart Lab. |
| [docs/SOUND_CREDITS.md](docs/SOUND_CREDITS.md) | Native voice samples, licensing and eventual credits. |
| [docs/FOOD_FRAMEWORK.md](docs/FOOD_FRAMEWORK.md) | Food identity/amount capture and partial meal research. |
| [docs/FOOD_CODE_AUDIT_REVIEW.md](docs/FOOD_CODE_AUDIT_REVIEW.md) | Audited native food-code mapping and gaps. |
| [docs/FOOD_REGISTRY_INTEGRATION.md](docs/FOOD_REGISTRY_INTEGRATION.md) | Current registry implementation and historical crash workaround. |
| [docs/CREATURE_EXPORT_SPEC.md](docs/CREATURE_EXPORT_SPEC.md) | Deferred privacy-safe creature type export. |
| [docs/LORE_INDEX.md](docs/LORE_INDEX.md) | Great Silence planned lore chronology, first-release requirement. |
| [docs/LORE_PERSISTENCE_RESEARCH.md](docs/LORE_PERSISTENCE_RESEARCH.md) | Unverified vanilla book/scroll save/uninstall hypothesis. |

## Documentation checkpoint

Update focused files and `docs/ROADMAP.md` with *verified* facts, then refresh this compact handoff and confirm GitHub commit(s). Keep historical contradictions in `PROJECT_MEMORY.md` explicitly labeled superseded rather than treating them as current instructions. No automatic or background work is implied.
