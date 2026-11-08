#!/usr/bin/env python3
"""Rebuild the Vintage Story runtime food profile JSON from the editable CSV.

Uses only the Python standard library. Run from any directory:
    python tools/build_food_profiles_json.py

The source CSV is the master; the compact JSON under mod/ is generated data.
"""

from __future__ import annotations

import argparse
import csv
import json
import re
import unicodedata
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_CSV = REPO_ROOT / "data" / "Ventage_Story_Vanilla_Food_Gas_Profiles_IMPLEMENTATION_BASELINE.csv"
DEFAULT_JSON = REPO_ROOT / "mod" / "lingamsdigestivesim" / "assets" / "lingamsdigestivesim" / "config" / "foodprofiles.json"


def slug(name: str) -> str:
    normalized = unicodedata.normalize("NFKD", name)
    ascii_name = normalized.encode("ascii", "ignore").decode("ascii").lower()
    return re.sub(r"[^a-z0-9]+", "-", ascii_name).strip("-")


def convert(rows: list[dict[str, str]]) -> dict:
    foods: list[dict] = []
    composite: list[dict] = []
    excluded: list[dict] = []
    used_keys: set[str] = set()
    used_ids: set[str] = set()

    for row in rows:
        key = slug(row["Food_name"])
        code = row["ID"]
        if not key or key in used_keys or code in used_ids:
            raise ValueError(f"Blank or duplicate key/ID: {code}, {key!r}")
        used_keys.add(key)
        used_ids.add(code)
        common = {"id": code, "key": key, "name": row["Food_name"]}
        category = row["Entry_type"]

        if category in ("Individual food", "Ingredient-only"):
            profile = {
                **common,
                "nutritionCategory": row["Vanilla_nutrition_group"],
                "digestiveFamily": row["Digestive_family"],
                "preparation": row["Preparation_or_form"],
                "portionQuantity": float(row["Portion_quantity"]),
                "portionUnit": row["Portion_unit"],
                "delayGameHours": float(row["PROPOSED_delay_before_gas_game_hours"]),
                "productionGameHours": float(row["PROPOSED_gas_production_duration_game_hours"]),
                "gasUnitsPerPortion": float(row["PROPOSED_gas_units_per_portion"]),
            }
            if category == "Ingredient-only":
                profile["ingredientOnly"] = True
            # Never make up codes: only bind a native game code after asset inspection.
            native_code = row["Vanilla_item_code_VERIFY_IN_GAME"].strip()
            if native_code:
                profile["itemCode"] = native_code
            if not (profile["portionQuantity"] > 0 and profile["delayGameHours"] >= 0
                    and profile["productionGameHours"] > 0 and profile["gasUnitsPerPortion"] >= 0):
                raise ValueError(f"Invalid numeric profile values for {code}")
            foods.append(profile)
        elif category == "Recipe rule; no fixed gas value":
            composite.append({**common, "resolution": "sum-actual-ingredients",
                              "countOnlyActuallyConsumedServing": True})
        elif category == "Exclusion/caution":
            excluded.append(common)
        else:
            raise ValueError(f"Unexpected entry type for {code}: {category!r}")

    return {
        "schemaVersion": 1,
        "profileSet": "seraph-default-draft-2026-11-08",
        "timeUnit": "game-hour",
        "gasUnit": "arbitrary-gas-unit",
        "description": ("Gameplay tuning estimates; full gas at 100 reference units is a separate simulation setting. "
                        "Food delay is one stomach waiting period, followed by gradual gut gas production. No burping."),
        "foods": foods,
        "compositeMeals": composite,
        "excludedIntermediates": excluded,
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--csv", type=Path, default=DEFAULT_CSV)
    parser.add_argument("--out", type=Path, default=DEFAULT_JSON)
    args = parser.parse_args()
    with args.csv.open("r", encoding="utf-8-sig", newline="") as inp:
        rows = list(csv.DictReader(inp))
    catalog = convert(rows)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(catalog, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"Wrote {len(catalog['foods'])} profiles, {len(catalog['compositeMeals'])} meal rules "
          f"and {len(catalog['excludedIntermediates'])} exclusions to {args.out}")


if __name__ == "__main__":
    main()