using BepInEx.Configuration;
using BepInEx;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace LaSiesta
{
    internal class ConfigurationFile
    {
        public static ConfigEntry<bool> debug;
        public static ConfigEntry<float> oneDayLength;
        public static ConfigEntry<float> oneHourLength;
        public static ConfigEntry<string> tooCloseForSiesta;

        public static ConfigEntry<bool> siestaOnlyAfternoon;
        public static ConfigEntry<float> siestaHours;

        public static ConfigEntry<bool> showDayInfoOnScreen;
        public static ConfigEntry<int> xPosClock;
        public static ConfigEntry<int> yPosClock;
        public static ConfigEntry<int> fontSize;

        private static ConfigFile config;

        internal static void LoadConfig(BaseUnityPlugin plugin)
        {
            {
                config = plugin.Config;

                debug = config.Bind<bool>("1 - Config", "DebugMode", false, "Enabling/Disabling the debugging in the console (default = false)");
                oneDayLength = config.Bind<float>("1 - Config", "OneDayLength", 1800f, "Length of one game day in ticks. Careful when changing this value (default = 1800)");
                oneHourLength = config.Bind<float>("1 - Config", "OneHourLength", 90f, "Length of one game hour in ticks. Careful when changing this value (default = 90)");
                tooCloseForSiesta = config.Bind<string>("1 - Config", "TooCloseForSiestaText", "Can't take siesta yet, night is too close", "Text to show when night is too close for a siesta (override to translate if you need)");

                siestaHours = config.Bind<float>("2 - General", "SiestaHours", 2f, "Number of hours of each siesta (default = 2)");
                
                showDayInfoOnScreen = config.Bind<bool>("3 - Clock", "ShowDayInfoOnScreen", false, "Shows day attributes on the screen (default = false)");
                xPosClock = config.Bind<int>("3 - Clock", "xPosClock", 70, "Horizontal Position of the clock from the minimap (default = 70)");
                yPosClock = config.Bind<int>("3 - Clock", "yPosClock", -150, "Vertical Position of the clock from the minimap (default = -150)");
                fontSize = config.Bind<int>("3 - Clock", "FontSize", 18, "Clock font size (default = 18)");

                xPosClock.SettingChanged += OnClockChanged;
                yPosClock.SettingChanged += OnClockChanged;
                fontSize.SettingChanged += OnClockChanged;
            }
        }

        private static void OnClockChanged(object sender, EventArgs e)
        {
            RectTransform clockRT = UIStatsPatch.timeTextObject.transform as RectTransform;
            clockRT.anchoredPosition = new Vector2(xPosClock.Value, yPosClock.Value);
            clockRT.GetComponent<Text>().fontSize = ConfigurationFile.fontSize.Value;
        }
    }
}
