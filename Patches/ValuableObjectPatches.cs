using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MapValueTracker.Patches
{
    [HarmonyPatch(typeof(ValuableObject))]
    static class ValuableObjectPatches
    {
        [HarmonyPatch("DollarValueSetLogic")]
        [HarmonyPostfix]
        static void DollarValueSet(ValuableObject __instance)
        {
            MapValueTracker.Logger.LogDebug("Started Valuable Object! " + __instance.name + " Val: " + __instance.dollarValueCurrent);
            MapValueTracker.totalValue += __instance.dollarValueCurrent;
            MapValueTracker.Logger.LogDebug("Total Val: " + MapValueTracker.totalValue);
        }
    }
}
