using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    
    //network action data
    string actionType = "idle";
    bool submittedAction = false;
    public Action action;
    int numPlayers;
    public List<Action> allActions = new List<Action>();   //we will dump every action we hear into this list

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
                PingServerRpc(action = new Action(actionType, transform.position, pointer.transform.position));     //send action data from the client -> server
                submittedAction = true;
                actionType = "idle";
            } else
            if (fraction < 1.8f && submittedAction == true) {
                submittedAction = false;
            }

            //if the client has heard what everyone wants to do, execute the turn
            if (allActions.Count == numPlayers)
            {
                resolveTurn();
            }
        }

        //hover
        float breathe = Mathf.Sin(2*Time.time) * 0.2f;
        transform.position = Position.Value + new Vector3(0.0f, breathe, 0.0f);
    }

    [ServerRpc(RequireOwnership=false)]
    void PingServerRpc(Action action, ServerRpcParams serverRpcParams = default) {

        var clientId = serverRpcParams.Receive.SenderClientId;
        //Debug.Log(action.printInfo());
        BroadcastClientRpc(clientId, action);       //after receiving the message from a client, broadcast: server -> all clients
    }

    [ClientRpc]
    void BroadcastClientRpc(ulong clientId, Action action) {

        //Debug.Log(action.printInfo());
        allActions.Add(action);
    }

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
    
    void resolveTurn()
    {
        Debug.Log("I have heard everyone's move, listed above is what people want to do ^^^^");
        foreach (Action action in allActions)
        {
            Debug.Log(action.printInfo());
        }
        
        //after resolving everything, clear the action list
        allActions.Clear();
    }
}