using BepInEx.Configuration;
using BepInEx;

namespace LaSiesta
{
    internal class ConfigurationFile
    {
        public static ConfigEntry<bool> debug;
        public static ConfigEntry<bool> showDayInfoOnScreen;
        public static ConfigEntry<bool> siestaOnlyAfternoon;
        public static ConfigEntry<int> siestaHours;

        private static ConfigFile config;

        internal static void LoadConfig(BaseUnityPlugin plugin)
        {
            {
                config = plugin.Config;

                debug = config.Bind<bool>("1 - General", "DebugMode", false, "Enabling/Disabling the debugging in the console (default = false)");
                showDayInfoOnScreen = config.Bind<bool>("1 - General", "ShowDayInfoOnScreen", false, "Shows day attributes on the screen for debug purposes main (default = false)");
                siestaOnlyAfternoon = config.Bind<bool>("1 - General", "SiestaOnlyAfternoon", false, "Restricts siesta time start only at afternoon time (default = true)");
                siestaHours = config.Bind<int>("1 - General", "SiestaHours", 2, "Number of hours of each siesta (default = 2, max = 24)");
            }
        }
    }
}
