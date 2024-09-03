using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MalbersAnimations.NetCode
{
    public class OwnerComponentEnabler : NetworkBehaviour
    {
        [Tooltip("The components you want to enable for owner only")]
        [SerializeField] MonoBehaviour[] _components;
        [Tooltip("The gameObjects you want to set active for owner only")]
        [SerializeField] GameObject[] _gameObjects;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                foreach (var component in _components)
                {
                    component.enabled = true;
                }

                foreach (var gameObject in _gameObjects)
                {
                    gameObject.SetActive(true);
                }
            }
        }
    }
}
