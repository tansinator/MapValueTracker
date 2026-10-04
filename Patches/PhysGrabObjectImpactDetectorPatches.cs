using HarmonyLib;
using UnityEngine;

namespace MapValueTracker.Patches
{
    /// <summary>
    /// Damage accounting. Deducts only the actual drop in dollarValueCurrent caused by this hit.
    /// Any value remaining on the object is deducted when the object is destroyed (see DestroyPhysGrabObjectPatch).
    ///
    /// Why not use the valueLost argument: on a shatter (&lt;15% of original), the game overwrites valueLost with the
    /// item's full pre-hit value, and on host/singleplayer the destroy chain (DestroyObject -> DestroyObjectRPC ->
    /// onDestroy -> DestroyPhysGrabObjectRPC) runs synchronously *inside* BreakRPC, i.e. before this postfix.
    /// Measuring the before/after delta is correct regardless of call order or argument mutation.
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabObjectImpactDetector), "BreakRPC")]
    static class BreakRPCPatch
    {
        static void Prefix(PhysGrabObjectImpactDetector __instance, out float __state)
        {
            __state = (__instance != null && __instance.valuableObject != null)
                ? __instance.valuableObject.dollarValueCurrent
                : 0f;
        }

        static void Postfix(PhysGrabObjectImpactDetector __instance, bool _loseValue, float __state)
        {
            if (!_loseValue || __instance == null || !SemiFunc.RunIsLevel())
                return;

            ValuableObject vo = __instance.valuableObject;
            if (vo == null)
                return;

            float lost = Mathf.Max(0f, __state - vo.dollarValueCurrent);
            MapValueTracker.totalValue = Mathf.Max(0f, MapValueTracker.totalValue - lost);

            MapValueTracker.Logger.LogDebug($"BreakRPC - {vo.name} lost {lost} (now {vo.dollarValueCurrent}). Map Remaining: {MapValueTracker.totalValue}");
        }
    }

    /// <summary>
    /// Destroy accounting. Deducts whatever value is still on the object (extraction, shatter remainder,
    /// death pit, despawn), unless the object was absorbed into a Valuable Box.
    /// </summary>
    [HarmonyPatch(typeof(PhysGrabObject), "DestroyPhysGrabObjectRPC")]
    static class DestroyPhysGrabObjectPatch
    {
        static void Postfix(PhysGrabObject __instance)
        {
            if (!SemiFunc.RunIsLevel() || __instance == null)
                return;

            int id = __instance.GetInstanceID();

            // Only account for each object once
            if (!MapValueTracker.destroyedIds.Add(id))
                return;

            // Absorbed into a Valuable Box: value moves to the box, not lost
            if (MapValueTracker.absorbedInBoxIds.Remove(id))
                return;

            // A Valuable Box itself was destroyed or extracted
            ItemValuableBox box = __instance.GetComponent<ItemValuableBox>();
            if (box != null)
            {
                if (box.CurrentValue > 0f)
                {
                    MapValueTracker.totalValue = Mathf.Max(0f, MapValueTracker.totalValue - box.CurrentValue);
                    MapValueTracker.Logger.LogDebug($"Valuable Box destroyed/extracted! Val: {box.CurrentValue}. Map Remaining: {MapValueTracker.totalValue}");
                }
                return;
            }

            ValuableObject vo = __instance.GetComponent<ValuableObject>();
            if (vo != null && vo.dollarValueCurrent > 0f)
            {
                MapValueTracker.totalValue = Mathf.Max(0f, MapValueTracker.totalValue - vo.dollarValueCurrent);
                MapValueTracker.Logger.LogDebug($"Destroyed Valuable Object! {vo.name} Val: {vo.dollarValueCurrent}. Map Remaining: {MapValueTracker.totalValue}");
            }
        }
    }

    /// <summary>
    /// Marks valuables being absorbed into an ItemValuableBox so their subsequent destroy is not deducted.
    /// Runs on all clients (host calls it directly; clients via StartAbsorbRPC).
    /// </summary>
    [HarmonyPatch(typeof(ItemValuableBox), "StartAbsorbLocal")]
    static class ItemValuableBoxAbsorbPatch
    {
        static void Prefix(PhysGrabObject target)
        {
            if (target != null)
            {
                MapValueTracker.absorbedInBoxIds.Add(target.GetInstanceID());
            }
        }
    }
}
