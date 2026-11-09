## VERIFIED: no gas after finished digestion — 2026-11-08 evening PST

**USER-VERIFIED FINAL ACCUMULATOR CHECK:** After a two-food active digestion test finished at **121.00%** gas with **zero pending or active contributions**, the user entered `/time add 2` (two further game hours) in the same Creative world and reported `/gas status` was **unchanged**. The completed food sources did not produce new gas after their retirement; accumulated gas remained stored, including the amount over 100% normal-full. Combined with earlier direct-food EAT-PROBE tests, pending meal closure and save/restart, partial digestion/restart, and full production, this is a **successful narrow player-direct-food accumulator validation**. Not covered: repeated-bite reset edge case, exact food-to-gas attribution, meals/liquids, animal production, player switching/multiplayer, mod disable/re-enable world safety. **Next gameplay milestone:** actual gas-consuming manual release plus simple audible placeholder fart sound and separate non-consuming sound diagnostics, followed by intentional crouch behavior; ensure no subtraction unless a genuine release occurs. Preserve one-step PowerShell workflow and tested DLL backup.

---

## Verified completion and 100% overflow — 2026-11-08 evening PST

**IN-GAME PASS (user screenshot):** After the restored partly digested two-food session reached **69.47% gas with 2 active sources**, user entered `/time add 8`. Game reported eight game hours added. `/gas status` then reported **121.00% (121.00 / 100.00 units)** and **"No pending or active foods."** Delta was **+51.53 units** after those 8 hours; total meal gain from pre-meal 15.40% was **+105.60 gas units**. This confirms current build can finish overlapping active contributions, prune completed records, and preserve gas above the 100% normal-full soft threshold (no silent clamping). No gas release, sound, or animal behavior is present yet. Next precise test: advance 2 more game hours *without eating* and verify gas stays at 121.00%, pending and active remain zero (ensure finished sources do not keep producing). Then evaluate when to implement the player release/sound gameplay. The exact food types/quantities behind the two contributions are not individually verified.

---

## Verified continued digestion after reload — 2026-11-08 evening PST

**USER-VERIFIED:** After the previous partial-digestion save/exit/restart test at **46.28% gas** with **two active contributions**, the user entered `/time add 2` in the same Creative test world. Vintage Story reported two game hours added, and `/gas status` then displayed **69.47% (69.47 / 100 units)** with **two active contributions, zero pending**. Delta = **+23.19 gas units**. Thus saved, partly completed digestion remains live and continues accumulating gas after a restart; contributions were not stalled. The exact per-food split is unverified. **Next test:** allow enough additional game hours for both contributions to complete, verify they are removed and gas does not increase further without eating; test overflow over 100% if reached naturally, as gas must not be silently clamped. Prior EAT-PROBE successful intake and initial/zero/pending/completed meal persistence remain established. One command per turn.

---

## Verified partial-digestion restart — 2026-11-08 evening PST

**PASS — real in-game screenshot:** With pending cabbage previously transferred to active digestion, the user used Vintage Story's `/time add 7` (integer hours; `7:00` is rejected), then `/gas status` showed **45.18% (45.18 / 100)** and **2 active food contributions, 0 pending**. User saved and fully restarted the game, then status showed **46.28% (46.28 / 100)** and **the same 2 active food contributions**. Mod retained the existing reservoir and live food progress across a full restart, with a small +1.10 gas delta plausibly attributable to natural game-time progression; no obvious replay/double-credit spike. This is a *partial-digestion* restart smoke test, separate from previously verified fully finished meal persistence and pending-window persistence. Do **not** infer exact cumulative accounting across restart solely from these two readings. The user had eaten multiple cabbages and two contributions are visible; exact per-food quantities, ingredient identity, and source of the 1.10 delta were not isolated. **Next test:** in the dedicated creative world, advance a modest further increment of calendar hours while 2 foods remain active, check gas increases appropriately and completion eventually prunes sources; optionally verify double-credit invariants with diagnostic breakdown later. One PowerShell or in-game command at a time.

---

## Verified pending meal save/restart — 2026-11-08 20:10 PST

**USER-VERIFIED IN-GAME:** Starting with previously earned **15.40% gas**, user ate **one cabbage** and immediately saved/exited Vintage Story, then fully restarted the same Creative test world. `/gas status` after restart showed **Gas: 15.40%** and **Pending: 1 food type(s), 16s remaining; active: 0 food contribution(s)**. Thus an incomplete 30-real-second player eating session is preserved across save/restart along with the accumulated reservoir; it is not prematurely activated and the saved remaining countdown is not reset to 30. This is separate from the earlier confirmed complete-meal restart test. Does not yet verify exact offline countdown pause semantics or active partial-production persistence. Next: stop eating, allow more than 16 real seconds with game active, then `/gas status` should show 0 pending and 1 active cabbage contribution; subsequently sleep for a partial-production test.

---

## Verified completion-state restart persistence — 2026-11-08/09

