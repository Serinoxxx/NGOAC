using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MalbersAnimations.NetCode
{

    [RequireComponent(typeof(MDamageable))]
    public class NetworkDamageable : NetworkBehaviour
    {
        MDamageable _damageable;
        ServerAuthAnimal _serverAuthAnimal;

        private void Start()
        {
            _damageable = GetComponent<MDamageable>();
            _serverAuthAnimal = GetComponent<ServerAuthAnimal>();
        }

        public void SendDamageToClients(float amount)
        {
            
            Debug.Log($"{gameObject.name} received {amount} damage");

            //Only host should handle the events
            if (!IsServer)
            { return; }

            Debug.Log($"Sending {amount} damage to clients");
            ReceiveDamageRpc(amount, _damageable.LastDamage.WasCritical, _damageable.HitDirection);
        }

        [Rpc(SendTo.NotMe)]
        void ReceiveDamageRpc(float amount, bool wasCritical, Vector3 direction)
        {
            Debug.Log($"{gameObject.name} just took {amount} damage!");
            _damageable.ReceiveDamage(direction, gameObject, _damageable.stats.stats[0].ID, amount, wasCritical, true, null, false);
        }

        // public void SendCriticalDamageToClients()
        // {
        //     Debug.Log($"{gameObject.name} received critical damage");

        //     //Only host should handle the events
        //     if (!IsServer)
        //     { return; }

        //     Debug.Log($"Sending critical damage to clients");
        //     ReceiveCriticalDamageRpc();
        // }

        // [Rpc(SendTo.NotMe)]
        // void ReceiveCriticalDamageRpc()
        // {
        //     Debug.Log($"{gameObject.name} just took critical damage!");
        //     _damageable.criticalReaction.TryReact(_serverAuthAnimal);
        // }
    }
}
