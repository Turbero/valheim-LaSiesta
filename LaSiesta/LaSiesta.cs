using BepInEx;
using HarmonyLib;

namespace LaSiesta
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class LaSiesta : BaseUnityPlugin
    {
        public const string GUID = "Turbero.LaSiesta";
        public const string NAME = "La Siesta";
        public const string VERSION = "1.0.3";

        private readonly Harmony harmony = new Harmony(GUID);

        void Awake()
        {
            ConfigurationFile.LoadConfig(this);
            harmony.PatchAll();
        }

        void onDestroy()
        {
            harmony.UnpatchSelf();
        }
    }
    
    [HarmonyPatch(typeof(EnvMan), "CalculateCanSleep")]
    public class CalculateCanSleepPatch
    {
        static bool Prefix(ref bool __result)
        {
            //Set return value to true
            __result = true;
            //Tell Harmony to not run the original method
            return false;
        }
    }
}
