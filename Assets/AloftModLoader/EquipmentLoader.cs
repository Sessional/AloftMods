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
                        
                    // TODO: ingest/populate the animation settings here.
                    //equipable.OverrideAnimations = ;
                    //equiable.ThirdPersonAnimations = ;
                    
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
            FieldInfo prefabToolField = AccessTools.Field(typeof(AloftModLoaderEquipable), nameof(AloftModLoaderEquipable.prefab));
            MethodInfo instantiateMethod = AccessTools.Method(typeof(GameObject), nameof(GameObject.Instantiate), parameters: new []{typeof(GameObject), typeof(Transform)});
            
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
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldfld, pathToolField),
                new(OpCodes.Brtrue, beforeLoadLogic),
                // todo: we can do more smart things...
                //  like bailing out if it isn't an instance...
                new(OpCodes.Ldarg_0),
                new(OpCodes.Castclass, typeof(AloftModLoaderEquipable)),
                new(OpCodes.Ldfld, prefabToolField),
                //new(OpCodes.Ldloc_0),
                // todo: figure out how to keep left/right hand selection...
                // another branch, some more pops? How can I branch & label on top of this branch in the new instructions?
                new(OpCodes.Ldarg_2),
                new(OpCodes.Call, instantiateMethod),
                new(OpCodes.Br_S, afterLoadLogic)
            };

            if (insertIndex != -1)
            {
                code.InsertRange(insertIndex, newInstructions);
            }
            
            Plugin.EquipmentLoader._logger.LogDebug("New code: " + string.Join("\n", newInstructions));

            return code;
        }
        
        public static IEnumerable<CodeInstruction> Init(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var code = new List<CodeInstruction>(instructions);
            FieldInfo pathToolField = AccessTools.Field(typeof(ScriptableEquipable), nameof(ScriptableEquipable.PathTool));
            FieldInfo prefabToolField =
                AccessTools.Field(typeof(AloftModLoaderEquipable), nameof(AloftModLoaderEquipable.prefab));
            // todo: ideally we'd have successfully hooked with this instead of string matching...
            // but it can't seem to find the generic overload here?
            //MethodInfo resourceLoad = AccessTools.Method(typeof(GameObject), nameof(Resources.Load)); //, parameters: new []{ typeof(string)}
            
            int insertIndex = -1;
            var rightAfterLoad = il.DefineLabel();
            var defaultPathToolInstantiation = il.DefineLabel();
            var foundFirstPathLoad = false;
            var foundFirstResourceLoad = false;
            
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].LoadsField(pathToolField) && !foundFirstPathLoad)
                {
                    // the previous call here is ldarg.1,
                    // after the null check, return to that instruction
                    insertIndex = i - 1;
                    code[i-1].labels.Add(defaultPathToolInstantiation);
                }
                
                var opCode = code[i].opcode;
                if (opCode == OpCodes.Call)
                {
                    var methodCalled = ((MethodInfo)code[i].operand);
                    Console.WriteLine("operand is" + methodCalled.ToString());

                    if (methodCalled.ToString().Contains("Load[GameObject]") && !foundFirstResourceLoad)
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
                // todo: we can do more smart things...
                //  like bailing out if it isn't an instance...
                new(OpCodes.Ldarg_1),
                new(OpCodes.Castclass, typeof(AloftModLoaderEquipable)),
                new(OpCodes.Ldfld, prefabToolField),
                new(OpCodes.Br_S, rightAfterLoad)
            };
            
            if (insertIndex != -1)
            {
                code.InsertRange(insertIndex, newInstructions);
            }
            
            Plugin.EquipmentLoader._logger.LogDebug("New code: " + string.Join("\n", newInstructions));

            return code;
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
