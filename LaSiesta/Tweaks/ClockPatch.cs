using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace LaSiesta.Tweaks
{
    [HarmonyPatch(typeof(Hud), "Awake")]
    public class ClockPatch
    {
        private static Text timeText;
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

            // Configuración del componente de texto
            timeText = timeTextObject.AddComponent<Text>();
            timeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            timeText.fontSize = 18;
            timeText.color = Color.white;
            timeText.alignment = TextAnchor.UpperCenter;

            // Ajustar la posición debajo del minimapa
            RectTransform rectTransform = timeText.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(70, -150);
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

            // Método para obtener la hora del juego en formato hh:mm
            private static string GetCurrentGameTime()
            {
                // Obtener la instancia de EnvMan
                EnvMan envMan = EnvMan.instance;
                if (envMan == null)
                {
                    return "00:00";  // Valor por defecto si no se puede obtener la hora
                }

                // Usar reflexión para acceder al campo privado 'm_smoothDayFraction'
                FieldInfo dayFractionField = typeof(EnvMan).GetField("m_smoothDayFraction", BindingFlags.NonPublic | BindingFlags.Instance);
                if (dayFractionField == null)
                {
                    Logger.LogError("Couldn't access m_smoothDayFraction.");
                    return "00:00";
                }

                // Obtener el valor de m_smoothDayFraction
                float smoothDayFraction = (float)dayFractionField.GetValue(envMan);

                // Convertir el tiempo fraccionado en horas y minutos
                int hours = (int)(smoothDayFraction * 24);              // Hora en formato de 24 horas
                int minutes = (int)((smoothDayFraction * 1440) % 60);   // Minutos (1440 = 24 horas * 60 minutos)

                // Formatear el resultado en hh22:mi
                return $"{hours:D2}:{minutes:D2}";
            }
        }
    }

}
