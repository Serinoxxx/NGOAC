// In NetworkShootable class

using System;
using MalbersAnimations;
using MalbersAnimations.Weapons;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.NetCode
{
    public class NetworkShootable : MonoBehaviour
    {
        private MShootable mShootable;
        private NetworkAnimal networkAnimal;

        private void Start()
        {
            Debug.Log("Start method called.");

            // Get the reference to MShootable using GetComponent
            mShootable = GetComponent<MShootable>();

            mShootable.OnEquiped.AddListener(AddFiredListener);
            mShootable.OnUnequiped.AddListener(RemoveFiredListener);
        }

        private void RemoveFiredListener(Transform arg0)
        {
            if (networkAnimal == null) return;

            mShootable.OnFireProjectile.RemoveListener(networkAnimal.SpawnProjectileOnOtherClients);
        }

        private void AddFiredListener(Transform arg0)
        {
            networkAnimal = GetComponentInParent<NetworkAnimal>();

            if (networkAnimal == null) return;

            if (mShootable != null)
            {
                Debug.Log("MShootable component found. Subscribing to events.");
                // Subscribe to the OnFireProjectile event of MShootable
                mShootable.OnFireProjectile.AddListener(networkAnimal.SpawnProjectileOnOtherClients);
            }
            else
            {
                Debug.LogError("MShootable component not found on the same GameObject.");
            }
        }



        // Make sure to clean up the event subscription when the object is destroyed
        private void OnDestroy()
        {
            if (networkAnimal == null) return;

            Debug.Log("OnDestroy method called.");
            if (mShootable != null)
            {
                mShootable.OnFireProjectile.RemoveListener(networkAnimal.SpawnProjectileOnOtherClients);
            }
        }
    }
}