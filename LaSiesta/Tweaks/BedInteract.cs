﻿using HarmonyLib;
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
            if (siestaConfirmDialog != null) return;
            Logger.Log("Postfix definiendo siestaConfirmDialog");

            siestaConfirmDialog = GameObject.Instantiate(Menu.instance.m_quitDialog.gameObject, Menu.instance.m_quitDialog.transform.parent.parent);
            siestaConfirmDialog.name = "SiestaConfirmDialog";

            UIGroupHandler dialog = siestaConfirmDialog.GetComponentInChildren<UIGroupHandler>();
            dialog.transform.Find("Exit").GetComponentInChildren<TextMeshProUGUI>().text = ConfigurationFile.sleepSiestaQuestion.Value;

            Button[] btns = dialog.GetComponentsInChildren<Button>();
            Button btnYes = btns[0];
            btnYes.onClick = new Button.ButtonClickedEvent();
            btnYes.onClick.AddListener(() => {
                Logger.Log("Yes button clicked");
                siestaConfirmDialog.SetActive(false);
                InventoryGui.instance.Hide();
                BedInteractPatch.skipToBeforeNight();
            });
            Button btnNo = btns[1];
            btnNo.onClick = new Button.ButtonClickedEvent();
            btnNo.onClick.AddListener(() => {
                Logger.Log("No button clicked");
                siestaConfirmDialog.SetActive(false);
                InventoryGui.instance.Hide();
            });
        }
    }

    [HarmonyPatch(typeof(Bed), "Interact")]
    public class BedInteractPatch
    {
        private static bool runningSiesta = false;

        public static bool Prefix(Humanoid human, bool repeat, bool alt, ref bool __result)
        {
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

            if (!EnvMan.IsNight())
            {
                if (BedInteractMenu.siestaConfirmDialog != null) {
                    BedInteractMenu.siestaConfirmDialog.SetActive(true);
                    InventoryGui.instance.Show(null);
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
            if (comparison < 0.75f)
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, ConfigurationFile.sleepSiestaMessage.Value);
                runningSiesta = true;
                turnCheatMode(true);
                Console.instance.TryRunCommand($"skiptime {timeInHours}", true);
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
