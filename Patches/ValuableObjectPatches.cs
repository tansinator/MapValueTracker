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
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void Start(ValuableObject __instance)
        {
            //__instance.gameObject.AddComponent<MyOnDestroy>();
            //MapValueTracker.Logger.LogDebug("Added OnDestroy");
        }
        [HarmonyPatch("DollarValueSetLogic")]
        [HarmonyPostfix]
        static void DollarValueSet(ValuableObject __instance)
        {
            MapValueTracker.Logger.LogDebug("Created Valuable Object! " + __instance.name + " Val: " + __instance.dollarValueCurrent);
            //MapValueTracker.totalValue += __instance.dollarValueCurrent;
            MapValueTracker.CheckForItems();
            MapValueTracker.Logger.LogDebug("After dollar value set Total Val: " + MapValueTracker.totalValue);
        }
    }
}
