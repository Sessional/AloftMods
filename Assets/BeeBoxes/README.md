# BeeBoxes
A mod built on top of the Aloft Mod Framework that adds a buildable bee box to the game.

## Installation for playing

Install [Aloft Mod Framework.](https://thunderstore.io/c/aloft/p/AloftModFramework/Aloft_Mod_Framework/)
- This includes installing BepInEx

Copy the contents of the package into the `Aloft` folder from the steampps. There will be 2 new streaming assets inside the AMF folder, and two new built files (.dll and pdb) in the `BepInEx/plugins` folder.
- Aloft/Aloft_Data/StreamingAssets/amf/beehive.assetbundle
- Aloft/Aloft_Data/StreamingAssets/amf/beehive.assetbundle
- Aloft/BepInEx/plugins/BeeBoxes.dll
- Aloft/BepInEx/plugins/BeeBoxes.pdb

## Notice:
The `BeeBox` in this package is placed at PopId 400,003. If anything is placed there instead, the behavior will be determined by load order most likely.