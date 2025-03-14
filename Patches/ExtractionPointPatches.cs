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
        [HarmonyPatch("StateSet")]
        [HarmonyPostfix]
        public static void StateSet(ExtractionPoint.State newState)
        {
            if (newState != ExtractionPoint.State.Complete)
                return;

            MapValueTracker.Logger.LogDebug("Extraction Complete!");
            MapValueTracker.ResetValues();
            float total = 0;
            List<ValuableObject> valuables = GameObject.FindObjectsOfType<ValuableObject>().ToList();
            foreach (var valuable in valuables)
            {
                MapValueTracker.Logger.LogDebug("Remaining item: " + valuable.dollarValueCurrent);
                total += valuable.dollarValueCurrent;
                MapValueTracker.totalValue += valuable.dollarValueCurrent;
            }
            MapValueTracker.Logger.LogDebug("Remaining total: " + total);
        }
    }
}
