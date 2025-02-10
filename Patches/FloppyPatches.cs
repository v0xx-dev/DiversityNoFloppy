using HarmonyLib;
using DiversityRemastered;
using DiversityRemastered.Patches;

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
