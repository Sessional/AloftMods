using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AloftModFramework.Equipment;
using BepInEx.Logging;
using HarmonyLib;
using Level_Manager;
using Player.Hands;
using Player.Player_Equip;
using Scriptable_Objects;
using Scriptable_Objects.Combat;
using UI.InGame;
using UnityEngine;
using Utilities;
using Logger = BepInEx.Logging.Logger;
using Object = UnityEngine.Object;

namespace AloftModLoader
{
    public class EquipmentLoader
    {
        private readonly ManualLogSource _logger;
        private readonly Harmony _harmony;
        private readonly List<ScriptableEquipable> scriptableEquippables;
        private readonly Material _interactableSelectableMaterial;

        public EquipmentLoader(ManualLogSource logger, Harmony harmony, List<Object> assets)
        {
            this._logger = logger;
            this._harmony = harmony;
            
            _logger.LogDebug("Loading material from standard workbench.");
            var workbench = Resources.Load("Platform Builder/Constructions/Machines/Pre_Construction_Workbench") as GameObject;
            var workbenchMeshRenderer = workbench.GetComponentsInChildren<MeshRenderer>().First();
            _interactableSelectableMaterial = workbenchMeshRenderer.material;

            scriptableEquippables = assets
                .FilterAndCast<Equippable>()
                .Select(x =>
                {
                    var equippable = ScriptableObject.CreateInstance<AloftModLoaderEquipable>();

                    equippable.ItemID = x.item.GetItemId();
                    equippable.EquipType = (PlayerEquip.Equipable) x.id;
                    
                    equippable.HandBehaviour = x.handBehaviour;

                    equippable.equipableTemplate = x.equippableTemplate;
                    if (x.equippableTemplate.template != null)
                    {
                        // These fields can ONLY be set if they came from a reference with a link to
                        // the values. If they are intended to be `vanilla` values, they can't be loaded
                        // here because they do not yet exist. See Plugin.LoadSceneCallback for a possible
                        // hook point to finalize these objects.
                        equippable.OverrideAnimations = x.equippableTemplate.template.overrideAnimations;
                        equippable.ThirdPersonAnimations = x.equippableTemplate.template.thirdPersonAnimations;
                    }
                    
                    equippable.prefab = x.heldItemPrefab;
                    equippable.prefab.GetComponentsInChildren<MeshRenderer>()
                        .ForEach(renderer =>
                        {
                            _logger.LogDebug("Replacing material with " + _interactableSelectableMaterial.name);
                            
                            var mainTex = renderer.material.GetTexture("_MainTex");
                            var normalTex = renderer.material.GetTexture("_BumpMap");
                            var detailMask = renderer.material.GetTexture("_DetailMask");

                            var newMaterial = new Material(_interactableSelectableMaterial);
                            newMaterial.name = "AloftModLoader_InteractableSelectable_Material";
            
                            _logger.LogDebug("Adding textures to material: " + mainTex + " " + normalTex + " " + detailMask);
                            newMaterial.SetTexture("_TextureAlbedo", mainTex);
                            newMaterial.SetTexture("_TextureNormals", normalTex);
                            newMaterial.SetTexture("_TextureMask", detailMask);
                            newMaterial.SetVector("_ColorSelect", new Vector4(1.6f, 1.3f, 0.5f, 1));
                            newMaterial.SetColor("_Color", new Color(1f, 1f, 1f, 1f));

                            renderer.material = newMaterial;
                        });
                    
                    // todo: grab from asset
                    equippable.HoldInLeftHand = false;
                    
                    //todo: carry forward instructions
                    equippable.Instructions = new UI_EquipableInstructions.InstructionMapping[0];

                    equippable.PathTool = null;
                    
                    equippable.CombatData = x.combatValues;
                    equippable.BowData = x.bowValues;
                    
                    return equippable;
                })
                .Cast<ScriptableEquipable>()
                .ToList();

        }

        public void Patch()
        {
            this._harmony.Patch(
                AccessTools.Method(typeof(ScriptableInventoryManager), nameof(ScriptableInventoryManager.GetEquipable)),
                null,
                new HarmonyMethod(AccessTools.Method(typeof(EquipmentLoader), nameof(EquipmentLoader.GetEquipable)))
            );

            this._harmony.Patch(
                AccessTools.Method(typeof(PlayerEquipThirdPerson), nameof(PlayerEquipThirdPerson.SpawnItemInHand)),
                null,
                null,
                new HarmonyMethod(AccessTools.Method(typeof(EquipmentLoader), nameof(EquipmentLoader.SpawnItemInHand)))
            );

            this._harmony.Patch(
                AccessTools.Method(typeof(Hands), nameof(Hands.Init)),
                null,
                null,
                new HarmonyMethod(AccessTools.Method(typeof(EquipmentLoader), nameof(EquipmentLoader.Init)))
            );
        }

        public static IEnumerable<CodeInstruction> SpawnItemInHand(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            FieldInfo pathToolField = AccessTools.Field(typeof(ScriptableEquipable), nameof(ScriptableEquipable.PathTool));
            MethodInfo instantiateItem = AccessTools.Method(typeof(EquipmentLoader),
                nameof(EquipmentLoader.SpawnHeldItem));
            
            var code = new List<CodeInstruction>(instructions);

            int insertIndex = -1;

            var foundFirstPathLoad = false;
            var foundFirstCallVirt = false;
            
            var beforeLoadLogic = il.DefineLabel();
            var afterLoadLogic = il.DefineLabel();

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].LoadsField(pathToolField) && !foundFirstPathLoad)
                {
                    
                    // the previous call here is ldarg.0,
                    // after the null check, return to that instruction
                    insertIndex = i - 1;
                    code[i - 1].labels.Add(beforeLoadLogic);
                    foundFirstPathLoad = true;
                }

