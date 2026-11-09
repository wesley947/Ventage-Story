# Development Roadmap

**Last updated:** 2026-11-08  
**Project stage:** Verified Vintage Story 1.22.7 test-world load: 195 food profiles + 13 composites + 6 exclusions on server/client. Native collectible audit also successfully generated in-game; audited 158 strong native identity/nutrition mappings for 122 profiles. Secondary bindings asset and C# reader recompiled successfully and **verified in-game on both server and client** (158 bindings each; Nov 8, 16:10).  
**Next step (CRASH HOTFIX, 2026-11-08 16:36):** The new DEBUG direct-food probe caused/strongly correlates with a **client item-type deserialization crash** (`ItemTypeNet.ReadItemTypePacket`, `Dictionary.ContainsKey(null)`) after first launch with correct DLL. It appended an **unregistered custom CollectibleBehavior to server item types at StartServerSide**, making their behavior list inconsistent with client type registration. **Removed the attachment call and deleted the behavior implementation in GitHub.** User must pull, rebuild, copy new DLL and assets, verify hashes, then test world load **before any additional feature development**. One PowerShell command per turn. Work on safe, explicitly registered event/patch strategy only after researching API.

## Documented, not yet implemented
- GitHub root `AIMAIN.md` directs future AI collaborators to the project files.
- `AGENTS.md` and `docs/` planning documents preserve decisions. Treat their existence as documentation, **not as evidence any feature works in the game**.
- The user compiled and loaded the initial template DLL in a separate Creative test world (no functionality yet). The GitHub food JSON and loader code are new and untested in the installed game. Approved internal Mod ID is `lingamsdigestivesim`; public display title remains open.

## Milestones

### M0 — Durable project handoff (in progress)
- [x] Create `AIMAIN.md` entry point.
- [x] Record collaboration workflow, decisions, current architecture, lore and animal-export requirements.
- [ ] Confirm documentation inventory and ensure AIMAIN reflects actual file paths.
- [ ] Review unresolved assumptions/possible contradictions; update and commit.
- [x] Write stepwise implementation/test-gate plan in `docs/IMPLEMENTATION_PLAN.md`.
- [x] Approve stable internal Mod ID `lingamsdigestivesim`; keep public title separately changeable.
- [x] Install official `VintageStory.Mod.Templates@1.22.0` and review `dotnet new vsmod --help`.
- [x] Verify expected game API DLL exists on development PC.

### M1 — Minimum viable working shell
- [ ] Inspect the user's actual Vintage Story 1.22.x installation and installed code-mod template, following `docs/IMPLEMENTATION_PLAN.md`.
- [ ] Create `modinfo.json`, project file, minimal ModSystem and neutral asset namespace.
- [x] Build and load the empty official-template mod successfully in a **separate test world** (user verified), then compile and load the **new JSON food registry** successfully too: both client/server logs confirm 195 profiles, 13 composites, 6 exclusions and two active ModSystems.
- [ ] Add a versioned, native JSON config file and basic logging.

### M2 — Entity data & lifecycle
- [ ] Confirm server-side per-entity persistence APIs using installed game and/or current source.
- [ ] **Separate animal intake from player intake:** players use sliding 30-second inactivity meal batches; animals use direct observed feeding events plus low species-based background intestinal gas, so un-fed vanilla penned animals can still occasionally release without inventing food records or changing survival/husbandry.
- [ ] Add one test attribute to a vanilla animal, save/reload, unload/reload and verify preservation.
- [ ] **No burps or modeled stomach:** stomach is solely a **configurable digestion delay** before per-ingredient intestinal gas production. No separate upper-gas reservoir or future burping scaffolding. Use `IGameCalendar.TotalHours`, preserve overlapping meals, custom calendar speed and sleep/unload catch-up.
- [ ] Define `DigestiveState` with **one persistent intestinal gas reservoir** and active delayed food contributions.
- [ ] Support additive overlapping meals with correct cumulative elapsed-time accounting.
- [ ] Research death/unload behavior before delayed one-off postmortem release implementation.

