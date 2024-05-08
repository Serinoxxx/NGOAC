using MalbersAnimations;
using MalbersAnimations.Controller;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkAnimal : NetworkBehaviour
{
    MAnimal _animal;
    Stats _stats;

    public NetworkVariable<float> _healthValue = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> _staminaValue = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    [SerializeField] StatID healthStatID;
    [SerializeField] StatID staminaStatID;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("Spawning...");
        Invoke(nameof(GetComponentsDelayed),0.1f);
    }

    // PlayerConectedEvents will add/remove some components, so we need to retrieve the components after a short delay
    void GetComponentsDelayed()
    {
        _animal = GetComponent<MAnimal>();
        _stats = GetComponent<Stats>();
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
}

