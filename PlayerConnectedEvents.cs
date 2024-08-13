using MalbersAnimations.Controller;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using MalbersAnimations.Weapons;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace MalbersAnimations.NetCode
{
    public class PlayerConnectedEvents : NetworkBehaviour
    {
        [SerializeField] UnityEventRaiser cameraEventRaiser;
        [SerializeField] TransformHook cameraTransformHook;
        private MalbersInput malbersInput;
        private MAnimal mAnimal;
        [SerializeField] private Stats statsLocal;
        [SerializeField] private Stats statsRemote;
        private MEventListener eventListener;
        private MDamageable damageable;

        private MWeaponManager weaponManager;
        private MInventory inventory;
        [SerializeField] MPickUp pickUpDrop;
        private MInteractor interactor;
        [SerializeField] GameObject staminaSprintData;

        private Aim aim;
        [SerializeField] Transform aimTarget;

        [SerializeField] StatID healthStatID;
        [SerializeField] StatID staminaStatID;

        public Transform GetMainCameraTarget()
        {
            return cameraTransformHook.transform;
        }

        private void Start()
        {
            mAnimal.ResetController();
        }

        public override void OnNetworkSpawn()
        {
            PlayerRegister.Instance.AddPlayer(this.NetworkObject);

            aim = GetComponentInChildren<Aim>();

            mAnimal = GetComponentInChildren<MAnimal>();
            interactor = GetComponentInChildren<MInteractor>();
            malbersInput = GetComponentInChildren<MalbersInput>();
            eventListener = GetComponentInChildren<MEventListener>();
            weaponManager = GetComponentInChildren<MWeaponManager>();
            inventory = GetComponentInChildren<MInventory>();
            damageable = GetComponentInChildren<MDamageable>();

            // Make sure to find the camera after the player connects
            mAnimal.FindCamera();
            if (IsServer)
            {
                //Host/Server manages damage
                EnableComponentIfNotNull(damageable);
            }

            if (IsOwner)
            {
                Destroy(statsRemote);
                damageable.stats = statsLocal;

                // Enable Player Specific Components
                EnableComponentIfNotNull(cameraEventRaiser);
                EnableComponentIfNotNull(cameraTransformHook);
                EnableComponentIfNotNull(malbersInput);
                EnableComponentIfNotNull(mAnimal);
                EnableComponentIfNotNull(eventListener);
                EnableComponentIfNotNull(weaponManager);
                EnableComponentIfNotNull(inventory);

                mAnimal.ResetController();

                if (interactor != null)
                {
                    var collider = interactor.GetComponent<Collider>();
                    collider.enabled = true;
                    interactor.InteractionArea = collider;
                    interactor.enabled = true;
                }
                else
                {
                    Debug.LogWarning("Attempting to enable a null component.");
                }

                if (staminaSprintData != null)
                {

                    staminaSprintData.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("Attempting to enable a null component.");
                }

                // Listen to the equipped weapon event
                weaponManager.OnEquipWeapon.AddListener(OnEquippedWeapon);
                weaponManager.OnUnequipWeapon.AddListener(OnUnequipWeapon);

                // Listen to the onAim event
                aim.OnAiming.AddListener(OnAiming);

                // Listen to the animal states and modes
                mAnimal.OnModeStart.AddListener(OnModeStart);
                mAnimal.OnStateChange.AddListener(OnStateChange);
                mAnimal.OnStanceChange.AddListener(OnStanceChange);
            }

            if (!IsOwner)
            {
                Destroy(statsLocal);
                damageable.stats = statsRemote;

                //If this is not ours then set the aim target
                aim.AimTarget = aimTarget;

                //Initialize the animal controller
                mAnimal.ResetController();

                //Shrink the pickup collider so it doesn't trigger UI events (can't seem to disable it, or other components re-enable it)
                var pickupCollider = interactor.GetComponentInChildren<BoxCollider>();
                pickupCollider.size = Vector3.zero;


            }
        }

        private void OnStanceChange(int arg0)
        {
            //throw new NotImplementedException();
        }

        private void OnStateChange(int stateID)
        {
            OnStateChangeServerRpc(stateID);
        }
        [Rpc(SendTo.Server)]
        private void OnStateChangeServerRpc(int stateID)
        {
            OnStateChangeClientRpc(stateID);
        }

        [Rpc(SendTo.NotOwner)]
        private void OnStateChangeClientRpc(int stateID)
        {
            mAnimal.State_Activate(stateID);
        }

        private void OnModeStart(int arg0, int arg1)
        {
            //throw new NotImplementedException();
        }

        private void EnableComponentIfNotNull(MonoBehaviour component)
        {
            if (component != null)
            {
                component.enabled = true;
            }
            else
            {
                Debug.LogWarning($"Attempting to enable a null component:{component.name}");
            }
        }

        private void LateUpdate()
        {
            //The owner should update the position of the aimTarget. The networkTransform component will send the updates to other clients
            if (IsOwner)
            {
                aimTarget.position = aim.RawPoint;
            }
        }

        #region OnAiming
        private void OnAiming(bool arg0)
        {
            OnAimingServerRpc(arg0);
        }

        [Rpc(SendTo.Server)]
        void OnAimingServerRpc(bool arg0)
        {
            OnAimingClientRpc(arg0);
        }

        [Rpc(SendTo.NotOwner)]
        void OnAimingClientRpc(bool arg0)
        {
            aim.OnAiming.Invoke(arg0);
        }
        #endregion

        #region WeaponManagerEvents

        MWeapon currentWeapon;
        void OnEquippedWeapon(GameObject weaponGO)
        {
            // Find the unique Id for the weapon to send to server and request pickup
            var weapon = weaponGO.GetComponent<MWeapon>();

            //We own this weapon, so call the rpc to replicate a hit when we deal damage
            weapon.OnHitPosition.AddListener((x)=>WeaponHitHandler(x));

            if (weapon is MShootable)
            {
                ((MShootable)weapon).OnFireProjectile.AddListener((x) => HandleProjectile(x)); //Call the event for the weapon
            }

            var networkWeapon = weaponGO.GetComponent<NetworkWeapon>();
            var uniqueWeaponId = networkWeapon.networkID;
            currentWeapon = weapon;
            EquipWeaponServerRpc(uniqueWeaponId);
            Debug.Log($"Owner: I'm picking up weapon id: {uniqueWeaponId}");

            // Tell the UI to update, must be done here so other clients don't updat their UI
            NetworkPlayerUIController.Instance.UpdateInventoryUI(weapon.Holster, weapon.WeaponMode);
        }

        private void HandleProjectile(GameObject x)
        {
            var projectile = x.GetComponent<MProjectile>();

            if (projectile != null)
            {
                SpawnProjectileServerRpc(x.transform.position, projectile.Velocity);
            }
        }
        [Rpc(SendTo.Server)]
        private void SpawnProjectileServerRpc(Vector3 position, Vector3 velocity)
        {
            MShootable shootable = currentWeapon as MShootable;
            var projectileGO = Instantiate(shootable.Projectile, position, Quaternion.identity);
            var projectile = projectileGO.GetComponent<MProjectile>();
            projectile.Velocity = velocity;
            projectile.Prepare(gameObject, shootable.Gravity, velocity, shootable.Layer, shootable.TriggerInteraction);
            projectile.Fire(velocity);
            projectile.GetComponent<NetworkObject>().Spawn();
        }

        void WeaponHitHandler(Vector3 position)
        {
            WeaponHitRpc(position);
        }
        [Rpc(SendTo.NotOwner)]
        void WeaponHitRpc(Vector3 position)
        {
            if (currentWeapon == null)
            {
                return;
            }

            currentWeapon.OnHit?.Invoke(currentWeapon.transform);
            Instantiate(currentWeapon.HitEffect, position, Quaternion.identity);
        }

        [Rpc(SendTo.Server)]
        void EquipWeaponServerRpc(int uniqueWeaponId)
        {
            //TODO: Verify client is allowed to pick up
            //...


            Debug.Log($"Server: I'm telling everyone that {gameObject.name} is picking up id: {uniqueWeaponId}");
            //Tell other clients to equip this weapon on this character
            EquipWeaponClientRpc(uniqueWeaponId);
        }

        [Rpc(SendTo.NotOwner)]
        void EquipWeaponClientRpc(int uniqueWeaponId)
        {
            Debug.Log($"Client: the server told me that {gameObject.name} is picking up id: {uniqueWeaponId}");
            var weapon = FindObjectsByType<NetworkWeapon>(FindObjectsSortMode.None).First(x => x.networkID == uniqueWeaponId);
            if (weapon != null)
            {
                currentWeapon = weapon.GetComponent<MWeapon>();
                var pickable = weapon.GetComponent<Pickable>();
                pickUpDrop.Item = pickable;
                pickUpDrop.PickUpItem();
                pickable.enabled = false;
            }
            else
            {
                //Weapon should be found but throw error if not
                Debug.LogError($"Invalid uniqueWeaponId {uniqueWeaponId}. Not found on client");
            }
        }

        private void OnUnequipWeapon(GameObject weaponGO)
        {
            currentWeapon = null;
            var weapon = weaponGO.GetComponent<MWeapon>();
            weapon.OnHitPosition.RemoveListener((x)=> WeaponHitHandler(x));
            NetworkPlayerUIController.Instance.UpdateInventoryUI(weapon.Holster, weapon.WeaponMode);

            if (weapon is MShootable)
            {
                ((MShootable)weapon).OnFireProjectile.RemoveListener((x) => HandleProjectile(x)); //Call the event for the weapon
            }
        }
        #endregion
    }
}
