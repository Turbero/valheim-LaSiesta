using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LaSiesta
{
    internal class UIStats
    {

        public static string GetCurrentGameTime()
        {
            // Obtener la instancia de EnvMan
            EnvMan envMan = EnvMan.instance;
            if (envMan == null)
            {
                Logger.LogError("EnvMan no está disponible.");
                return "00:00";  // Valor por defecto si no se puede obtener la hora
            }

            // Usar reflexión para acceder al campo privado 'm_smoothDayFraction'
            FieldInfo dayFractionField = typeof(EnvMan).GetField("m_smoothDayFraction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (dayFractionField == null)
            {
                Logger.LogError("No se pudo acceder a m_smoothDayFraction.");
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
