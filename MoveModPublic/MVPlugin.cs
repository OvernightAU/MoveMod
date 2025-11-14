using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MoveModPublic.Modules;
using MoveModPublic.Patches;
using System;
using System.Linq;
using UnityEngine;

namespace MoveModPublic;

[BepInAutoPlugin("com.pietro420.movemod")]
[BepInProcess("Among Us.exe")]
public partial class MVPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    public static MVPlugin Instance { get; private set; }

    public bool IgnoreZoom  { get; set; }

    public MoveWithMouse ModInstance;

    public delegate void LogCallback(string condition, string stackTrace, LogType type);
    public ConfigEntry<int> MoveRateLimit { get; private set; }


    public override void Load()
    {
        MoveRateLimit = Config.Bind("General", "MoveRateLimit", 0,
                        "Controls how often networked movement logic runs relative to FixedUpdate calls.\n" +
                        "0 or 1 means the logic runs every FixedUpdate (no rate limiting).\n" +
                        "Values greater than 1 run the logic once every N FixedUpdate calls, reducing update frequency.");

        Instance = this;
        ModInstance = AddComponent<MoveWithMouse>();
        ClassInjector.RegisterTypeInIl2Cpp<MoveWithMouse>();
        Harmony.PatchAll();
    }

    public static int GetRealVersion()
    {
        return Constants.GetVersion(Constants.Year, Constants.Month, Constants.Day, Constants.Revision);
    }

    public static int GetLastVentId(PlayerControl pc)
    {
        CoEnterVentPatch.VentIdMap.TryGetValue(pc, out var id);
        return id;
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start)), HarmonyPriority(Priority.VeryHigh)]
    public class ShowModStamp
    {
        public static void Postfix()
        {
            ModManager.Instance.ShowModStamp();

            var plugins = IL2CPPChainloader.Instance.Plugins;
            var ignoredMods = new[] { "com.gurge44.endlesshostroles", "MalumMenu" };

            Instance.IgnoreZoom = plugins.Keys.Any(mod => ignoredMods.Contains(mod) || mod.ToLower().Contains("townofhost"));
        }
    }
}