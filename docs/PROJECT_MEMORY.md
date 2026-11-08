# Project Memory — Vintage Story Digestive-Gas Mod

**Last updated:** 2026-11-08  
**Status:** Planning; GitHub documentation initialization. No confirmed compiled mod, finalized internal mod ID, or public release.

## Purpose
Create a humorous but carefully engineered digestive-gas simulation for Vintage Story 1.22.x (C#/.NET 10). Include players and eligible ground animals, vanilla and optional third-party species; keep player effects cosmetic. The public title is unsettled. *Cabbage Carnage* was a previous working title; the repository is named `Ventage-Story`. Do not conflate repository name, mod ID, and final display name.

The first public release should include some well-crafted in-world *Great Silence* lore. Audio samples can be placeholders during development but should be revisited before release.

## Essential simulation invariants
1. **Food intake, production, upper/lower storage, and gas release are separate stages.** Movement and fear affect release probability, not magical gas generation.
2. **Overlapping meals are additive.** New food never zeroes existing gas or restarts prior food contributions. Each consumed portion has its own upper/lower gas *potential* and progress over game time. The contributions still being digested can be compactly stored. Completed contributions can be pruned after accounting for all gas they produced; do not silently discard unproduced gas.
3. Upper gas (belching) may appear earlier than intestinal gas (flatulence). Timing, magnitude, food influence, and species physiology remain to be researched and tuned.
4. A little baseline spontaneous gas may develop even without a detected high-gas meal. It should be rare enough to preserve surprise and not create a steady chorus.
5. Both reservoirs are depleted by releases; **zero available gas means no release**. Slow/calm creatures can accumulate and occasionally release a larger amount; movement, jumping, being scared/chased increases the chance of a small stored-gas release, but it must remain uncommon. Use cooldowns.
6. Death may trigger **one optional delayed postmortem release**, using remaining reservoir gas, several seconds later. No guaranteed postmortem sound when the reservoir is empty. Confirm death lifecycle and cancel/skip safely if the entity unloads or no longer exists. Bowel contents/dung are not in v1.
7. Game time controls digestion. For loaded entities, periodically process elapsed time; after unload/reload, account for missed digestive progress without replaying old noises or fabricating precise movement history. Honor reservoir capacity and avoid double counting.
8. Server owns authoritative digestive state using the game's per-entity persistence mechanism (candidate: `Entity.Attributes`; verify API and save semantics in installed 1.22 build). Only synchronize what clients need.
9. Creature herd audio budget / positional sound / cooldowns should prevent a 30–40-goat pen from becoming deafening. Audible effects must not be the sole representation of physical release.
10. World gameplay and nutrition must stay intact. Player effects should not cause damage, movement penalties, survival disadvantages or other punitive mechanics.

## Planned data model (not yet implemented)
Persist under a neutral namespace/tree on each relevant entity, with versioned schema:
- `schemaVersion`; `lastUpdateTotalHours`
- `upperGas`, `lowerGas`
- Small collection of **active digestion contributions**: stable food code or compact profile snapshot, portion/potential, start time and phase progress or last-processed timestamp
- `lastReleaseTime` / upper/lower cooldown data, and a guard for the one-off postmortem event if needed
- Optional last identified meal as debugging information, not the complete historic diet
- Do not blindly trust vanilla `lastMealEatenTotalHours` to identify which food was consumed: it gives a time, not necessarily the consumed item.
- Keep only active work, merge compatible contributions **without erasing remaining potential**, and bound allocations. Capacity overflow policy still needs definition.

## Food effects
- Survey **all vanilla foods**, including raw, cooked and prepared meals. Food profile values: upper/lower gas potential, onset, duration, portion scaling, and possibly relief effects from existing foods.
- Actual food code and complete consumption-event detection must be verified for players, trough feeding, hand-feeding, vegetation, dropped food and modded animal feeding. Prefer observing successful consumption without changing feeding logic.
- Profiles are data, not hardcoded if/else by cabbage; species can adjust relative food effects. Some foods are neutral; no speculative numeric values are finalized.
- Investigate existing vanilla foods that may provide relief without introducing extra items.

## Species registry and third-party support
- Compile a **complete list of vanilla creatures from the installed entity assets**; discuss each one before final classification. Initial proposed classes: excluded, fallback-default, shared profile, and custom profile.
- Relevant terrestrial mammals and rideable land animals are candidates, not just goats. Players use their own profile. Birds/flying species (including modded crows), fish/aquatic species, and mechanical/temporal creatures should be excluded initially.
- A neutral fallback can support eligible creatures even with no custom profile, with rare baseline gas. Eligibility must not assume every entity that moves is a mammal.
- Match modded creatures by domain-qualified entity code / variant patterns and optional source mod. **Profiles for uninstalled mods must be inert, not fatal dependencies.** More specific overrides take precedence; explicit exclusion wins.
- Do not assert every modded creature uses native food AI. Verify its registration and feeding code.
- **User-requested export facility:** provide a species/creature metadata export (JSON or copyable text), suitable for a GitHub Issue or ModDB comment requesting future support. Include game/mod version, entity codes, relevant registered attributes, behaviors, available diet/feeding definitions and known detection capability where safe and available. Avoid personal data, player names, save IDs, seeds and world coordinates. See [CREATURE_EXPORT_SPEC.md](CREATURE_EXPORT_SPEC.md).
- Later, proactively inspect animal mods and their source/definitions for compatibility candidates.

## Diagnostics and test UX
- Do **not** require looking at or clicking fleeing/hostile animals. Offer admin-only commands to list loaded animals by type/area, inspect by entity ID, report herd aggregate stats, track events for a selected animal, and export species definitions.
- Example conceptual commands: `/gas list 100 goat`, `/gas inspect <id>`, `/gas herd 80`, `/gas watch <id>`, `/gas export creatures`. These are proposals, not implemented commands.
- Live lookup only covers loaded entities; do not force chunks to remain loaded merely for debugging.
- Use creative for calm testing (user confirms that frightened/aggro reactions are absent), survival for fear-induced fart frequency. Fear should increase chance modestly, not make it deterministic.

## Configuration and UI
- First support a native JSON configuration with sensible defaults. Research ConfigKit/Config Lib or a native custom dialog for accessible in-game settings; the standard main-menu Mods listing is not assumed to permit per-mod UI before loading a world.
- Possible controls: enable effects, audio volume/distance, herd concurrency budget, probability of background release, digestion multipliers, death release, logging/export/privacy, species settings. Expose few options initially; expand gradually.
- Simulation/config authority belongs on server; private sound playback preferences can remain client-side. Do not require an optional UI library for the mod to function unless consciously chosen.

## Sound design
- Placeholder shared audio first: soft/small, medium, large, and belch. A release size and species profile select a **tagged sound type**, not a hardcoded file count per species.
- Vocabulary to assess: floofer, poof, pop, putt, squeaker, flutter, gravelly, sputter, rumbler, trumpeter, bubbler, and silent. Larger animals may favor bassy sounds; smaller ones higher/shorter.
- First synthetic samples were reviewed by the user and sounded like low-frequency fan/room noise; NOT approved as polished content.
- Investigate properly licensed third-party libraries with author, source, license and edit notes in `SOUND_CREDITS.md`. Sound audition should be done when user is home, not at work without headphones.
- Use sensible positional sound, output distance, concurrency limits; never play a backlog of missed wilderness farts.

## Experimental herd startle cascade (idea, 2026-11-08)

**Status: interesting feature idea, not approved for first release, not implemented.** Player or animal makes an **audible, sufficiently loud release** while near a group of eligible terrestrial animals; some may startle, which temporarily increases their chance of releasing any gas already in their reservoir. A secondary release might startle a few more nearby creatures, creating short-lived, emergent herd chaos. As the herd's reservoir pressure drops and agitation dissipates, events self-extinguish. The aim is **rare, surprising mayhem**, not constant unavoidable noise or animal damage.

Important invariants:
- A startled creature **never receives gas from fear**; fear changes *release probability* only. Upper/lower stores still deplete with each actual emission and never go negative.
- Treat a release as a semantic server-side event with position, intensity and an optional `startleEligible` quality; playing a positional sound alone must not be assumed to make vanilla animal AI hear/react.
- Affected creatures should have species temperament/sensitivity, distance attenuation, reservoir fraction and individual chance; some remain oblivious. A small release can be too quiet to trigger anything.
- Include per-entity *startle refractory period*, global/local cascade budget, short event lifetime and maximum propagation depth; never recursively dispatch indefinitely or amplify fear merely because the same event was heard twice. Apply an event ID to prevent feedback reprocessing.
- Distinguish subtle startle (turn, twitch, brief movement) from real fleeing. **Do not automatically hijack animal AI or make penned livestock escape**; verify game emotion/AI hooks. Stronger physical reactions should be rare, configurable and tested in survival (creative fear AI behaves differently).
- No chain reaction from animals with empty gas; sound concurrency limits can suppress playback without falsifying server-side gas accounting. Do not create offscreen 'replay storms' on chunk load.
- Test with 1, 5, and the user's 30–40 penned goats, then modded animals. Preserve performance with bounded spatial queries and event queues.
- Future options might be `enableStartleCascade`, `startleRadius`, `maxCascadeDepth`, `maxStartledAnimalsPerEvent`, `cooldownSeconds`; parameter values are **not selected**.
- Official Vintage Story entity documentation describes emotion states and AI tasks including `fleeentity`; engine API supports `PlaySoundAt`, but there is **no established generic sound-to-fear linkage** in our research. Confirm hooks before coding.

## Lore and release
- *The Great Silence* is part of the first public release, though collectible tapestry fragments, complex manure/fertilizer integration and large sound libraries belong on later roadmap.
- Lore should fit established Vintage Story tone but use **recognizably funny, lightly veiled references** rather than making the topic so cryptic players miss it.
- Maintain index of authors, dates, story arcs, entry formats, journal section and loot rarity. Include scrolls **and books** in ruins/cracked vessels/chests if compatible with verified game assets; consider a custom resonator cylinder or two if practical.
- See [LORE_INDEX.md](LORE_INDEX.md) for the full chronology and canon cautions.

## Technical foundation pending verification
- Entity lifecycle, save data and death callbacks; food-consumption events for player and animals; calendar time and unload semantics; client/server positional audio; data-driven asset and loot registration; full entity catalog.
- Build from an intentionally small neutral-named mod system; no giant framework before validating hooks.
- All examples in past chat are **conceptual** until compiled and tested.

## Later feature-creep backlog (not initial architecture blockers)
- More individualized bear, bison, goat, rabbit, raccoon and other species sound recordings.
- Collectible historical tapestry fragments and larger lore mystery.
- Animal dung (pellets/pies/droppings), decay/cleanup, farm fertilizer; consider integration with existing manure mods rather than duplicating them.
- Optional complex dietary relief, more biological realism, and expanded UI.
