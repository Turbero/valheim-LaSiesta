using HarmonyLib;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace LaSiesta
{


    [HarmonyPatch(typeof(Bed), "Interact")]
    public class BedInteractPatch
    {
        private static bool runningSiesta = false;

        static bool Prefix(Humanoid human, bool repeat, bool alt, ref bool __result)
        {
            if (repeat)
            {
                __result = false;
                return false;
            }

            Player player = human as Player;
            if (player == null)
            {
                __result = false;
                return false;
            }

            if (!EnvMan.IsNight())
            {
                skipToBeforeNight();
                __result = true; // Returns successful interaction
                return false; // Blocks running original code
            }
            return true;  // Run original code if it is night
        }

        private static void skipToBeforeNight()
        {
            if (runningSiesta) return;

            float oneDay = ConfigurationFile.oneDayLength.Value;
            float oneHour = ConfigurationFile.oneHourLength.Value;
            float hours = ConfigurationFile.siestaHours.Value;
            float timeInHours = hours * oneHour;
            Logger.Log($"One day: {oneDay}, one hour: {oneHour}, hours: {hours}, Seconds to skip: {timeInHours}");

            float dayFraction = (float)typeof(EnvMan).GetField("m_smoothDayFraction", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(EnvMan.instance);
            float comparison = dayFraction + (timeInHours / oneDay);
            Logger.Log($"old fraction: {dayFraction}, new fraction: {comparison}");

            //Don't do if night coming. m_smoothDayFraction = 0,75 at 18:00. 
            if (comparison < 0.75f)
            {
                turnCheatMode(true);
                Console.instance.TryRunCommand($"skiptime {timeInHours}", true);
                turnCheatMode(false);
                //_ = WaitForSecondsAsyncOnly(10);
            } else
            {
                turnCheatMode(false);
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Can't take siesta yet, night is too close"); //TODO Localize
            }
        }

        private static async Task WaitForSecondsAsyncOnly(float seconds)
        {
            await Task.Delay((int)(Math.Max(0f, seconds) * 1000)); // to miliseconds
            Player.m_localPlayer.AttachStop();
        }

        private static void turnCheatMode(bool cheat)
        {
            Type terminalType = typeof(Terminal); // m_cheat is in Terminal
            FieldInfo cheatField = terminalType.GetField("m_cheat", BindingFlags.NonPublic | BindingFlags.Static);
            cheatField.SetValue(Console.instance, cheat);
            Logger.Log($"** CheatMode {cheat}");
        }
    }
}
