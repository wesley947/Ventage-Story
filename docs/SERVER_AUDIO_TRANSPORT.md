# Server-triggered audio in Vintage Story 1.22.7

**Last updated:** 2026-11-14. **Evidence:** static inspection of the released 1.22.7 engine DLLs, bundled game systems, and stock client Ogg files. The client and dedicated-server archives contain identical `VintagestoryLib.dll` files (SHA-256 `e08f22b493b92feaf0aaeb79d22437ea0f7efc38aa7f72a04a47f98bc0e40df0`). The dedicated-server archive matched the MD5 in Anego's release manifest. The existing trumpet diagnostic has been heard in single-player. This audit did not run a multiplayer listening session or rank presets by ear.

## How vanilla clients receive sound

| Path | Server sends | Vanilla client does | Fit for gas releases |
| --- | --- | --- | --- |
| `PlaySoundAt` and `PlaySoundFor` | `ServerMain.SendSound` writes `Packet_ServerSound` (18) with an asset name, XYZ, initial pitch, volume, range, and sound category. `PlaySoundAt` sends to playing clients within range. `PlaySoundFor` sends to one recipient with the engine's all-zero position convention. | `GeneralPacketHandler.HandleSound` calls `ClientMain.PlaySoundAt`. The client range-checks positional sounds, resolves an exact path, sound set, or wildcard, and starts the local Ogg as a positional one-shot. Missing assets are logged. The all-zero position is listener-relative. | Use `PlaySoundAt` with explicit coordinates and exact filenames. A one-shot packet has no timestamp, seek offset, stop handle, loop flag, filter setting, or live pitch or volume curve. The overload with explicit `EnumSoundType` forwards it. The server's `SoundAttributes` overload drops its `Type` and uses the default category in this release. |
| Player emotes and entity talk | `ModSystemEmotes` checks the server-owned player definition's `attributes.emotes` list and sends the emote string in entity event 197. `EntityTalkUtil.Talk` sends a known talk type via entity event 1231. | `EntityPlayer.OnReceivedServerPacket` starts the named animation and maps six built-in emotes to voice routines. `EntityTalkUtil` uses fixed client routines for pitch bends, fades, and certain vibrato. Voice sounds are placed at eye height. | Adding a name to the emote allowlist can expose it as a server action. A new talk type or emote name gives the stock client no new voice routine. Built-in voices are fixed effects with the entity's voice identity. |
| Animation sound cues | `EntityTypeNet` sends entity definitions with serialized animation metadata and sound attributes in `ServerAssets` at join. Runtime animation updates identify the animation and its speed. | `ClientSystemStartup` reconstructs definitions, `AnimationManager` resolves the animation, and `ClientAnimator` runs the sound cues as their frames are reached. `AnimationManager.ShouldPlaySound` supports ordinary positioned shots and animation-scoped loops with stock assets. | A server-authored definition can convey timed cues to a vanilla client. The animation must resolve against available shape data. Playback follows the animator and entity lifecycle. Useful for sound tied to a gesture or machine, with more coupling than a free-standing sound. A loop still gives no arbitrary pitch or volume curve. |
| Specific gameplay events and state | The server can send an existing event such as lightning or synchronize state such as a resonator's disc. Item interactions can also cause an ordinary native sound. | Lightning plays the built-in thunder effect. The resonator looks up the disc's configured stock track and plays it. Footsteps and bone flute demonstrate native sound dispatch with local playback and source-player suppression where appropriate. | These routes run their feature's existing behavior. They provide no general audio renderer for a Fart Lab preset. |

`Packet_Client` and the inspected vanilla command and entity handlers expose no supported counterpart that asks an untouched server to play an arbitrary asset, pitch, or layer sequence for other players. A client-side `PlaySoundAt` ends in local playback. An ordinary `/emote` or held-item interaction can run the server's built-in effects. A coordinated release sound designed by Cabbage Carnage needs server cooperation to reach clean clients through the native one-shot API. This audit does not change D63's unmodified-server release requirement.

### Playback and timing implications

