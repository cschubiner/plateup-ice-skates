using System;
using System.IO;
using UnityEngine;
using Kitchen;

namespace IceSkates.Workshop.Diagnostics;

internal static class LiveTrailCapture
{
    private static readonly bool Enabled = Array.IndexOf(Environment.GetCommandLineArgs(), "--iceskates-trail-capture") >= 0;
    private static bool captured;

    internal static void TryCapture(Component owner, TrailRenderer trail)
    {
        if (!Enabled || captured || owner.GetComponent<PlayerView>() == null || trail.positionCount < 12) return;
        captured = true;
        string directory = Path.Combine(Application.persistentDataPath, "IceSkatesChecks", KitchenIceSkates.Mod.MOD_VERSION);
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "live-streaks.png");
        ScreenCapture.CaptureScreenshot(path);
        KitchenIceSkates.Mod.LogInfo($"LIVE TRAIL CAPTURE: {path}; points={trail.positionCount}; bounds={trail.bounds}; camera={Camera.main?.name}");
    }
}
