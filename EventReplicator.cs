using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class EventReplicator : NetworkBehaviour
{

    //Events
    public UnityEvent OnReplicate;
    public UnityEvent<int> OnReplicateInt;
    public UnityEvent<string> OnReplicateString;
    public UnityEvent<float> OnReplicateFloat;
    public UnityEvent<Vector3> OnReplicateVector3;
    public UnityEvent<ReplicationData> OnReplicateData;

    //Methods
    public void Replicate() { ReplicateServerRpc(); }
    public void Replicate(int arg0) { ReplicateIntServerRpc(arg0); }
    public void Replicate(string arg0) { ReplicateStringServerRpc(arg0); }
    public void Replicate(float arg0) { ReplicateFloatServerRpc(arg0); }
    public void Replicate(Vector3 arg0) { ReplicateVector3ServerRpc(arg0); }
    public void Replicate(ReplicationData arg0) { ReplicateDataServerRpc(arg0); }

    //ServerRpcs
    [Rpc(SendTo.Server)] public void ReplicateServerRpc() { ReplicateClientRpc(); }
    [Rpc(SendTo.Server)] public void ReplicateIntServerRpc(int arg0) { ReplicateIntClientRpc(arg0); }
    [Rpc(SendTo.Server)] public void ReplicateStringServerRpc(string arg0) { ReplicateStringClientRpc(arg0); }
    [Rpc(SendTo.Server)] public void ReplicateFloatServerRpc(float arg0) { ReplicateFloatClientRpc(arg0); }
    [Rpc(SendTo.Server)] public void ReplicateVector3ServerRpc(Vector3 arg0) { ReplicateVector3ClientRpc(arg0); }
    [Rpc(SendTo.Server)] public void ReplicateDataServerRpc(ReplicationData arg0) { ReplicateDataClientRpc(arg0); }

    //CLientRpcs
    [Rpc(SendTo.Everyone)] public void ReplicateClientRpc() { OnReplicate?.Invoke(); }

    [Rpc(SendTo.Everyone)] public void ReplicateIntClientRpc(int arg0) { OnReplicateInt?.Invoke(arg0); }

    [Rpc(SendTo.Everyone)] public void ReplicateStringClientRpc(string arg0) { OnReplicateString?.Invoke(arg0); }

    [Rpc(SendTo.Everyone)] public void ReplicateFloatClientRpc(float arg0) { OnReplicateFloat?.Invoke(arg0); }

    [Rpc(SendTo.Everyone)] public void ReplicateVector3ClientRpc(Vector3 arg0) { OnReplicateVector3?.Invoke(arg0); }

    [Rpc(SendTo.Everyone)] public void ReplicateDataClientRpc(ReplicationData arg0) { OnReplicateData?.Invoke(arg0); }
}

public struct ReplicationData : INetworkSerializable
{
    public Vector3 Position;
    public Quaternion Rotation;

    public int arg0;
    public string arg1;
    public float arg2;

    // INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Rotation);
        serializer.SerializeValue(ref arg0);
        serializer.SerializeValue(ref arg1);
        serializer.SerializeValue(ref arg2);
    }
    // ~INetworkSerializable
}