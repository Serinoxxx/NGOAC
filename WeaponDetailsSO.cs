using MalbersAnimations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.NetCode
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Malbers Animations/Netcode/WeaponDetailsSO", fileName = "New Weapon Details")]
    public class WeaponDetailsSO : ScriptableObject
    {
        [System.Serializable]
        public class WeaponDetail
        {
            public WeaponID weaponID;
            public Sprite sprite;
            public string weaponName;
        }

        public WeaponDetail[] weaponDetails;
        
    }
}
