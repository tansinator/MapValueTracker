using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace MapValueTracker.Config
{
    public enum DisplayModes
    {
        AlwaysOn,
        OnMapKey,
        ValueRatio
    }

    public enum Positions
    {
        Default,
        LowerRight,
        BottomRight,
        Custom
    }

    internal class Configuration
    {
        public static ConfigEntry<DisplayModes> DisplayMode;
        public static ConfigEntry<float> ValueRatio;
        public static ConfigEntry<bool> StartingValueOnly;
        public static ConfigEntry<Positions> UIPosition;
        public static ConfigEntry<Vector2> CustomPositionCoords;

        public static ConfigEntry<bool> RequireShopUpgrade;
        public static ConfigEntry<bool> TeamWideUnlock;

        public static event Action OnUIPositionChanged;

        public static void Init(ConfigFile config)
        {
            config.SaveOnConfigSet = false;

            RequireShopUpgrade = config.Bind(
                "Shop Progression",
                "RequireShopUpgrade",
                false,
                "If true, the Map Value Tracker will only be visible after purchasing the Map Value Tracker upgrade from the Shop ($4,000 - $7,000). Default is false."
            );

            TeamWideUnlock = config.Bind(
                "Shop Progression",
                "TeamWideUnlock",
                true,
                "If true and RequireShopUpgrade is enabled, purchasing the upgrade unlocks the tracker for all players on the team. Default is true."
            );

            DisplayMode = config.Bind(
                "Display",
                "DisplayMode",
                DisplayModes.AlwaysOn,
                "How the tracker is displayed on screen:\n- AlwaysOn: Always visible on HUD.\n- OnMapKey: Only visible while holding or toggling the Map key (Tab by default).\n- ValueRatio: Automatically appears when remaining map value reaches or drops below the ValueRatio threshold (also appears while holding Map key)."
            );

            ValueRatio = config.Bind(
                "Display",
                "ValueRatio",
                2.0f,
                new ConfigDescription(
                    "Ratio of remaining map value to extraction goal. Used when DisplayMode is set to ValueRatio. (e.g. 2.0 = displays when remaining value is <= 2x the goal).",
                    new AcceptableValueRange<float>(0.5f, 5.0f)
                )
            );

            StartingValueOnly = config.Bind(
                "Display",
                "StartingValueOnly",
                false,
                "Toggle to keep the Map Value fixed to the level's initially generated value. Will not update value in real time from breaking items, killing enemies, or extracting loot."
            );

            UIPosition = config.Bind(
                "Position",
                "UIPosition",
                Positions.Default,
                "Preset position of the Value Tracker UI element on screen."
            );

            CustomPositionCoords = config.Bind(
                "Position",
                "CustomPositionCoords",
                new Vector2(0, 0),
                "Custom X,Y coordinates when UIPosition is set to Custom. Bottom-right corner is (0, 0). Default is (0, 225)."
            );

            MigrateOldConfig(config);

            UIPosition.SettingChanged += (_, _) => OnUIPositionChanged?.Invoke();
            CustomPositionCoords.SettingChanged += (_, _) => OnUIPositionChanged?.Invoke();

            config.Save();
            config.SaveOnConfigSet = true;
        }

        static void MigrateOldConfig(ConfigFile cfg)
        {
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            if (orphanedEntriesProp == null)
                return;

            var orphanedEntries = orphanedEntriesProp.GetValue(cfg) as Dictionary<ConfigDefinition, string>;
            if (orphanedEntries == null || orphanedEntries.Count == 0)
                return;

            bool oldAlwaysOn = true;
            bool oldUseRatio = false;
            bool hasOldAlwaysOn = false;

            foreach (var kvp in orphanedEntries)
            {
                string section = kvp.Key.Section;
                string key = kvp.Key.Key;
                string val = kvp.Value;

                if (section == "Default" && key == "AlwaysOn" && bool.TryParse(val, out var aOn))
                {
                    oldAlwaysOn = aOn;
                    hasOldAlwaysOn = true;
                }
                else if (section == "Default" && key == "UseValueRatio" && bool.TryParse(val, out var uRatio))
                {
                    oldUseRatio = uRatio;
                }
                else if (section == "Default" && key == "StartingValueOnly" && bool.TryParse(val, out var sVal))
                {
                    StartingValueOnly.Value = sVal;
                }
                else if (section == "Default" && key == "ValueRatio" && float.TryParse(val, out var vRatio))
                {
                    ValueRatio.Value = vRatio;
                }
                else if (section == "UIPosition" && key == "UIPosition" && Enum.TryParse<Positions>(val, out var uPos))
                {
                    UIPosition.Value = uPos;
                }
                else if (section == "UIPosition" && key == "CustomPositionCoords")
                {
                    try
                    {
                        CustomPositionCoords.Value = (Vector2)TomlTypeConverter.ConvertToValue(val, typeof(Vector2));
                    }
                    catch
                    {
                        // Ignore parse failure on custom coordinates
                    }
                }
            }

            if (hasOldAlwaysOn)
            {
                if (oldAlwaysOn)
                    DisplayMode.Value = DisplayModes.AlwaysOn;
                else if (oldUseRatio)
                    DisplayMode.Value = DisplayModes.ValueRatio;
                else
                    DisplayMode.Value = DisplayModes.OnMapKey;
            }

            orphanedEntries.Clear();
        }
    }
}