**Animals Need Water** adds functionality to water troughs located in animal houses. Water them and your animals will be happy! Though, if you ignore them, your animals won't like it - they don't really enjoy being thirsty.

## Install
 -  [Install the latest version of SMAPI](https://smapi.io/).
 -  Install Animals Need Water [from Nexus mods](https://www.nexusmods.com/stardewvalley/mods/6196).
 -  Run the game using SMAPI.

## For mod authors
- To make your mod compatible with ANW content-wise (map changes, recolors, etc.), see the example Content Patcher content pack in [examples](./docs/examples). 
- To access ANW state at runtime from your SMAPI mod, see the [API docs](./API-documentation.md).

## Translating
| Language             | Status                                                                                                                                                                                                     |
|----------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Chinese              | [✓](https://github.com/gzhynko/StardewMods/blob/master/AnimalsNeedWater/i18n/zh.json) (credit to [motiam](https://github.com/motiam), [Kasakana](https://www.nexusmods.com/stardewvalley/users/171835688)) |
| French               | [✓](https://github.com/gzhynko/StardewMods/blob/master/AnimalsNeedWater/i18n/fr.json) (credit to [Caranud](https://www.nexusmods.com/stardewvalley/users/745980))                                          |
| German               | [~](https://github.com/gzhynko/StardewMods/blob/master/AnimalsNeedWater/i18n/de.json) (credit to [Makytar](https://www.nexusmods.com/stardewvalley/users/51740796))                                        |
| Hungarian            | [✓](https://github.com/gzhynko/StardewMods/blob/master/AnimalsNeedWater/i18n/hu.json) (credit to [szatoka](https://www.nexusmods.com/stardewvalley/users/47532583))                                        |
| Italian              | ✕                                                                                                                                                                                                          |
| Japanese             | [~](https://github.com/gzhynko/StardewMods/blob/master/AnimalsNeedWater/i18n/ja.json) (credit to [TinyCordelia](https://next.nexusmods.com/profile/TinyCordelia))                                          |
| Korean            | [✓](https://github.com/gzhynko/StardewMods/blob/master/AnimalsNeedWater/i18n/ko.json) (credit to seint_s)                                        |
| Brazilian Portuguese | [~](https://github.com/gzhynko/StardewMods/blob/master/AnimalsNeedWater/i18n/pt-BR.json) (credit to [tramontina](https://www.nexusmods.com/stardewvalley/users/36215665))                                  |
| Russian              | [✓](https://github.com/gzhynko/StardewMods/blob/master/AnimalsNeedWater/i18n/ru.json)                                                                                                                      |
| Spanish              | [~](https://github.com/gzhynko/StardewMods/blob/master/AnimalsNeedWater/i18n/es.json) (credit to [bpsys](https://www.nexusmods.com/users/72952373))                                                        |
| Turkish              | ✕                                                                                                                                                                                                          |

Key: ✓ = full translation available, ~ = partial translation available, ✕ = no translation yet.

**Contributing**
If you'd like to help me translate the mod, here's how:

 1. Copy the `default.json` file from the `i18n` folder with one of the following names, depending on the language you choose to translate the mod to: 
	|  Language | File Name |
	| -- | -- |
	|  Chinese | `zh.json` |
	|  French | `fr.json` |
	|  German | `de.json` |
	|  Hungarian | `hu.json` |
	|  Italian | `it.json` |
	|  Japanese | `ja.json` |
	|  Korean | `ko.json` |
	|  Portuguese | `pt.json` |
	|  Spanish | `es.json` |
	|  Turkish | `tr.json` |
 2. For each `key` translate the `value` . For example:
	``` 
	"dummy-key": "dummy-value"
	    ^- key     ^- value,
	                  translate this
	```
	Please *don't* translate the `{{tokens}}`.
	
 3. Start SMAPI to test your translation. If everything seems to work OK, you're good to go!
 
 After you are done translating, you can either create a new pull-request or issue here *or* [email](mailto:gleb.zhinko@gmail.com) me your translation file and I'll credit you as the author of the translation.

## See also
 - Animals Need Water on [Nexus Mods](https://www.nexusmods.com/stardewvalley/mods/6196)
