# Portable fart sound library — shared editor/player format (working draft)

**Status:** Designing a **single portable versioned JSON format** that both Fart Lab (the authoring tool) **exports** and Cabbage Carnage (the gameplay mod) **imports, validates, chooses and plays**. **Define this format first**, then implement/test the common renderer and authoring GUI. The separate Fart Lab mod is a *possible* future architecture, not a confirmed decision. Nothing below is built into either mod yet; numbers/examples are proposed for discussion.

## GAS-AWARE FORMAT CONTRACT — newest user requirement (2026-11-08 21:15 PST)

**New explicit user design:** Every saved fart has an **actual gas cost / required reserve**. Include several distinct presets at **75%, 50%, 25%, 10%** and smaller costs, not merely one sound at each threshold. When an entity releases gas, filter the library to **only affordable farts** based on that entity's current stored gas and randomly choose from that eligible pool. Sound characteristics (four pitch points, ADSR, tremolo, duration, instrument, gravel/ending, random ranges) are saved in the same preset and interpreted identically by the editor's Preview and the gameplay runtime. **Preview is always free**.

**Proposed unambiguous unit:** `gasCostPercentOfNormalFull` is percentage of the entity's **normal-full capacity** (the test player's normal capacity is 100 units); a preset costing 75 thus requires **75 units**, regardless of whether the player currently has 80, 121 or 250 units. **It is not 75% of whatever is currently stored**—otherwise even a player with 0.1 units could supposedly afford a large 75%-of-current fart. Validate this interpretation with the user before schema freeze. Normal-full is a scaling reference, **not** a hard reservoir clamp.

### Example JSON — illustrative, not frozen

```json
{
  "format": "lingams-fart-library",
  "schemaVersion": 1,
  "libraryId": "curated",
  "presets": [
    {
      "id": "thunderous-trumpet",
      "name": "Thunderous Trumpet",
      "gasCostPercentOfNormalFull": 75,
      "category": "large",
      "selectionWeight": 1.0,
      "instrument": "game:sounds/voice/trumpet",
      "durationSeconds": 1.5,
      "pitchPoints": [
        [0.00, 0.72], [0.30, 0.82],
        [0.70, 0.68], [1.00, 0.80]
      ],
      "amplitudeADSR": {
        "attackSeconds": 0.03,
        "decaySeconds": 0.20,
        "sustainLevel": 0.72,
        "releaseSeconds": 0.18
      },
      "tremolo": {"rateHz": 4.0, "depth": 0.25},
      "randomPitchPercent": 5
    },
    {
      "id": "big-tuba",
      "name": "Big Tuba",
      "gasCostPercentOfNormalFull": 50,
      "category": "rumble",
      "selectionWeight": 1.0,
      "instrument": "game:sounds/voice/tuba",
      "durationSeconds": 1.9
    },
    {
      "id": "gravelly-25",
      "name": "Gravelly 25",
      "gasCostPercentOfNormalFull": 25,
      "category": "gravelly",
      "selectionWeight": 1.0,
      "instrument": "game:sounds/voice/harmonica",
      "durationSeconds": 0.7
    },
    {
      "id": "quick-squeak",
      "name": "Quick Squeak",
      "gasCostPercentOfNormalFull": 10,
      "category": "squeaker",
      "selectionWeight": 1.0,
      "instrument": "game:sounds/voice/clarinet",
      "durationSeconds": 0.22
    }
  ]
}
```

Each sample is only illustrative; **populate many different sounds at each tier** after hands-on audition. All optional sound fields need documented sensible defaults, so a simple fixed-pitch native trumpet remains valid. Different fart styles need flexible pitch/duration flow within bounded ranges (D59); a 75-unit release need not last longer than a 10-unit release.

### Gameplay selection contract

