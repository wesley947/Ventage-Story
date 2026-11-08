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

