## Proposed authoring/runtime split (2026-11-08 evening PST)

User suggested making **Fart Lab a separate Vintage Story editor mod**, exporting **versioned JSON sound recipes** into Ventage Story / Cabbage Carnage, rather than embedding the editor into the gameplay mod. This is an exploratory design, not yet built or finalized; the main mod should be able to load curated presets **without Fart Lab installed**. The proposed interchange structure, four-point normalized pitch envelope, ADSR, tremolo, optional finale, random selection weights and input-validation constraints are described in [FART_PRESET_FORMAT.md](FART_PRESET_FORMAT.md). JSON is **not rendered audio**; client audio engine must implement timed sound modifications and multiplayer fallback. D60 still blocks first public release until varied audio has been heard and approved.

---

# Vintage Story audio API audit — fart library research

**Research checkpoint:** 2026-11-08, official Vintage Story 1.22.7 public API reference. This is a feature inventory and engineering experiment plan; it is **not** proof each control works with the installed trumpet .ogg or on remote clients. Do not promise audio effects or custom synthesis without an in-game audition.

## User-governed release gate

Before the **first public release**, regardless of whether its version is called **0.1.0 or 1.0.0**, the mod must have a **good, varied, convincing library of fart sounds**. The sound personality and variety are core gameplay, not optional polish deferred to later updates. The user-facing "Fart Saver" or sound editor is a **possible future feature**, not approved for public exposure. Prototype a developer-only sound lab if it speeds creation/audition of the built-in library. Release only after listening-based approval and functional regressions, with no numeric count of presets imposed without user agreement.

## What the 1.22.7 public API actually provides

| Mechanism | Public API | Control | Side | Confidence / limitation |
| --- | --- | --- | --- | --- |
| Simple sound / 3D broadcast | `IWorldAccessor.PlaySoundAt(...)`, `PlaySoundFor(...)` | Native asset, initial pitch or randomized pitch, volume, range, entity/location, recipients | Server & client | **In-game verified**: `/gas sound` native `game:sounds/voice/trumpet` heard; repeated pitch variations audible; sound-only amount unchanged. One-shot playback does **not** provide a retained cross-network handle for live envelope edits. |
| JSON/struct randomization | `SoundAttributes`: `Location`, `Pitch` (`NatFloat`), `Volume` (`NatFloat`), `Range`, `Type` | Weighted randomized initial pitch/volume via configured distributions; category attenuation/volume | Server & client | Documented; not a built-in synth or timing envelope |
| Controlled sampled playback | `IClientWorldAccessor.LoadSound(SoundParams)` → `ILoadedSound` | Load game-owned `.ogg` sample and keep a client-side handle; `Start`, `Stop`, `Pause`, `Toggle`, `Dispose` | Client | Documented; cleanup/dispose is required for retained handles |
| Live pitch | `ILoadedSound.SetPitch(float)`, `SetPitchOffset(float)` | Four-node pitch curve, glide, vibrato/pitch wobble and subtle randomized bends | Client | Documented that it works **during playback**. Repeated changes require code-driven scheduling; no native four-point sequencer |
| Live volume and fades | `ILoadedSound.SetVolume(float)`, `FadeIn`, `FadeOut`, `FadeTo`, `FadeOutAndStop` | Attack/decay/sustain/release; tremolo, flutter, pop cutoff, gradual tails | Client | Native fade operations exist; a full ADSR/LFO requires mod scheduling. Overlapping FadeXXX calls may cancel completion callback; test event timing |
| Length / trim / repeat | `ILoadedSound.SoundLengthSeconds`, `PlaybackPosition {get;set;}`, `SetLooping(bool)` | Inspect source duration; skip into source, stop early, or loop to extend | Client | The sample's effective playback speed can vary with pitch; exact seek precision, click-free loops and hard-stop timing **must be tested** |
| Filtering | `SoundParams.LowPassFilter`, `ILoadedSound.SetLowPassfiltering(float)` | Dull/soften high frequencies, distant/muffled coloration | Client | Only a low-pass control explicitly exposed here; **no built-in high-pass/band-pass, distortion, compressor or equalizer exposed on this interface** |
| Reverb | `SoundParams.ReverbDecayTime`, `ILoadedSound.SetReverb(float)` | Decay/room ambience effect | Client | Documented; acoustic suitability and whether client environment/settings affect it not tested |
| Position & attenuation | `SoundParams.Position`, `RelativePosition`, `Range`, `ReferenceDistance`; `ILoadedSound.SetPosition` | Player/animal location, movement-following 3D sound, listener distance falloff | Client; server can send positioned one-shots | Client preview or client-installed audio controller needed for continuous motion of modulated sound |
| Playback state | `ILoadedSound.IsPlaying`, `HasStopped`, `IsReady`, `IsDisposed`, `IsPaused`, `Channels` | Prevent overlaps, handle cleanup and audition timing | Client | Documented, not verified in mod |
| Initial sample parameters | `SoundParams.Pitch`, `Volume`, `SoundType`, `ShouldLoop`, `DisposeOnFinish` | Initialize each preview, respect game sound volume type and handle lifetime | Client | Documented. Avoid excessive sustained or overlapping voices |

### What is *not* provided by these public audio interfaces

