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
        [SerializeField] NetworkObject _networkObject;

        public override void Mode_Activate(ModeID ModeID)
        {
            base.Mode_Activate(ModeID.ID);
        }

        [Rpc(SendTo.Owner)]
        private void Mode_ActivateRpc(int modeID)
        {
            base.Mode_Activate(modeID);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ServerAuthAnimal))]
    public class ServerAuthAnimalEditor : MAnimalEditor
    {
    }
#endif
}
