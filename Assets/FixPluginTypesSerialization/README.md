# FixPluginTypesSerialization

Hook into the native Unity engine for adding BepInEx plugin assemblies into the assembly list that is normally used for the assemblies sitting in the game Managed/ folder.

This solves a behavor where custom Serializable structs and such stored in plugin assemblies are not properly getting deserialized by the engine. See [the unity issue tracker](https://issuetracker.unity3d.com/issues/assetbundle-is-not-loaded-correctly-when-they-reference-a-script-in-custom-dll-which-contains-system-dot-serializable-in-the-build).

A republish of [this mod](https://thunderstore.io/package/RiskofThunder/FixPluginTypesSerialization/) into the Aloft community, with a new logo without the typo... Originally sourced from [here](https://github.com/xiaoxiao921/FixPluginTypesSerialization). Please send issues to the [aloft mods github](https://github.com/Sessional/AloftMods) instead of the original one. It seems to come with no support from the original author.

### Installation

- Copy the `patchers/FixPluginTypesSerialization.dll` file into your `BepInEx/patchers` folder.

- Copy the `config/FixPluginTypesSerialization.cfg` folder into your `BepInEx/config` folder.

### Special Thanks

- Horse [for the original code base](https://github.com/BepInEx/BepInEx.Debug/tree/master/src/MirrorInternalLogs)

- KingEnderBrine

- Twiner

- NebNeb for the icon
