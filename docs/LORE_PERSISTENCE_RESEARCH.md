# Lore Persistence Research — Embedded Vanilla Item-Stack Text

**Last updated:** 2026-11-08  
**Status:** Confirmed source-code observations and an UNTESTED Vintage Story 1.22.x design hypothesis. **No in-game test or assets implemented yet.**

## Question
Can Ventage-Story introduce rare custom-written lore books and scrolls using **existing vanilla collectible item types**, placing the actual writing in each item stack's saved attributes, so the discovered physical object remains readable after the mod is disabled/removed?

## What vanilla source actually establishes

1. [ItemBook.cs](https://github.com/anegostudios/vssurvivalmod/blob/master/Systems/WritingSystem/ItemBook.cs) handles ordinary books and scroll-like readable items. A readable stack is recognized if it contains `text` or `textCodes`; a stack's `title` controls the displayed item name. It opens `GuiDialogReadonlyBook` for readable content.
2. [ModSystemEditableBook.cs](https://github.com/anegostudios/vssurvivalmod/blob/master/Systems/WritingSystem/ModSystemEditableBook.cs) saves handwritten book `text` and `title` with `slot.Itemstack.Attributes.SetString(...)` and marks the slot dirty. Transcription to vanilla parchment copies `text`, `title` and author metadata to the new item stack. This is **per-item data**, not only a reference to a game asset.
3. [GuiDialogReadonlyBook.cs](https://github.com/anegostudios/vssurvivalmod/blob/master/Systems/WritingSystem/GuiDialogReadonlyBook.cs) distinguishes:
   - Direct embedded `text` and `title` (read without an external translation lookup).
   - `textCodes` and `titleCode`, which are resolved with `Lang.Get` against language assets. If those assets came from an uninstalled mod, the original localized content will not necessarily be available.
4. [ItemRandomLore.cs](https://github.com/anegostudios/vssurvivalmod/blob/master/Item/ItemRandomLore.cs) inherits from `ItemBook`, reads a per-stack `category`, and raises a `loreDiscovery` event including the entire cloned item stack. It implements `IGroundStoredParticleEmitter` and emits the vanilla lore-hint particles; `ShouldSpawnGSParticles` currently always returns true in the inspected source. Whether a **new, mod-added journal category** can be handled without the mod must be independently verified.
5. [BlockEntitySign.cs](https://github.com/anegostudios/vssurvivalmod/blob/master/BlockEntity/BESign.cs) stores text in the **placed block entity** via `ToTreeAttributes` / `FromTreeAttributes`. However [BlockSign.cs](https://github.com/anegostudios/vssurvivalmod/blob/master/Block/BlockSign.cs)'s `GetDrops` creates a fresh sign item and does not copy the block entity text, so writing on the sign normally does NOT travel on its dropped item. Sign persistence is not proof of item-drop text persistence.
6. [Vintage Story Remapper](https://wiki.vintagestory.at/Modding:The_Remapper) explains that custom modded item codes may become missing/unknown if their mod disappears. Using **existing game-owned item codes** avoids that particular issue; it does not guarantee every book placement or journal interaction survives unchanged.

## Preferred test design (not yet approved as implementation)
- Use a **vanilla readable book or lore scroll item** (exact registered code must be verified from the installed 1.22.x assets) rather than defining `ventage:hello-world-book`.
- Spawn a new `ItemStack` of this **vanilla** item, with direct attributes: `title = "Hello World"`, `text = "Hello World"`. Consider `signedby` and `category` separately after base persistence is proven.
- Once such an item is generated from a ruin loot pool, the **item type** is the shared vanilla book/scroll, while each **instance's content** is stored on its own item stack. An 'individual unique asset' is not required for each writing. Every identical generated copy can have the same embedded attributes; each is an item-stack instance.
- To distribute, patch loot tables for cracked vessels/chests/ruins to produce the vanilla item type with appropriate stack attributes, if supported in this exact path. A tiny server-side generator is an alternative if loot patching cannot assign stack attributes. Do not rewrite vanilla loot behavior unnecessarily.
- Prefer embedded literal text for uninstall persistence. **Tradeoff:** copies contain their own text, and embedded content does not automatically re-localize when players switch languages or pick up later corrections to the mod's asset strings.
- Lore journal categorization, library entry de-duplication, particle behavior and automatic chapter creation **are separate concerns** from physical item persistence. A vanilla `ItemRandomLore` subtype may give us normal discovery and particles, but a custom category must be tested on uninstall.

## Mandatory test matrix (always use a COPY of a throwaway test world)
1. Generate/obtain a vanilla book item from our test code with `title` and `text` = `Hello World`, using no custom item code. Verify reading and tooltip.
2. Put copies (or duplicates) into **player inventory, chest/container, shelf/bookcase, and on ground**. Save and restart with mod installed; inspect each. Note that ground items can naturally despawn.
3. Save a world backup. **Disable/remove the mod on the test copy only**; load the world and verify each physical item's identity, `title` and full `text` remain readable. Then reenable and check again.
4. Repeat with the exact vanilla lore-scroll subtype. Add `category` on one copy; verify journal capture and behavior of stored journal discovery after the mod is disabled, especially if category labels/language assets were mod-provided.
5. Validate it can actually appear through **rare chest/cracked-vessel/ruin loot**, rather than manual spawning only; ensure generation affects loot added after install as expected.
6. Verify that it is not silently altered by missing-mod remapping on save. Never interpret a successful test in one inventory location as proof of all storage contexts.

## Multiplayer trading test (2026-11-08 — requested)
**Design goal, not verified:** A player can discover duplicate books/scrolls generated by Ventage Story, trade one to another player, and the recipient can read the item's embedded text **even if that client has no Ventage Story installation**. Books should remain physical collectibles for shelves, trade and giveaways, not only journal-unlock tokens.

Test as separate scenarios:
1. Mod installed on server and available where required for world loot generation; recipient joins with **no client-side mod**. Check joining/handshake requirements, inventory transfer, readable embedded `title`/`text`, author metadata, storage/shelf placement and trading back.
2. Fully disable/remove mod **on a copied multiplayer server/world** after a book is saved. Have two unmodded clients trade the saved vanilla-item book and verify attributes survive save/restart.
3. Test already-read duplicate trade: first player's journal says 'already discovered'; second player's journal is initially missing the entry. Verify whether an ordinary vanilla item records a new journal chapter for the second player or merely remains readable. If a custom journal chapter is mod-dependent, do not claim it will register when the mod is absent.
4. Check what happens to special particles, untranslated/custom category metadata, and lore appearance without client-side assets. These features can degrade gracefully without damaging the physical book.

**No assumptions about required-client mods:** A particular mod packaging/setup may require clients to load it when connecting to a server. Test the actual installed 1.22.x build and our eventual mod type; do not claim a no-mod client can always join a modded server. Maintain lore generation primarily server-side if practical.

## Proposed decision, pending test
**Prefer a game-owned item code plus self-contained stack `text` and `title` attributes for first-release lore collectibles** to minimize uninstall risk and avoid modded collectible-ID remapping. Verify the specific book/scroll types, loot-generator behavior, custom journal category and reader UI in the user's own Vintage Story 1.22.x installation before describing it as proven.

This is an important architectural improvement over assuming every lore book requires a new mod-specific item definition.
