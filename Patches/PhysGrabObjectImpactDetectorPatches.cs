using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapValueTracker.Patches
{
    [HarmonyPatch(typeof(PhysGrabObjectImpactDetector))]
    public static class PhysGrabObjectImpactDetectorPatches
    {

        [HarmonyPatch("BreakRPC")]
        [HarmonyPostfix]
        static void StartPostFix(float valueLost, PhysGrabObjectImpactDetector? __instance)
        {
            MapValueTracker.Logger.LogDebug("BreakRPC - Current Value: " + MapValueTracker.totalValue);

            ValuableObject vo = __instance?.GetComponent<ValuableObject>();

            MapValueTracker.Logger.LogDebug("BreakRPC - Valuable Object current value: " + vo?.dollarValueCurrent);
            MapValueTracker.Logger.LogDebug("BreakRPC - Value lost: " + valueLost);

            MapValueTracker.totalValue -= valueLost;

            MapValueTracker.Logger.LogDebug("BreakRPC - After Break Value: " + MapValueTracker.totalValue);
        }

        [HarmonyPatch(typeof(PhysGrabObject), "DestroyPhysGrabObjectRPC")]
        [HarmonyPostfix]
        public static void DestroyPhysGrabObjectPostfix(PhysGrabObject __instance)
        {
            if (SemiFunc.RunIsLevel())
            {
                var vo = __instance.GetComponent<ValuableObject>();
                MapValueTracker.Logger.LogInfo("Destroying (DPGO)!");
                MapValueTracker.Logger.LogDebug("Destroyed Valuable Object! " + vo.name + " Val: " + vo.dollarValueCurrent);
                MapValueTracker.totalValue -= vo.dollarValueCurrent;
                MapValueTracker.Logger.LogDebug("Map Remaining Val: " + MapValueTracker.totalValue);
            }
        }
    }
}
