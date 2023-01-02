using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    private NetworkVariable<float> masterClock = new NetworkVariable<float>();

    //network action data
    public int actionType = 0;      //action type, 0 = idle, 1 = move, 2 = fireball, 3 = magic burst
    public Vector3 target;
    bool submittedAction = false;

    //screen pointer
    public Camera camera;
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

    }

    void Update()
    {
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
            detectAction();
            
            //then the client submits action data to server on each second
            float fraction = Mathf.Repeat(masterClock.Value, 1.0f);
            if (fraction > 0.94f && submittedAction == false) {
                Debug.Log("sent the server a message at: " + Time.time);
                PingServerRpc(Time.frameCount, actionType, target);     //send all action data client -> server
                submittedAction = true;
                actionType = 0;
            } else
            if (fraction < 0.8f && submittedAction == true) {
                submittedAction = false;
            }
        }

        //hover
        float breathe = 0.0007f*Mathf.Sin(2*Time.time);
        transform.position = new Vector3(transform.position.x, transform.position.y + breathe, transform.position.z);
    }

    [ServerRpc]
    void PingServerRpc(int somenumber, int actionType, Vector3 target, ServerRpcParams serverRpcParams = default) {

        var clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log("Client ID: " + clientId + ", frameCount: " + somenumber + ", actionType: " + actionType + ", target: " + target);
    }
    

    void detectAction() {
        if (Input.GetKeyDown("a")) {
            actionType = 1;
        } else
        if (Input.GetKeyDown("s")) {
            actionType = 2;
        }
        target = pointer.transform.position;
    }
}