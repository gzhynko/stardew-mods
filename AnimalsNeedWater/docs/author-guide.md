<- [README.md](../README.md)

This document helps mod authors make their mods compatible with Animals Need Water (ANW) or alter ANW's behavior.

| What you want                                               | Where                                                        |
|-------------------------------------------------------------|--------------------------------------------------------------|
| Troughs in a custom/redesigned animal building              | ANW's `Mods/GZhynko.AnimalsNeedWater/TroughPlacements` asset |
| A custom exterior overlay over a custom/redesigned building | ANW's `Mods/GZhynko.AnimalsNeedWater/TroughPlacements` asset |
| A custom item animals can drink from                        | `CustomFields` on your `Data/BigCraftables` entry            |
| Custom animals that drink differently or don't drink at all | `CustomFields` on your `Data/FarmAnimals` entry              |

Every ANW CustomField key is prefixed `GZhynko.AnimalsNeedWater/`, and all values are strings: e.g, `"true"`, not`true`.

Add ANW as an optional dependency in your mod's `manifest.json`, and wrap ANW-specific patches in
`"When": { "HasMod": "GZhynko.AnimalsNeedWater" }` so your pack works without it.

## Contents

- [Building profiles](#building-profiles)
- [Water sources](#water-sources)
- [Animal settings](#animal-settings)

## Building profiles

Trough placements, tilesheets and empty-trough overlays are defined in ANW's `Mods/GZhynko.AnimalsNeedWater/TroughPlacements` asset. 
See the [example content pack](examples/) - its `content.json` is a good starting point.

ANW will log any issues it encounters while reading the `TroughPlacements` asset, incl. if e.g. tile coordinates are outside the target map. 

Use the `anw_toggle_overlay` command in the SMAPI console to highlight your profile tiles in-game.

## Water sources

Any Big Craftable can become something animals drink from. Animals only drink from these when placed indoors.

Add these to the `CustomFields` of your own `Data/BigCraftables` entry:

| Key                   | Required | Type | Default | Meaning                                                                                                                      |
|-----------------------|----------|------|---------|------------------------------------------------------------------------------------------------------------------------------|
| `IsWaterSource`       | yes      | bool | -       | Marks this item as a water source. Omitting it means none of the other keys do anything.                                     |
| `NeedsRefilling`      | no       | bool | `true`  | If false - always counts as full, never empties, sprite never changes. If true - needs refilling whenever an animal uses it. |
| `FillableByCan`       | no       | bool | `true`  | The player can fill it with a watering can.                                                                                  |
| `FillableBySprinkler` | no       | bool | `true`  | A sprinkler placed to cover the object fills it at the start of the day.                                                     |
| `EmptySpriteIndex`    | no       | int  | `0`     | Sprite frame shown when empty.                                                                                               |
| `FullSpriteIndex`     | no       | int  | `1`     | Sprite frame shown when full.                                                                                                |

Example Content Patcher edit:

```json
{
  "Action": "EditData",
  "Target": "Data/BigCraftables",
  "Entries": {
    "{{ModId}}_StoneFountain": {
      "Name": "Stone Fountain",
      "DisplayName": "Stone Fountain",
      "Description": "A fountain your animals can drink from.",
      "Texture": "Mods/{{ModId}}/StoneFountain",
      "CustomFields": {
        "GZhynko.AnimalsNeedWater/IsWaterSource": "true",
        "GZhynko.AnimalsNeedWater/NeedsRefilling": "false"
      }
    }
  },
  "When": {
    "HasMod": "GZhynko.AnimalsNeedWater"
  }
}
```

See ANW's own Water Bowl for a working reference; its entry is in [
`Core/Content/Editors/WaterBowlContentEditor.cs`](../Core/Content/Editors/WaterBowlContentEditor.cs), its sprite is in [
`assets/items/waterBowl.png`](../assets/items/waterBowl.png).

## Animal settings

Add these to the `CustomFields` of your own `Data/FarmAnimals` entry. You can also edit vanilla animals this way.

Note: every key is optional. leaving any of the below out uses ANW's defaults.

| Key                        | Type | Default         | Meaning                                                                                       |
|----------------------------|------|-----------------|-----------------------------------------------------------------------------------------------|
| `NeedsWater`               | bool | `true`          | If false, the animal ignores ANW entirely: no thirst, no penalty, no water emotes or bonuses. |
| `WaterIntervalDays`        | int  | `1`             | How many days it can go without water before being penalized. Minimum `1`.                    |
| `CanDrinkOutside`          | bool | `true`          | Whether it seeks out water outdoors.                                                          |
| `ThirstyFriendshipPenalty` | int  | player's config | Friendship lost when left thirsty.                                                            |
| `DailyWaterFriendship`     | int  | player's config | Friendship gained on a day it had water.                                                      |
| `WitnessedWaterFriendship` | int  | player's config | Extra friendship when it sees the player fill its water.                                      |
| `ThirstyEmoteId`           | int  | `28` (sad tear) | Emote shown when thirsty.                                                                     |
| `DrinkOutsideEmoteId`      | int  | `32` (:3)       | Emote shown when drinking outdoors.                                                           |
| `WaterBonusEmoteId`        | int  | `20` (heart)    | Emote shown when the player fills its water.                                                  |

Example Content Patcher edit:

```json
{
  "Action": "EditData",
  "Target": "Data/FarmAnimals",
  "Fields": {
    "{{ModId}}_ClockworkHen": {
      "CustomFields": {
        "GZhynko.AnimalsNeedWater/NeedsWater": "false"
      }
    },
    "{{ModId}}_DesertLizard": {
      "CustomFields": {
        "GZhynko.AnimalsNeedWater/WaterIntervalDays": "3",
        "GZhynko.AnimalsNeedWater/CanDrinkOutside": "false"
      }
    }
  },
  "When": {
    "HasMod": "GZhynko.AnimalsNeedWater"
  }
}
```

Note: A longer interval is a grace period, not a drinking schedule. An animal with `WaterIntervalDays: 3` still drinks
whenever water is available; it's just not penalized until it has gone 3 days without water.
