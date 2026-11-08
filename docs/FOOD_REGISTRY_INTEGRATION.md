# Runtime Food Registry — Integration Notes

**State (2026-11-08):** **Verified working in Vintage Story 1.22.7:** compilation succeeded, JSON present in packaged `bin/Debug/Mods/mod`, client and server logs both confirmed 195 foods + 13 composites + 6 exclusions and **two registered ModSystems**. The new native food-code audit below is committed but **not yet pulled/compiled/tested locally**. The existing generated template files are still local-only; do not overwrite them.

## The working files

- Human-editable master: `data/Ventage_Story_Vanilla_Food_Gas_Profiles_IMPLEMENTATION_BASELINE.csv` — full 214-row review spreadsheet, source notes and statuses.
- Game runtime: `mod/lingamsdigestivesim/assets/lingamsdigestivesim/config/foodprofiles.json` — **195 actual food/ingredient profiles**, **13 composite-meal rules**, **six excluded/intermediate entries**. It contains only required lookup, portion, food group, food family and digestion values; no review notes, source URLs or CSV housekeeping columns. All data is provisional gameplay tuning.
- Reader: `mod/lingamsdigestivesim/src/Foods/FoodProfileRegistry.cs` — dictionary lookup by stable ID (`F005`), key (`cabbage`) and **verified** native code (none bound yet); quantity scaling and simple cumulative-delay gas calculation.
- Asset loader: `mod/lingamsdigestivesim/src/Foods/FoodProfilesModSystem.cs` — automatically loaded as an additional `ModSystem` in the one DLL, reads the standard game asset at `AssetsLoaded`. It **does not** alter hunger, produce gas, create sounds or patch item consumption.
- `mod/lingamsdigestivesim/Directory.Build.targets` — MSBuild automatically imports this and copies `assets/**/*` into the mod build output **without editing the locally generated `.csproj`**.
- `tools/build_food_profiles_json.py` — **reproducible standard-library Python CSV→JSON converter**. Rebuilds the runtime asset from the editable baseline with a single command: `python tools/build_food_profiles_json.py`. It validates duplicate keys, types, and nonnegative profile values, and never invents a native game item code. The converter was tested against the current CSV and reproduced exactly the existing JSON.

The permanent `modid` remains `lingamsdigestivesim` and the normal one-mod packaging is unchanged.

## Design decisions applied

- **No modeled stomach or burping:** Each food profile has `delayGameHours`, `productionGameHours`, and `gasUnitsPerPortion`. At completed player intake-session hour `t`, gas starts at `t + delayGameHours`, gradually reaches its potential over `productionGameHours`, then its source record expires. The produced gas remains in one conserved reservoir until an actual fart.
- **Portions matter:** `gasUnitsPerPortion` belongs to a specific `portionQuantity` and `portionUnit`, not necessarily one whole item. Milk is **8 per 0.1 liter**; cabbage is **24 per item**. Be careful when converting liquids to actual quantity.
- **Lower dairy values** adopted from the latest chat proposal for balancing: milk 8 / 0.1 L, cottage cheese 7 / 0.1 L, cheddar 16 / slice, blue cheese 18 / slice, waxed cheddar 16 / slice. Bell peppers stay increased: 12 / item, pickled bell peppers 10.2 / item. All other estimates left as in the previously supplied revised CSV.
- **Exact item codes still unverified:** The CSV's `Vanilla_item_code_VERIFY_IN_GAME` field was blank. The runtime catalog intentionally does **not** invent an item-code mapping; `TryGetByItemCode` returns false until actual codes are audited and added as `itemCode` fields. **The dictionary is usable by key/id now, but automatic ingestion requires mapping/interception and an actual consumption event**. Do not claim the registry alone makes food consumption operational.
- **Composites do not get standalone fixed gas:** Soup, stew, pies, scrambled eggs, bowls/crocks should eventually call the game's actual-portion ingredient resolver; sum the relevant ingredient contributions once. The 13 composite entries are descriptions of **planned resolution behavior**, not working recipe detectors yet.
- **Caution/exclusions:** The six source entries marked inedible/intermediate do not get digestive gas profiles. The base catalog also includes mushroom and beverage categories where edibility/game IDs need further verification. The mod should not override vanilla spoilage, poison, hunger or health mechanics.

## Building with the user's local template

