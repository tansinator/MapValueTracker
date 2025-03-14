using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace MapValueTracker.Config
{

	internal class Configuration
    {
        public static ConfigEntry<bool> AlwaysOn;
        public static ConfigEntry<float> ValueRatio;

        public static void Init(ConfigFile config)
        {
            config.SaveOnConfigSet = false;

            AlwaysOn = config.Bind(
                "Default",
                "AlwaysOn",
                true,
                "Toggle to always display map value when an extraction goal is active."
            );

            ValueRatio = config.Bind(
                "Default",
                "ValueRatio",
                2.0f,
                "Ratio of Map Value to Extraction Goal. Ex: 20k map value to 10k goal is 2.0"
            );

            ClearOrphanedEntries(config);
            config.Save();
            config.SaveOnConfigSet = true;
        }

        static void ClearOrphanedEntries(ConfigFile cfg)
        {
            // Find the private property `OrphanedEntries` from the type `ConfigFile` //
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            // And get the value of that property from our ConfigFile instance //
            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg);
            // And finally, clear the `OrphanedEntries` dictionary //
            orphanedEntries.Clear();
        }
    }

}