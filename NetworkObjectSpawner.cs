using MalbersAnimations;
using MalbersAnimations.Reactions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BearTag.NetCode
{

    public class NetworkObjectSpawner : NetworkBehaviour
    {
        public static NetworkObjectSpawner Instance;

        [SerializeField] NetworkObject networkObject;
        [SerializeField] Transform spawnPoint;

        [SerializeField] NetworkObject bearNetworkObject;
        [SerializeField] NetworkObject wolfNetworkObject;


        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Spawns the Network Object at the Spawn Point described in the Inspector
        /// </summary>
        public void SpawnNetworkObject()
        {
            SpawnNetworkObject(networkObject, spawnPoint.position, spawnPoint.lossyScale, spawnPoint.rotation);
        }

        /// <summary>
        /// Spawns the Network Object at the Spawn Point provided
        /// </summary>
        /// <param name="networkObject">the Network Object to be spawned</param>
        /// <param name="spawnPoint">the Spawn Point</param>
        public void SpawnNetworkObject(NetworkObject networkObject, Vector3 spawnPoint, Vector3 scale = default, Quaternion rotation = default)
        {
            if (!IsHost)
            {
                return;
            }

            var instantiatedNetworkObject = Instantiate(networkObject, spawnPoint, rotation, null);
            SceneManager.MoveGameObjectToScene(instantiatedNetworkObject.gameObject,
            SceneManager.GetSceneByName(gameObject.scene.name));
            instantiatedNetworkObject.transform.localScale = scale;
            instantiatedNetworkObject.Spawn(destroyWithScene: true);
        }

        public void SpawnBearHere(Vector3 position, Vector3 scale = default, Quaternion rotation = default)
        {
            if (!IsHost)
            {
                return;
            }
            SpawnBearHereRpc(position, scale, rotation);
        }

        [Rpc(SendTo.Server)]
        public void SpawnBearHereRpc(Vector3 position, Vector3 scale = default, Quaternion rotation = default)
        {
            SpawnNetworkObject(bearNetworkObject, position, scale, rotation);
        }

        public void SpawnWolfHere(Vector3 position, Vector3 scale = default, Quaternion rotation = default)
        {
            if (!IsHost)
            {
                return;
            }
            SpawnWolfHereRpc(position, scale, rotation);
        }

        [Rpc(SendTo.Server)]
        public void SpawnWolfHereRpc(Vector3 position, Vector3 scale = default, Quaternion rotation = default)
        {
            SpawnNetworkObject(wolfNetworkObject, position, scale, rotation);
        }
    }
}