From the existing working PowerShell directory inside `mod/lingamsdigestivesim`, after pulling the remote changes, run `dotnet build`. The target should copy the JSON asset into `bin/Debug/Mods/mod/assets/lingamsdigestivesim/config/foodprofiles.json`. The first compilation exposed a `CS0012` dependency on Newtonsoft.Json through `IAsset.ToObject<T>()`. Fixed by reading `IAsset.ToText()` and deserializing with .NET 10's built-in `System.Text.Json.JsonSerializer.Deserialize<T>()` and case-insensitive property names. **No extra DLL or `.csproj` reference is required**. User pulled fix, recompiled successfully (0 errors), installed files, and verified both client and server JSON load messages. In the game log, look for `Loaded 195 food profiles, 13 composite meal rules and 6 excluded intermediates (0 verified item-code bindings).`

**Important:** GitHub currently does not track the user's *existing locally generated* `.csproj`, `modinfo.json` or template starter `ModSystem` file. Pulling these new, noncolliding files should not overwrite that generated local skeleton, but a subsequent `git status` should confirm there are no conflicts. The local `.csproj` and starter files should be staged/committed separately when the user is ready. Avoid replacing their local mod template with a newly invented skeleton.

## Compilation issue resolved in source (2026-11-08)

The first `dotnet build` with the new loader failed with two `CS0012` errors (missing `Newtonsoft.Json` assembly for the types `JsonSerializerSettings` and `JToken`) at the call to `asset.ToObject<DigestiveFoodCatalog>()`. This is a transitive compile-time dependency of the API helper despite Newtonsoft not being named in our code. The loader now uses `asset.ToText()` plus `System.Text.Json.JsonSerializer.Deserialize<DigestiveFoodCatalog>(..., new JsonSerializerOptions { PropertyNameCaseInsensitive = true })` in `FoodProfilesModSystem.cs`. That accepts the JSON's camelCase fields with PascalCase C# properties; it has no need to modify the user's locally generated `.csproj` or add a Newtonsoft reference. **Source fix committed; not yet recompiled or tested on user machine.** User should pull `main` and rerun the same build command.

## First native food-code identification audit (2026-11-08)

To stop guessing the 195 native identifiers, added `mod/lingamsdigestivesim/src/Foods/VanillaFoodCodeAudit.cs` and called it from `FoodProfilesModSystem.StartServerSide` **in DEBUG builds only**. It inspects `api.World.Collectibles` after registration and writes one report at the user's data directory:

`%APPDATA%\\VintagestoryData\\ModData\\lingamsdigestivesim\\vanilla-food-code-audit.csv`

The report contains every registered **vanilla-domain `game:` item/block code** and native food category/satiety where directly specified, plus **tentative string-based matching suggestions** from our JSON profile names/keys. Missing profiles are included as special rows in the same CSV. The suggestions are **NOT verified** and are deliberately **NOT bound** to `TryGetByItemCode`; manually inspect actual matches and choose a stable native code or pattern/variant mapping. The audit only writes a text file at server startup in a debug build, with a summary in `server-main.log`. It does not touch the game's eating, health, saves, existing items or fluids. Although the code was checked against current `vsapi` method signatures (`ICoreServerAPI`, `IWorldAccessor.Collectibles`, `CollectibleObject.NutritionProps`, `ICoreAPI.GetOrCreateDataPath`, `ModSystem.StartServerSide`), the code has **not yet been built and exercised on the user's PC**.

**One command per user turn.** To test: Git pull; compile; copy Debug output to installed mod; launch test world; inspect audit summary and CSV. The next development milestone is successful food-consumption interception (actual stack + serving) and precise name/code binding; avoid fake consumption inferred from saturation alone.

## Next real implementation

1. Inspect the user's installed Vintage Story 1.22.7 item/block assets to bind real vanilla `game:...` collectible codes and possible variants to these keys; create an audit for uncertain food matches.
2. Capture *successful* actual player eating, using the native saturation event for broad nutrition categories and the meal/content ItemStacks for exact identities/servings. Make sure one mixed bowl does not get counted twice from multiple per-category saturation callbacks. For unknown food, a broad category fallback is a **future decision**, not in this JSON.
3. Then plug the catalog's cumulative gas calculation into the 30-second sliding player eating window, single persistent reservoir, and animal intake logic, and test saves/reloads.