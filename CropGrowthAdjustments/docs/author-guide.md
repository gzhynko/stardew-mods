<- [README.md](../README.md)

This document helps mod authors create a content pack for Crop Growth Adjustments (CGA).

For a working example, see the [example pack](./examples/%5BCGA%5D%20ExampleAdjustment) in this repository or [Grapes All Year Round](https://www.nexusmods.com/stardewvalley/mods/7759) on Nexus.

See the main [README.md](../README.md) for other info.

## Contents

- [Get started](#get-started)
- [The adjustments.json format](#the-adjustmentsjson-format)
- [Field reference](#field-reference)
- [How seasons work](#how-seasons-work)
- [Special sprites](#special-sprites)
- [Troubleshoot](#troubleshoot)
- [Migration guide](#migration-guide)

## Get started

A CGA content pack is a normal SMAPI content pack: a folder in `Mods/` containing a `manifest.json` and an `adjustments.json`, plus an `assets/` folder if you use special sprites.

```
Mods/
   [CGA] Your Pack Name/
      manifest.json
      adjustments.json
      assets/
        example.png
```

Your `manifest.json` must declare the pack as a content pack for CGA:

```js
{
  "Name": "Your Pack Name",
  "Author": "you",
  "Version": "1.0.0",
  "Description": "One or two sentences describing what the pack changes.",
  "UniqueID": "YourName.YourPackName",
  "MinimumApiVersion": "4.2.0",
  "UpdateKeys": [],
  "ContentPackFor": {
    "UniqueID": "GZhynko.CropGrowthAdjustments"
  }
}
```

See the [SMAPI content pack documentation](https://stardewvalleywiki.com/Modding:Content_packs) for more info.

## The adjustments.json format

`adjustments.json` is a list of adjustments, one per crop. A complete example:

```js
[
    {
        // the crop to adjust, identified by the name of its harvested produce
        "CropProduceName": "Grape",

        // seasons the crop grows in
        "SeasonsToGrowIn": "spring, summer, fall, winter",

        // seasons the crop actually bears produce in. Outside of these (but within
        // its grow seasons), the crop grows to maturity and then waits.
        "SeasonsToProduceIn": "fall",

        // optional: seasons where the crop pauses growth entirely. Omit this field if not desired.
        "SeasonsToHibernateIn": "winter",

        // optional: locations where the crop keeps its vanilla behavior. Omit this field if not desired.
        "LocationsWithDefaultSeasonBehavior": "indoors",

        // optional: replace the crop's sprites in certain seasons. Omit this field if not desired.
        "SpecialSpritesForSeasons": [
            {
                "Season": "winter",
                "Sprites": "assets/grape-winter.png",
                // optional: don't apply this sprite for crops planted in these locations. Omit this field if not desired.
                "LocationsToIgnore": "indoors"
            }
        ]
    }
    // , { ...another adjustment... }
]
```

## Field reference

| Field | Required | Format | Notes |
| --- | --- | --- | --- |
| `CropProduceName` | yes* | item name | The name of the crop's **harvested produce** (e.g. `"Grape"`, not `"Grape Starter"`). CGA ignores case and spaces. |
| `CropProduceItemId` | yes* | item id | Alternative to `CropProduceName`. Use this when the produce item's *name* is changed by another mod (e.g. via Content Patcher), or to target an item unambiguously. Qualified (`(O)398`) and unqualified (`398`) ids both work, as do item ids introduced by other mods (`YourModId_Your_New_Crop_Produce`). |
| `SeasonsToGrowIn` | yes | season list | Seasons the crop grows in. |
| `SeasonsToProduceIn` | yes | season list | Seasons the crop can finish growing and bear produce in. |
| `SeasonsToHibernateIn` | no | season list | Seasons where the crop pauses entirely: no growth, no produce, no death. Must not overlap `SeasonsToProduceIn`. |
| `LocationsWithDefaultSeasonBehavior` | no | location list | Locations where CGA leaves the crop fully vanilla (no produce hold-back, no hibernation). |
| `SpecialSpritesForSeasons` | no | list of entries | Seasonal sprite replacements. see [Special sprites](#special-sprites). |

*One of `CropProduceName` or `CropProduceItemId` is required.

**Season lists** are comma-separated strings of `spring`, `summer`, `fall`, `winter` (case and spacing don't matter): `"spring, summer"`. An unknown season name is an error and the whole adjustment is skipped.

**Location lists** are comma-separated location names, matched against the game's internal location names ignoring case and spaces (`"Farm"`, `"IslandWest"`, `"Custom_MyModLocation"`, ...). The special keyword `indoors` matches every location that is not outdoors.

## How seasons work

For each season, what the crop does depends on which of the three season lists include it:

| Season is listed in… | Can plant? | What happens |
| --- | --- | --- |
| grow and produce | yes | Fully normal: grows, matures, produces. |
| grow only | yes | Grows normally, but freezes one step before maturing. It finishes on the first day of the next produce season. Regrowing crops that already produced hold their regrowth the same way. |
| hibernate (alone or with grow) | yes | Complete pause: no growth, no produce, no death. Hibernate takes priority over growing. |
| produce only | yes | Behaves like grow + produce. |
| none of the three | no | The crop dies, and seeds can't be planted. |

Rules and consequences:

- Every listed season is a plantable season. The game decides plantability and survival from the same data, so a crop that survives a season can also be planted in it. A seed planted in a hibernate season simply waits, planted in a grow-only season it grows and then waits.
- A season listed nowhere kills the crop, same as vanilla out-of-season behavior. If you want the crop to survive winter without growing, use `SeasonsToHibernateIn`.
- A season may not be in both `SeasonsToProduceIn` and `SeasonsToHibernateIn`; that adjustment will be skipped during loading.
- Locations where the game itself ignores seasons ignore all of this: greenhouse interiors (including greenhouse-flagged mod locations) and everywhere on Ginger Island. There the crop grows and produces year-round, exactly like vanilla.
- Other interiors (farmhouse, sheds, ...) are NOT exempt automatically: they follow the valley's season, so your grow/produce/hibernate rules apply to garden pots there too. In vanilla, indoor pots grow and produce year-round - if you want to keep that behavior, list `indoors` in `LocationsWithDefaultSeasonBehavior`. Omit it if the crop should follow your season rules even in an indoor pot

Note: the crop fairy can still instantly mature a crop in any season, and a crop that is already harvestable when its produce season ends keeps its produce until picked

## Special sprites

Each entry in `SpecialSpritesForSeasons` has:

| Field | Required | Notes |
| --- | --- | --- |
| `Season` | yes | Exactly one season. |
| `Sprites` | yes | Path to the sprite image, relative to the pack folder (e.g. `"assets/grape-winter.png"`). |
| `LocationsToIgnore` | no | Location list; crops planted in these locations keep their normal sprites. The special keyword `indoors` matches every location that is not outdoors. |

### The sprite image

The image must be exactly 128x32 pixels: one crop row in the same layout the game uses in `TileSheets/crops` - eight 16x32 frames:

| 0 | 1 | 2-5 | 6 | 7 |
| --- | --- | --- | --- | --- |
| seeds (variant) | seeds/first phase | growth phases | mature/harvestable | regrowing after harvest |

The easiest way to get the layout right: copy your crop's row out of the game's `TileSheets/crops` (or the crop's own spritesheet for custom crops) and repaint it. Keep every frame in place - the game picks the frame from the crop's growth phase, so a misplaced frame shows the wrong stage. How many growth frames your crop actually uses depends on its number of phases; unused frames are simply never shown. Frame 7 only matters for crops that regrow after harvest.

Notes:
- Sprites are applied per crop, based on the crop's own location - a grape vine outdoors can be snowy while one in the shed is not, at the same time.
- Sprite changes take effect at the start of each day (and immediately at planting).
- Crops with a tinted color overlay (like some berries) read the overlay from the same row, so repaint frames 6-7 accordingly.

## Troubleshoot

Any of the following may appear on game start if CGA encounters an issue while validating your content pack.

| Message | Meaning / fix |
| --- | --- |
| `Expected adjustments.json to be present in the folder` | The pack folder has no `adjustments.json`. Check the file name and that it isn't nested one folder deeper. |
| `Error while parsing adjustments.json` | Broken JSON - a missing comma or bracket. Check the file with a JSON validator. |
| `adjustments.json contains no adjustments` | The file parsed, but the list is empty. |
| `either CropProduceName or CropProduceItemId must be specified` | The adjustment doesn't say which crop it targets. |
| `invalid SeasonsToGrowIn/SeasonsToProduceIn/SeasonsToHibernateIn: no seasons specified` | The field is missing or empty. `SeasonsToGrowIn` and `SeasonsToProduceIn` are always required. |
| `invalid ...: unknown season 'x'` | A typo in a season name. Only `spring`, `summer`, `fall`, `winter` are valid. |
| `a season cannot be both in SeasonsToProduceIn and SeasonsToHibernateIn` | Contradictory configuration; remove the season from one of the lists. |
| `sprites file '...' does not exist in the content pack` | The `Sprites` path doesn't match a file in your pack. Note paths are relative to the pack folder. |
| `sprites file '...' must be exactly 128x32 pixels` | Resize/re-export the image - see [the sprite image](#the-sprite-image). |
| `Unable to assign ID to <name>` | No item with that name exists. Check the spelling of `CropProduceName`; if another mod renames the item, use `CropProduceItemId` instead. |
| `Unable to get the crop data for <name>` | The item exists, but no crop produces it. `CropProduceName` should name the harvested produce, not the seed/starter. |

## Migration guide
### 1.x -> 2.0.0
- 1.x validation used to be more lenient: a season typo was silently dropped, a missing field caused daily errors but parts of the adjustment still worked. Now invalid adjustments are skipped entirely with one of the errors above.
