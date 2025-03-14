using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using MapValueTracker.Config;

namespace MapValueTracker
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class MapValueTracker : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "MapValueTracker";
        public const string PLUGIN_NAME = "MapValueTracker";
        public const string PLUGIN_VERSION = "1.0.5";

        public static new ManualLogSource Logger;
        private readonly Harmony harmony = new Harmony("Tansinator.REPO.MapValueTracker");

        public static MapValueTracker instance;
        public static GameObject textInstance;
        public static TextMeshProUGUI valueText;

        public static float totalValue = 0f;

        public void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            if (instance == null)
            {
                instance = this;
            }

            Configuration.Init(Config);

            harmony.PatchAll();
        }

        public static void ResetValues()
        {
            totalValue = 0;

            Logger.LogDebug("In ResetValues()");

            /*if (!SemiFunc.RunIsLevel())
                return;

            ValuableDirector vd = ValuableDirector.instance;

            if (vd == null || vd.valuableList == null)
            {
                return;
            }

            int valuables = vd.valuableList.Count;

            for (int i = 0; i < valuables; i++)
            {
                totalValue += (int)vd.valuableList[i].dollarValueCurrent;
            }*/

            Logger.LogInfo("Total Map Value: " + totalValue);
        }
    }

    public class MyOnDestroy : MonoBehaviour
    {
        void OnDestroy()
        {
            MapValueTracker.Logger.LogInfo("Destroying!");
            var vo = GetComponent<ValuableObject>();
            MapValueTracker.Logger.LogDebug("Destroyed Valuable Object! " + vo.name + " Val: " + vo.dollarValueCurrent);
            MapValueTracker.totalValue -= vo.dollarValueCurrent;
            MapValueTracker.Logger.LogDebug("Total Val: " + MapValueTracker.totalValue);
        }
    }
}