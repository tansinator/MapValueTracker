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
            MapValueTracker.Logger.LogDebug("Room generation started. Now val is " + MapValueTracker.totalValue);
        }

        [HarmonyPatch("GenerateDone")]
        [HarmonyPostfix]
        public static void GenerateDonePostfix()
        {
            // StartRoomGeneration only runs on the host; GenerateDone runs on every client,
            // so reset here too before taking the initial count.
            MapValueTracker.ResetValues();
            MapValueTracker.Logger.LogDebug("Generation done. Computing initial map items.");
            MapValueTracker.CheckForItems();
            MapValueTracker.totalValueInit = MapValueTracker.totalValue;
            MapValueTracker.Logger.LogDebug("Generation done. Now val is " + MapValueTracker.totalValue + ". Init Value: " + MapValueTracker.totalValueInit);
        }
    }
}
