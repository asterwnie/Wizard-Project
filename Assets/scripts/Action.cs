using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Action : INetworkSerializable  //to have a custom class sent over RPC or NetworkVariable, it must implement INetworkSerializable
{
    public string type;
    public Vector3 myPosition;
    public Vector3 targetPosition;

    public Action(string action, Vector3 pos1, Vector3 pos2)
    {
        type = action;
        myPosition = pos1;
        targetPosition = pos2;
    }

    public Action() { }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter     //must be implemented by class for internet transmission
    {
        serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref myPosition);
        serializer.SerializeValue(ref targetPosition);
    }

    public string printInfo()
    {
        return "type: " + type + " target: " + targetPosition + " source: " + myPosition;
    }
}