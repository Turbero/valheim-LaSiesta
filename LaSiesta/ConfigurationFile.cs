using BepInEx.Configuration;
using BepInEx;
using System;
using UnityEngine;
using UnityEngine.UI;
using ServerSync;
using LaSiesta.Tweaks;
using TMPro;

namespace LaSiesta
{
    internal class ConfigurationFile
    {
        private static ConfigEntry<bool> _serverConfigLocked = null;
        
        public static ConfigEntry<bool> debug;
        public static ConfigEntry<float> oneDayLength;
        public static ConfigEntry<float> oneHourLength;
        public static ConfigEntry<string> tooCloseForSiesta;

        public static ConfigEntry<float> siestaHours;
        public static ConfigEntry<string> sleepSiestaQuestion;
        public static ConfigEntry<string> sleepSiestaMessage;

        public static ConfigEntry<bool> showDayInfoOnScreen;
        public static ConfigEntry<int> xPosClock;
        public static ConfigEntry<int> yPosClock;
        public static ConfigEntry<int> fontSize;

        private static ConfigFile configFile;
        
        private static readonly ConfigSync ConfigSync = new ConfigSync(LaSiesta.GUID)
        {
            DisplayName = LaSiesta.NAME,
            CurrentVersion = LaSiesta.VERSION,
            MinimumRequiredVersion = LaSiesta.VERSION
        };

        internal static void LoadConfig(BaseUnityPlugin plugin)
        {
            {
                configFile = plugin.Config;

                _serverConfigLocked = config("1 - General", "Lock Configuration", true,
                "If on, the configuration is locked and can be changed by server admins only.");
                _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

                debug = config("1 - Config", "DebugMode", false, "Enabling/Disabling the debugging in the console (default = false)");
                oneDayLength = config("1 - Config", "OneDayLength", 1800f, "Length of one game day in ticks. Careful when changing this value (default = 1800)");
                oneHourLength = config("1 - Config", "OneHourLength", 90f, "Length of one game hour in ticks. Careful when changing this value (default = 90)");
                tooCloseForSiesta = config("1 - Config", "TooCloseForSiestaText", "Can't take siesta yet, night is too close", "Text to show when night is too close for a siesta (override to translate if you need)");

                siestaHours = config("2 - General", "SiestaHours", 2f, "Number of hours of each siesta (default = 2)");
                sleepSiestaQuestion = config("2 - General", "SleepSiestaQuestion", "Sleep siesta?", "Number of hours of each siesta");
                sleepSiestaMessage = config("2 - General", "SleepSiestaMessage", "Enjoy the nap!", "Number of hours of each siesta");

                showDayInfoOnScreen = config("3 - Clock", "ShowDayInfoOnScreen", false, "Shows day attributes on the screen (default = false)", false);
                xPosClock = config("3 - Clock", "xPosClock", 70, "Horizontal Position of the clock from the minimap (default = 70)", false);
                yPosClock = config("3 - Clock", "yPosClock", -150, "Vertical Position of the clock from the minimap (default = -150)", false);
                fontSize = config("3 - Clock", "FontSize", 18, "Clock font size (default = 18)", false);

                sleepSiestaQuestion.SettingChanged += OnSettingChanged;
                sleepSiestaMessage.SettingChanged += OnSettingChanged;
                xPosClock.SettingChanged += OnSettingChanged;
                yPosClock.SettingChanged += OnSettingChanged;
                fontSize.SettingChanged += OnSettingChanged;
            }
        }

        private static void OnSettingChanged(object sender, EventArgs e)
        {
            RectTransform clockRT = ClockPatch.timeTextObject.transform as RectTransform;
            clockRT.anchoredPosition = new Vector2(xPosClock.Value, yPosClock.Value);
            clockRT.GetComponent<Text>().fontSize = ConfigurationFile.fontSize.Value;
            BedInteractMenu.siestaConfirmDialog.GetComponentInChildren<UIGroupHandler>().
                transform.Find("Exit").GetComponentInChildren<TextMeshProUGUI>().text = ConfigurationFile.sleepSiestaQuestion.Value;
        }

        private static ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new ConfigDescription(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = configFile.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
    }
}
