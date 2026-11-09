# Development roadmap — Lingam's Vintage Story digestive-gas mod

**Last reconciled:** 2026-11-08, 21:15 PST. This file describes **current status and next actions**, not a running log. Earlier experiments and timestamped screenshots are preserved in [PROJECT_MEMORY.md](PROJECT_MEMORY.md); current facts summarized in [AIMAIN.md](../AIMAIN.md).

**Current stage:** Working **Debug-build player-direct-food digestive accumulator**, persistent across restarts; audible vanilla trumpet diagnostic and manual 60%-of-current gas release. Not a public-release candidate. **Next focus: audio research and a developer-only sound audition harness.** See [AUDIO_API_AUDIT.md](AUDIO_API_AUDIT.md).

## Verified in-game milestones

- [x] Vintage Story 1.22.7 .NET 10 test-world build and working food-asset load: **195 profiles / 13 composite descriptions / 6 exclusions / 158 vetted item bindings**.
- [x] Server-only **Debug** successful direct-food Harmony observation for three directly eaten items (violet web cap, rye, spelt); old unregistered CollectibleBehavior injection was removed after an actual startup crash.
- [x] Namespaced persistent player reservoir initial **15–20%**; retained **63% and zero** after shutdown/restart, without recreating saved zero.
- [x] Two recognized distinct foods queued in pending 30-real-second session; transitioned to two active contributions when countdown ended; pending cabbage session **16s remaining** survived save/restart. Repeated-bite reset **not separately tested**.
- [x] Game-time delayed/gradual gas production: 15.40% meal completion; partial active sessions survived restart; subsequent +2 game hours advanced **46.28 → 69.47%**, finished at **121.00%**, 0 active, and stayed **121.00%** after another +2 hours. Conservation and overflow smoke tests **passed for the tested player**, but exact per-food attribution is pending.
- [x] Native server positional `game:sounds/voice/trumpet` plays audibly; randomized initial pitch differences noticed; `/gas sound` does **not** consume gas.
- [x] `/gas fart` temporary manual diagnostic released **72.60 gas units** from **121.00%** leaving **48.40%** confirmed by status. The temporary 60% algorithm produces endlessly shrinking residues at low gas and is **not acceptable final behavior**.
- [ ] Verify a dedicated **Release** build intake pathway; current EAT-PROBE subscription is `#if DEBUG` only.
- [ ] Test exact bite-count/amount accounting, repeat-bite 30s reset, mixed prepared meals, liquids, other consumption hooks, deliberate zero-residual cleanup, multiple players and mod removal/reinstall.

## Highest priority — sound library (D60: release blocker)

**No first public release, regardless of whether version is called `0.1.0` or `1.0.0`, until a user-approved, varied, funny, convincing sound library has been built and auditioned.**

1. [ ] Review [Audio API Audit](AUDIO_API_AUDIT.md) and verify via client audio prototype: `ILoadedSound` sample length, SetPitch/SetVolume **during** playback, stop/fades, position and disposal; do **not** consume gas in audio previews.
2. [ ] Audition eight known game-owned `assets/game/sounds/voice` instruments. Keep plain trumpet as one valid simple sound, not the entire library.
3. [ ] Prototype **four-point pitch timeline**, separate ADSR amplitude, slow irregular tremolo, mild pitch wobble, bounded timing, clipping/stop and optional small high-pitched final pop/squeak. Graveled texture is a listening result, not an API guaranteed effect.
4. [ ] Experiment with low-pass, reverb, loops/trim and layered samples; record which operations actually work, and whether pitch changes also alter effective duration. No claim of independent pitch-preserving time stretch or built-in distortion.
5. [ ] Build a **curated named preset library** of varied short pops, squeakers, gravelly blasts, wavering releases and lower rumblers. Gas amount, flow, pitch and time are related **loosely with overlapping ranges** (D59); no infinite squeakers or deep-only short blasts.
6. [ ] Consider **small developer-only Fart Lab** GUI and optionally named-sound save/cache only when helpful; **public player editor/Fart Saver distribution is undecided**.
7. [ ] Test server one-shot fallback vs richer client-only live envelope and audibility to unmodded multiplayer clients. Do not conflate working singleplayer trumpet with deployment compatibility.

## Core gameplay hardening

- [ ] Implement actual gas-conserving manual release tiers and **low-residual forced squeak-pop clearing exact zero**, rather than the diagnostic infinite-60%-fraction tail.
- [ ] Implement deliberate crouch/hold and movement-triggered opportunities, with debounce, configurable probabilities, cooldowns and gas-pressure-dependent amounts; zero gas never produces a fart.
- [ ] Naturally driven overpressure release near ~125%, subject to tuning; storage may exceed 100% without silent capping. No gas release sound backlog on relog.
- [ ] Player qualitative pressure feedback / optional Character Stats `C` section, after gameplay/audio work.
- [ ] Separate player vs animal intake; eligible terrestrial animals only, no birds/flying creatures, with occasional species-specific background gas even without tracked food. Test sample entities and crowded ~40-goat pen in survival (creative suppresses ordinary fear).
- [ ] Data-driven third-party animal profiles and privacy-safe creature export; see [CREATURE_EXPORT_SPEC.md](CREATURE_EXPORT_SPEC.md).
- [ ] Extend food mapper for prepared multi-ingredient servings, liquids, cheese slices, milk and relevant foods; do not double-count nutrition events. Align portion scaling and milk quantity warnings in [FOOD_FRAMEWORK.md](FOOD_FRAMEWORK.md).

## First-public-release safety and content gates

- [ ] **D55:** Build on a copy of a throwaway Vintage Story world and prove mod enabled → saved → disabled/removed → world playable → saved → re-enabled, with native items/recipes intact. Prior unregistered server behavior injection is permanently prohibited.
- [ ] Cross-client server/client testing, including nearby unmodded listeners where technically possible; zero client install on an unmodified server is an unproven stretch goal.
- [ ] **D16:** Include *some* Great Silence lore in the first public version. Research safe reuse of vanilla readable item stacks; verify persistence when mod removed/traded, and don't drown out vanilla lore. [LORE_INDEX.md](LORE_INDEX.md), [LORE_PERSISTENCE_RESEARCH.md](LORE_PERSISTENCE_RESEARCH.md).
- [ ] Sound licensing and credits, mod information, packaging, versioning, README/listing and source distribution; built-in game sounds should be referenced rather than copied.
- [ ] Verify clean-clone reproducible build, preserving the user's existing local template files. **GitHub does not yet track local `.csproj`, `modinfo.json`, template starter ModSystem and build scripts.** Review `git status` before separately committing those.
- [ ] Test production/Release configuration; Debug EAT-PROBE isn't a shipping intake mechanism yet.
- [ ] Obtain user's approval of fart sound library quality **before** first public release.

## Documentation and branch workflow

At audit start, GitHub showed **one remote branch: main**, and all prior work had landed there. Preserve a named checkpoint of the working digestive/trumpet prototype for safety. Use a separate `feature/...` branch for **future experimental audio UI/code** rather than continuing unreviewed prototype work directly on main; confirm user local branch before any pull. A Git *tree* is a commit's file snapshot, not an independent active branch.

Use [AIMAIN.md](../AIMAIN.md) as entry, [DECISIONS.md](DECISIONS.md) for authoritative user choices and [PROJECT_MEMORY.md](PROJECT_MEMORY.md) for detailed past results. After a successful new milestone, update these files and verify the GitHub commit. **One PowerShell command per response**, as requested.
