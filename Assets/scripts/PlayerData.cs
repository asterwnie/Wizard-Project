using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    private NetworkVariable<float> masterClock = new NetworkVariable<float>();
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<int> numClients = new NetworkVariable<int>();


    //network action data
    int actionType = 0;      //action type, 0 = idle, 1 = move, 2 = fireball, 3 = magic burst
    Vector3 target;          // coordinates of the screen pointer
    bool submittedAction = false;
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
            DetectAction();
            
            //then the client submits action data to server at an interval
            float fraction = Mathf.Repeat(masterClock.Value, 3.0f);
            if (fraction > 2.94f && submittedAction == false) {
                PingServerRpc(actionType, target, transform.position);     //send all action data from the client -> server
                submittedAction = true;
                actionType = 0;
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
    void PingServerRpc(int actionType, Vector3 target, Vector3 myPosition, ServerRpcParams serverRpcParams = default) {

        var clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log("Client ID: " + clientId + ", actionType: " + actionType + ", target: " + target + ", myPosition: " + myPosition);
        BroadcastClientRpc(clientId, actionType, target, myPosition);       //after receiving the message from a client, broadcast: server -> all clients
    }

    [ClientRpc]
    void BroadcastClientRpc(ulong clientId, int actionType, Vector3 target, Vector3 myPosition) { 

        Debug.Log("Client ID: " + clientId + ", actionType: " + actionType + ", target: " + target + ", myPosition: " + myPosition);


    }

    [ServerRpc]
     void SubmitPositionRequestServerRpc(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        Position.Value = pos;
        Debug.Log(pos);
    }

    void DetectAction() {
        if (Input.GetKeyDown("a")) {
            actionType = 1;
        } else
        if (Input.GetKeyDown("s")) {
            actionType = 2;
        }
        target = pointer.transform.position;
    }
}