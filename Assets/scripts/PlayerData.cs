using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    private NetworkVariable<float> masterClock = new NetworkVariable<float>();
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<int> numClients = new NetworkVariable<int>();

    bool hasMoved = false;

    //network action data
    string actionType = "idle";
    bool submittedAction = false;
    public Action action;
    int numPlayers;
    public List<Action> allActions = new List<Action>();   //we will dump every action we hear into this list

    //screen pointer
    Camera camera;
    public GameObject pointer;

    void Start() {
        //grab camera and instantiate screen pointer sphere
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointer.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        Destroy(pointer.GetComponent<SphereCollider>());
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SubmitPositionRequestServerRpc(new Vector3(Random.Range(0.0f, 10.0f), 1.0f, Random.Range(0.0f, 10.0f)));
        }
    }

    void Update()
    {

        //calculate the number of players in the game
        numPlayers = GameObject.FindGameObjectsWithTag("Player").Length;

        //move pointer sphere to mouse location
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                pointer.transform.position = hit.point;
            }
        }

        if (IsServer)
        {
            //server solely dictates the clock
            masterClock.Value += Time.deltaTime;
        }

        if (IsClient) {

            //the client figures out what action
            DetectKeys();
            
            //then the client submits action data to server at an interval
            float fraction = Mathf.Repeat(masterClock.Value, 3.0f);
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

        if (hasMoved == false)
        {
            transform.position = Position.Value;
        }
    }

    [ServerRpc(RequireOwnership=false)]
    void PingServerRpc(Action action, ServerRpcParams serverRpcParams = default) {

        var clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log(action.printInfo());
        BroadcastClientRpc(clientId, action);       //after receiving the message from a client, broadcast: server -> all clients
    }

    [ClientRpc]
    void BroadcastClientRpc(ulong clientId, Action action) {

        //Debug.Log(action.printInfo());
        allActions.Add(action);     //add incoming action to the list
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

    void resolveTurn()
    {
        Debug.Log("resolving moves");

        foreach (Action action in allActions)   //resolve each action individually
        {

            Debug.Log(action.printInfo());
            
            GameObject actingPlayer = null;
            
            //find out which player sent this action message
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in allPlayers)     
            {
                if (player.transform.position == action.myPosition)
                {
                    actingPlayer = player;
                }
            }

            if (action.type == "move" && actingPlayer != null)
            {
                Debug.Log("player at " + actingPlayer.transform.position + " wants to move");
                actingPlayer.transform.position = action.targetPosition;
                hasMoved = true;
            }

        }
        
        //after resolving everything, clear the action list
        allActions.Clear();
    }
}