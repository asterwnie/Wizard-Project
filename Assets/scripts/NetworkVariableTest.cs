using Unity.Netcode;
using UnityEngine;

public class NetworkVariableTest : NetworkBehaviour
{
    private NetworkVariable<float> masterClock = new NetworkVariable<float>();
    private float last_t = 0.0f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            masterClock.Value = 0.0f;
            Debug.Log("Server's uptime var initialized to: " + masterClock.Value);
        }
    }

    void Update()
    {
        var t_now = Time.time;
        if (IsServer)
        {
            masterClock.Value = Time.time;
            Debug.Log("I decree the time is: " + masterClock.Value);
        }
        if (IsClient) {
            Debug.Log("Server says that the value is: " + masterClock.Value);
        }
    }
}