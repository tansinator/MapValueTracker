using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapValueTracker.Patches
{
    [HarmonyPatch(typeof(LevelGenerator))]
    static class LevelGeneratorPatches
    {
        [HarmonyPatch("StartRoomGeneration")]
        [HarmonyPrefix]
        public static void StartRoomGeneration()
        {
            MapValueTracker.Logger.LogDebug("Generating Started. Resetting to zero.");
            MapValueTracker.ResetValues();
            MapValueTracker.Logger.LogDebug("Reset. Now val is " + MapValueTracker.totalValue);
        }
    }
}
