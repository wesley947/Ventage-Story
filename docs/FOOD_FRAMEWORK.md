# Food Framework: Reusing Vintage Story Nutrition Events and Ingredient Resolvers

**Design investigation — November 8, 2026.** No C# implementation yet. Target Vintage Story 1.22.7, mod ID `lingamsdigestivesim`. Exact gas potential, transit and fermentation parameters not approved.

## New insight: the game already performs useful classification

Vintage Story already classifies the food a player consumes as **Fruit, Vegetable, Grain, Protein or Dairy**, and mixed cooking-pot meals handle multiple ingredient categories. Balanced nutrition increases the player's maximum health over time; it is **not the same thing as immediate food-specific HP healing**. We should reuse native consumption and ingredient processing instead of separately reproducing all cooking recipes.

**Verified official sources:**
- [VS API EntityAgent.ReceiveSaturation](https://github.com/anegostudios/vsapi/blob/master/Common/Entity/EntityAgent.cs) calls `EntityBehavior.OnEntityReceiveSaturation(float saturation, EnumFoodCategory foodCat, float saturationLossDelay, float nutritionGainMultiplier)` on behaviors when receiving saturation. This is an **existing hook** for coarse food categories, without Harmony in principle, **provided the mod behavior is correctly registered/attached**. `ReceiveSaturation` is not guaranteed to mean a particular ItemStack was physically eaten: other code/mods may grant saturation; several calls can originate from one cooked-meal consumption. Its `saturation` input is not necessarily the actual amount ultimately added after hunger clamping.
- [VS API Collectible.tryEatStop](https://github.com/anegostudios/vsapi/blob/master/Common/Collectible/Collectible.cs) on successful ordinary direct-food eating retrieves `FoodNutritionProperties`, calls `ReceiveSaturation(..., FoodCategory, ...)`, then `slot.TakeOut(1)`. A profile lookup of the **actual consumed stack** can identify cabbage vs carrot and egg vs peanut.
- [Survival content BlockMeal.Consume](https://github.com/anegostudios/vssurvivalmod/blob/master/Systems/Cooking/BlockMeal.cs) obtains **ingredient-wise** `FoodNutritionProperties[] multiProps` and calls `ReceiveSaturation` per profile, with `FoodCategory` and satiety, separately accumulating `Health`. It calls `GetContentNutritionProperties` over **content ItemStacks**; `IBlockMealContainer.GetContents`, `GetNonEmptyContents`, `GetQuantityServings`, `GetRecipeCode` expose recipe details. **Crucially** `BlockMeal.Consume` calculates `servingsNeeded = Clamp((MaxSaturation - Saturation) / mealSatpoints, 0, 1)`, and `servingsToEat = Min(remainingServings, servingsNeeded)`: do not assume an entire bowl was actually consumed when nearly full.
- [BlockPie](https://github.com/anegostudios/vssurvivalmod/blob/master/Block/BlockPie.cs) stores crust/filling/topping content stacks and inherits meal logic; need actual eaten slice/portion.
- [Vintage Story Nutrition wiki](https://wiki.vintagestory.at/Nutrition) explains five nutrition bars and health-bonus relationship, and notes full-satiety nutrition gain is capped. [Cooking wiki](https://wiki.vintagestory.at/Cooking) details cookable ingredients and food families.

## Recommended layered capture — avoid double-counting

**Layer A: Nutrition-category notification** — attach a small mod behavior to eligible Seraphs and observe `OnEntityReceiveSaturation`. Gives (category, offered saturation), cheap and useful for diagnostics and basic category fallback, not a reliable detailed ingested meal. A single soup may send several callbacks. Do not naively turn every callback into a meal without identifying the consuming action or a deduplication/correlation policy.

**Layer B: Real consumption context** — for known direct foods and crafted meals inspect the pre-consumption ItemStack/meal `GetContents` **at the point a server-side consumption really succeeded**, with precisely the proportion actually eaten. A plain carrot yields a carrot contribution; cooked cabbage-and-peanut stew yields both individual ingredient contributions, not just Vegetable+Protein; egg+cheese dish yields separate protein+dairy profiles. If the captured context cannot resolve the exact ingredient, fall back to the coarse category only *once*. If detailed food was captured, do **not** separately add the category event from A: **one actual mouthful = one additive intake record**. Investigate whether a behavior alone supplies enough context; targeted Harmony intercept may be needed for `tryEatStop`, `BlockMeal.Consume` or placed meals. Avoid unnecessarily patching the entire food catalog.

**Layer C: Data-driven digestive overrides** — use a simple table keyed by exact base item code or food family, optionally cooking/preservation form. Five broad families serve as catch-all defaults; special high-gas overrides distinguish cabbage, legumes (peanuts/soybeans), milk, cheeses and eggs, then expand to every vanilla food/variant via **runtime inventory of installed registered items/blocks**. Eggs have no lactose, so their elevated gas is a separate Seraph-specific behavior; aged cheeses generally have much less lactose than milk. Food profile **gas units per actual consumed amount** are independent of satiety and max-health bonuses.

## Why *not* enumerate every meal combination

Recipes combine a finite ingredient catalog in many dishes, including claypot meals, pies, liquids and preserved variants. The source exposes individual content stacks, and cooked-meal nutrition already computes over them. Resolve ingredients at meal-eating time and multiply by actual fraction eaten, instead of listing hundreds of thousands of recipe combinations as separate JSON entries.

At game startup, build a compact index of registered **vanilla edible content**, including food codes, base family, nutrition and liquid properties, plus composite container types. A **first-pass static override table** handles interesting foods. Generate/report an audit of **unclassified foods** so total vanilla coverage is measurable rather than assumed. Third-party modded foods can use nutritional-category fallback.

## Example and quality gates

1. Four plain carrots eaten in one 30-second rolling inactivity session: one pending mixed-food session with `carrot:4`.
2. Alternate carrot, peanut, carrot, peanut: `carrot:2, peanut:2` regardless of order, still one session; exact food profile overrides rather than only two category totals.
3. Cabbage + peanut mixed stew: existing game resolver tells us exact constituent food stacks and proportions; add each ingredient's gas potential **only once**.
4. When only 0.2 servings of a meal is eaten due to satiety constraints, count 0.2, not 1.
5. Food category notification from a health/admin/mod source without an actual eaten item must not accidentally register an invented meal.
6. At full satiety, directly eaten foods may still actually consume an item while cooked meals may consume no serving; use **actual consumption result**, never nutrition-bar change alone.
7. On a successful player intake, refresh the pending session's inactivity deadline to **30 real seconds**, **not +30 seconds**, and timestamp finished stomach meal at inactivity closure (user preference). Serialize pending data with gas and digestion progress.
8. Animals receive distinct direct-feeding observations and slow species baseline intestinal gas; don't impose the player eating-session timer.

## Digestion handoff — simple delay only

**Accepted user clarification (2026-11-08):** Once the player's resettable 30-second intake session closes, it records one timestamp for the mixed meal. For each consumed ingredient, lookup its actual eaten amount and food profile, then create a **delayed intestinal-gas contribution** with `startHour = mealClosedHour + digestiveDelayGameHours`, `durationGameHours` and `totalGasPotential`. There is **no progressive stomach transit**, stomach reservoir or burping: “stomach” is the waiting period only. Contributions accumulate gas gradually into one persistent shared reservoir; completed contributions expire without removing gas already generated. Animal direct feeding uses equivalent delayed contributions; background animal production runs independently.

## Next engineering slice

**Meaningful integrated proof** (not isolated 1-line logger): Implement a lightweight food event capture + normalization + diagnostics for plain item, multi-ingredient meal and liquid/slice, with deduplication and actual portion handling. Load the runtime vanilla edible-content registry and print a one-shot diagnostic audit for unmatched foods; do *not* build gas/sound simulation until basic captured intake is validated. Then assign initial gastro profiles and integrate stomach queue.
