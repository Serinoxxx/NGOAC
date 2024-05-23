using MalbersAnimations;
using MalbersAnimations.Controller;
using MalbersAnimations.NetCode;
using MalbersAnimations.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class NetworkAnimal : NetworkBehaviour
{
    ServerAuthAnimal _animal;
    Stats _stats;

    public NetworkVariable<float> _healthValue = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> _staminaValue = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    [SerializeField] StatID healthStatID;
    [SerializeField] StatID staminaStatID;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("Spawning...");
        Invoke(nameof(GetComponentsDelayed), 0.1f);
        
    }

    // PlayerConectedEvents will add/remove some components, so we need to retrieve the components after a short delay
    void GetComponentsDelayed()
    {
        _animal = GetComponent<ServerAuthAnimal>();
        _stats = GetComponent<Stats>();
        _animal.OnStateChange.AddListener(OnStateChangeHandler);
    }

    private void OnStateChangeHandler(int stateID)
    {
        Debug.Log($"Informing others of new state: {stateID}");
        StateChangedRpc(stateID);
    }

        [Rpc(SendTo.NotOwner)]
    private void StateChangedRpc(int stateID)
    {
        Debug.Log($"Received new state: {stateID}");
        _animal.Set_State(stateID);
    }

    private void FixedUpdate()
    {
        //Return if stats is null
        if (_stats == null) return;

        //If we're the owner, set the networkVar value to the stat value
        if (IsOwner)
        {
            _healthValue.Value = _stats.Stat_GetValue(healthStatID);
            _staminaValue.Value = _stats.Stat_GetValue(staminaStatID);
        }
        //If we're not the owner, set the stat value to the networkVar value
        else
        {
            _stats.Stat_SetValue(healthStatID, _healthValue.Value);
            _stats.Stat_SetValue(staminaStatID, _staminaValue.Value);
        }
    }

    public void ActivateMode(int ModeID)
    {
        if (IsServer && !IsOwner)
        {
            Debug.Log($"SERVER: Sending activate mode {ModeID} to owner");
            ActivateModeRpc(ModeID);
        }
        else
        {
            Debug.Log($"activating mode {ModeID}");
            _animal.Mode_TryActivate(ModeID);
        }
    }

    [Rpc(SendTo.Owner)]
    private void ActivateModeRpc(int ModeID)
    {
        Debug.Log($"RPC CLIENT: activating mode {ModeID}");
        _animal.Mode_TryActivate(ModeID);
    }

    public void SpawnProjectileOnOtherClients(GameObject projectileGO)
    {
        Debug.Log("SpawnProjectileOnOtherClients method called.");
        // Send the RPC to all clients except the one invoking the RPC to spawn the projectile with the same trajectory
        var projectile = projectileGO.GetComponent<MProjectile>();
        SpawnProjectileOnOtherClientsRpc(projectileGO.transform.position, projectileGO.transform.rotation, projectile.Velocity, projectile.Gravity);
    }

    Vector3 projectilePosition;
    Quaternion projectileRotation;
    Vector3 projectileVelocity;
    Vector3 projectileGravity;
    [Rpc(SendTo.NotMe)]
    private void SpawnProjectileOnOtherClientsRpc(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 gravity)
    {
        projectilePosition = position;
        projectileRotation = rotation;
        projectileVelocity = velocity;
        projectileGravity = gravity;
        Debug.Log("SpawnProjectileOnOtherClientsRpc method called.");
        var mShootable = GetComponentInChildren<MShootable>();
        mShootable.EquipProjectile();
        mShootable.OnFireProjectile.AddListener(PrepareProjectile);
        mShootable.FireProjectile();
        mShootable.OnFireProjectile.RemoveListener(PrepareProjectile);
    }

    private void PrepareProjectile(GameObject projectileGO)
    {
        var projectile = projectileGO.GetComponent<MProjectile>();
        Debug.Log("PositionAndRotateProjectile method called.");
        projectile.Prepare(gameObject, projectileGravity, projectileVelocity, projectile.m_hitLayer, projectile.TriggerInteraction);
        projectileGO.transform.SetPositionAndRotation(projectilePosition, projectileRotation);
    }
}

