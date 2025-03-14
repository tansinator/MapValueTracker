using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using MapValueTracker.Config;
using TMPro;
using UnityEngine;

namespace MapValueTracker.Patches
{
    [HarmonyPatch(typeof(RoundDirector))]
    public static class RoundDirectorPatches
    {
    //    [HarmonyPatch("StartRoundLogic")]
    //    [HarmonyPostfix]
    //    public static void StartRoundLogicPostfix()
    //    {

    //    }

        [HarmonyPatch(typeof(RoundDirector), "Update")]
        [HarmonyPostfix]
        public static void UpdateUI()
        {
            int? currentGoal = Traverse.Create(RoundDirector.instance).Field("extractionHaulGoal").GetValue<int>();
            bool extractActive = Traverse.Create(RoundDirector.instance).Field("extractionPointActive").GetValue<bool>(); 
            bool allExtractionPointsCompleted = Traverse.Create(RoundDirector.instance).Field("allExtractionPointsCompleted").GetValue<bool>();

            if (!SemiFunc.RunIsLevel())
                return;

            if (MapValueTracker.textInstance == null)
            {
                GameObject hud = GameObject.Find("Game Hud");
                GameObject haul = GameObject.Find("Tax Haul");

                if (hud == null || haul == null)
                {
                    return;
                }

                TMP_FontAsset font = haul.GetComponent<TMP_Text>().font;
                MapValueTracker.textInstance = new GameObject();
                MapValueTracker.textInstance.SetActive(false);
                MapValueTracker.textInstance.name = "Value HUD";
                MapValueTracker.textInstance.AddComponent<TextMeshProUGUI>();

                MapValueTracker.valueText = MapValueTracker.textInstance.GetComponent<TextMeshProUGUI>();
                MapValueTracker.valueText.font = font;
                MapValueTracker.valueText.color = new Vector4(0.7882f, 0.9137f, 0.902f, 1);
                MapValueTracker.valueText.fontSize = 24f;
                MapValueTracker.valueText.enableWordWrapping = false;
                MapValueTracker.valueText.alignment = TextAlignmentOptions.BaselineRight;
                MapValueTracker.valueText.horizontalAlignment = HorizontalAlignmentOptions.Right;
                MapValueTracker.valueText.verticalAlignment = VerticalAlignmentOptions.Baseline;

                MapValueTracker.textInstance.transform.SetParent(hud.transform, false);

                RectTransform component = MapValueTracker.textInstance.GetComponent<RectTransform>();

                component.pivot = new Vector2(1f, 1f);
                component.anchoredPosition = new Vector2(1f, -1f);
                component.anchorMin = new Vector2(0f, 0f);
                component.anchorMax = new Vector2(1f, 0f);
                component.sizeDelta = new Vector2(0f, 0f);
                component.offsetMax = new Vector2(0, 225f);
                component.offsetMin = new Vector2(0f, 225f);

                return;
            }
            if (MapValueTracker.valueText != null && ((currentGoal != null && currentGoal != 0) || !allExtractionPointsCompleted) && (Configuration.AlwaysOn.Value || ((MapValueTracker.totalValue / (float)currentGoal) <= Configuration.ValueRatio.Value)))
            {
                RectTransform component = MapValueTracker.textInstance.GetComponent<RectTransform>();

                component.pivot = new Vector2(1f, 1f);
                component.anchoredPosition = new Vector2(1f, -1f);
                component.anchorMin = new Vector2(0f, 0f);
                component.anchorMax = new Vector2(1f, 0f);
                component.sizeDelta = new Vector2(0f, 0f);
                component.offsetMax = new Vector2(0, 225f);
                component.offsetMin = new Vector2(0f, 225f);

                MapValueTracker.textInstance.SetActive(true);

                MapValueTracker.valueText.SetText("Map: $" + MapValueTracker.totalValue.ToString("N0"));
            }
            else
                MapValueTracker.textInstance.SetActive(false);
        }
    }
}
