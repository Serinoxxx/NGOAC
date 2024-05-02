using MalbersAnimations.Controller;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.NetCode
{
    public class ServerAuthAnimal : MAnimal
    {
        NetworkAnimal _networkAnimal;

        //These simply don't work
        //public override void Mode_Activate(ModeID ModeID)
        //{
        //    Debug.Log("modeActivate0");
        //    base.Mode_Activate(ModeID);
        //}

        //public new bool Mode_TryActivate(int ModeID, int AbilityIndex)
        //{
        //    Debug.Log("modeTryactivate");
        //    return base.Mode_TryActivate(ModeID, AbilityIndex);
        //}

        private NetworkAnimal GetNetworkAnimal()
        {
            if (_networkAnimal != null)
            {
                _networkAnimal = GetComponent<NetworkAnimal>();
            }
            return _networkAnimal;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ServerAuthAnimal))]
    public class ServerAuthAnimalEditor : MAnimalEditor
    {

    }
#endif
}
