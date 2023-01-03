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
            Debug.Log("Server: Resolving moves.");

            foreach (Action action in actionsQueue)   //resolve each action individually
            {

                Debug.Log(action.printInfo());

                GameObject actingPlayer = NetworkManager.Singleton.ConnectedClients[action.ownerId].PlayerObject.gameObject;
                //PlayerData playerData = actingPlayer.GetComponent<PlayerData>();

                if (action.type == "fireball")
                {
                    // spawns a fireball that *should* be networked in theory
                    Debug.Log("Server: Broadcasting spell to clients.");
                    GameManager.Instance.ExecuteSpell(actingPlayer.transform.position, action); // spawn on server
                    BroadcastSpellClientRpc(actingPlayer.transform.position, action); // broadcast to clients
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
        if (IsClient)
        {
            Debug.Log("Client: Received spell broadcast. Executing spell.");
            GameManager.Instance.ExecuteSpell(origin, action); // executes it for the client
        }  
    }
}
