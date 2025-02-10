using BepInEx;
using BepInEx.Logging;
using DiversityNoFloppy.Patches;
using HarmonyLib;

namespace DiversityNoFloppy
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("Chaos.Diversity", BepInDependency.DependencyFlags.HardDependency)]
    public class DiversityNoFloppy : BaseUnityPlugin
    {
        public static DiversityNoFloppy Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            Patch();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Removing floppy reader and disk spawning...");

            Harmony.PatchAll(typeof(FloppyPatches));
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }
}
