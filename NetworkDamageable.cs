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

        private void Start()
        {
            _damageable = GetComponent<MDamageable>();
        }

        public void SendDamageToClients(float amount)
        {
            Debug.Log($"{gameObject.name} received {amount} damage");

            //Only host should handle the events
            if (!IsServer)
            { return; }

            Debug.Log($"Sending {amount} damage to clients");
            ReceiveDamageRpc(amount);
        }

        [Rpc(SendTo.NotMe)]
        void ReceiveDamageRpc(float amount)
        {
            Debug.Log($"{gameObject.name} just took {amount} damage!");
            _damageable.ReceiveDamage(Vector3.forward, gameObject, _damageable.stats.stats[0].ID, amount, false, true, null, false);
        }
    }
}
