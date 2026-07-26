<- [README.md](../README.md)

**Animals Need Water** provides an API allowing modders to access some of its data.

## Interface
Copy this interface into your mod, then request the API as shown in [Accessing the API](#accessing-the-api).

```csharp
public interface IAnimalsNeedWaterAPI
{
    List<long> GetAnimalsLeftThirstyYesterday();
    bool WasAnimalLeftThirstyYesterday(FarmAnimal animal);

    List<string> GetBuildingsWithWateredTrough();

    bool IsAnimalFull(FarmAnimal animal);
    bool DoesAnimalHaveAccessToWater(FarmAnimal animal);
    List<long> GetFullAnimals();
}
```

## Methods
**GetAnimalsLeftThirstyYesterday**

```csharp
List<long> GetAnimalsLeftThirstyYesterday()
```

Returns a ```List<long>``` of the ```myID``` of each animal that was left without access to water yesterday.
You can get the actual ```FarmAnimal``` instance with ```Utility.getAnimal(id)```.

**WasAnimalLeftThirstyYesterday**

```csharp
bool WasAnimalLeftThirstyYesterday(FarmAnimal animal)
```

Returns a ```bool``` defining whether the given animal was left thirsty yesterday. The list of animals considered thirsty is computed during DayEnding, hence the safest place to access this is during an event later in the game loop, e.g. DayStarted.

**GetBuildingsWithWateredTrough**

```csharp
List<string> GetBuildingsWithWateredTrough()
```

Returns a ```List<string>``` containing the unique interior names of animal buildings whose troughs are currently watered.

**IsAnimalFull**

```csharp
bool IsAnimalFull(FarmAnimal animal)
```

Returns a ```bool``` defining whether the given animal has drunk from an outside water source today.

**DoesAnimalHaveAccessToWater**

```csharp
bool DoesAnimalHaveAccessToWater(FarmAnimal animal)
```

Returns a ```bool``` defining whether the given animal currently has access to water. This is ```true``` if the animal has drunk outside today, or if its home building has a watered trough or a full water source and the animal can actually reach it - meaning the animal is either inside its home, or its home's animal door is open. If the animal has no home, only the drank-outside check applies.
Note: this describes physical access to water, not whether the animal needs it. An animal with a multi-day water interval can have no access while still being fine, and an animal configured not to need water at all will still return ```false`` when there is no water nearby.

**GetFullAnimals**

```csharp
List<long> GetFullAnimals()
```

Returns a ```List<long>``` of the ```myID``` of each animal that has drunk from an outside water source today.
You can get the actual ```FarmAnimal``` instance with ```Utility.getAnimal(id)```.

## Accessing the API
See [Modder Guide/APIs/Integrations](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations#Using_an_API) on the official SDV wiki.
