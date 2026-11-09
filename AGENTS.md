# Collaboration and Development Rules

This repository is for a Vintage Story C#/.NET 10 mod currently being planned. The public mod name is not finalized; `Ventage-Story` is the GitHub repository name.

**Start with [AIMAIN.md](AIMAIN.md)**, then read the latest project documentation before changing code.

## Workflow
- Work incrementally. Explain the intent and the minimal code change before implementing it.
- When giving the user Windows PowerShell instructions, **provide one command at a time and wait for the result**.
- Verify actual installed Vintage Story 1.22.x API behavior, game assets, entity codes, and filesystem paths; do not mistake illustrative snippets for tested code.
- Build a tiny working mod shell first, and test each capability before expanding.
- Keep the core simulation, species/food data, audio, lore, configuration and diagnostics independent where reasonable.
- No speculative invasive Harmony patches. Prefer native APIs where feasible; use a narrow patch only after verifying the needed observation hook is absent.
- Treat `modinfo.json` display title as changeable. Choose a neutral, stable mod ID before serialization/assets are released. Don't embed a provisional title such as Cabbage Carnage in C# namespaces, comments, persistent attribute keys, or public IDs.
- Preserve vanilla mechanics, saved-world compatibility, multiplayer server authority, and mod compatibility.
- **Uninstall-safe by design:** never patch shipped game files on disk, persist vanilla asset mutations, introduce required replacement vanilla item/entity identifiers, or append unregistered collectible behaviors to already registered networked items. Any mod-specific saved data must be namespaced and harmless when the mod is absent. Favor passive observation; if a narrowly verified runtime patch is necessary, it must be scoped, reversible/unpatched on unload, and tested client/server and add/remove. Regress existing world open/save/quit with mod enabled, disabled and re-enabled before release.
- Maintain reasonable performance with dozens of loaded animals and avoid noise explosions.
- Avoid committing user-specific machine paths, world coordinates, save data, access tokens or other personal information to this public repository.
- A suggested value or mocked example is **not** an agreed parameter. Track decisions and unresolved questions.
- Test both vanilla game behavior and interactions with relevant mods; distinguish claims verified by reading source from in-game tests.

## Testing
- A dedicated test world with creative mode simplifies inspection and animal containment. **User confirms from experience that creative mode suppresses ordinary fear/aggro reactions**; therefore use survival testing for frightened/chased animals.
- For survival tests, favor controlled setups rather than chasing wildlife across the map.
- Preserve the user's established world and files; do not make destructive changes without explicit agreement.
- Include diagnostics that do not require targeting a fleeing animal.

## Project documentation checkpoint
Before transitioning to another substantial milestone, or after a consequential design change, important API finding, test outcome or bug fix:
1. Remind the user about the documentation checkpoint.
2. Update `docs/PROJECT_MEMORY.md`, `docs/DECISIONS.md`, `docs/ROADMAP.md`, `docs/LORE_INDEX.md`, and any focused technical documents as relevant.
3. Commit changes and verify GitHub success before saying documentation was saved.
4. Keep the `Next step` and `Last updated` fields current.

There are no automatic background GitHub updates. Small changes can be batched into a checkpoint rather than interrupting every exchange.
