using Unity.Netcode;
using UnityEngine;

public class NetworkVariableTest : NetworkBehaviour
{
    private NetworkVariable<float> masterClock = new NetworkVariable<float>();

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

            if (Input.GetKeyDown(KeyCode.P)) {
                PingServerRpc(Time.frameCount); // Client -> Server
            }
        }
    }

    [ServerRpc]
    void PingServerRpc(int somenumber, ServerRpcParams serverRpcParams = default) {

        var clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log("Client ID: " + clientId + ", frameCount: " + somenumber);
    }
    
}