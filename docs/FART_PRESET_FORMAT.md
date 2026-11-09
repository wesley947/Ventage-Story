# Portable Fart Preset JSON — proposed interchange format

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
