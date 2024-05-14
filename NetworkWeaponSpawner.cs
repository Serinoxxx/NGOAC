using MalbersAnimations.Weapons;
using Unity.Netcode;
using UnityEngine;

namespace MalbersAnimations.NetCode
{
    public class NetworkWeaponSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private Transform spawnPoint;
        
        public void Spawn()
        {
            Debug.Log("Check is server");
            if (!IsServer) return;
            Debug.Log("Check has weapon prefab");
            if (!weaponPrefab) return;
Debug.Log("Get random number");
            int randomNumber = Random.Range(100000, 999999);
Debug.Log("SpawnRPC");
            SpawnRPC(randomNumber);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SpawnRPC(int networkWeaponID)
        {
            Debug.Log("Spawned RPC");
            var newWeapon = GameObject.Instantiate(weaponPrefab, spawnPoint.position, spawnPoint.rotation);
            //Set the weapon ID to a unique value
            var weapon = newWeapon.GetComponent<NetworkWeapon>();

            weapon.networkID = networkWeaponID;
        }


    }
}