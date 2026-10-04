using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using MapValueTracker.Config;
using System.Collections.Generic;

namespace MapValueTracker
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency("Zehs.REPOLib", BepInDependency.DependencyFlags.SoftDependency)]
    public class MapValueTracker : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "MapValueTracker";
        public const string PLUGIN_NAME = "MapValueTracker";
        public const string PLUGIN_VERSION = "1.3.1";

        public static new ManualLogSource Logger;
        private readonly Harmony harmony = new Harmony("Tansinator.REPO.MapValueTracker");

        public static MapValueTracker instance;
        public static GameObject textInstance;
        public static TextMeshProUGUI valueText;

        public static float totalValue = 0f;
        public static float totalValueInit = 0f;

        // PhysGrabObject instance IDs whose value has already been deducted on destroy.
        // Also used by CheckForItems to skip objects whose Object.Destroy is still pending this frame.
        public static readonly HashSet<int> destroyedIds = new HashSet<int>();
        // PhysGrabObject instance IDs absorbed into an ItemValuableBox (value moves to the box, not lost).
        public static readonly HashSet<int> absorbedInBoxIds = new HashSet<int>();

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

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Zehs.REPOLib"))
            {
                InitShopUpgrade();
            }
            else
            {
                Logger.LogInfo("[MapValueTracker] REPOLib not detected. Shop upgrade registration skipped.");
            }

            harmony.PatchAll();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void InitShopUpgrade()
        {
            Upgrades.MapValueTrackerUpgradeManager.Initialize();
        }

        public static void ResetValues()
        {
            totalValue = 0f;
            totalValueInit = 0f;
            destroyedIds.Clear();
            absorbedInBoxIds.Clear();

            Logger.LogDebug("In ResetValues() - Reset totalValue and totalValueInit to 0");
        }

        public static void CheckForItems(ValuableObject ignoreThis = null)
        {
            if (RoundDirector.instance == null || !RoundDirector.instance.allExtractionPointsCompleted)
            {
                totalValue = 0f;
                ValuableObject[] valuableObjects = Object.FindObjectsOfType<ValuableObject>();

                for (int i = 0; i < valuableObjects.Length; i++)
                {
                    ValuableObject vo = valuableObjects[i];
                    if (vo != null && vo != ignoreThis && vo.gameObject.activeInHierarchy && !IsPendingDestroy(vo.physGrabObject))
                    {
                        totalValue += vo.dollarValueCurrent;
                    }
                }

                // Also include any value currently stored inside Valuable Boxes on the map
                ItemValuableBox[] boxes = Object.FindObjectsOfType<ItemValuableBox>();
                for (int i = 0; i < boxes.Length; i++)
                {
                    ItemValuableBox box = boxes[i];
                    if (box != null && box.gameObject.activeInHierarchy && !IsPendingDestroy(box.GetComponent<PhysGrabObject>()))
                    {
                        totalValue += box.CurrentValue;
                    }
                }

                Logger.LogDebug("After CheckForItems Total Val: " + totalValue);
            }
        }

        private static bool IsPendingDestroy(PhysGrabObject pgo)
        {
            return pgo != null && destroyedIds.Contains(pgo.GetInstanceID());
        }
    }
}