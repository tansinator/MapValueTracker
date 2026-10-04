using UnityEngine;

namespace MapValueTracker.Upgrades
{
    public class ItemUpgradeMapValueTracker : MonoBehaviour
    {
        private ItemToggle itemToggle;

        private void Start()
        {
            itemToggle = GetComponent<ItemToggle>();
        }

        public void Upgrade()
        {
            string steamId = null;
            if (itemToggle != null)
            {
                var playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(itemToggle.playerTogglePhotonID);
                if (playerAvatar != null)
                {
                    steamId = SemiFunc.PlayerGetSteamID(playerAvatar);
                }
            }

            if (string.IsNullOrEmpty(steamId) && SemiFunc.PlayerAvatarLocal() != null)
            {
                steamId = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
            }

            MapValueTrackerUpgradeManager.UnlockForPlayer(steamId);
        }
    }
}
