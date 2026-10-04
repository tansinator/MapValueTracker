using HarmonyLib;

namespace MapValueTracker.Patches
{
    /// <summary>
    /// Shared rules for counting a valuable that receives its dollar value mid-level
    /// (enemy drops, surplus valuables, etc.).
    /// Valuables that exist at generation time are counted by CheckForItems() in GenerateDone instead.
    /// </summary>
    static class ValuableSpawnAccounting
    {
        public static bool LevelGenerated =>
            LevelGenerator.Instance != null && LevelGenerator.Instance.Generated;

        public static void Add(ValuableObject vo, float value, string source)
        {
            MapValueTracker.totalValue += value;
            MapValueTracker.Logger.LogDebug($"Spawned Valuable Object ({source})! {vo.name} Val: {value}. Total Val: {MapValueTracker.totalValue}");
        }
    }

    /// <summary>
    /// Host / singleplayer. DollarValueSetLogic is called more than once per valuable
    /// (ValuableDirector.SpawnValuable, then again from the DollarValueSet coroutine) but only
    /// assigns a value on the first call, so only count the call that flips dollarValueSet false -> true.
    /// </summary>
    [HarmonyPatch(typeof(ValuableObject), "DollarValueSetLogic")]
    static class DollarValueSetLogicPatch
    {
        static void Prefix(ValuableObject __instance, out bool __state)
        {
            __state = __instance.dollarValueSet;
        }

        static void Postfix(ValuableObject __instance, bool __state)
        {
            if (__state || !__instance.dollarValueSet)
                return;
            if (!SemiFunc.IsMasterClientOrSingleplayer() || !ValuableSpawnAccounting.LevelGenerated)
                return;

            ValuableSpawnAccounting.Add(__instance, __instance.dollarValueCurrent, "Logic");
        }
    }

    /// <summary>
    /// Clients. The host sends DollarValueSetRPC to Others; only count it if it actually set the value.
    /// </summary>
    [HarmonyPatch(typeof(ValuableObject), "DollarValueSetRPC")]
    static class DollarValueSetRPCPatch
    {
        static void Prefix(ValuableObject __instance, out bool __state)
        {
            __state = __instance.dollarValueSet;
        }

        static void Postfix(ValuableObject __instance, float value, bool __state)
        {
            if (__state || !__instance.dollarValueSet)
                return;
            if (!ValuableSpawnAccounting.LevelGenerated)
                return;

            ValuableSpawnAccounting.Add(__instance, value, "RPC");
        }
    }
}