- `LoadedSoundNative` applies the packet pitch through OpenAL, clamped by the game to 0.1–3.0. Pitch changes playback rate too. A 1.28-second tuba at pitch 0.5 takes roughly 2.56 seconds, absent other listener-side effects. Lowering a long trumpet is therefore a poor way to make a short puff.
- `SoundParams` clamps initial volume to 0–1. The stock client also applies sound settings and environmental effects. The engine warns about poor positional attenuation for stereo sources. Use verified mono samples.
- On-demand loads can start asynchronously. Server-delayed events are tick-scheduled and have no client playback timestamp. Exact phase alignment, audio-rate tremolo, and tight repeated restarts are unsuitable as requirements. A few widely enough spaced layers or simultaneous layers with appropriate built-in attacks and tails are reasonable. Use explicit paths so clients do not independently choose wildcard variants.
- For server-only playback pass `dualCallByPlayer: null` so the triggering player receives the same event. Use an explicit release origin near the rear and the entity's dimension-aware position convention. The current entity overload mechanically places the sound at half the selection-box height.
- The inspected server's join identification advertises universal mods. A package genuinely marked `Server` and using stock assets avoids an audio dependency on listeners. The project's local release packaging is outside the tracked checkout and a clean dedicated-client join still needs a live check.

## Smallest implementation to audition

Keep the existing server-side gas system and native playback path. Replace its single trumpet choice with a small table of stock sample names, pitch, volume, and optional delay. Use normal `PlaySoundAt` for each layer and the existing server event callback only for delayed layers. Capture the source position at release. Use one to three shots as an initial design target. The simulation still decides whether a release occurs. Add a free admin audition command with an explicit point, so the same command can exercise stock clips and full presets from the dedicated console. Build a repeating beacon only if repeated localization checks need it. Other mods can reuse the design without making a shared library a prerequisite.

### Verified stock source material

All clips below are mono and present in the released 1.22.7 client. Durations are measured at original speed. The right column is a candidate use to audition.

| Asset | Duration | Candidate use |
| --- | ---: | --- |
| `game:sounds/effect/squish1` and `game:sounds/effect/squish2` | 0.200 s and 0.238 s | Brief wet transient or two offset quiet sputters. |
| `game:sounds/effect/clothrip` | 0.517 s | Short rasp. Try slowing it moderately or layering it quietly under a puff. |
| `game:sounds/block/bellowslarge/bellowlarge-out1` | 0.965 s | Air body. Try higher playback rate to shorten it. |
| `game:sounds/voice/tuba` | 1.279 s | Long, low body. The measured spectrum is much less bright than the current trumpet. |
| `game:sounds/voice/trumpet` | 1.278 s | Existing control for the listening comparison. |

Start with a single squish, a bellows puff plus quieter cloth rip, and a tuba with two short squish accents. These are audition candidates. Quality, relative layer loudness, and remote timing remain listening questions. Use the existing trumpet as the control in one clean-client session. The vanilla `.debug sound list` command can log currently active assets and positions if a clip is hard to identify.

## Review sources

- [Anego's 1.22.7 release](https://www.vintagestory.at/blog.html/news/v1227-auction-house-upgrades-r449/) and [release manifest](https://api.vintagestory.at/stable.json).
- [Vintage Story binary inspection guide](https://wiki.vintagestory.at/Modding:Decompiler). In `VintagestoryLib.dll`, see `ServerMain.PlaySoundAtExceptPlayer`, `ServerMain.SendSound`, `Packet_ServerSound`, `GeneralPacketHandler.HandleSound`, `ClientMain.PlaySoundAtInternal`, `LoadedSoundNative`, `EntityTypeNet`, and `ClientSystemStartup.LoadEntityTypes`. In `VSEssentials.dll`, see `ModSystemEmotes` and the weather systems. In `VintagestoryAPI.dll`, see `EntityPlayer`, `EntityTalkUtil`, `AnimationManager`, and `ClientAnimator`.
- [World sound API](https://apidocs.vintagestory.at/api/Vintagestory.API.Common.IWorldAccessor.html), [event scheduler](https://apidocs.vintagestory.at/api/Vintagestory.API.Common.IEventAPI.html), and [animation sound definition](https://apidocs.vintagestory.at/json-docs/jsondocs/Vintagestory.API.Common.AnimationSound.html).
- [OpenAL pitch definition](https://github.com/kcat/openal-soft/blob/master/include/AL/al.h) and [client-only feasibility audit](CLIENT_ONLY_AUDIO_FEASIBILITY.md).