1. Read and validate stored `gasUnits` and `normalCapacityUnits`. `availablePercent = 100 * gasUnits / normalCapacityUnits`; can exceed 100%.
2. Evaluate each preset: `costUnits = normalCapacityUnits * gasCostPercentOfNormalFull / 100`. It is affordable when `0 < costUnits <= gasUnits`. Enforce any optional species/action/pressure suitability separately.
3. Randomly select from the affordable candidate pool using bounded `selectionWeight`; optionally apply pressure bias and anti-repeat logic without breaking the basic affordability guarantee. Example for 100-unit capacity: 121% enables **75/50/25/10**; 48.4% enables **25/10**; 13% enables **10**; 8% needs smaller presets or a special fallback; 0% emits **nothing**.
4. On a **real server-authoritative release**, subtract the selected gas cost **exactly once**; never clamp production or remove gas for mere sound preview / failed release rolls. Sound renderer receives the chosen preset and position, not arbitrary permission to adjust gas.
5. **Very low gas / forced relief exception:** a special **tiny brief squeak/pop** can consume the **exact remaining trace** and set reservoir to 0; don't play a full trumpet and don't repeat a 60%-fraction asymptotic tail. This dynamic low-reserve release is not a normal fixed-cost 10% entry.
6. **Emergency overpressure compatibility:** D47 requires release of **at least 50% of current stored gas** in major overpressure events. At extreme gas above 150% full, a fixed 75%-capacity preset is insufficient for that minimum. Before implementing emergency release, explicitly add a **scalable-cost emergency preset** or other suitably large cost tiers to the schema; **never silently overcharge a fixed-cost preset** or delete surplus gas. Exact scalable mode awaits design review.

**Validation:** Reject/skip invalid or unsupported schema, nonfinite/negative costs, duplicate IDs, bad pitch-node times, unbounded durations, impossible ADSR, hazardous loudness/polyphony, unknown asset paths or executable/remote references. Whitelist current eight `game:sounds/voice/` instruments. Fart Lab's JSON output and Cabbage Carnage's input must be verified with **the same test fixtures**, including round-trip save/load, exact gas deductions, zero-gas silence, near-empty pop and randomized eligible selection. No custom .ogg assets need copying; JSON is a **recipe** that needs the supported game audio renderer, not a standalone audio recording.

**First release gate D60:** a varied, convincing library built with and understood by this shared format is required **before** first public `0.1.0` or `1.0.0`, even if Fart Lab remains private.

---

## Earlier Fart Lab format exploration — retained as history; schema fields superseded by contract above

**Design discussion checkpoint:** 2026-11-08. **Status: proposal only.** No Fart Lab mod, JSON parser, client synthesizer or actual preset export/import is implemented. The user suggested **Fart Lab as a standalone Vintage Story sound-design mod**, with Ventage Story / Cabbage Carnage importing approved sound recipes. Keep the release audio quality gate (D60) separate from the decision of whether to distribute the editor to ordinary users.

## Architecture: optional authoring tool, independent runtime

- **Fart Lab (proposed standalone mod):** client-side audio audition and preset editing. Use native game-owned voice samples, live sample pitch/volume/time adjustments, ADSR, tremolo and optional layered notes. Every **Preview** is local, deliberate and **never consumes gas**.
- **Ventage Story / Cabbage Carnage runtime:** parses the versioned **data-only preset collection** and selects an eligible preset when the server's actual gas-release event supplies *available quantity*, flow/duration and position. It must still function **without Fart Lab installed**.
- **Compatibility contract:** both tools implement the **same validated JSON schema** and equivalent playback interpretation. Prefer common shared source/library at build time if feasible; do **not** require an active dependency on the editing mod. Keep version migration explicit and support import/export across worlds and machines.
- **Singleplayer vs multiplayer:** advanced continuous client-side pitch/volume changes may require an appropriate client runtime to reproduce the sound. A server with a gameplay mod can broadcast native **one-shot** positional sounds to unmodded listeners, as already demonstrated locally for trumpet, but fidelity on other clients has **not** been proven. Design a compatibility fallback instead of promising rich-envelope playback to vanilla multiplayer listeners.
- **Not binary audio:** JSON stores **sound recipes**, not .ogg/.wav PCM or encoded playable recordings. A compatible Vintage Story runtime is needed to render them. We should not redistribute the game's voice audio.

