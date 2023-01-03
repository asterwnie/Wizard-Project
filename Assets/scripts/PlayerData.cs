using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<int> numClients = new NetworkVariable<int>();

    bool hasMoved = false;

    //network action data
    string actionType = "idle";
    bool submittedAction = false;
    public Action action;
    int numPlayers;
    //public List<Action> allActions = new List<Action>();   //we will dump every action we hear into this list

    //screen pointer
    Camera camera;
    public GameObject pointer;
    public LayerMask playLayer;

    //player
    public GameObject playerPrefab;
    public GameObject player;

    static int maxHealth = 100;
    int currentHealth;

    void Start() {
        //grab camera and instantiate screen pointer sphere
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        pointer = GameManager.Instance.pointerSelected;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // move player to random location
            SubmitPositionRequestServerRpc(new Vector3(Random.Range(0f, 9f), 1f, Random.Range(0f, 9f)));
            GameManager.Instance.localPlayer = this;
            currentHealth = maxHealth;
        }
    }

    void Update()
    {

        //calculate the number of players in the game
        numPlayers = GameObject.FindGameObjectsWithTag("Player").Length;

        MovePointer();
        
        if (IsClient) {

            //the client figures out what action
            DetectKeys();
            
            //then the client submits action data to server at an interval
            float fraction = Mathf.Repeat(GameManager.Instance.GetServerClock().Value, 3.0f); // last float is the round length
            if (fraction > 2.94f && submittedAction == false) {
                PingServerRpc(actionType, pointer.transform.position);     //send action data from the client -> server
                submittedAction = true;
                actionType = "idle";
            } else
            if (fraction < 1.8f && submittedAction == true) {
                submittedAction = false;
            }
        }

        //hover
        float breathe = Mathf.Sin(2*Time.time) * 0.2f;
        //transform.position = Position.Value + new Vector3(0.0f, breathe, 0.0f);
        if (hasMoved == false)
        {
            transform.position = Position.Value;
        }
    }

    [ServerRpc(RequireOwnership=false)]
    void PingServerRpc(string actionType, Vector3 target, ServerRpcParams serverRpcParams = default) {

        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Action action = new Action(actionType, clientId, target);

        Debug.Log(action.printInfo());
        SpellHandler.Instance.actionsQueue.Add(action);
        //BroadcastClientRpc(clientId, action);       //after receiving the message from a client, broadcast: server -> all clients
    }
/*
    [ClientRpc]
    void BroadcastClientRpc(ulong clientId, Action action) 
    {
        //SpellHandler.Instance.actionsQueue.Add(action); //add incoming action to the list
    }*/

    [ServerRpc]
     void SubmitPositionRequestServerRpc(Vector3 pos, ServerRpcParams rpcParams = default)
     {
        Position.Value = pos;
     }

    void DetectKeys() {
        if (Input.GetKeyDown("a")) {
            actionType = "move";
        } else
        if (Input.GetKeyDown("s")) {
            actionType = "fireball";
        }
    }

    void MovePointer()
    {
        //move pointer sphere to mouse location
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, playLayer))
            {
                pointer.SetActive(true);
                pointer.transform.position = hit.point + new Vector3(0f, 0.25f, 0f);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            pointer.SetActive(false);
        }
    }
    
    
}