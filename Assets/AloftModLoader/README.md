# Aloft Mod Loader

The Aloft Mod Loader is a significantly more complicated beast than the Aloft Mod Framework. Some changes (like new additions) to the framework might actually require reversing aspects of the game's code and understanding how to properly patch new content on top of what currently exists. The code that takes care of patching and how to load the data is housed here. This plugin is a requirement for all of the plugins in this project that aim to add new content. Primarily because the loading logic looks the same for all assetbundles independent of the content added.

To make progress on adding new content to Aloft Mod Loader, some tools might be helpful:

- UnityExplorer or RuntimeUnityEditor: for viewing how content works inside the game
- dnSpy, dotPeek, ILSpy: for digging through the game code to understand how things stack together.
- AssetRipper: for digging through assets and their settings to understand how things work
