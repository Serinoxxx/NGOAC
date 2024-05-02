using MalbersAnimations;
using MalbersAnimations.Controller;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkAnimal : NetworkBehaviour
{
    MAnimal _animal;


    private void Start()
    {
        _animal = GetComponent<MAnimal>();
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

