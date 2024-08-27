# API Documentation
**Animals Need Water** provides an API allowing modders to access some of its data.

## Methods
**GetAnimalsLeftThirstyYesterday**

*No parameters*

Returns a ```long``` myID of each animal left without a watered trough yesterday. 
You can get the actual FarmAnimal instance with ```Utility.getAnimal(id)```.

**WasAnimalLeftThirstyYesterday**

Requires a ```FarmAnimal``` instance.

Returns a ```bool``` defining whether the animal was left thirsty yesterday.

**GetBuildingsWithWateredTrough**

*No parameters*

Returns a ```List<string>```  containing a list of animal buildings with watered troughs.

**IsAnimalFull**

Requires a ```FarmAnimal``` instance.

Returns a ```bool``` defining whether the animal was able to drink outside today.

**DoesAnimalHaveAccessToWater**

Requires a ```FarmAnimal``` instance.

Returns a ```bool``` defining whether the animal was able to drink outside OR the trough inside its home is filled.

**GetFullAnimals**

*No parameters*

Returns a ```long``` myID of each animal that was able to drink outside.
You can get the actual FarmAnimal instance with ```Utility.getAnimal(id)```.

## Accessing the API
See [Modder Guide/APIs/Integrations](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations#Using_an_API) on the official SDV wiki.