- No exposed **independent playback-rate/time-stretch** parameter that preserves original pitch. OpenAL-style pitch shifts commonly alter playback rate and duration; verify actual 1.22.7 behavior by ear and timing tests.
- No exposed arbitrary synthesizer oscillator, waveform/sample generation, granular sampler, waveform display/analysis, multiband EQ, compressor, stereo panning automation, clipping distortion or in-place native four-keyframe/ADSR sequencer.
- **Tremolo**, **vibrato**, timed **sputtering**, **four-node pitch envelope**, **ADSR** and **randomized ornamental end pops** are behaviors our own client code can implement by scheduling repeated live parameter updates and/or layering short loaded sounds; API does not hand us one `PlayFart` DSP function.
- No documented server-owned generic live `ILoadedSound` handle for remote vanilla clients. The proven server `PlaySoundAt` approach sends one-shot native sound events; an unmodded client should not be assumed to reproduce an arbitrary multi-second evolving envelope. **First prove what unmodded listeners actually hear** before making compatibility claims.
- For arbitrary duration from a fixed `.ogg`: truncate/fade or loop/overlap cautiously. Pitch is not a pitch-preserving speed knob. Loop clicklessness, sample sections and pitch modulation range are audio-asset dependent.

## Existing game-owned sound palette

Known from the user's installed assets `assets/game/sounds/voice/`: `accordion.ogg`, `altoflute.ogg`, `clarinet.ogg`, `harmonica.ogg`, `oboe.ogg`, `sax.ogg`, `trumpet.ogg`, `tuba.ogg`. Use `game:sounds/voice/<basename>`, not redistributed original recordings. `trumpet` is now **confirmed audible**, with noticeable randomized initial pitch in the current prototype; no other instrument processed in-game yet.

## Proposed experimental program, in order

1. **Source inventory:** measure/display source `SoundLengthSeconds` for all eight samples; audition each unmodified, capture qualitative notes. Prior developer/player subjective categories (small alto flute/clarinet/oboe, gravelly harmonica/tuba, loud trumpet/sax/accordion) are ideas, not final limits.
2. **Single-sample client harness (no gas):** controlled `ILoadedSound` preview with pitch, volume, looping, seeking, stop/fade; verify timer accuracy, sound lifetime, cleanup, and performance.
3. **Envelope test:** adjustable four-node pitch over time, separate ADSR volume, slow irregular volume tremolo, and optional quick squeak/pop overlay; confirm sample playback actually tracks updates smoothly, without crackle or excessive overlaps.
4. **Texture test:** compare lower pitch/slower apparent playback, slight pitch wobble, low-pass/reverb and staccato volume gating to discover what *actually* sounds gravelly; do not assume low-pass alone creates roughness or tremolo produces distortion.
5. **Duration/flow coupling:** choose gas expelled and release-flow profile independently with a **gentle overlapping pitch-to-flow bias** (D59), allow short high-flow gravelly blasts and long but finite low rumbles, and enforce maximum squeaker durations. Trace-gas forced relief is a very brief squeak/pop clearing remaining gas to exactly zero. Audio preview must **never** consume gas.
6. **Curated preset library:** record named seed/preset parameters, adjustable controlled randomness and audition results. Preserve some simple unmodified trumpet sounds, not every release needs complex effects. Decide with user what counts as a good-enough range of distinctive small/medium/large events; no arbitrary minimum count yet.
7. **Multiplayer fidelity:** test what server-only broadcast with unmodified clients actually supports, then design conditional fallback or replication for rich client envelopes if needed. Preserve nearby players' ability to hear core events wherever feasible and respect no-mod/one-client/server deployment limitations. Ensure no audio storm on farms.
8. **User-facing editor?** Only after effective sounds and safe presets exist, **ask the user** whether to grant ordinary players a Fart Saver/editor. Do not assume this feature ships with the first release.

## Already tested versus remaining

- **Passed:** version 1.22.7 server-side positional `/gas sound` built-in trumpet audible and pitch difference perceptible; sound-only command leaves gas untouched. Food gas accumulator and temporary real `/gas fart` accounting have been tested separately.
- **Not yet tested:** all other sample timbres, live `ILoadedSound` manipulation, four-point pitch automation, ADSR, arbitrary duration, tremolo/vibrato, filters/reverb, layering, client/server equivalence, public preset saving and 0.1.0-quality curated sound library.

## Authoritative API documentation

- 1.22.7 API reference landing page: https://apidocs.vintagestory.at/
- ILoadedSound: https://apidocs.vintagestory.at/api/Vintagestory.API.Client.ILoadedSound.html
- SoundParams: https://apidocs.vintagestory.at/api/Vintagestory.API.Client.SoundParams.html
- IClientWorldAccessor: https://apidocs.vintagestory.at/api/Vintagestory.API.Client.IClientWorldAccessor.html
- IWorldAccessor: https://apidocs.vintagestory.at/api/Vintagestory.API.Common.IWorldAccessor.html
- SoundAttributes: https://apidocs.vintagestory.at/api/Vintagestory.API.Common.SoundAttributes.html

**Engineering rule:** First audition and verify narrow API behaviors; then build the local Fart Lab only as much as necessary to create the approved sound library. Do not ship public audio assets from game content or claim an unverified DSP feature. Avoid invasive modification of vanilla audio/voice machinery. Always preserve working test DLL/worlds.