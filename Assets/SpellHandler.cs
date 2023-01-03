using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpellHandler : NetworkBehaviour
{
    public List<Action> actionsQueue = new List<Action>();

    public static SpellHandler Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    public void ResolveTurn()
    {
        if (IsServer)
        {
            Debug.Log("resolving moves");

            foreach (Action action in actionsQueue)   //resolve each action individually
            {

                Debug.Log(action.printInfo());

                GameObject actingPlayer = NetworkManager.Singleton.ConnectedClients[action.ownerId].PlayerObject.gameObject;
                //PlayerData playerData = actingPlayer.GetComponent<PlayerData>();

                if (action.type == "fireball")
                {
                    // spawns a fireball that *should* be networked in theory
                    BroadcastSpellClientRpc(actingPlayer.transform.position, action);
                }

                if (action.type == "move" && actingPlayer != null)
                {
                    Debug.Log("player at " + actingPlayer.transform.position + " wants to move");
                    actingPlayer.transform.position = action.targetPosition;
                    //hasMoved = true;
                }

            }

            //after resolving everything, clear the action list
            actionsQueue.Clear();
        }

    }

    [ClientRpc]
    void BroadcastSpellClientRpc(Vector3 origin, Action action)
    {
        Debug.Log("cast spell");
        Spell fireball = new SpellFireball(); // placeholder
        StartCoroutine(fireball.ExecuteSpell(origin, action.targetPosition));
    }
}