### M3 — Creature census, food profiles and mod compatibility
- [ ] Generate accurate complete vanilla entity catalog from installed assets.
- [ ] Classify each: excluded / eligible default / shared type / custom.
- [ ] Add data-driven optional domain-qualified third-party profiles; ignore missing mods.
- [ ] Build JSON/plain-text species metadata exporter, suitable for issue submissions. See `CREATURE_EXPORT_SPEC.md`.
- [ ] Survey raw, cooked and prepared vanilla foods and define preliminary profiles.
- [x] Convert the 214-row reviewed food CSV into **195 runtime food JSON profiles, 13 composite recipe rules and 6 exclusions**; add a C# registry loader and MSBuild content-copy target. **Build/packaging and in-game client + server loading verified.** See `docs/FOOD_REGISTRY_INTEGRATION.md`.
- [x] **Native collectible audit:** compiled, installed and generated CSV from the user's test world. 18,614 rows; 158 high-confidence exact code matches for 122 profiles, 73 profiles still need special handling. See `docs/FOOD_CODE_AUDIT_REVIEW.md`.
- [x] **Secondary binding file:** `config/foodcodebindings.json` and reader (158 codes to 122 profiles) compiled successfully, loaded and verified by client and server logs at 16:10 on Nov 8, 2026.
- [ ] Audit actual vanilla game item/block codes for the 195 profiles, then connect ingestion event resolution to profile lookup (current JSON deliberately has no unverified itemCode bindings).
- [ ] **Future mushroom potency (deferred):** Eventually give selected hallucinogenic mushroom foods, such as liberty caps, higher gas potential than ordinary mushrooms while preserving all native special effects. Verify exact edible codes and consumption amounts first; see `docs/FOOD_FRAMEWORK.md`.
- [ ] **Direct-food intake probe:** First DEBUG prototype **FAILED** after correctly installing DLL: game crashed during client item-type loading (Nov 8 16:36). Dangerous server-only post-registration `CollectibleBehaviors` modification **removed in hotfix**. Require reliable load test and a different API interception method before resuming. 
- [ ] **Mixed meals:** verify reading ingredient item stacks from claypots, bowls, crocks and pies, calculate only the portion actually consumed, and combine per-ingredient delayed gas contributions; investigate neutral and reducing foods without erasing previous reservoirs.

- [ ] Verify actual successful-feeding pathways for animals and players; record limitations.

### M4 — Functional simulation
- [ ] Implement background gas production and delayed **intestinal** meal-derived production (no v1 burps), with a **persistent, non-leaking gas reservoir** for players/animals. Gas production never stops or disappears during release cooldown, meal expiry or capacity pressure; only explicit flatulence subtracts stored gas. Pressure thresholds may raise release probability, never silently clamp gas. Test conservation during time skips and across reload.
- [ ] **Overpressure >100% test:** define 100% as normal-full reference (soft limit). Past this, strongly increase odds of a large real fart; target near-certain relief around 125%, reducing current gas by at least 50%. Consider a true release-event fallback at/above 125% (not a clamp), with species-specific capacity, deterministic tests for interval/frames and conservation.
- [ ] Implement small sporadic and larger threshold releases, movement/jump/fear influences.
- [ ] Prototype **pressure-aware crouch relief**: at high pressure a >1s crouch can expel ~75% of stored lower gas with high but non-guaranteed chance; at low pressure (e.g. 25%) an independent >=2s sustained crouch can squeeze out current gas as a small random squeaker. No prior fart required; empty stays silent. Verify 1.22 crouch state, cooldowns and multiplayer.
- [ ] Provide a first **qualitative player digestive-pressure status** (e.g. diagnostic command), then investigate **the built-in Character Stats panel as first-choice UI** (opened via C). Test whether it can be extended safely via 1.22 API; if not, use a compact companion character panel. Avoid a permanent gas HUD/meter by default.
- [ ] Add death hook and delayed one-off release if viable.
- [ ] Handle long elapsed game-time intervals and save/unload/reload, without audio backlog.
- [ ] Add entity/area/herd diagnostics and bounded event logging.

