using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Action : INetworkSerializable  //to have a custom class sent over RPC or NetworkVariable, it must implement INetworkSerializable
{
    public string type;
    public ulong ownerId;
    public Vector3 targetPosition;

    public Action(string action, ulong id, Vector3 targetPos)
    {
        type = action;
        ownerId = id;
        targetPosition = targetPos;
    }

    public Action() { }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter     //must be implemented by class for internet transmission
    {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref ownerId);
        serializer.SerializeValue(ref targetPosition);
    }

    public string printInfo()
    {
        return "type: " + type + " target: " + targetPosition + " owner ID: " + ownerId;
    }
}