# Aloft Mods

This repository houses two primary components and a collection of small examples (that are also fully functioning plugins).
1. **Aloft Mod Framework**: a library built on top of Aloft to facilitate modding of the game.
2. **Aloft Mod Loader**: an plugin built on top of the Mod Framework that takes care of loading the content from the assetbundle and plugging it into Aloft in the proper way to integrate successfuly.
3. **Sample projects**: several sample projects that demonstrate how to mod Aloft. There might be variations of three different styles: code-only, content-only, and code&content.

A grouping of random mods to demonstrate how to do things

## Setting up for development

- Ensure Aloft is imported via Thunderkit
- Copy in the DLLs from [BepInEx 6.0.0-pre.2](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.2) to the Assets folder


### Running/building the plugins

There is really only one Pipeline (see thunderkit) that is needed for all of the plugins today. Using the quick pickers in the Unity toolbar, select that pipeline and a manifest for the plugin to build and hit the `Execute` button. Note: if a non-standard steam directory is used, it might be necesary to tweak the manifest and the staging directories. DLLs and asset bundles should appear in the Aloft directory under BepInEx/plugins or Aloft_Data/StreamingAssets/amf

## Aloft Mod Framework

The Aloft Mod Framework is intended to be a light library that dependencies can build on top of. It is reasonable and feasible to add new content to Aloft without writing any code. As such, this framework is really intended for adding `ScriptableObjects` to Unity where one can add new content without really ever having to learn how to code.

## Aloft Mod Loader

The Aloft Mod Loader is a significantly more complicated beast than the Aloft Mod Framework. Some changes (like new additions) to the framework might actually require reversing aspects of the game's code and understanding how to properly patch new content on top of what currently exists. The code that takes care of patching and how to load the data is housed here. This plugin is a requirement for all of the plugins in this project that aim to add new content. Primarily because the loading logic looks the same for all assetbundles independent of the content added.

To make progress on adding new content to Aloft Mod Loader, some tools might be helpful:

- UnityExplorer or RuntimeUnityEditor: for viewing how content works inside the game
- dnSpy, dotPeek, ILSpy: for digging through the game code to understand how things stack together.
- AssetRipper: for digging through assets and their settings to understand how things work


## Mods in here and what they do:
- **BeeBoxes**: A mod that adds a buildable beehive to produce wax and honey. Relies on the Aloft Mod Framework to work (content and code).
- **CustomBuildingTileset**: a content-only mod that adds new building tilesets, along with a new sub-tab inside the building screen to house them. Relies on the Aloft Mod Framework to work (content only).
- **CustomFood**: an example of how to create a new meal, a new recipe and a new food item to eat.
- **CustomWorkbench**: a code and content mod that adds a new workbench (a mortar) that hooks into the existing crafting system in the game and permits special recipes that aren't craftable on other stations. Relies on the Aloft Mod Framework to work (content and code only).
- **NoPoop**: A mod that disables droppings of manure (caveat: possibly more things) from farm animals. Code-only plugin, does not rely on the Aloft Mod Framework.
- **PumpkinCrops**: an example of a content-only mod that adds new spawnable resources in the game and allows them to be harvested for resources. Relies on the Aloft Mod Framework to work (content only).