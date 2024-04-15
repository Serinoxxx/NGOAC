using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MalbersAnimations.NetCode
{
    [System.Serializable]
    public class UIHolster
    {
        public HolsterID holster;
        public Image image;
    }

    public class NetworkPlayerUIController : MonoBehaviour
    {
        [SerializeField] List<UIHolster> UIHolsters;
        [SerializeField] WeaponSpriteMappingSO weaponSpriteMap;

        MInventory playerInventory;

        public static NetworkPlayerUIController Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (PlayerRegister.Instance == null)
            {
                Debug.LogError("This component cannot function without a PlayerRegister in the scene!");
                return;
            }

            PlayerRegister.Instance.OnPlayerAdded += PlayerRegister_OnPlayerAdded;
        }

        private void PlayerRegister_OnPlayerAdded(object sender, PlayerAddedEventArgs e)
        {
            if (e.playerNetworkObject.IsOwner)
            {
                playerInventory = e.playerNetworkObject.GetComponent<MInventory>();
            }
        }

        internal void UpdateInventoryUI(HolsterID holsterID, WeaponID weaponID)
        {
            foreach (var uiHolster in UIHolsters)
            {
                if (holsterID == uiHolster.holster)
                { 
                    // Found a weapon in this holster, set the sprite
                    var sprite = weaponSpriteMap.weaponSpriteMap.First(x=>x.weaponID =  weaponID).sprite;
                    uiHolster.image.sprite = sprite;
                }
            }

        }
    }
}
