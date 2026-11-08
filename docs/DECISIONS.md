# Decision Log

**Last updated:** 2026-11-08. This log distinguishes confirmed direction, proposed mechanisms and open issues. Dates are approximate within the planning discussion.

| ID | Decision | Status / rationale |
| --- | --- | --- |
| D01 | Use an extensible C#/.NET 10 mod for Vintage Story 1.22.x; verify actual APIs before coding. | **Agreed.** Incremental workflow and one PowerShell command at a time. |
| D02 | Public name remains changeable; do not hardcode Cabbage Carnage, Ventage Story, or another temporary title into permanent identifiers. | **Agreed.** The approved internal mod ID is `lingamsdigestivesim` (2026-11-08); the repo remains `Ventage-Story` unless separately renamed. |
| D03 | Each entity holds versioned, saved digestive state instead of an external historical database. | **Agreed principle; exact 1.22 storage hook to verify.** |
| D04 | Each consumed portion contributes upper/lower gas potential over its own digestive schedule. Overlapping meals accumulate; new meals never reset older meals or reservoir values. | **Agreed.** Limited active contributions; no unbounded meal history or lost potential on merge. |
| D05 | Separate production, upper/lower accumulation and release; background production can cause rare natural floofers without a recent special food. | **Agreed.** Earlier cabbage-only/no-cabbage-no-gas premise is superseded. |
| D06 | Investigate all vanilla foods including raw/cooked/prepared variants and species-specific responses. | **Agreed.** Exact food values unresolved; research before balance commitments. |
| D07 | Ground-dwelling animals and players included; birds/flyers, aquatic and nonbiological creatures excluded at first. | **Agreed.** Exact census and eligibility mechanism unresolved. |
| D08 | Species rules defined through extensible data profiles; absent third-party mod profiles safely ignored. | **Agreed.** Optional modded creatures can be added without mandatory dependencies. |
| D09 | Provide a safe creature metadata export as JSON/plain text to make community support requests easy to post to GitHub Issues/ModDB comments. | **Agreed design requirement.** Exact data fields/command pending verification. |
| D10 | Fear/chasing/jumping/movement moderately increases chance of releasing stored gas; this is still rare, cooldown-limited and impossible if reservoir is empty. | **Agreed.** Test in survival; user confirms creative suppresses fear/aggro and is useful for calm tests. |
| D11 | Calm animals may retain gas until a substantial release; do not let a 30–40 goat herd become a constant cacophony. | **Agreed.** Separate audio budget from gas accounting. |
| D12 | Optional delayed postmortem gas release may occur a few seconds after an entity's death, once only. | **Agreed concept.** Mechanism and real-world timing to test; dung postponed. |
| D13 | Diagnostic commands should inspect loaded animals by ID/type/radius/herd without requiring aim or proximity to the individual animal. | **Agreed.** Command syntax examples are not final. |
| D14 | Begin with native JSON configuration; optionally provide in-game mod settings using a compatible library or custom UI, without assuming main-menu Mod Manager support. | **Agreed direction.** UI technology to evaluate. |
| D15 | Use shared placeholder sounds during development; classify by release quality (squeak, flutter, gravelly, poof, etc.), allow shared/species sound profiles. | **Agreed.** Final sound selection and credits nearer release. |
| D16 | Lore will be included in the **first public release**, not entirely deferred. | **Agreed.** Scrolls/books first, possible resonator recordings if practical; tapestries later. |
| D17 | Great Silence old-world cause: upper-class embarrassment and prestige medicine, **not fear**. Underground refuge admission later made suppression obligatory amid plague/Rot. | **Agreed story direction.** Historical fictional interpretation, not claim of vanilla canon. |
| D18 | Renewed surface diets and acclimation may undo suppression; returning gas first frightens unfamiliar settlers, then medical reassurance and laughter follow. | **Agreed story arc.** Avoid asserting all humanity spent generations underground; Seraph history differs. |
| D19 | Write lore seriously but with clear, lightly veiled fart references; keep story segments, chronology, authors and placement coherent. | **Agreed.** Future lore spreadsheet/document useful. |
| D20 | Put documentation on GitHub, with one root `AIMAIN.md` pointing to architecture, decisions, roadmap and lore. Remind user of documentation checkpoints and commit changes. | **Agreed and partially implemented.** `AIMAIN.md` committed earlier; this log records next steps. |
| D21 | Do not play sounds for events with no possible listener, nor replay an offscreen backlog after chunk reload. | **Agreed design direction.** Loaded entities can simulate state even out of hearing range. |

| D22 | Protect original game's lore discovery pace: player's **vanilla journal completion** should gradually increase rare *additional* Ventage lore drops; never take away vanilla lore chances. | **Proposed, preferred direction (2026-11-08).** Per-player journal and vessel hooks look promising; chest generation timing, denominator, thresholds, and multiplayer require verification. See `LORE_INDEX.md`. |

| D23 | Allow duplicate lore books and scrolls to remain collectible; repeated physical items can decorate shelves or be traded and should not automatically be excluded. Distinguish new-to-journal status from validity of a physical repeat item. | **Agreed correction (2026-11-08).** Ordinary vanilla world lore can repeat; the Resonance Archives' special player-aware selection does not establish universal duplicate prevention. Do not add an anti-duplicate mechanism without later user agreement. |

| D24 | Any apparently contagious goat-fart commotion must **emerge from independent animal hearing/fear and reservoir-driven release mechanics**. Do not design scripted mayhem, special cascade events or herd frenzy modes. | **Agreed clarification (2026-11-08).** Normal sound perception, temperament, current gas amount and each entity's cooldowns determine outcome; occasional chains are incidental, not a feature-specific sequence. See `PROJECT_MEMORY.md`. |

| D25 | Explore per-ingredient, portion-aware gas modeling for mixed meals (including egg/vegetable combinations and legumes), plus neutral and potential gas-reducing food effects. | **Research direction (2026-11-08), not settled values.** Verify native meal `contents`, partial servings and feeding hooks. Do not reset prior gas or equate nutrition category with fermentability. |
| D26 | Defer hunger-driven stomach growling for Seraphs to a later update, separate from digestive gas reservoirs. | **Agreed deferral (2026-11-08).** Not part of the initial release. If revisited, tie to satiety with rare audio/cooldowns and no damage, debuff or gas production. |

| D27 | Brand original, full-size mods consistently with **Lingam's ...** in the public display name; `Ventage Story` may no longer be best as the published title. | **Preferred direction (2026-11-08).** Exact title still under discussion; *Lingam's* is the creator brand, not a reason to change technical IDs or the GitHub repository immediately. |
| D28 | Adopt `lingamsdigestivesim` as the stable internal Mod ID for the digestive mod's initial project template. | **User approved (2026-11-08).** Verify registry uniqueness before publishing; no source project generated yet. |

## Important open questions
- Confirm `lingamsdigestivesim` has no conflicting public mod registration before release; decide a stable persisted-attribute namespace after inspecting actual 1.22 API.
- Find reliable hooks for successful consumption by each supported species/feeding method; determine what can be inferred from registered entity metadata.
- Exact food effects, digestion curves, gas capacity, overflow behavior, release probability and cooldown defaults are not chosen.
- Validate persistent storage and behavior attachment to vanilla and third-party entities; investigate multiplayer/client/server separation.
- Confirm exact library and packaging steps for settings UI, lore loot/journal registration, custom resonator sounds.
- Determine privacy-safe export content, UI/commands and permissions.
- Clarify initial included species list from actual installed game assets.
