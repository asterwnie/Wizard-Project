using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpellHandler : NetworkBehaviour
{
    public List<Action> actionsQueue;

    public static SpellHandler Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    public void ExecuteQueuedActions()
    {

    }
}