**USER VERIFIED:** Following successful completion of the two-food digestion test producing **15.40% gas**, the player saved the test world, completely exited and restarted Vintage Story, and reported that `/gas status` remained as expected (15.40% and no pending/active foods). This confirms retention of **completed digestion gas** after restart without observed replay in this test. It does **not** yet verify restart of a pending 30-second meal or a **partially produced active** food contribution, nor does it prove repeat-bite timer reset, cross-player behavior or animals. **Next test:** eat a known bound single cabbage (`game:vegetable-cabbage`, F005, 4.8 game-hour delay plus 9 game-hour linear production), close the 30s window, then advance ~7–9 game-hours via bed sleep to create partially produced gas before save/exit/restart. First confirm cabbage ingestion is observed. Compare status and gas before/after restart to rule out double-credit. Preserve one-step instructions.

---

## Verified first automatic gas cycle — 2026-11-08/09

**In-game verified (one Creative test-world player):** New digestive module compiled cleanly, installed with matching SHA256 DLL hashes, game loaded, status read correctly. Two distinct direct food types appeared in a **23s remaining** pending session with 0% gas. After 30s inactivity, status showed **two active contributions** and zero pending. After sleeping (calendar time advanced), gas automatically became **15.40% (15.40/100)** and there were **no active/pending foods**. The full direct-food path and completed digestion/gas production have been demonstrated for this small meal. This does not yet prove 30s reset on subsequent food, exact ingredient profiles, partial accrual, restart conservation, or general food support.

**Next:** save/exit/restart and check that 15.40% is still there without replay; then deliberate intermediate-progress and multiple-meal tests before considering persistence across digestion verified. Release/sound/animal paths remain absent.

---

# Development Roadmap

**Last updated:** 2026-11-08  
**Project stage:** Verified Vintage Story 1.22.7 test-world load: 195 food profiles + 13 composites + 6 exclusions on server/client. Native collectible audit also successfully generated in-game; audited 158 strong native identity/nutrition mappings for 122 profiles. Secondary bindings asset and C# reader recompiled successfully and **verified in-game on both server and client** (158 bindings each; Nov 8, 16:10).  
**Next step (2026-11-08 evening PST):** Player gas persistence is VERIFIED for one creative-test-world player using the server-only `PlayerGasStateModSystem`: first status initialized at 15.19%, repeated status did not reroll, 63.00% survived a full save/exit/restart, and 0.00% also survived without reinitialization. `gasUnits` and capacity are in a versioned server-side entity subtree. Diagnostic admin commands `/gas status` and `/gas set` work. **Next gameplay gate:** implement a minimum real gas-consuming fart release + basic audible sound; add sound-only diagnostic separately; then test deliberate crouch tiers. Do not claim player meal session, animals, sound, digestion or mod uninstall are implemented. One PowerShell command per response.

- **Confirmed installation:** latest source committed in `mod/lingamsdigestivesim/src/Simulation/PlayerGasStateModSystem.cs` (commit `a2295c4`), .NET 10 Debug build succeeded, copied DLL matched build SHA256; same creative test world loaded. All earlier EAT-PROBE results remain prior verification.
- **Persistent zero is valid:** explicitly distinguish `StateKey` absence (first initialize at 15–20%) from an existing state with `gasUnits = 0`; this is proven across one game restart.
- **Priorities:** build audible release mechanics and fun gameplay before Great Silence lore assets, retaining the complete lore planning documents for later.

## New untested player digestion source — 2026-11-08

**SOURCE ONLY — do not mark implemented/tested until the user builds and runs it.** A new server-only `PlayerDigestionModSystem` subscribes to the existing successful direct-food Harmony probe's notification event (DEBUG builds), records food as a saved player-specific pending 30-real-second inactivity session, resets (not adds) remaining time after each bite, closes at current world game hour, calculates each food's finite delayed intestinal production and adds only unaccounted new gas to the tested server-side player reservoir. It persists active and pending state in a versioned JSON field in that entity's namespaced attributes; pending timers pause while disconnected. `/gas status` reports session counts. **Not implemented:** other food consumption paths, animals, release, sound, UI. **Next:** git pull; Debug build; fix compile errors before deploying, preserve EAT-PROBE backup; test no startup crash, timer reset, closure, delayed production, restart conservation, zero gas semantics. One PowerShell command at a time.

## Release-blocking world safety requirement (2026-11-08)

**Explicit user requirement:** A user can install, disable or uninstall this mod and continue using the **same existing Vintage Story world** without missing vanilla types, broken recipes/inventory or mod-dependent loading. Never change vanilla game files or item/block/recipe definitions permanently; avoid custom replacement item IDs or server-only late networked collectible behavior injection. Future digestive saved state must be separate, namespaced, versioned, and safely ignored when code is absent. A runtime observation patch, if used, must be narrowly scoped and reversible, with client/server serialization and no-duplicate/eating tests. **Acceptance tests (not yet performed):** create a copy of test world; load/save with mod on; disable/remove; reload same world, verify inventory/food and normal mechanics, save/restart again; reinstall mod and verify stability. Do not declare uninstallation safe until actually tested.

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
- [x] **Player persistence smoke test:** versioned namespaced `Entity.Attributes` state (15–20% on first use, admin `/gas status`, `/gas set`); in-game save/exit/restart verified 63.00% and 0.00% for one player. Animal persistence, unload, multiplayer and uninstall compatibility remain untested.
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
- [x] **Direct-food intake probe:** First injected-behavior approach failed and was removed. Replacement DEBUG Harmony observer verified violet web cap, rye and spelt consumption with corresponding satiety increases. Other intake paths and uninstall regression remain untested. 
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
