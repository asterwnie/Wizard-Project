using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    private NetworkVariable<float> masterClock = new NetworkVariable<float>();
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<int> numClients = new NetworkVariable<int>();


    //network action data
    string actionType = "idle";
    Vector3 target;          // coordinates of the screen pointer
    bool submittedAction = false;
    public Action action;
    GameObject[] allPlayers;
    int numPlayers;

    //screen pointer
    Camera camera;
    public GameObject pointer;

    //player
    public GameObject playerPrefab;
    public GameObject player;

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
        allPlayers = GameObject.FindGameObjectsWithTag("Player");
        numPlayers = allPlayers.Length;
        //Debug.Log(numPlayers);

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
        }

        //hover
        float breathe = 0.0007f*Mathf.Sin(2*Time.time);
        //transform.position = new Vector3(transform.position.x, transform.position.y + breathe, transform.position.z);

        transform.position = Position.Value + new Vector3(0.0f, breathe, 0.0f);
    }

    [ServerRpc(RequireOwnership=false)]
    void PingServerRpc(Action action, ServerRpcParams serverRpcParams = default) {

        var clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log("Client ID: " + clientId + ", actionType: " + action.type + ", target: " + action.targetPosition + ", myPosition: " + action.myPosition);
        BroadcastClientRpc(clientId, action);       //after receiving the message from a client, broadcast: server -> all clients
    }

    [ClientRpc]
    void BroadcastClientRpc(ulong clientId, Action action) { 

        Debug.Log("Client ID: " + clientId + ", actionType: " + action.type + ", target: " + action.targetPosition + ", myPosition: " + action.myPosition);


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
}