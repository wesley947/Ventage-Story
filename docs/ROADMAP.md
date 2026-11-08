# Development Roadmap

**Last updated:** 2026-11-08  
**Project stage:** documentation and architectural bootstrap.  
**Next step:** validate the existing GitHub documentation (especially creature exporter, overlapping food contributions, and lore plan), choose a neutral permanent mod ID, then create the smallest compilable Vintage Story 1.22.x mod shell.

## Documented, not yet implemented
- GitHub root `AIMAIN.md` directs future AI collaborators to the project files.
- `AGENTS.md` and `docs/` planning documents preserve decisions. Treat their existence as documentation, **not as evidence any feature works in the game**.
- No known mod DLL, in-game test, established internal mod ID or release package.

## Milestones

### M0 — Durable project handoff (in progress)
- [x] Create `AIMAIN.md` entry point.
- [x] Record collaboration workflow, decisions, current architecture, lore and animal-export requirements.
- [ ] Confirm documentation inventory and ensure AIMAIN reflects actual file paths.
- [ ] Review unresolved assumptions/possible contradictions; update and commit.
- [ ] Choose stable neutral mod ID (name displayed to players can change later).

### M1 — Minimum viable working shell
- [ ] Inspect the user's actual Vintage Story 1.22.x installation and known good C#/.NET 10 mod build pattern.
- [ ] Create `modinfo.json`, project file, minimal ModSystem and neutral asset namespace.
- [ ] Build and load successfully in a **separate test world**; record exact successful steps.
- [ ] Add a versioned, native JSON config file and basic logging.

### M2 — Entity data & lifecycle
- [ ] Confirm server-side per-entity persistence APIs using installed game and/or current source.
- [ ] Add one test attribute to a vanilla animal, save/reload, unload/reload and verify preservation.
- [ ] Define `DigestiveState` and add upper/lower reservoirs with active contributions.
- [ ] Support additive overlapping meals with correct cumulative elapsed-time accounting.
- [ ] Research death/unload behavior before delayed one-off postmortem release implementation.

### M3 — Creature census, food profiles and mod compatibility
- [ ] Generate accurate complete vanilla entity catalog from installed assets.
- [ ] Classify each: excluded / eligible default / shared type / custom.
- [ ] Add data-driven optional domain-qualified third-party profiles; ignore missing mods.
- [ ] Build JSON/plain-text species metadata exporter, suitable for issue submissions. See `CREATURE_EXPORT_SPEC.md`.
- [ ] Survey raw, cooked and prepared vanilla foods and define preliminary profiles.
- [ ] Verify actual successful-feeding pathways for animals and players; record limitations.

### M4 — Functional simulation
- [ ] Implement background gas production, delayed meal-derived upper/lower production, reservoir caps.
- [ ] Implement small sporadic and larger threshold releases, movement/jump/fear influences.
- [ ] Add death hook and delayed one-off release if viable.
- [ ] Handle long elapsed game-time intervals and save/unload/reload, without audio backlog.
- [ ] Add entity/area/herd diagnostics and bounded event logging.

### M5 — Audio and settings
- [ ] Begin with short common placeholder audio effects, appropriately labeled and credited.
- [ ] Add sound categories/tagged selection and positional playback.
- [ ] Add concurrency/cooldowns so 30–40 goats create elevated activity but not nonstop sound.
- [ ] Support native JSON settings; evaluate optional ConfigKit/Config Lib or custom in-game UI.
- [ ] Validate client/server ownership and multiplayer hearing.

### M6 — Great Silence lore (required for first release)
- [ ] Plan coherent chronology/sections/authors in `LORE_INDEX.md`.
- [ ] Compare tone and asset structure with verified vanilla lore books/scrolls.
- [ ] **Hello World persistence test:** spawn vanilla book/scroll with `title` and embedded `text` ItemStack attributes, save, store in inventory/chest/shelf/ground, disable mod on copied world, check readability and identity. Details: `docs/LORE_PERSISTENCE_RESEARCH.md`.
- [ ] Draft readable, mildly irreverent ancient medical and social histories, not excessively opaque.
- [ ] Implement at least some **rare discoverable scrolls/books**, and test journal and ruin loot behavior.
- [ ] Investigate **journal-aware additional loot**: vanilla-lore discovery ratio per player, original vanilla roll priority, rising bonus Ventage lore odds late-game; verify cracked-vessel break hooks, pre-generated chests, multiplayer, and non-destructive drop behavior (see `LORE_INDEX.md`).
- [ ] Explore one or two original resonator cylinder recordings if practical; not a blocker if disproportionate effort.

### M7 — Quality and first public release
- [ ] Survival/creative regression tests; ensure fear events only tested with true fleeing AI.
- [ ] Performance around 30–40 goats and modded fauna; sound mix sanity check.
- [ ] Verify no unintended entity, save, player nutrition, combat or wildlife changes.
- [ ] Pick and clear usable quality sound assets / licensing, update `SOUND_CREDITS.md`.
- [ ] Prepare README, credits, license, version, icon, release ZIP, source and ModDB listing.
- [ ] Check docs and GitHub sync before releasing.

## Later / feature-creep queue
- Rich species-specific sound libraries and audio tuning.
- More custom modded fauna support.
- Additional lore books, scrolls, historical figures, resonator recordings, and collectible tapestry fragments.
- Animal dung, pellets/pies, decay, farm fertilizer and potential manure-mod integration.
- Advanced digestive interactions, relief foods, complex settings interface and more specialized AI.
- **Emergent herd reactions, not an event feature:** ordinary audible releases might independently startle nearby animals, which may then release existing gas under normal individual rules. Do not script a cascade or implement a special herd-frenzy system. Test only after general animal sound response and digestion are working. See `PROJECT_MEMORY.md`.

## Highest-priority technical unknowns
1. Actual installed game API hook for successful ingestion and food identity across various creatures.
2. Reliable attachment and persistence of the behavior in vanilla and modded entities.
3. Entity definitions and diet metadata suitable for a safe exporter.
4. Correct game-time processing during unload/reload; bounded state on repeated small meals.
5. Sound and config API details; lore asset/journal/loot integration.

## Documentation checkpoint
At the end of a substantial milestone or after a major decision/test result, remind the user to update GitHub memory docs, commit changes, and change **this file's Next step and Last updated**. No asynchronous tracking or presumed commits.

## New chat recovery
Start at repository root `AIMAIN.md`; a new assistant should follow its documentation map. The user should not have to remember any other filenames.