### M5 — Audio and settings
- [ ] **Audition procedural musical fart effects from built-in Vintage Story instrumental voice samples**, using user's preliminary category pools: squeakers (alto flute/clarinet/oboe), loose (harmonica/tuba), loud (sax/trumpet/accordion). Prototype pitch/duration/volume envelopes; verify game assets, 1.22 API, separate animal species sound profiles, listener experience, and multicast behavior. Sound-type-specific sampling is preferred over a fixed or player-configured fart voice; do not add a player voice-selection setting to v1 unless reapproved. Do not copy proprietary game audio into the package.
- [ ] Begin with short common placeholder audio effects, appropriately labeled and credited.
- [ ] Add sound categories/tagged selection and positional playback.
- [ ] Add concurrency/cooldowns so 30–40 goats create elevated activity but not nonstop sound.
- [ ] Support native JSON settings; evaluate optional ConfigKit/Config Lib or custom in-game UI.
- [ ] Validate client/server ownership and multiplayer hearing.
- [ ] **Multiplayer installation matrix:** test server+modded client, server+unmodded client, and **only one player/client has the mod installed** (server does not). Document what can and cannot be replicated to unmodded clients; never assume client-only mods can authoritatively change world behavior.
- [ ] **Minimum acceptance gate — unmodded listeners:** the modded player's actual releases must be audible to nearby **unmodded clients**. In particular test **only one modded client joining an unmodified server**; do not imply this is possible without native permitted actions or server support. Schedule hands-on test with user's son when together around Thanksgiving 2026. If only server-installed version meets the goal, flag the incompatibility explicitly before public release. Animal-wide portable effects and particles are stretch targets.
- [ ] **Zero-client-install auditory proof of concept:** on a server with the mod, broadcast a built-in `game:sounds/voice/oboe` (or another confirmed voice instrument) using `PlaySoundAt` with controlled pitch; verify a **server mod + entirely unmodded listener** hears it. Optionally test vanilla `EntityTalkUtil.Talk(EnumTalkType.Shrug)` as a native rising-voice packet, noting its eye-height position and fixed type envelope. No player voice selector required.
- [ ] **Optional interactive audio experiment:** during an already active gas release, let timely crouch/stand change pitch/duration without generating extra gas. Check whether server-only staggered short sound events are acceptable for vanilla clients, or whether smooth `ILoadedSound.SetPitch` needs an optional client mod. Avoid spam/overlap, reject empty reservoir and honor release bounds.

- [ ] **Dairy and eggs high-impact profiles (2026-11-08):** Profile milk, cottage cheese, cheddar, blue cheese, eggs, cabbage and legumes (peanuts/soybeans), including mixed dishes such as cheese pie and scrambled eggs with cheese. Keep quantities/delays adjustable. Milk/dairy lactose mechanism must not be incorrectly applied to eggs; user wants intentionally strong Seraph reactions. Test game 1.22.7 item IDs before coding.

### M6 — Great Silence lore (required for first release)
- [ ] Add a **fictional dairy-scarcity/lactase-intolerance lore thread** to the Great Silence: underground refuge food restrictions, rediscovered surface dairy, the physician's investigations, and egg-related phenomena as a separate mechanism. Don't imply all Seraphs canonically descended from cave-dwellers.

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
- **Portable client-only presence experiment (optional, not first-release blocker):** can a lone modded client on an otherwise-unmodified multiplayer server invoke *permitted vanilla networked emotes* to create voice sounds that all vanilla clients hear? Test simple `/emote shrug` first; document sound-asset and curve restrictions. Do **not** claim animals/other players can acquire server-side digestive behavior from a client-only mod, or imply arbitrary sound injection into unmodified servers. Avoid spam, respect server configuration. Full multiplayer digestion remains server-side.

- **Visual gas cues (multiplayer):** investigate brief, tastefully restrained **rear-end vanilla particle puffs** aligned with real release events. Prefer effects all clients can see **when server-side support permits**; first verify built-in particle networking and whether unmodded clients can see server-emitted vanilla particles. Treat client-only installation as a separate, likely limited case. Visuals are deferred from sound-first release unless cheap and safe.
- Rich species-specific sound libraries and audio tuning.
- More custom modded fauna support.
- Additional lore books, scrolls, historical figures, resonator recordings, and collectible tapestry fragments.
- Animal dung, pellets/pies, decay, farm fertilizer and potential manure-mod integration.
- Advanced digestive interactions, relief foods, complex settings interface and more specialized AI.
- **Bear upright stance (possible organic trigger):** inspect active bear standing/lookout animation or AI task in VS 1.22; consider slight reservoir-dependent release probability at the natural posture transition, without forcing a release or scripting an event. See `PROJECT_MEMORY.md`.
- **Later update only — hunger gurgles:** if revisited, investigate player satiety, subtle hunger-scaled stomach sounds with cooldowns independent of gas state, and client/server sound ownership. Explicitly excluded from initial release scope.
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
