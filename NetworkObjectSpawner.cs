using MalbersAnimations;
using MalbersAnimations.Reactions;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalbersAnimations.NetCode
{

    public class NetworkObjectSpawner : NetworkBehaviour
    {
        [SerializeField] NetworkObject networkObject;
        [SerializeField] Transform spawnPoint;
        public void SpawnNetworkObject()
        {

            if (!IsHost)
            {
                return;
            }

            var instantiatedNetworkObject = Instantiate(networkObject, spawnPoint.position, spawnPoint.rotation, null);
            SceneManager.MoveGameObjectToScene(instantiatedNetworkObject.gameObject,
                SceneManager.GetSceneByName(gameObject.scene.name));
            instantiatedNetworkObject.transform.localScale = spawnPoint.lossyScale;
            instantiatedNetworkObject.Spawn(destroyWithScene: true);
        }
    }
}
