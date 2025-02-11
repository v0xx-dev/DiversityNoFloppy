using HarmonyLib;
using DiversityRemastered;
using DiversityRemastered.Patches;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

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
                codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FloppyPatches), "WalkerDoorFixer")));
            }
            else
            {
                DiversityNoFloppy.Logger.LogError("Walker door sequence not found!");
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


        [HarmonyPatch(typeof(StartOfRoundPatch))]
        [HarmonyPatch(nameof(StartOfRoundPatch.ResetShipFurniture))]
        [HarmonyPrefix]
        private static bool DiskRespawnBlockerPatch()
        {
            DiversityNoFloppy.Logger.LogDebug("Blocking respawning of the floppy reader!");
            return false;
        }
        
    }
}
