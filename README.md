# Aloft Mods

This repository houses two primary components and a collection of small examples (that are also fully functioning plugins).

1. **Aloft Mod Framework**: a library built on top of Aloft to facilitate modding of the game.
2. **Aloft Mod Loader**: an plugin built on top of the Mod Framework that takes care of loading the content from the assetbundle and plugging it into Aloft in the proper way to integrate successfuly.
3. **Sample projects**: several sample projects that demonstrate how to mod Aloft. There might be variations of three different styles: code-only: a mod using only code; content-only: a mod relying solely on the Aloft Mod Framework to create new content; and a mixture that uses both to add new content and new behaviors.

## Setting up for development

- Ensure Aloft is imported via Thunderkit. Ensure `Aloft.dll` is configured to `auto reference`.
- Copy in the DLLs from [BepInEx 5.4.21](https://thunderstore.io/c/aloft/p/BepInEx/BepInExPack_Aloft/) (5.4.23.2 also works) to the Assets folder. 5 has to be used for support from the patcher to fix serialized types.

### Running/building the plugins

There is really only one Pipeline (see thunderkit) that is needed for all of the plugins today. Using the quick pickers in the Unity toolbar, select that pipeline and a manifest for the plugin to build and hit the `Execute` button. DLLs and asset bundles should appear in the Aloft directory under BepInEx/plugins.

## Aloft Mod Framework

The Aloft Mod Framework is intended to be a light library that dependencies can build on top of. It is reasonable and feasible to add new content to Aloft without writing any code. As such, this framework is really intended for adding `ScriptableObjects` to Unity where one can add new content without really ever having to learn how to code.

## Aloft Mod Loader

The Aloft Mod Loader is a significantly more complicated beast than the Aloft Mod Framework. Some changes (like new additions) to the framework might actually require reversing aspects of the game's code and understanding how to properly patch new content on top of what currently exists. The code that takes care of patching and how to load the data is housed here. This plugin is a requirement for all of the plugins in this project that aim to add new content. Primarily because the loading logic looks the same for all assetbundles independent of the content added.

To make progress on adding new content to Aloft Mod Loader, some tools might be helpful:

- UnityExplorer or RuntimeUnityEditor: for viewing how content works inside the game
- dnSpy, dotPeek, ILSpy: for digging through the game code to understand how things stack together.
- AssetRipper: for digging through assets and their settings to understand how things work

## Mod samples in here and what they do:
- **AdditionalWorkbenchRecipe**: recipe to craft coal at the workbench.
- **BeeBoxes**: buildable beehive to produce wax and honey.
- **NewFood**: new cooking ingredient (lobster) and a new edible meal (lobster platter).
- **NewSpawnableResource**: spawnable rock that drops water opal that spawns naturally on islands
    (using random biome spawners).
- **NewTileset**: new building tilesets, along with a new sub-tab inside the building screen to house them.
- **NewWorkbench**: new workbench and a new set of recipes that it can craft.
- **NoPoop**: disables droppings of manure from farm animals.

## Building a new plugin:
- [Set up for development](#setting-up-for-development)
- Create a thunderkit manifest (often easiest to copy a new one). Note: when copying one, the quick access option needs to be toggled for it to appear in the quick access menu.
- Depend on AloftModFramework
- Add assembly definitions
- Add asset bundle definitions. Make sure to name them with the ending `.amf.assetbundle` if you want the AloftModFramework to pick them up and auto load the content correctly.
