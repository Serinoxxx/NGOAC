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
        private MDisplayTrajectory displayTrajectory;

        private void Start()
        {
            Debug.Log("Start method called.");

            // Get the reference to MShootable using GetComponent
            mShootable = GetComponent<MShootable>();
            displayTrajectory = GetComponent<MDisplayTrajectory>();

            mShootable.OnEquiped.AddListener(OnEquiped);
            mShootable.OnUnequiped.AddListener(OnUnequiped);
            
        }

        private void OnUnequiped(Transform arg0)
        {
            if (networkAnimal == null) return;

            displayTrajectory.enabled = false;

            //Remove listeners
            mShootable.OnFireProjectile.RemoveListener(networkAnimal.SpawnProjectileOnOtherClients);
            mShootable.OnFireProjectile.RemoveListener(DestroyProjectile);

        }

        private void OnEquiped(Transform arg0)
        {
            networkAnimal = GetComponentInParent<NetworkAnimal>();

            if (networkAnimal == null) return;

            if (networkAnimal.IsOwner)
            {
                displayTrajectory.enabled = true;
                mShootable.Enabled = true;
            }
            else
            {
                mShootable.Enabled = false;
            }


            if (mShootable != null)
            {
                Debug.Log("MShootable component found. Subscribing to events.");
                // Subscribe to the OnFireProjectile event of MShootable
                if (networkAnimal.IsOwner)
                {
                    //Only the owner should spawn the projectile on other clients
                    mShootable.OnFireProjectile.AddListener(networkAnimal.SpawnProjectileOnOtherClients);
                }
                else
                {
                    //Destroy any projectile that is fired by other clients but triggered by this client
                    mShootable.OnFireProjectile.AddListener(DestroyProjectile);
                }
            }
            else
            {
                Debug.LogError("MShootable component not found on the same GameObject.");
            }
        }

        private void DestroyProjectile(GameObject arg0)
        {
            Destroy(arg0);
        }

        // Make sure to clean up the event subscription when the object is destroyed
        private void OnDestroy()
        {
            if (networkAnimal == null) return;

            Debug.Log("OnDestroy method called.");
            if (mShootable != null)
            {
                mShootable.OnFireProjectile.RemoveListener(networkAnimal.SpawnProjectileOnOtherClients);
                mShootable.OnFireProjectile.RemoveListener(DestroyProjectile);
            }
        }
    }
}