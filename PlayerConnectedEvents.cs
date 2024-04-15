using MalbersAnimations.Controller;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using MalbersAnimations.Weapons;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MalbersAnimations.NetCode
{
    public class PlayerConnectedEvents : NetworkBehaviour
    {
        [SerializeField] UnityEventRaiser cameraEventRaiser;
        [SerializeField] TransformHook cameraTransformHook;
        private MalbersInput malbersInput;
        private MAnimal mAnimal;
        private Stats stats;
        private MEventListener eventListener;
        private MDamageable damageable;

        private MWeaponManager weaponManager;
        private MInventory inventory;
        [SerializeField] MPickUp pickUpDrop;
        private MInteractor interactor;
        [SerializeField] GameObject staminaSprintData;

        private Aim aim;
        [SerializeField] Transform aimTarget;

        public override void OnNetworkSpawn()
        {
            PlayerRegister.Instance.AddPlayer(this.NetworkObject);

            aim = GetComponentInChildren<Aim>();

            mAnimal = GetComponentInChildren<MAnimal>();
            interactor = GetComponentInChildren<MInteractor>();
            malbersInput = GetComponentInChildren<MalbersInput>();
            stats = GetComponentInChildren<Stats>();
            eventListener = GetComponentInChildren<MEventListener>();
            weaponManager = GetComponentInChildren<MWeaponManager>();
            inventory = GetComponentInChildren<MInventory>();
            damageable = GetComponentInChildren<MDamageable>();

            if (IsOwner)
            {
                // Enable Player Specific Components
                EnableComponentIfNotNull(cameraEventRaiser);
                EnableComponentIfNotNull(cameraTransformHook);
                EnableComponentIfNotNull(malbersInput);
                EnableComponentIfNotNull(mAnimal);
                EnableComponentIfNotNull(stats);
                EnableComponentIfNotNull(eventListener);
                EnableComponentIfNotNull(weaponManager);
                EnableComponentIfNotNull(inventory);
                EnableComponentIfNotNull(damageable);

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
        void OnEquippedWeapon(GameObject weaponGO)
        {
            // Find the unique Id for the weapon to send to server and request pickup
            var weapon = weaponGO.GetComponent<MWeapon>();
            var uniqueWeaponId = weapon.WeaponID;
            EquipWeaponServerRpc(uniqueWeaponId);
            Debug.Log($"Owner: I'm picking up weapon id: {uniqueWeaponId}");

            // Tell the UI to update, must be done here so other clients don't updat their UI
            NetworkPlayerUIController.Instance.UpdateInventoryUI(weapon.Holster, weapon.WeaponMode);

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
            var weapon = FindObjectsOfType<MWeapon>().First(x => x.WeaponID == uniqueWeaponId);
            if (weapon != null)
            {
                var pickable = weapon.GetComponent<Pickable>();
                pickUpDrop.Item = pickable;
                pickUpDrop.PickUpItem();
            }
            else
            {
                //Weapon should be found but throw error if not
                Debug.LogError($"Invalid uniqueWeaponId {uniqueWeaponId}. Not found on client");
            }
        }

        private void OnUnequipWeapon(GameObject weaponGO)
        {
            var weapon = weaponGO.GetComponent<MWeapon>();
            NetworkPlayerUIController.Instance.UpdateInventoryUI(weapon.Holster, weapon.WeaponMode);
        }
        #endregion
    }
}
