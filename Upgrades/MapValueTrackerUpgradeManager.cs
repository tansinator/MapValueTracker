using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using REPOLib.Modules;
using UnityEngine;

namespace MapValueTracker.Upgrades
{
    public static class MapValueTrackerUpgradeManager
    {
        public const string ItemName = "Item Upgrade Map Value Tracker";
        public const string DisplayName = "Map Value Tracker";

        public static readonly Dictionary<string, int> playerUpgradeMapValueTracker = new Dictionary<string, int>();

        private static bool isInitialized = false;
        private static Texture2D cachedIconTexture = null;

        public static void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            try
            {
                Item vanillaItem = Resources.Load<Item>("Items/Item Upgrade Map Player Count");
                if (vanillaItem == null)
                {
                    Item[] allItems = Resources.LoadAll<Item>("Items");
                    foreach (Item itm in allItems)
                    {
                        if (itm != null && (itm.name == "Item Upgrade Map Player Count" || itm.itemName == "Map Player Count"))
                        {
                            vanillaItem = itm;
                            break;
                        }
                    }
                }

                if (vanillaItem == null)
                {
                    MapValueTracker.Logger.LogWarning("[MapValueTracker] Could not find vanilla 'Item Upgrade Map Player Count' to clone.");
                    return;
                }

                GameObject vanillaPrefab = vanillaItem.prefab != null && vanillaItem.prefab.Prefab != null
                    ? vanillaItem.prefab.Prefab
                    : Resources.Load<GameObject>("Items/Item Upgrade Map Player Count");

                if (vanillaPrefab == null)
                {
                    MapValueTracker.Logger.LogWarning("[MapValueTracker] Could not find vanilla prefab for 'Item Upgrade Map Player Count'.");
                    return;
                }

                // Clone Item ScriptableObject
                Item customItem = UnityEngine.Object.Instantiate(vanillaItem);
                customItem.name = ItemName;
                customItem.itemName = DisplayName;
                customItem.description = "Displays the total value of valuables remaining on the map.";
                customItem.minPlayerCount = 1;
                customItem.maxAmountInShop = 1;
                customItem.maxPurchase = true;
                customItem.maxPurchaseAmount = 1;

                Value val = ScriptableObject.CreateInstance<Value>();
                val.valueMin = 4000;
                val.valueMax = 7000;
                customItem.value = val;

                // Clone Prefab
                GameObject customPrefab = UnityEngine.Object.Instantiate(vanillaPrefab);
                customPrefab.name = ItemName;
                UnityEngine.Object.DontDestroyOnLoad(customPrefab);
                customPrefab.hideFlags = HideFlags.HideAndDontSave;

                // Apply custom visuals and materials
                ApplyCustomMaterials(customPrefab);

                // Replace upgrade component
                var oldUpgrade = customPrefab.GetComponent<ItemUpgradeMapPlayerCount>();
                if (oldUpgrade != null)
                {
                    UnityEngine.Object.DestroyImmediate(oldUpgrade);
                }

                var newUpgrade = customPrefab.AddComponent<ItemUpgradeMapValueTracker>();
                var itemUpgrade = customPrefab.GetComponent<ItemUpgrade>();
                if (itemUpgrade != null)
                {
                    itemUpgrade.upgradeEvent.RemoveAllListeners();
                    itemUpgrade.upgradeEvent.AddListener(newUpgrade.Upgrade);
                }

                var itemAttributes = customPrefab.GetComponent<ItemAttributes>();
                if (itemAttributes != null)
                {
                    itemAttributes.item = customItem;
                }

                // Register with REPOLib
                Items.RegisterItem(itemAttributes);
                MapValueTracker.Logger.LogInfo("[MapValueTracker] Successfully registered 'Map Value Tracker' shop upgrade ($4,000 - $7,000) with REPOLib.");
            }
            catch (Exception ex)
            {
                MapValueTracker.Logger.LogError($"[MapValueTracker] Failed to register shop upgrade: {ex}");
            }
        }