                if (code[i].opcode == OpCodes.Callvirt && !foundFirstCallVirt)
                {
                    foundFirstCallVirt = true;
                    code[i].labels.Add(afterLoadLogic);
                }
            }
            
            var newInstructions = new List<CodeInstruction>() {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, pathToolField),
                new(OpCodes.Brtrue, beforeLoadLogic),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Call, instantiateItem),
                new(OpCodes.Br_S, afterLoadLogic)
            };

            if (insertIndex != -1)
            {
                code.InsertRange(insertIndex, newInstructions);
            }
            
            if (Plugin.configLogDebugILPatches.Value) Plugin.EquipmentLoader._logger.LogDebug("New code: " + string.Join("\n", newInstructions));

            return code;
        }

        public static GameObject SpawnHeldItem(ScriptableEquipable equipable, Transform parentLeft, Transform parentRight)
        {
            var modEquipable = equipable as AloftModLoaderEquipable;
            if (modEquipable is null) return null;

            var transform = equipable.HoldInLeftHand ? parentLeft : parentRight;

            return Object.Instantiate<GameObject>(modEquipable.prefab, transform);
        }
        
        public static IEnumerable<CodeInstruction> Init(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var code = new List<CodeInstruction>(instructions);
            FieldInfo pathToolField = AccessTools.Field(typeof(ScriptableEquipable), nameof(ScriptableEquipable.PathTool));
            MethodInfo instantiateFromPrefab =
                AccessTools.Method(typeof(EquipmentLoader), nameof(EquipmentLoader.InstantiateFromPrefab));
            var resourcesLoadFunc = new Func<String, GameObject>(Resources.Load<GameObject>);
            var resourcesLoadMethodInfo = resourcesLoadFunc.GetMethodInfo().GetGenericMethodDefinition().MakeGenericMethod(typeof(GameObject));
            
            int insertIndex = -1;
            var rightAfterLoad = il.DefineLabel();
            var defaultPathToolInstantiation = il.DefineLabel();
            var foundFirstPathLoad = false;
            var foundFirstResourceLoad = false;
            
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].LoadsField(pathToolField) && !foundFirstPathLoad)
                {
                    foundFirstPathLoad = true;
                    // the previous call here is ldarg.1,
                    // after the null check, return to that instruction
                    insertIndex = i - 1;
                    code[i-1].labels.Add(defaultPathToolInstantiation);
                }
                
                var opCode = code[i].opcode;
                if (opCode == OpCodes.Call)
                {
                    var methodCalled = ((MethodInfo)code[i].operand);
                    if (methodCalled == resourcesLoadMethodInfo && !foundFirstResourceLoad)
                    {
                        foundFirstResourceLoad = true;
                        code[i + 1].labels.Add(rightAfterLoad);
                    }
                }
            }
            
            var newInstructions = new List<CodeInstruction>() {
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldfld, pathToolField),
                new(OpCodes.Brtrue, defaultPathToolInstantiation),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Call, instantiateFromPrefab),
                new(OpCodes.Br_S, rightAfterLoad)
            };
            
            if (insertIndex != -1)
            {
                code.InsertRange(insertIndex, newInstructions);
            }
            
            if (Plugin.configLogDebugILPatches.Value) Plugin.EquipmentLoader._logger.LogDebug("New code: " + string.Join("\n", newInstructions));

            return code;
        }

        public static GameObject InstantiateFromPrefab(ScriptableEquipable equipable)
        {
            var modEquipable = equipable as AloftModLoaderEquipable;
            if (modEquipable is null) return null;
            
            return modEquipable.prefab;
        }
        
        public static ScriptableEquipable GetEquipable(ScriptableEquipable __result, PlayerEquip.Equipable equip)
        {
            
            if (__result == null)
            {
                ScriptableEquipable modEquippable = Plugin.EquipmentLoader.scriptableEquippables.FirstOrDefault(x => x.EquipType == equip);
                Plugin.EquipmentLoader._logger.LogDebug("Equipable is : " + modEquippable);
                if (modEquippable != null)
                {
                    var equipable = modEquippable as AloftModLoaderEquipable;

                    
                    if (equipable != null)
                    {
                        // Because the following checks involve adding a new dependency to this framework
                        // currently just not checking at all.
                        //if (equipable.OverrideAnimations != null) return equipable;
                        switch (equipable.equipableTemplate.vanillaTemplate)
                        {
                            case VanillaEquippableTemplate.None:
                                equipable.OverrideAnimations = equipable.equipableTemplate.template.overrideAnimations;
                                equipable.ThirdPersonAnimations = equipable.equipableTemplate.template.thirdPersonAnimations;
                                break;
                            case VanillaEquippableTemplate.Pickaxe:
                                Plugin.EquipmentLoader._logger.LogDebug("Finding override animations");
                                var pickaxeOverride = Level.ConstantManager.ConstantManagers.InventoryManager.Equipables.FirstOrDefault(x =>
                                    x.ItemID == ItemID.ID.ToolPickaxeStone);
                                if (pickaxeOverride != null)
                                {
                                    Plugin.EquipmentLoader._logger.LogDebug("Setting override animations");
                                    equipable.OverrideAnimations = pickaxeOverride.OverrideAnimations;
                                    equipable.ThirdPersonAnimations = pickaxeOverride.ThirdPersonAnimations;                                    
                                }
                                break;
                        }

                        return equipable;
                    }

                    /*if (modEquippable.OverrideAnimations != null)
                    {
                        return modEquippable;                        
                    }*/
                    
                    return modEquippable;
                }
            }

            return __result;
        }
    }
}
