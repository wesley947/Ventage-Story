## Shipped 1.22.7 implementation evidence — 2026-11-14

See the [server audio transport audit](SERVER_AUDIO_TRANSPORT.md) for the inspected one-shot packet, client asset lookup, emote vs. talk handling, synchronized animation sound metadata, the `SoundAttributes.Type` overload detail, and measured source clip durations. It also distinguishes static verification from the dedicated-server listening check.

---

## D62 deeper answer: YES, vanilla voice packets generate live modulated notes on listeners — 2026-11-10

**User's accurate objection:** Vanilla player/trader/villager voices are not merely one-shot fixed-pitch samples. The **server can send a known voice behavior and *each unmodified client* renders its own smoothly modulated musical sound using `ILoadedSound`**. Verified directly in public upstream source:

- `EntityTalkUtil.Talk(EnumTalkType)` if called server-side executes `sapi.Network.BroadcastEntityPacket(entity.EntityId, TalkPacketId=1231, SerializerUtil.Serialize(talkType))`. Vanilla `EntityPlayer.OnReceivedServerPacket` handles packet 1231 and calls its local `talkUtil.Talk(tt)`. `EntityPlayer.Initialize` constructs `talkUtil` on **both server and client**, so a compatible server mod can trigger native `player.talkUtil.Talk(EnumTalkType.Shrug)` or `Thrust`, etc. **without a client mod**.
- Clients' `EntityTalkUtil.OnRenderTick` continuously interpolates pitch and volume (`SetPitch` and `FadeTo`) on sounds already playing. `PlaySound` chooses the preexisting voice `.ogg`, positions it at **entity eye/head** height, with **Range=8**. `EnumTalkType` only has the 13 built-in types: Meet, Idle, Hurt, Hurt2, Smallhurt, Death, Purchase, Complain, Goodbye, IdleShort, Laugh, Thrust, Shrug. Specific potentially flatulent-shaped patterns: `Thrust` 0.12s 0.5→0.8 pitch (quick rising pop), `Shrug` 0.6s 0.9→1.5 (squeak), `Death` 2.3s 0.75→0.3 (long descending groan), `Complain` repeated 0.05s rising fragments, `Laugh` repeated short notes. **These patterns are hard-coded in unmodified listeners**.
- `EntityVillager.OnReceivedServerPacket` handles recognized `EntityServerPacketId.Talk` and delegates to local TalkUtil. `TalkAction.Start` running server-side sends the recognized `Talk` packet; this is another proof of server-initiated, client-rendered synth. Trader `EntityTrader` initializes `talkUtil` **on client only**, and `GuiDialogTrader` can call it **locally**, so **not every vendor utterance** demonstrates broadcast to all nearby clients. Distinguish vendor-local conversations from server-triggered NPC speech.
- **What this enables:** A **server-installed Cabbage Carnage** can dispatch native talk events from a vanilla player entity and listeners **without our client mod** can hear modulated voices. The server chooses *which built-in talk type* and *when*, and may sequence events. This is materially more expressive than `serverApi.World.PlaySoundAt` fixed samples. Test whether selected voice timbres make compelling flatus, event interactions, audibility/8-block range and source head positioning. The server does **not** send arbitrary four-point ADSR values, raw sample name or Fart Lab JSON; unmodded clients interpret only predefined enum patterns. Setting a server-side `talkUtil.pitchModifier` by itself does **not** transmit that property to recipient clients. Player voice choice is normally synced through vanilla character/skin attributes and subject to server validation.
- **D63 remains an independent hard requirement:** On a *completely unmodified server*, a lone modded client cannot call its server-side `Talk()` broadcaster; ordinary `/emote` commands are the available known path to cause vanilla server to send predefined voice events. Therefore this finding **greatly strengthens D62 server-companion feasibility**, without proving unrestricted client-only broadcast on a vanilla server.

