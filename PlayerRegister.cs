using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MalbersAnimations.NetCode
{
    public class PlayerAddedEventArgs : EventArgs
    {
        public NetworkObject playerNetworkObject;
    }

    public class PlayerRegister : MonoBehaviour
    {
        public static PlayerRegister Instance { get; private set; }

        public event EventHandler<PlayerAddedEventArgs> OnPlayerAdded;

        [SerializeField]
        private List<NetworkObject> playerNetworkObjects = new List<NetworkObject>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            playerNetworkObjects.RemoveAll(x => x.OwnerClientId == clientId);
        }

        public void AddPlayer(NetworkObject playerNetworkObject)
        {
            playerNetworkObjects.Add(playerNetworkObject);
            OnPlayerAdded?.Invoke(this, new PlayerAddedEventArgs() { playerNetworkObject = playerNetworkObject });
        }

        public void RemovePlayer(NetworkObject playerNetworkObject)
        {
            playerNetworkObjects.Remove(playerNetworkObject);
        }

        public NetworkObject GetPlayerByClientId(ulong clientId)
        {
            return playerNetworkObjects.Find(x => x.OwnerClientId == clientId);
        }

        public List<NetworkObject> GetPlayerNetworkObjects()
        {
            playerNetworkObjects.RemoveAll(x => x == null);

            return playerNetworkObjects;
        }

        public NetworkObject GetOwnedNetworkObject()
        {
            return playerNetworkObjects.Find(x => x.IsOwner);
        }


    }
}