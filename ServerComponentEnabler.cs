using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MalbersAnimations.NetCode
{
    public class ServerComponentEnabler : NetworkBehaviour
    {
        [Tooltip("The components you want to enable for server only")]
        [SerializeField] MonoBehaviour[] _components;
        [Tooltip("The gameObjects you want to set active for server only")]
        [SerializeField] GameObject[] _gameObjects;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
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
