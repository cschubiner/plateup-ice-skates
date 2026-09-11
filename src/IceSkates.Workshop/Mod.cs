using System.Reflection;
using HarmonyLib;
using IceSkates.Workshop.GDO;
using IceSkates.Workshop.Runtime;
using KitchenLib;
using KitchenLib.Event;
using KitchenMods;
using UnityEngine;

namespace KitchenIceSkates;

public sealed class Mod : BaseMod, IModSystem
{
    public const string MOD_GUID = "clay.plateup.iceskates";
    public const string MOD_NAME = "Ice Skates";
    public const string MOD_VERSION = "0.3.1";
    public const string MOD_AUTHOR = "Clay";
    public const string MOD_GAMEVERSION = ">=1.5.0";

#if DEBUG
    public const bool DEBUG_MODE = true;
#else
    public const bool DEBUG_MODE = false;
#endif

    public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly())
    {
    }

    protected override void OnInitialise()
    {
        SkateRuntimeState.Reset("mod initialise");
        LogInfo($"{MOD_GUID} v{MOD_VERSION} initialised. Debug={DEBUG_MODE}");
        LogInfo($"Game {Application.version}; KitchenLib {typeof(BaseMod).Assembly.GetName().Version}; Harmony {typeof(Harmony).Assembly.GetName().Version}");
    }

    protected override void OnPostActivate(KitchenMods.Mod mod)
    {
        LogInfo("Registering Ice Skates GDOs.");
        AddGameDataObject<IceSkatesItem>();
        AddGameDataObject<IceSkatesProviderAppliance>();

        Events.BuildGameDataEvent += (_, _) =>
        {
            LogInfo("Game data built for Ice Skates.");
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "--iceskates-self-test") >= 0)
                IceSkates.Workshop.Diagnostics.RuntimeChecks.Schedule();
        };
    }

    public static void LogInfo(string message) => Debug.Log($"[{MOD_NAME}] {message}");
    public static void LogWarning(string message) => Debug.LogWarning($"[{MOD_NAME}] {message}");
    public static void LogError(string message) => Debug.LogError($"[{MOD_NAME}] {message}");
}
