using Unity.Netcode;
using UnityEngine;

public class NetworkVariableTest : NetworkBehaviour
{
    private NetworkVariable<float> masterClock = new NetworkVariable<float>();

    //action data
    public int actionType = 0;      //action type, 0 = idle, 1 = move, 2 = fireball, 3 = magic burst
    public Vector3 target;
    bool submittedAction = false;

    public override void OnNetworkSpawn()
    {
        
    }

    void Update()
    {
        if (IsServer)
        {
            //server fully dictates the clock
            masterClock.Value += Time.deltaTime;

        }

        if (IsClient) {
            
            //client submits action data to server each second
            float mantissa = Mathf.Repeat(masterClock.Value, 1.0f);
            if (mantissa > 0.94f && submittedAction == false) {
                Debug.Log("sent the server a message at: " + Time.time);
                PingServerRpc(Time.frameCount, actionType, target);
                submittedAction = true;
            } else
            if (mantissa < 0.8f && submittedAction == true) {
                submittedAction = false;
            }
        }
    }

    [ServerRpc]
    void PingServerRpc(int somenumber, int actionType, Vector3 target, ServerRpcParams serverRpcParams = default) {

        var clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log("Client ID: " + clientId + ", frameCount: " + somenumber + ", actionType: " + actionType + ", target: " + target);
    }
    
}