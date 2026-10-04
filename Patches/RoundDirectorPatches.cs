using HarmonyLib;
using MapValueTracker.Config;
using TMPro;
using UnityEngine;

namespace MapValueTracker.Patches
{
    [HarmonyPatch(typeof(RoundDirector))]
    public static class RoundDirectorPatches
    {
        private static float lastDisplayedValue = -1f;
        private static bool lastStartingValueOnly = false;

        static RoundDirectorPatches()
        {
            Configuration.OnUIPositionChanged += UpdateCoordinates;
        }

        public static void UpdateCoordinates()
        {
            if (MapValueTracker.textInstance != null)
            {
                RectTransform rect = MapValueTracker.textInstance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    SetCoordinates(rect);
                }
            }
        }

        private static void SetCoordinates(RectTransform component)
        {
            component.pivot = new Vector2(1f, 1f);
            component.anchoredPosition = new Vector2(1f, -1f);
            component.anchorMin = new Vector2(0f, 0f);
            component.anchorMax = new Vector2(1f, 0f);
            component.sizeDelta = new Vector2(0f, 0f);

            Vector2 offset = Configuration.UIPosition.Value switch
            {
                Positions.LowerRight => new Vector2(0f, 125f),
                Positions.BottomRight => new Vector2(0f, 0f),
                Positions.Custom => Configuration.CustomPositionCoords.Value,
                _ => new Vector2(0f, 225f)
            };

            component.offsetMax = offset;
            component.offsetMin = offset;
        }

        [HarmonyPatch("ExtractionCompleted")]
        [HarmonyPostfix]
        public static void ExtractionComplete()
        {
            if (!SemiFunc.RunIsLevel())
                return;

            MapValueTracker.Logger.LogDebug("Extraction Completed!");
            MapValueTracker.CheckForItems();
            MapValueTracker.Logger.LogDebug("Checked after Extraction. Val is " + MapValueTracker.totalValue);
        }

        [HarmonyPatch(typeof(RoundDirector), "Update")]
        [HarmonyPostfix]
        public static void UpdateUI()
        {
            if (!SemiFunc.RunIsLevel())
            {
                if (MapValueTracker.textInstance != null && MapValueTracker.textInstance.activeSelf)
                {
                    MapValueTracker.textInstance.SetActive(false);
                }
                return;
            }

            RoundDirector director = RoundDirector.instance;
            if (director == null)
                return;

            int currentGoal = director.extractionHaulGoal;
            bool allExtractionPointsCompleted = director.allExtractionPointsCompleted;

            if (MapValueTracker.textInstance == null)
            {
                GameObject hud = GameObject.Find("Game Hud");
                GameObject haul = GameObject.Find("Tax Haul");

                if (hud == null || haul == null)
                    return;

                TMP_FontAsset font = haul.GetComponent<TMP_Text>().font;
                MapValueTracker.textInstance = new GameObject("Value HUD");
                MapValueTracker.textInstance.SetActive(false);

                MapValueTracker.valueText = MapValueTracker.textInstance.AddComponent<TextMeshProUGUI>();
                MapValueTracker.valueText.font = font;
                MapValueTracker.valueText.color = new Vector4(0.7882f, 0.9137f, 0.902f, 1);
                MapValueTracker.valueText.fontSize = 24f;
                MapValueTracker.valueText.enableWordWrapping = false;
                MapValueTracker.valueText.alignment = TextAlignmentOptions.BaselineRight;
                MapValueTracker.valueText.horizontalAlignment = HorizontalAlignmentOptions.Right;
                MapValueTracker.valueText.verticalAlignment = VerticalAlignmentOptions.Baseline;

                MapValueTracker.textInstance.transform.SetParent(hud.transform, false);

                RectTransform component = MapValueTracker.textInstance.GetComponent<RectTransform>();
                SetCoordinates(component);

                lastDisplayedValue = -1f;
                return;
            }

            if (!Upgrades.MapValueTrackerUpgradeManager.IsTrackerUnlocked())
            {
                MapValueTracker.textInstance.SetActive(false);
                return;
            }

            if (MapValueTracker.valueText != null && (currentGoal > 0 || !allExtractionPointsCompleted))
            {
                float valueToDisplay = Configuration.StartingValueOnly.Value ? MapValueTracker.totalValueInit : MapValueTracker.totalValue;
                if (Mathf.Abs(valueToDisplay - lastDisplayedValue) > 0.01f || lastStartingValueOnly != Configuration.StartingValueOnly.Value)
                {
                    lastDisplayedValue = valueToDisplay;
                    lastStartingValueOnly = Configuration.StartingValueOnly.Value;
                    MapValueTracker.valueText.SetText("Map: $" + valueToDisplay.ToString("N0"));
                }

                bool mapPressed = (MapToolController.instance != null && MapToolController.instance.mapToggled) || SemiFunc.InputHold(InputKey.Map);

                switch (Configuration.DisplayMode.Value)
                {
                    case DisplayModes.AlwaysOn:
                        MapValueTracker.textInstance.SetActive(true);
                        break;

                    case DisplayModes.ValueRatio:
                        bool ratioReached = currentGoal > 0 && (MapValueTracker.totalValue / (float)currentGoal) <= Configuration.ValueRatio.Value;
                        MapValueTracker.textInstance.SetActive(ratioReached || mapPressed);
                        break;

                    case DisplayModes.OnMapKey:
                    default:
                        MapValueTracker.textInstance.SetActive(mapPressed);
                        break;
                }
            }
            else
            {
                MapValueTracker.textInstance.SetActive(false);
            }
        }
    }
}