        public static Texture2D GetIconTexture()
        {
            if (cachedIconTexture != null) return cachedIconTexture;

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("MapValueTracker.Resources.tracker_icon.jpg");
                if (stream != null)
                {
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);

                    cachedIconTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    cachedIconTexture.LoadImage(data);
                    cachedIconTexture.name = "MapValueTrackerIcon";
                }
            }
            catch (Exception ex)
            {
                MapValueTracker.Logger.LogWarning($"[MapValueTracker] Could not load embedded tracker_icon.jpg: {ex}");
            }

            return cachedIconTexture;
        }

        private static void ApplyCustomMaterials(GameObject prefab)
        {
            Texture2D icon = GetIconTexture();

            foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material mat = materials[i];
                    if (mat == null) continue;

                    string matName = mat.name.ToLowerInvariant();
                    bool isScreenOrDisplay = matName.Contains("screen") ||
                                             matName.Contains("display") ||
                                             matName.Contains("icon") ||
                                             matName.Contains("face") ||
                                             mat.mainTexture != null;

                    if (isScreenOrDisplay)
                    {
                        if (icon != null)
                        {
                            mat.mainTexture = icon;
                            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", icon);
                            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", icon);
                        }

                        mat.EnableKeyword("_EMISSION");
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            mat.SetColor("_EmissionColor", new Color(0.2f, 0.95f, 0.35f) * 1.8f);
                        }
                    }
                    else
                    {
                        Color chassisColor = new Color(0.12f, 0.20f, 0.15f);
                        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", chassisColor);
                        if (mat.HasProperty("_Color")) mat.SetColor("_Color", chassisColor);
                    }
                }
                renderer.materials = materials;
            }
        }

        public static void UnlockForPlayer(string steamId)
        {
            if (string.IsNullOrEmpty(steamId) && SemiFunc.PlayerAvatarLocal() != null)
            {
                steamId = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
            }

            if (!string.IsNullOrEmpty(steamId))
            {
                playerUpgradeMapValueTracker[steamId] = 1;
                MapValueTracker.Logger.LogInfo($"[MapValueTracker] Unlocked Map Value Tracker for player: {steamId}");
            }
        }

        public static bool IsTrackerUnlocked()
        {
            if (!Config.Configuration.RequireShopUpgrade.Value)
            {
                return true;
            }

            PlayerAvatar local = SemiFunc.PlayerAvatarLocal();
            if (local != null)
            {
                string localId = SemiFunc.PlayerGetSteamID(local);
                if (!string.IsNullOrEmpty(localId) &&
                    playerUpgradeMapValueTracker.TryGetValue(localId, out int lvl) && lvl > 0)
                {
                    return true;
                }
            }

            if (Config.Configuration.TeamWideUnlock.Value)
            {
                foreach (var kvp in playerUpgradeMapValueTracker)
                {
                    if (kvp.Value > 0) return true;
                }
            }

            return false;
        }

        public static void ResetProgress()
        {
            playerUpgradeMapValueTracker.Clear();
            MapValueTracker.Logger.LogInfo("[MapValueTracker] Upgrade progress reset for new run.");
        }
    }

    [HarmonyPatch(typeof(StatsManager), "Start")]
    internal static class StatsManagerStartPatch
    {
        private static void Postfix(StatsManager __instance)
        {
            if (!__instance.dictionaryOfDictionaries.ContainsKey("playerUpgradeMapValueTracker"))
            {
                __instance.dictionaryOfDictionaries.Add("playerUpgradeMapValueTracker", MapValueTrackerUpgradeManager.playerUpgradeMapValueTracker);
                __instance.upgradesInfo.Add("playerUpgradeMapValueTracker", new StatsManager.UpgradeInfo
                {
                    displayName = "Map Value Tracker",
                    displayNameLocalized = null
                });
            }
        }
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.ResetProgress))]
    internal static class RunManagerResetProgressPatch
    {
        private static void Postfix()
        {
            MapValueTrackerUpgradeManager.ResetProgress();
        }
    }
}
