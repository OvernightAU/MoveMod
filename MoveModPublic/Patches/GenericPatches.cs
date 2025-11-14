using HarmonyLib;
using MoveModPublic.Modules;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoveModPublic.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
[HarmonyPriority(Priority.LowerThanNormal)]
class GenericPatches
{

    static void Postfix(ref int __result)
    {
        if (SceneManager.GetActiveScene().name == "FindAGame")
        {
            return;
        }
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
        {
            if (MVPlugin.GetRealVersion() != __result) return;
            if (MVConstants.DisableModdedProtocol) return;
            __result += 25;
        }
    }
}
[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
public static class IsVersionModdedPatch
{
    public static void Postfix(ref bool __result)
    {
        if (MVConstants.DisableModdedProtocol) return;
        __result = true;
    }
}
[HarmonyPatch(typeof(ProgressTracker), nameof(ProgressTracker.Start))]
class TaskUpdatePatch
{
    static void Postfix(ProgressTracker __instance)
    {
        __instance.transform.GetChild(2).GetComponent<TextMeshPro>().text = TranslationController.Instance.currentLanguage.languageID switch
        {
            SupportedLangs.Portuguese or SupportedLangs.Brazilian => "Feito por pietro420",
            _ => "Made by pietro420",
        };
    }
}
[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
public static class CoEnterVentPatch
{
    public static Dictionary<PlayerControl, int> VentIdMap = [];

    public static void Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
    {
        VentIdMap[__instance.myPlayer] = id;
    }
}