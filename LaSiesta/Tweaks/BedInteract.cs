using HarmonyLib;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaSiesta.Tweaks
{
    public class BedInteractMenu
    {
        public static GameObject siestaConfirmDialog;

        public static void loadDialog()
        {
            if (siestaConfirmDialog != null)
            {
                return;
            }

            Logger.Log("Creating siestaConfirmDialog...");
            siestaConfirmDialog = GameObject.Instantiate(
                GameObject.Find("_GameMain/LoadingGUI/PixelFix/IngameGui/Menu/MenuRoot/ExitConfirm"),
                GameObject.Find("_GameMain/LoadingGUI/PixelFix/IngameGui/Menu/MenuRoot").transform);
            siestaConfirmDialog.name = "SiestaConfirmDialog";

            UIGroupHandler dialog = siestaConfirmDialog.GetComponentInChildren<UIGroupHandler>();
            dialog.transform.Find("Exit").GetComponentInChildren<TextMeshProUGUI>().text =
                ConfigurationFile.sleepSiestaQuestion.Value;

            Button[] btns = dialog.GetComponentsInChildren<Button>();
            Button btnYes = btns[0];
            btnYes.onClick = new Button.ButtonClickedEvent();
            btnYes.onClick.AddListener(() =>
            {
                Logger.Log("Yes button clicked");
                showSiestaDialog(false);
                BedInteractPatch.skipToBeforeNight();
            });
            Button btnNo = btns[1];
            btnNo.onClick = new Button.ButtonClickedEvent();
            btnNo.onClick.AddListener(() =>
            {
                Logger.Log("No button clicked");
                showSiestaDialog(false);
            });
        }

        public static void showSiestaDialog(bool show)
        {
            siestaConfirmDialog.SetActive(show);
            if (show)
            {
                Menu.instance.Show();
                Menu.instance.transform.Find("MenuRoot/Menu/MenuEntries").gameObject.SetActive(false);
                Menu.instance.transform.Find("MenuRoot/Menu/ornament").gameObject.SetActive(false);
            }
            else
            {
                Menu.instance.transform.Find("MenuRoot/Menu/MenuEntries").gameObject.SetActive(true);
                Menu.instance.transform.Find("MenuRoot/Menu/ornament").gameObject.SetActive(true);
                Menu.instance.Hide();
            }
        }
    }

    [HarmonyPatch(typeof(Bed), "Interact")]
    public class BedInteractPatch
    {
        private static bool runningSiesta = false;

        public static bool Prefix(Humanoid human, bool repeat, bool alt, ref bool __result)
        {
            Logger.Log("BedInteractPatch Prefix...");
            BedInteractMenu.loadDialog();

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
            Logger.Log("Checking IsNight...");
            if (!EnvMan.IsNight())
            {
                Logger.Log("Showing siestaConfirmDialog...");
                if (BedInteractMenu.siestaConfirmDialog != null)
                {
                    BedInteractMenu.showSiestaDialog(true);
                    __result = true; // Returns successful interaction
                    return false; // Blocks running original code
                }
            }
            return true;  // Run original code if it is night
        }

        public static void skipToBeforeNight()
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
            //Comparing with integer numbers
            if ((int)(Math.Round(comparison-0.005, 2)*100) < 75)
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, ConfigurationFile.sleepSiestaMessage.Value);
                runningSiesta = true;
                turnCheatMode(true);
                Console.instance.TryRunCommand($"skiptime {timeInHours}", true);
                Game.instance.GetPlayerProfile().m_playerStats.m_stats.IncrementOrSet(PlayerStatType.Cheats, -1);
                turnCheatMode(false);
                _ = WaitForSecondsAsyncOnly(10);
            } else
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, ConfigurationFile.tooCloseForSiesta.Value);
            }
        }

        private static async Task WaitForSecondsAsyncOnly(float seconds)
        {
            await Task.Delay((int)(Math.Max(0f, seconds) * 1000)); // to miliseconds
            runningSiesta = false;
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
