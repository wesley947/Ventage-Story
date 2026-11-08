# Project Memory — Vintage Story Digestive-Gas Mod

**Last updated:** 2026-11-08  
**Status:** Planning/bootstrap; no confirmed compiled mod or public release. Approved internal Mod ID: `lingamsdigestivesim`; public display title remains undecided.

## Purpose
Create a humorous but carefully engineered digestive-gas simulation for Vintage Story 1.22.x (C#/.NET 10). Include players and eligible ground animals, vanilla and optional third-party species; keep player effects cosmetic. The creator wants a consistent **Lingam's ...** public naming identity for original full mods. The exact display title is unsettled; *Cabbage Carnage* and *Ventage Story* were previous working titles. The existing GitHub repository is named `Ventage-Story` and the approved permanent internal Mod ID is `lingamsdigestivesim`. Do not conflate repository name, mod ID, and final display name.

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

## Mixed meals and deferred hunger gurgles (2026-11-08)

**Status:** Mixed-meal ingredient effects remain in core research; **hunger gurgling is explicitly deferred to a later update** (user decision, 2026-11-08). Neither feature is implemented; no gas values, hunger thresholds or reduction mechanisms have been finalized.

### Multiple ingredients and portion-aware digestion
- Evaluate the **actual ingredients and quantities in a consumed meal**, not just its displayed recipe name or vanilla nutrition category. Eggs plus cabbage can differ from eggs alone; soybeans and peanuts are legumes even though vanilla may categorize them as `protein`.
- Vintage Story's [meal-container modding guide](https://wiki.vintagestory.at/Modding:Meal_Container) indicates a meal's ingredient item stacks are saved as `contents` attributes on its container (pot, bowl, crock), so inspecting them is plausible. [Cooking documentation](https://wiki.vintagestory.at/Cooking/en) includes scrambled eggs with optional vegetables and stews containing legumes. Verify exact 1.22 APIs for each container type, partial serving size and moment of successful consumption; **do not infer that a person swallowed all ingredients remaining in a pot**.
- For food profiles consider upper/lower gas potential, timing, cooking/process modifier and a per-ingredient portion weight; sum or otherwise combine these into one **additive digestion contribution for the portion eaten** without erasing other active meals or existing reservoirs. Any realistic interactions/synergies between foods are a later optional refinement, not a required special 'eggs + cabbage' hardcoded recipe.
- User's proposed rough classes: cabbage and legumes high gas potential; ordinary meat low/none; some neutral foods; possible foods/herbs that **reduce** digestive load. Eggs may alter smell characteristics more than quantity—speculative and unnecessary for an initial sound-only release. Food-to-gas relationships depend on digestive physiology and individual variability; avoid presenting table values as medical facts.
- **Separate two effects of a 'reducing' food:** (a) reduce only *future, unproduced potential* of that meal/active digestion, without retroactively wiping prior meals, versus (b) a limited, biologically-plausible easing/absorption of *existing* gas over time. Both are proposals requiring research; no instant magically negative gas reservoir.
- Preserve vanilla nutrition, satiety, food consumption, safety, and other gameplay mechanics unchanged. Modded recipe ingredients should use data-driven fallbacks.

### Later enhancement only: hunger-based stomach rumbling (player)
- **Explore** rare stomach rumbling/gurgling when the Seraph becomes increasingly hungry, independently of upper/lower stored gas. The game's [satiety documentation](https://wiki.vintagestory.at/Satiety) reports a 1,500-point default scale and [EntityBehaviorHunger](https://apidocs.vintagestory.at/json-docs/jsondocs/Vintagestory.GameContent.EntityBehaviorHunger.html) exposes hunger/saturation properties; verify per-player runtime access on the authoritative side.
- Proposed curve: at high satiety no or almost no hunger growls; occasional subtle sound at moderate hunger; modestly more likely when very hungry. Use random intervals and cooldowns—not a repetitive sound every time the bar decreases, not any damage/debuff, and not an automatic fart/belch.
- Do not consume gas reservoirs for hunger growls. Choose appropriate non-comic stomach audio distinct from intestinal sounds. Investigate whether existing ambient/idle audio or mods already cover this to avoid overlap; allow settings to disable or attenuate it.
- Hunger gurgling can coexist with active digestion but should not directly manufacture gas.

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

## Emergent reactions to audible releases (user clarification, 2026-11-08)

**Confirmed design principle:** Any temporary herd commotion must arise **organically from existing individual creature mechanics**. There must be **no preplanned/special 'mayhem event', scripted chain reaction, goat-frenzy mode, cascade trigger, forced sequence, group-wide fear effect or artificial finale.** A player release and animal release are treated as the same kind of ordinary audible stimulus, subject to physical loudness/distance and species sensitivity.

At the level of a *single* animal:
- Normal digestion and food history build gas in upper/lower reservoirs according to game time. An animal with no stored gas cannot release it merely because it is startled.
- Any sound that the animal is actually configured to notice may cause an ordinary startled response, according to **its own** hearing radius, temperament, current fear/agitation, chance and ordinary AI state. A tiny/quiet fart should often attract no attention.
- Fear/startle/movement may increase that individual's chance of releasing **already stored** gas, using the same regular gas-release mechanics used everywhere; a release consumes its own reservoir and respects ordinary per-animal cooldowns.
- If that release is audible, nearby animals may each evaluate it independently under the **same** generic rules. If none responds, the chain ends; if others respond, further sounds are just ordinary sounds, not 'next stages' of a planned cascade.
- As each individual's gas depletes and normal fear dissipates, the herd naturally calms. All outcomes are contingent on current state, proximity, temperament and chance; there is no target count or desired crescendo.

**Technical constraint:** Vintage Story positional audio playback may not itself be sensed by animal AI. If needed, we can expose an ordinary local sound/stimulus notification from a gas release, handled by the same animal perception logic as any other relevant stimulus. This is an implementation signal, **not** a scheduled 'mayhem' gameplay event. Do not build a separate cascade manager, propagation-depth cap, global herd-frenzy meter, cascade budget or special `enableStartleCascade` mode. Rely on general per-entity reaction thresholds/cooldowns and bounded spatial/perception work for performance. Keep the distinction between sound being muted for client audio budget and an actual physical event available for server-side sensing.

This possibility should be tested *after* ordinary gas/AI behavior works, in a controlled survival pen containing 1, 5, then ~40 goats. Test for believable rare responses and performance, without tuning to guarantee any particular comedic outcome. No special mechanic is committed to first release.

## Player crouch posture and pressure release (design idea, 2026-11-08)

**User request:** If a player holds crouch for **more than about one real-time second**, and their *lower/intestinal* gas reservoir is sufficiently **loaded**, there should be a **high chance** of an ordinary fart. This is intended as an organic posture-sensitive release, **not a guaranteed scripted sound**, new gas production, or a separate minigame. Specific pressure threshold and probability remain to be balanced after core simulation works.

Potential implementation (not yet API-verified):
- Observe player crouch/sneak input or the authoritative game state; on the transition from not crouching to crouching, start a one-second timer.
- If still crouching after the delay, perform **one pressure- and cooldown-dependent release check per continuous crouch**, not a repeated roll every tick. Standing before the deadline cancels that opportunity.
- A heavily loaded player has a high (but less than certain) chance of release; an empty reservoir **always yields silence**. A successful release consumes stored gas according to the normal simulation.
- Preserve vanilla crouch/sneak, player movement and nutrition; do not fabricate gas or turn this into a forced stealth penalty. Any nearby animal reaction would be a consequence of the **existing** general audible-stimulus mechanics, never a bespoke chain-reaction event.
- Maintain ordinary cooldowns so repeatedly tapping crouch cannot trigger rapid-fire releases. Test especially while crouching for long periods, repeated crouch attempts and multiplayer.
- Verify whether crouch state is reliably observable on the server under Vintage Story 1.22 before implementing.

**Naming:** The technical ID `lingamsdigestivesim` is approved; the public display title **remains a variable/placeholder** pending a later naming decision. Do not confuse display title with code IDs or the GitHub repo name.

## Bear upright idle posture and gas-release opportunity (idea, 2026-11-08)

**User observation:** Vintage Story bears sometimes rise onto hind legs and look around. If the pose or associated AI task can be observed, its ordinary postural transition could momentarily make the bear somewhat more likely to release **existing** gas, as an incidental release during a stretch; do not generate gas or demand a sound. The underlying appearance may be an alert lookout pose rather than a true stretch. Avoid forcing a fart every time the bear stands up or creating any new bear animation.

**API research:** The [game bear wiki](https://wiki.vintagestory.at/Bear) confirms occasional hind-leg standing; [IAnimationManager](https://apidocs.vintagestory.at/api/Vintagestory.API.Common.IAnimationManager.html) has `IsAnimationActive(params string[] anims)`; [entity AI debug documentation](https://wiki.vintagestory.at/Modding:Entity_Behavior_taskai) identifies `/entity debug 1` on server plus `.clientconfig showentitydebuginfo 1` on client to see active tasks and animations. The existing [Noisy Bears](https://mods.vintagestory.at/noisybears) mod uses JSON AI task sound hooks to produce a growl on hind-leg standing (historical versions, not tested for 1.22). **Exact bear animation/task code and server visibility have not been inspected in an installed 1.22 build.**

**Potential implementation:** Discover the actual posture task/animation first. Upon transition to standing upright, **wait approximately one real-time second** before evaluating a one-time, small release-probability opportunity; do not emit gas at the instant the bear rises. At the time of evaluation verify the bear remains upright, is alive, has sufficient stored gas and is not on release cooldown. An unsuccessful roll is normal: standing never guarantees a fart, and nothing should be forced on the animation. If the bear lowers itself before the delay ends, cancel the pending opportunity. Avoid rolling every tick while upright. Use the same per-entity gas reservoir and ordinary release logic as other movement/posture influences. The one-second value is an initial user-suggested tuning point, subject to in-game testing. If animations are client-only, prefer a validated server AI-task state or equivalent rather than a client-authored release.

**Test:** Observe a wild bear in an isolated throwaway survival test world with entity debugging active. Confirm precise animation/task codes while upright; verify that no special release occurs if reservoir is empty, that the bear can stand normally without farting, and that movement/AI is unchanged. Do not disturb the user's established world merely to test this.

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
