# Runtime Food Registry — Integration Notes

**State (2026-11-08):** JSON catalog, C# reader and content-copy build target added as a **source integration slice**. Build/in-game load **not yet tested on the user's Windows Vintage Story 1.22.7 installation**. The existing generated mod template files were deliberately left untouched, because they are still uncommitted in the user's local clone.

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

From the existing working PowerShell directory inside `mod/lingamsdigestivesim`, after pulling the remote changes, run `dotnet build`. The target should copy the JSON asset into `bin/Debug/Mods/mod/assets/lingamsdigestivesim/config/foodprofiles.json`. No need to manually add a reference to Newtonsoft.Json; the loader uses the API's built-in `IAsset.ToObject<T>()`. In the game log, look for `Loaded 195 food profiles, 13 composite meal rules and 6 excluded intermediates (0 verified item-code bindings).`

**Important:** GitHub currently does not track the user's *existing locally generated* `.csproj`, `modinfo.json` or template starter `ModSystem` file. Pulling these new, noncolliding files should not overwrite that generated local skeleton, but a subsequent `git status` should confirm there are no conflicts. The local `.csproj` and starter files should be staged/committed separately when the user is ready. Avoid replacing their local mod template with a newly invented skeleton.

## Next real implementation

1. Inspect the user's installed Vintage Story 1.22.7 item/block assets to bind real vanilla `game:...` collectible codes and possible variants to these keys; create an audit for uncertain food matches.
2. Capture *successful* actual player eating, using the native saturation event for broad nutrition categories and the meal/content ItemStacks for exact identities/servings. Make sure one mixed bowl does not get counted twice from multiple per-category saturation callbacks. For unknown food, a broad category fallback is a **future decision**, not in this JSON.
3. Then plug the catalog's cumulative gas calculation into the 30-second sliding player eating window, single persistent reservoir, and animal intake logic, and test saves/reloads.