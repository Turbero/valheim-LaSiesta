using System;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace LaSiesta.Tweaks
{
    [HarmonyPatch(typeof(Hud), "Awake")]
    public class ClockPatch
    {
        private static TextMeshProUGUI timeText;
        public static GameObject timeTextObject;

        static void Postfix(Hud __instance)
        {
            // Crear un nuevo objeto de texto y añadirlo al minimapa
            GameObject minimap = Hud.instance?.transform.Find("hudroot")?.transform.Find("MiniMap")?.transform.Find("small")?.gameObject;            
            if (minimap == null)
            {
                Logger.LogError("Minimap not found.");
                return;
            }

            timeTextObject = new GameObject("LaSiestaTimeText");
            timeTextObject.transform.SetParent(minimap.transform);

            timeText = timeTextObject.AddComponent<TextMeshProUGUI>();
            timeText.font = getFontAsset("Valheim-AveriaSansLibre");
            timeText.fontSize = ConfigurationFile.fontSize.Value;
            timeText.color = Color.white;
            timeText.alignment = TextAlignmentOptions.TopRight;

            RectTransform rectTransform = timeText.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(ConfigurationFile.xPosClock.Value, ConfigurationFile.yPosClock.Value);
        }
        
        private static TMP_FontAsset getFontAsset(String name)
        {
            Logger.Log($"Finding {name} font...");
            var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var font in allFonts)
            {
                if (font.name == name)
                {
                    Logger.Log($"{name} font found.");
                    return font;
                }
            }
            Logger.LogError($"{name} font NOT found.");
            return null;
        }

        [HarmonyPatch(typeof(Hud), "Update")]
        public static class Hud_Update_Patch
        {            
            static void Postfix()
            {
                timeTextObject.SetActive(ConfigurationFile.showDayInfoOnScreen.Value);
                if (!ConfigurationFile.showDayInfoOnScreen.Value) return;

                if (timeText != null)
                {
                    timeText.text = GetCurrentGameTime();
                }
            }

            private static string GetCurrentGameTime()
            {
                EnvMan envMan = EnvMan.instance;
                if (envMan == null)
                {
                    return "00:00";
                }

                FieldInfo dayFractionField = typeof(EnvMan).GetField("m_smoothDayFraction", BindingFlags.NonPublic | BindingFlags.Instance);
                if (dayFractionField == null)
                {
                    Logger.LogError("Couldn't access m_smoothDayFraction.");
                    return "00:00";
                }

                float smoothDayFraction = (float)dayFractionField.GetValue(envMan);

                int hours = (int)(smoothDayFraction * 24);              // Hora en formato de 24 horas
                int minutes = (int)((smoothDayFraction * 1440) % 60);   // Minutos (1440 = 24 horas * 60 minutos)

                return $"{hours:D2}:{minutes:D2}";
            }
        }
    }

}
