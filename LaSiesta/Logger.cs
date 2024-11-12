using UnityEngine;

namespace LaSiesta
{
    public static class Logger
    {
        internal static void Log(object s)
        {
            if (!ConfigurationFile.debug.Value)
            {
                return;
            }

            var toPrint = $"{LaSiesta.NAME} {LaSiesta.VERSION}: {(s != null ? s.ToString() : "null")}";

            Debug.Log(toPrint);
        }

        internal static void LogWarning(object s)
        {
            var toPrint = $"{LaSiesta.NAME} {LaSiesta.VERSION}: {(s != null ? s.ToString() : "null")}";

            Debug.LogWarning(toPrint);
        }

        internal static void LogError(object s)
        {
            var toPrint = $"{LaSiesta.NAME} {LaSiesta.VERSION}: {(s != null ? s.ToString() : "null")}";

            Debug.LogError(toPrint);
        }
    }
}
