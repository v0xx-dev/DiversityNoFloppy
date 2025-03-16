using HarmonyLib;
using DiversityRemastered;
using DiversityRemastered.Patches;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;

namespace DiversityNoFloppy.Patches
{
    public class FloppyPatches
    {
        [HarmonyPatch(typeof(StartOfRoundPatch))]
        [HarmonyPatch(nameof(StartOfRoundPatch.StartPostFix))]
        [HarmonyPrefix]
        private static bool ReaderBlockerPatch()
        {
            DiversityNoFloppy.Logger.LogDebug("Blocking spawning of the floppy reader!");
            return false;
        }

        [HarmonyPatch(typeof(DiversityManager))]
        [HarmonyPatch(nameof(DiversityManager.SpawnDisk))]
        [HarmonyPrefix]
        private static bool DiskBlockerPatch()
        {
            DiversityNoFloppy.Logger.LogDebug("Disk spawning blocked!");
            return false;
        }

        [HarmonyPatch(typeof(DiversityManager))]
        [HarmonyPatch(nameof(DiversityManager.SetupRound))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> WalkerDoorFixerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            CodeMatch[] walkerDoorBlock = [new CodeMatch(OpCodes.Ldloc_S),
                                            new CodeMatch(OpCodes.Callvirt),
                                            new CodeMatch(OpCodes.Callvirt),
                                            new CodeMatch(OpCodes.Callvirt),
                                            new CodeMatch(OpCodes.Callvirt),
                                            new CodeMatch(OpCodes.Ldc_I4_0),
                                            new CodeMatch(OpCodes.Ldelem_Ref),
                                            new CodeMatch(OpCodes.Ldstr), 
                                            new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(ContentLoader), nameof(ContentLoader.extraAssets))), 
                                            new CodeMatch(OpCodes.Ldstr),
                                            new CodeMatch(OpCodes.Callvirt),
                                            new CodeMatch(OpCodes.Callvirt),
                                            new CodeMatch(OpCodes.Nop)];    

            codeMatcher.MatchForward(false, walkerDoorBlock);

            if (codeMatcher.IsValid)
            {
                DiversityNoFloppy.Logger.LogDebug("Walker door sequence found!");
                // Remove part of the sequence
                codeMatcher.Advance(4);
                codeMatcher.RemoveInstructions(walkerDoorBlock.Length - 4);
                codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FloppyPatches), nameof(WalkerDoorFixer))));
            }
            else
            {
                DiversityNoFloppy.Logger.LogError("Walker door sequence not found!");
            }

            return codeMatcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(DiversityManager))]
        [HarmonyPatch(nameof(DiversityManager.SetupRound))]
        [HarmonyTranspiler]
        [HarmonyDebug]
        private static IEnumerable<CodeInstruction> DoorFinderFixerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            CodeMatch[] walkerDoorBlock = [new CodeMatch(OpCodes.Nop),
                                            new CodeMatch(OpCodes.Call),
                                            new CodeMatch(OpCodes.Ldsfld),
                                            new CodeMatch(OpCodes.Dup),
                                            new CodeMatch(OpCodes.Brtrue),
                                            new CodeMatch(OpCodes.Pop),
                                            new CodeMatch(OpCodes.Ldsfld),
                                            new CodeMatch(OpCodes.Ldftn), 
                                            new CodeMatch(OpCodes.Newobj), 
                                            new CodeMatch(OpCodes.Dup),
                                            new CodeMatch(OpCodes.Stsfld),
                                            new CodeMatch(OpCodes.Call),
                                            new CodeMatch(OpCodes.Call),];    

            codeMatcher.MatchForward(false, walkerDoorBlock);

            if (codeMatcher.IsValid)
            {
                DiversityNoFloppy.Logger.LogDebug("Door finder sequence found!");
                // Replace the entire sequence
                codeMatcher.RemoveInstructions(walkerDoorBlock.Length);
                codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FloppyPatches), nameof(DoorFinderFixer))));
            }
            else
            {
                DiversityNoFloppy.Logger.LogError("Door finder sequence not found!");
            }

            return codeMatcher.InstructionEnumeration();
        }

        private static void WalkerDoorFixer(MeshRenderer? doorRenderer)
        {
            Material[]? doorMaterials = doorRenderer?.materials;
            if (doorMaterials != null && doorMaterials.Length > 0)
            {
                doorMaterials[0].SetTexture("_MaskMap", ContentLoader.extraAssets.LoadAsset<Texture2D>("Assets/custom/diversity/extras/textures/Grunge4.png"));
                doorRenderer!.materials = doorMaterials;
            }
        }

        private static DoorLock[] DoorFinderFixer()
        {
            DoorLock[] doorLocks = Object.FindObjectsByType<DoorLock>(FindObjectsSortMode.None).Where(x => x.gameObject.name == "Cube" &&
                                                                                                (x.transform.parent?.parent?.gameObject.name.Contains("SteelDoor") ?? false)
                                                                                                ).ToArray();

            return doorLocks;
        }


        [HarmonyPatch(typeof(StartOfRoundPatch))]
        [HarmonyPatch(nameof(StartOfRoundPatch.ResetShipFurniture))]
        [HarmonyPrefix]
        private static bool DiskRespawnBlockerPatch()
        {
            DiversityNoFloppy.Logger.LogDebug("Blocking respawning of the floppy reader!");
            return false;
        }

        // static Shader? litShader = null!;
        // static Shader? unlitShader = null!;

        // [HarmonyPatch(typeof(StartOfRound))]
        // [HarmonyPatch(nameof(StartOfRound.Start))]
        // [HarmonyPostfix]
        // private static void ShaderLoaderPatch(StartOfRound __instance)
        // {
        //     litShader ??= Shader.Find("HDRP/Lit");
        //     unlitShader ??= Shader.Find("HDRP/Unlit");

        //     //Get all the materials in resources
        //     Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
        //     FixMaterialShaders(materials);
        //     __instance.StartNewRoundEvent.AddListener(FixMaterialsOnLoad);
        // }

        // private static void FixMaterialShaders(Material[] materials)
        // {
        //     int count = 0;
        //     foreach (Material material in materials)
        //     {
        //         if (material == null || material.shader == null)
        //         {
        //             continue;
        //         }

        //         // Save the keywords
        //         string[] shaderKeywords = material.shaderKeywords;
        //         int renderQueue = material.renderQueue;

        //         if (material.shader.name == "HDRP/Lit")
        //         {
        //             material.shader = litShader;
        //         }
        //         else if (material.shader.name == "HDRP/Unlit")
        //         {
        //             material.shader = unlitShader;
        //         }

        //         for (int i = 0; i < shaderKeywords.Length; i++)
        //         {
        //             //Set the keywords back
        //             material.EnableKeyword(shaderKeywords[i]);
        //         }

        //         material.renderQueue = renderQueue;
        //         count++;
        //     }
        //     DiversityNoFloppy.Logger.LogDebug($"Fixed {count} materials!");
        // }

        // private static void FixMaterialsOnLoad()
        // {
        //     //Get all the materials in the scenes
        //     Material[] materials = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Select(x => x.sharedMaterial).ToArray();
        //     FixMaterialShaders(materials);
        // }
        
    }
}
