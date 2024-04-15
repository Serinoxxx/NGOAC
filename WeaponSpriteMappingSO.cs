using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.NetCode
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Malbers Animations/Netcode/WeaponSpriteMapping", fileName = "New Weapon Sprite Mapping")]
    public class WeaponSpriteMappingSO : ScriptableObject
    {
        [System.Serializable]
        public class WeaponSprite
        {
            public WeaponID weaponID;
            public Sprite sprite;
        }

        public WeaponSprite[] weaponSpriteMap;
    }
}