Sources: [`vsapi/EntityTalkUtil.cs`](https://github.com/anegostudios/vsapi/blob/master/Common/Entity/EntityTalkUtil.cs), [`vsapi/EntityPlayer.cs`](https://github.com/anegostudios/vsapi/blob/master/Common/Entity/EntityPlayer.cs), [`vssurvivalmod/EntityVillager.cs`](https://github.com/anegostudios/vssurvivalmod/blob/master/Lore/Village/EntityVillager.cs), [`vssurvivalmod/TalkAction.cs`](https://github.com/anegostudios/vssurvivalmod/blob/master/Systems/EntityActivitySystem/Action/TalkAction.cs), [`vssurvivalmod/EntityTrader.cs`](https://github.com/anegostudios/vssurvivalmod/blob/master/Systems/Trading/EntityTrader.cs), and [official EntityTalkUtil API](https://apidocs.vintagestory.at/api/Vintagestory.API.Util.EntityTalkUtil.html).

---

## D63 expanded vanilla event research — 2026-11-09

The single-modified-client + vanilla-server + vanilla-listener requirement was researched against client/server `EntityPlayer`, `EntityTalkUtil`, player `CharacterSystem`, `EntityBehaviorExtraSkinnable`, vanilla `ItemFlute`, animation-sound metadata/lifecycle, resonator and entity activity `PlaySoundAction`, and `IClientNetworkAPI`. **Real examples:** `/emote` server→vanilla client native dynamic musical voices; **bone flute held item** `World.PlaySoundAt` in both app sides (native elkcall fixed pitch 0.75/volume 0.5/range 32) is server-routed without a sound mod but requires real item/cooldown. **Limits:** reselecting Seraph voice/pitch is server-permission gated; animation sound can be authored in game entity configuration but isn't a generic client-to-vanilla-server request; raw network sender does not imply a server handler for arbitrary audio path/pitch. For precise positive/negative evidence and test plan see [CLIENT_ONLY_AUDIO_FEASIBILITY.md](CLIENT_ONLY_AUDIO_FEASIBILITY.md). Core remote custom fart remains unproven; do not imply Fart Lab JSON can work in D63 deployment.

---

## D63 RELEASE FEASIBILITY STOP-GATE — no server mod, no other client mod (2026-11-09)

User explicitly clarified goal: ONE modded **client only**, joins an **unmodified vanilla server**, with all other players **unmodified**, yet they hear actual custom farts nearby. This **supersedes the server-installable D62 playback plan as an acceptable release architecture**. The current trumpet source is server-only (`ShouldLoad(Server)`, `serverApi.World.PlaySoundAt(...)`) and worked in singleplayer integrated server only. It does not establish vanilla-server remote broadcast.

**Upstream client→server pathway check:** `EntityPlayer.OnReceivedClientPacket(IServerPlayer, int, byte[])` currently forwards to base then explicitly handles `EntityClientPacketId.SitfloorEdge` (296); the native voice `Talk` event is **server→client**: `EntityTalkUtil.Talk` uses `sapi.Network.BroadcastEntityPacket` packet 1231; vanilla clients interpret it to render internally modulated musical voices. Likewise player emote `EntityServerPacketId.Emote` (197) is received by vanilla clients and maps selected animations to built-in `EnumTalkType` sounds (`wave`→Meet, `nod`→Purchase, `rage`→Complain, `shrug`→Shrug, `facepalm`→IdleShort, `laugh`→Laugh). The **client can ask an unmodified server to perform existing emotes via the documented `/emote` chat command**, but the command has a fixed menu and cannot include arbitrary sample ID, pitch envelope, ADSR, custom time offsets or JSON. `IClientNetworkAPI.SendEntityPacket` sends a packet TO the server, but no generic server-to-listener rebroadcast handler for arbitrary client-designed sound events is exposed. `IWorldAccessor.PlaySoundAt` invoked on the client only plays locally; invoking the **server** `PlaySoundAt` requires server code. Custom network channel needs corresponding server handler, unavailable on unmodified server.

**Strict verdict as of source investigation:** A custom gas engine and sounds can run locally on client; a client-only mod could potentially use a built-in vanilla `/emote` route to cause a **fixed existing voice/emote sound** on other vanilla clients, but this **does not satisfy arbitrary realistic/funny Fart Lab JSON sound playback**. No documented arbitrary vanilla client→server→other clients sound relay with sound-path/pitch/volume parameters. Do NOT promise this. First test actual vanilla emote sound with clean listener, perhaps evaluate pre-existing sonic effects, then decide feasibility; do not pursue unsupported packet injection as if it were a normal API. A dedicated, server-side mod would enable native broadcast but is specifically outside the user's acceptance criteria. If vanilla sound rendering of custom farts is impossible under these constraints, be candid and reconsider project goals with user, not silently change specs.

**Source:** [EntityPlayer.cs](https://github.com/anegostudios/vsapi/blob/master/Common/Entity/EntityPlayer.cs) (client/server packet handlers and `talkTypeByAnimation`); [EntityTalkUtil.cs](https://github.com/anegostudios/vsapi/blob/master/Common/Entity/EntityTalkUtil.cs) (server-to-client 1231 broadcast and client pitch/volume modulation); [IClientNetworkAPI](https://apidocs.vintagestory.at/api/Vintagestory.API.Client.IClientNetworkAPI.html); [Vintage Story `/emote` command](https://wiki.vintagestory.at/List_of_server_commands/en); [Modding: Network API](https://wiki.vintagestory.at/Modding:Network_API).

---

## Vanilla player events: client action, server routing, unmodded-client audio (2026-11-09)

**New confirmed upstream code examples addressing user's exact question:**

1. **Footstep audio uses a dual client/server route.** In upstream [`EntityPlayer.cs`](https://github.com/anegostudios/vsapi/blob/master/Common/Entity/EntityPlayer.cs), `PlayStepSound` selects the current block's `Walk` sound, then calls the private `PlaySound(player, sound, volume, yOffset)`, which in turn calls `World.PlaySoundAt(sound, x, y, z, dimension, srvplayer, volume)`. `srvplayer = player as IServerPlayer` is passed as `dualCallByPlayer` on server to exclude the initiating player, because their client **already plays the same step locally**; remote listeners receive the server-side routed sound. This is a concrete builtin example of a player event with both local prediction and server broadcast, avoiding doubled audio. See `IWorldAccessor.PlaySoundAt` doc for `dualCallByPlayer`.
2. **Voice/emotes broadcast EVENT PACKETS, not server-rendered audio.** [`EntityTalkUtil.Talk`](https://github.com/anegostudios/vsapi/blob/master/Common/Entity/EntityTalkUtil.cs) on server calls `sapi.Network.BroadcastEntityPacket(entity.EntityId, TalkPacketId, SerializerUtil.Serialize(talkType))`. Vanilla client `EntityPlayer.OnReceivedServerPacket` recognizes `TalkPacketId` and emote animation IDs (`wave`, `nod`, `rage`, `shrug`, `facepalm`, `laugh`) and calls its own `talkUtil.Talk`, which uses **client-side** `ILoadedSound` + continuous `SetPitch` and `FadeTo`. Thus an **unmodded client can run an expressive native dynamic sound envelope initiated by a server EVENT** when the event is **already recognized by unmodified game code**.
3. **Crucial limitation:** Native built-in `EnumTalkType`/emote packet only exposes the game's own preset patterns/voice assets; it is **not** a generic Fart Lab JSON or arbitrary multi-node pitch envelope protocol. Do **not** assume custom packet IDs or new talk types can be understood by vanilla clients. Client-only mod cannot arbitrarily make a vanilla server broadcast custom sound events. The server-side mod's current `PlaySoundAt` trumpet remains confirmed only in local singleplayer; unmodified remote client multiuser acceptance still requires testing.

**Design opportunity:** Compare three unmodded-client-compatible audio strategies: (a) server native `PlaySoundAt` one-shots with random initial pitch/volume, (b) bounded sequences of native sound calls, and (c) **existing** built-in expressive `EntityTalkUtil` talk/emote events (dynamic pitch performed by every vanilla client), while treating existing emotes' voice-position, selected voice and fixed musical patterns as limitations. Do not call (c) an arbitrary four-point ADSR editor. This finding expands our **possible server-initiated, vanilla-rendered dynamic sounds**, without making unsupported promises.

---

## D62 — SERVER-AUDIBLE AUDIO FIRST (2026-11-09 verified API and C# source)

**Critical user objective:** The fun depends on nearby players hearing releases **without installing our mod themselves**. **Current audio test was SERVER-INITIATED:** `mod/lingamsdigestivesim/src/Simulation/PlayerGasStateModSystem.cs` loads via `ShouldLoad(Server)` and calls `serverApi.World.PlaySoundAt(TrumpetSound, entity, null, pitch, 20f, volume)`. `/gas sound` and `/gas fart` both use it. The listener's CLIENT still renders the native `game:sounds/voice/trumpet` .ogg. This was **audible in singleplayer**, not yet tested with a different unmodded remote listener.

**Precisely supported with `IWorldAccessor` from a server mod:** select a pre-existing **client-owned** `AssetLocation` and source entity/world coordinates/dimension; set or randomly generate an **INITIAL pitch**; set or randomize **INITIAL volume**; specify 3D **attenuation range**; choose `EnumSoundType` (game's volume-category routing); send world-positioned sound to relevant listeners via `PlaySoundAt` or target one listener via `PlaySoundFor`; avoid duplicated client+server playback through `dualCallByPlayer`. `SoundAttributes` additionally holds `Location`, `Pitch: NatFloat`, `Volume: NatFloat`, `Range`, `Type`. **There is no server `PlaySoundAt(SoundParams)` overload** in the public `IWorldAccessor`; `SoundParams` and `ILoadedSound` are under the **client** API.

**Server composition possibility:** `IEventAPI.RegisterCallback` and `RegisterGameTickListener` can schedule **several separate one-shot native sound events**, each with its own initial pitch/volume/asset/position and short temporal offsets, to approximate an upward-note flourish, flutter/sputter or layered rumbler. The **server cannot stop, trim, loop, fade or change pitch/volume** of an individual client's already-started native one-shot via these methods; all events play their original sample (whose effective speed may vary with pitch). Frequent staggered clips may sound muddy, overlap or spam networking; audition and rate-limit. This method is a **composition hypothesis, NOT an already tested four-node ADSR reproduction**.

**Deployment constraint:** With the gameplay mod **installed on the server**, an unmodified client should be capable of rendering a native game sound referenced by the server, subject to correct mod metadata/handshake/server setup; test with an actually unmodded connected client before calling it verified. If **only one client** has our mod while the **server is vanilla**, the client cannot assume it can inject arbitrary server sound events for everyone else. Vanilla emotes/voice packets might furnish a restricted special case, but are not a general-purpose sound-broadcast API. The `modinfo.json` `side` and `requiredOnClient` flags control packaging/handshake; **the user's local generated `modinfo.json` is not yet tracked in GitHub**, so we cannot claim no client install is required for the *current package*.

**Format/product consequence:** **D61 preset format MUST be server-playable first**: each library preset needs a valid **native server one-shot or bounded timed-sequence playback plan**, and gas cost/selection remain server-authoritative. The Fart Lab preview needs a **server-fidelity audition mode** that demonstrates exactly what an unmodded recipient would hear; elegant continuous client-only pitch-graph/ADSR effects are optional progressive enhancements, **not required playback for the core first-release library**. Do not promise full four-point continuous ADSR on an unmodded client. This priority outranks earlier proposals to build only a client `ILoadedSound` synthesizer.

**Authoritative sources:**
- [IWorldAccessor server/client sound calls](https://apidocs.vintagestory.at/api/Vintagestory.API.Common.IWorldAccessor.html) — explicitly lists `PlaySoundAt`, `PlaySoundFor`, pitch/range/volume and volume type.
- [SoundAttributes](https://apidocs.vintagestory.at/api/Vintagestory.API.Common.SoundAttributes.html) — initial random pitch and volume, range, type.
- [IEventAPI](https://apidocs.vintagestory.at/api/Vintagestory.API.Common.IEventAPI.html) — elapsed-ms callbacks and tick scheduling, not sample-accurate.
- [Client ILoadedSound](https://apidocs.vintagestory.at/api/Vintagestory.API.Client.ILoadedSound.html) — direct live-pitch/volume controls only for client mod runtime.
- [Modinfo](https://wiki.vintagestory.at/index.php/Modinfo) — `side`, `requiredOnClient`, `requiredOnServer` rules.

---

## D61 playback contract prerequisite (2026-11-08, 21:15 PST)

**Before a substantial editor or audio engine**, define a **shared JSON preset schema** so whatever Fart Lab produces, the gameplay mod can load and execute with identical envelope and duration behavior. Each entry has an **explicit gas cost**, with multiple presets at example 75/50/25/10% of normal-full capacity and perhaps lower tiers. Real server-side releases filter to presets with cost no larger than current stored gas; randomly select among eligible sounds, debit the cost only on a real release. Preview must never consume gas. Define special final-residue squeak/pop and scalable overpressure mode so D47 remains satisfied. Versioning, default values, round-trip fixture and source/sample whitelist in [FART_PRESET_FORMAT.md](FART_PRESET_FORMAT.md). Sound API features remain experimental until heard in game. A separate Fart Lab mod is under consideration, not approved as deployment fact.

---

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