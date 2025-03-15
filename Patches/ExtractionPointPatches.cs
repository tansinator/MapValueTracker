using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MapValueTracker.Patches
{
    [HarmonyPatch(typeof(ExtractionPoint))]

    static class ExtractionPointPatches
    {
        [HarmonyPatch("StateComplete")]
        [HarmonyPostfix]
        public static void StateComplete()
        {
           if (SemiFunc.RunIsLevel())
               return;

            MapValueTracker.Logger.LogDebug("Extraction Complete!");

            MapValueTracker.CheckForItems();

            MapValueTracker.Logger.LogDebug("Checked after Extraction. Val is " + MapValueTracker.totalValue);
        }
    }
}