## Proposed version 1 JSON instance — illustrative, NOT an approved format

```json
{
  "format": "lingams-fart-presets",
  "schemaVersion": 1,
  "presets": [
    {
      "id": "gravelly-surprise",
      "name": "The Gravelly Surprise",
      "sample": "game:sounds/voice/trumpet",
      "archetype": "gravelly",
      "durationSeconds": 1.4,
      "pitchEnvelope": [
        { "time": 0.00, "multiplier": 0.72 },
        { "time": 0.28, "multiplier": 0.82 },
        { "time": 0.76, "multiplier": 0.68 },
        { "time": 1.00, "multiplier": 1.45 }
      ],
      "amplitudeADSR": {
        "attackSeconds": 0.03,
        "decaySeconds": 0.18,
        "sustainLevel": 0.68,
        "releaseSeconds": 0.15
      },
      "tremolo": {
        "frequencyHz": 5.0,
        "depth": 0.30
      },
      "vibrato": {
        "frequencyHz": 3.5,
        "depthSemitones": 0.15
      },
      "finale": {
        "type": "optional-short-squeak",
        "sample": "game:sounds/voice/altoflute",
        "probability": 0.25
      },
      "randomize": {
        "pitchMultiplierPercent": 5,
        "durationPercent": 10
      },
      "enabledForRandomPool": true,
      "selectionWeight": 1.0
    }
  ]
}
```

- The four normalized `pitchEnvelope` times must start at 0 and end at 1, strictly increase, and each pitch multiplier must pass a safe bounds check. For longer curves, we can later support arbitrary node counts without breaking version 1 or keep exactly four and migrate deliberately.
- The **ADSR** envelope is independent of the pitch trajectory; the release phase must fit within the event's total duration.
- The **archetype** is an audio category, not a biological gas quantity. The actual server release decides how much gas is expended; sound presets should never directly modify stored gas or invent gas. The sound preset may suggest a suitable flow or length range, subject to authoritative bounds.
- `sample` and optional finale must refer to an allowed set of existing game-owned sounds: accordion, altoflute, clarinet, harmonica, oboe, sax, trumpet and tuba under `game:sounds/voice/`. No arbitrary filesystem paths, remote URLs, object construction or executable expressions.
- All numeric values require finite safe min/max bounds; lengths, amplitudes, pitch multipliers, modulation frequencies and probability must be clamped or rejected. A corrupted or newer unsupported schema should fail safely, keeping the game loadable.
- Audio preview can store a temporary draft; saved presets should be **named and persistent** across Fart Lab restarts. Export/import should be explicit and a curated selection can be embedded as gameplay mod assets. Avoid assuming one app's local mod-config folder is automatically shared with another mod.
- `enabledForRandomPool` and `selectionWeight` are proposed personal/curator options. User can choose whether to expose these to ordinary players later, and whether to ship Fart Lab at all. Do not use a preset to bypass pressure limits, animal audio budgets or cooldowns.

## Development sequence

1. In an **isolated audio harness**, verify that `ILoadedSound` actually supports the required continuous pitch/volume controls, stop/fade, accurate duration handling and sample layering in the user's Vintage Story 1.22.7 installation.
2. Freeze a minimal **playback schema** only after confirming what the engine can reproduce, using a single common parser/validator.
3. Implement editing/Preview/Randomize in Fart Lab; save/export a few handcrafted parameter presets to a JSON file. Import those files independently in the gameplay mod and verify equivalent playback.
4. Build **a varied curated sound library** for the first release (D60). Determine ordinary-player Fart Lab distribution **later**. Prioritize safe, sensible multiplayer fallback.
5. Preserve and test version migration (schemaVersion increases) before breaking older preset libraries.

**Source research:** [AUDIO_API_AUDIT.md](AUDIO_API_AUDIT.md); [PROJECT_MEMORY.md](PROJECT_MEMORY.md); [DECISIONS.md](DECISIONS.md); [ROADMAP.md](ROADMAP.md).
