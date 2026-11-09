## CURRENT SCOPE STATUS — reconciled 2026-11-08 21:15 PST

Creature export remains a **deferred requirement/proposal**, with no implementation or actual creature persistence test completed. Player gas storage, food digestion and manual trumpet release have already passed narrowly scoped tests; this does **not** prove animal data support. The proposed export is opt-in and privacy-safe, never automatic transmission. Continue from [AIMAIN.md](../AIMAIN.md) and [ROADMAP.md](ROADMAP.md) when animal work resumes.

---

# Creature Exporter and Support Request Design

**Last updated:** 2026-11-06  
**Status:** Agreed feature requirement; command format and Vintage Story API implementation are still unverified.

## Problem
The game and third-party mods introduce many animal entities. A user requesting digestive support should not have to hand-copy obscure JSON or chase and target a fleeing gazelle. We want a simple opt-in, privacy-safe export that can be pasted into a GitHub issue or ModDB comment, enabling targeted support in a future build.

## Use case
1. A player installs another mod that adds a new terrestrial animal.
2. Our mod notices the type is not explicitly supported, or its fallback classification is uncertain.
3. The player chooses an export command or future settings-UI action.
4. A small human-readable or JSON report lists candidate entity codes and available related metadata, with clear 'unknown/not available' markers rather than false claims.
5. They review the report and paste/upload it to a support request. Later we research the actual mod and add data-driven profiles, if appropriate.

No automatic uploading, telemetry, world-data collection, or issue creation is required.

## Proposed output shape (fictional example only)
```json
{
  "exportSchemaVersion": 1,
  "gameVersion": "1.22.x",
  "sourceModId": "exampleanimals",
  "entityCode": "exampleanimals:forestboar-adult",
  "entityClass": "unknown",
  "registeredBehaviors": ["example-behavior"],
  "feedingBehaviors": ["unknown"],
  "nutritionOrDietData": "unknown",
  "currentSupport": "unconfigured",
  "candidateProfile": "terrestrial-omnivore",
  "detectionConfidence": "requires-code-review"
}
```
The sample is illustrative and does **not** claim those attributes or behavior names exist in Vintage Story.

## Export scope and safeguards
- Use registered entity definition/type metadata wherever possible; do not need live individual entity targeting.
- Allow specifying a mod/domain, entity code/pattern, or unknown/unsupported only. Include option for exporting a small summary vs one detailed entity.
- Include exact domain-qualified type code, source mod, game version, digestible AI/feeding behavior indicators only when verifiable, profile match, and reason for exclusion/uncertainty.
- Keep output modest in length and avoid duplicate variants where an informative grouped family summary suffices.
- Never include world seed, saved-world name, absolute installation path, coordinates, player identity, inventories, ownership data, secrets, or individual animal instance IDs by default.
- Provide readable plain text for comments and optional JSON file for GitHub Issues, ideally with preview and confirmation before sharing.
- An absent third-party mod should produce no errors or missing-dependency demand; corresponding configured profile remains dormant.
- Profile matching precedence: explicit exclusion > exact species override > grouped family/shared > cautiously eligible fallback; actual rule evaluation needs tests.
- Record whether a modded animal's successful feeding can really be observed; presence of an `eat`-named behavior alone is not proof.
- Friendly issue-template suggestion: game version, source animal mod/version, entity export, animal examples, whether feeding was observed, what should happen.

## Related diagnostic commands (not yet implemented)
- `/gas export creatures` — registered creature metadata, optionally filtered.
- `/gas list <radius> <species>` — inspect loaded nearby entities.
- `/gas inspect <entity-id>` — examine a loaded animal's digestive state without looking directly at it.
- `/gas herd <radius>` — aggregate status and release metrics.
- `/gas watch <entity-id>` — bounded event log for one animal.

Commands are sketches; check command registration permissions and API availability. Live state inspection requires a loaded entity; type export should not need an individual loaded creature.

## Later proactive work
Inspect popular third-party animal mod source and assets and submit/update profile definitions for compatible animals. Where possible allow sharing support additions as small data-file changes rather than new C# code.

## Acceptance criteria
- A player can request support for an unknown mod animal without writing code or chasing it.
- Output is reproducible and clearly identifies the mod/entity involved.
- Player can review and remove information before public sharing.
- Adding a profile for an uninstalled mod changes nothing, causes no crashes, and produces no hard dependency.
- Exported fields that were not observable are honestly marked unknown.
