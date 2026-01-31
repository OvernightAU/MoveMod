using HarmonyLib;
using MoveModPublic.Modules;
using Reactor.Utilities;
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
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
public static class StoreOutOfBoundsCollider
{
    public static PolygonCollider2D OutOfBoundsCollider = null;
    public static EdgeCollider2D EdgeOutOfBoundsCollider = null;

    public static void Postfix(ShipStatus __instance)
    {
        EdgeCollider2D EdgeCollider = null;

        if (__instance.TryCast<SkeldShipStatus>())
        {
            EdgeCollider = __instance.transform.Find("starfield").GetComponent<EdgeCollider2D>();
        }
        else if (__instance.TryCast<MiraShipStatus>())
        {
            EdgeCollider = __instance.transform.Find("CloudGen").GetComponent<EdgeCollider2D>();
        }
        else if (__instance.TryCast<PolusShipStatus>())
        {
            EdgeCollider = __instance.transform.Find("OuterBoundary").GetComponent<EdgeCollider2D>();
        }
        else if (__instance.TryCast<AirshipStatus>())
        {
            EdgeCollider = __instance.transform.Find("Boundary").GetComponent<EdgeCollider2D>();
        }
        else if (__instance.TryCast<FungleShipStatus>())
        {
            EdgeCollider = __instance.transform.Find("GhostBoundary").GetComponent<EdgeCollider2D>();
        }
        else
        {
            Logger<MVPlugin>.Warning("Boundary Collider Not Found! Please patch ShipStatus.Awake and set MoveModPublic.Patches.StoreOutOfBoundsCollider.OutOfBoundsCollider manually.");
        }

        if (EdgeCollider != null)
        {
            EdgeOutOfBoundsCollider = EdgeCollider;
            OutOfBoundsCollider = EdgeCollider.gameObject.AddComponent<PolygonCollider2D>();

            Vector2[] originalPoints = EdgeCollider.points;
            Vector2 center = GetCentroid(originalPoints);
            Vector2[] shrunkPoints = new Vector2[originalPoints.Length];

            for (int i = 0; i < originalPoints.Length; i++)
            {
                Vector2 direction = originalPoints[i] - center;
                shrunkPoints[i] = center + direction * 0.92f; 
            }

            OutOfBoundsCollider.points = shrunkPoints;
            OutOfBoundsCollider.isTrigger = true;
        }
    }

    private static Vector2 GetCentroid(Vector2[] points)
    {
        Vector2 sum = Vector2.zero;
        foreach (Vector2 point in points)
        {
            sum += point;
        }
        return sum / points.Length;
    }
}