# Sound Sources, Licenses and Attribution

**Last updated:** 2026-11-06

No third-party sounds have been selected or approved for release. The first experimental procedural WAVs sounded like low-frequency room/fan noise to the user and are **not a final sound library**.

Use temporary sounds while validating gameplay. Later audition relevant sounds privately and replace temporary assets with appropriate recordings or improved synthesis. Keep an entry for **each included audio asset**:

| Asset / internal ID | Original title | Creator | Source link | License | Edits | Credit wording | Approved? |
| --- | --- | --- | --- | --- | --- | --- | --- |
| *None selected yet* | | | | | | | |

Do not confuse 'royalty-free' with permission to redistribute or modify. Verify the terms for each candidate asset (CC0 is often easiest); identify creators and link sources even where attribution is optional. The user wants to acknowledge contributors in release documents. Be conservative with unverified soundboards and compilations.

Possible sound classification tags (design vocabulary, not present files): floofer, poof, pop, putt, squeaker, flutter, gravelly, sputter, rumbler, trumpeter, bubbler, silent; plus belches.

Test the interplay of release size, animal profile, distance, volume, cooldown and herd concurrency. A large goat pen should sound active, **not** continuously deafening.

## Built-in Vintage Story instrument voice investigation — 2026-11-08

The user proposed making comic squeaker/flutter/rumbler sounds by using **the game's own instrumental Seraph/trader voice samples**, modulating pitch, timing and volume (rather than relying exclusively on externally sourced recordings). This is an **audition and feasibility experiment**, not yet an approved final sound library.

Source research: [upstream EntityTalkUtil.cs](https://github.com/anegostudios/vsapi/blob/master/Common/Entity/EntityTalkUtil.cs) uses `ILoadedSound` with variable pitch, volume and duration for the Peanuts-like pseudo-speech; default trader timbre `sounds/voice/saxophone`. [upstream EntityPlayer.cs](https://github.com/anegostudios/vsapi/blob/master/Common/Entity/EntityPlayer.cs) and [BehaviorExtraSkinnable.cs](https://github.com/anegostudios/vssurvivalmod/blob/master/Entity/Behavior/BehaviorExtraSkinnable.cs) indicate Seraph voice samples are selected by instrument and voice pitch through the skin UI.

Prefer **referencing built-in game-owned assets at runtime**, not copying their .ogg files into our mod distribution. Verify local 1.22 file paths/asset domain and API compatibility. Potential technique: use short pitch/volume envelopes in an independent client playback utility, with a low-positioned spatial sound source and server-authorized release. A musical burp/fart aesthetic may be funny but might not actually sound like gas; audition with the user and compare with licensed recordings. Do **not** claim implementation, asset licensing for redistribution, or sound approval until tested.


**Verified local VS 1.22.7 voice assets:** `%APPDATA%\Vintagestory\assets\game\sounds\voice\` includes `accordion.ogg`, `altoflute.ogg`, `clarinet.ogg`, `harmonica.ogg`, `oboe.ogg`, `sax.ogg`, `trumpet.ogg`, and `tuba.ogg`. Note `sax.ogg` (not `saxophone.ogg`, despite the API's default asset path). These are game-owned assets; refer to them through the game's asset system at runtime instead of bundling copies. Next audition an original sound, then try custom envelopes. No user-approved effects yet.


## Uploaded tuba sample acoustic analysis — 2026-11-08

The user uploaded their installed Vintage Story `assets/game/sounds/voice/tuba.ogg` sample to ChatGPT and requested waveform/envelope analysis. **Measured on uploaded file, not a proposed asset:** mono 44.1 kHz audio, duration **1.279 s**, file obtained from VS 1.22.7, peak absolute sample amplitude about **0.637**. A 10 ms RMS envelope rises from near silence to high amplitude across roughly the first **0.15–0.23 s**, stays strong for the middle with some natural amplitude wavering and gradual decline, then drops rapidly to silence mostly in the last **0.1 s**. Harmonic energy suggests a fundamental of approximately **88 Hz (near F2)**, with strong harmonics around **176, 263, 351, 439 Hz**. Subjectively described by user as rapid attack and delayed 'release', corrected to 'decay'; measured result better described as attack, extended sustain/gradual decay, final quick release. **Recording alone cannot establish any hidden synthesis controls.** Audio has not yet been tested as a generated fart.

Experimental opportunity: use an extracted/played fragment or (preferably) game-engine `ILoadedSound` envelopes to shorten sustain and control pitch glide/volume for squeaker vs rumbler. A 1.28-second sustained sample is too long for many small gas sounds if played unmodified. Actual pitch shifting or clipping must be checked for naturalness and artifacts. Do not copy game's `.ogg` into distributed mod; runtime reference only. The user's source file was provided solely for analysis. 

## Proposed sound-type instrument pools — user audition (2026-11-08)

User listened to local eight built-in voice sounds and prefers choosing instruments according to **release category**, not giving each player a fixed or configurable fart voice. **Current preferred experiment**; custom player 'fart instrument' menu is deliberately **deprioritized** as added complexity that might detract from the humor. Exact mapping is provisional and sound processing is untested.

| Release category | Game sample filenames |
| --- | --- |
| Squeakers | `altoflute.ogg`, `clarinet.ogg`, `oboe.ogg` |
| Loose farts | `harmonica.ogg`, `tuba.ogg` |
| Loud farts | `sax.ogg`, `trumpet.ogg`, `accordion.ogg` |

The core release event should choose a category using actual stored/released intestinal gas and event characteristics (with suitable variations); the audio utility may choose a sample within that category with some randomness and varying pitch, duration and envelope. Mod-defined animal profiles may weight categories/instruments without requiring extra client UI. All remain runtime references to vanilla sounds; user listening impressions are not yet approved processed sound effects.

**Squeaker pitch correction (2026-11-08):** User notes that small squeakers ordinarily **increase in pitch as gas is squeezed out**. Design the initial squeaker sound envelope as an **ascending pitch glide over the short release**, rather than the previously suggested falling pitch (possibly with a brief final high squeal and quick fade, if listening tests support it). Try the alto flute, clarinet and oboe pools first. Keep pressure-driven timing, source pitch, envelope shape and variation tunable; verify by audition rather than declaring one exact curve correct. Do not apply this rising contour automatically to loose or loud releases.
