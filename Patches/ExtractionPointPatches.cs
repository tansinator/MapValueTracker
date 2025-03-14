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
        [HarmonyPatch(typeof(ExtractionPoint), "HaulGoalSetRPC")]
        [HarmonyPostfix]
        public static void HaulGoalSetRPC(ExtractionPoint __instance)
        {
            if (SemiFunc.RunIsLevel())
            {
                MapValueTracker.CheckForItems();
            }
        }
    }
}
