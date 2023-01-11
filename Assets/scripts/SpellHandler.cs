using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpellHandler : NetworkBehaviour
{
    public List<Action> actionsQueue = new List<Action>();
    public bool isResolving = false;

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
            StartCoroutine(ResolveActions());
        }
    }

    IEnumerator ResolveActions()
    {
        isResolving = true;
        Debug.Log("Server: Resolving " + actionsQueue.Count + " move(s).");
        foreach (Action action in actionsQueue)   //resolve each action individually
        {

            //Debug.Log(action.printInfo());

            GameObject actingPlayer = NetworkManager.Singleton.ConnectedClients[action.ownerId].PlayerObject.gameObject;

            if (action.type == "spell")
            {
                // spawns a fireball that *should* be networked in theory
                Debug.Log("Server: Broadcasting spell to clients.");
                ExecuteSpell(actingPlayer.transform.position, action); // spawn on server
                BroadcastSpellClientRpc(actingPlayer.transform.position, action); // broadcast to clients
            }

            if (action.type == "move" && actingPlayer != null)
            {
                Debug.Log("player at " + actingPlayer.transform.position + " wants to move");
                actingPlayer.transform.position = action.targetPosition;
                //hasMoved = true;
            }

            // wait a bit before executing next move
            float deltaTime = 0;
            float waitTime = 0.5f;
            while(deltaTime < waitTime)
            {
                deltaTime += Time.deltaTime;
                yield return null;
            }
        }

        // reset each player's mana
        foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
            NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<PlayerData>().ResetMana();

        //after resolving everything, clear the action list
        actionsQueue.Clear();
        isResolving = false;
        GameManager.Instance.ResetRoundTime();
    }

    [ClientRpc]
    void BroadcastSpellClientRpc(Vector3 origin, Action action)
    {
        if (IsClient && !IsServer)
        {
            Debug.Log("Client: Received spell broadcast. Executing spell.");
            ExecuteSpell(origin, action); // executes it for the client
        }  
    }

    public void ExecuteSpell(Vector3 origin, Action action)
    {
        Spell spell;
        switch (action.spellType)
        {
            case Spell.SpellType.FIREBALL:
                spell = new SpellFireball();
                StartCoroutine(spell.ExecuteSpell(origin, action.targetPosition));
                return;
            case Spell.SpellType.SPELL_BURST:
                spell = new SpellBurst();
                StartCoroutine(spell.ExecuteSpell(origin, action.targetPosition));
                return;
            case Spell.SpellType.ICE_SHARD:
                spell = new SpellIceShard();
                StartCoroutine(spell.ExecuteSpell(origin, action.targetPosition));
                return;
            case Spell.SpellType.ORB_SHIELD:
                spell = new SpellOrbShield();
                StartCoroutine(spell.ExecuteSpell(origin, action.targetPosition));
                return;
        }
    }
}
