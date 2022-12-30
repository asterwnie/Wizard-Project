using Unity.Netcode;
using UnityEngine;

public class HelloWorldPlayer : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            MoveRandom();
        }
    }

    public void MoveRandom()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            transform.position = GetRandomPositionOnPlane();
            Position.Value = GetRandomPositionOnPlane();
        }
        else
        {
            SubmitPositionRequestServerRpc(GetRandomPositionOnPlane());
        }
    }

    public void MoveRelative(Vector3 relativeMove)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            transform.position += relativeMove;
            Position.Value += relativeMove;
        }
        else
        {
            SubmitPositionRequestServerRpc(Position.Value + relativeMove);
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        Position.Value = pos;
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(0, 10), Random.Range(0, 10), 0);
    }

    void Update()
    {

        // move player with WASD
        if(IsOwner)
        {
            Vector2 movementInput = GetMovementInputs();
            //check if any real movement happened
            if (movementInput.magnitude > 0)
            {
                // submit a movement request
                MoveRelative(new Vector3(movementInput.x, movementInput.y, 0f));
            }
        }
       

        //add a hovering effect
        Vector3 hover = new Vector3(0f, 0f, 0.5f * Mathf.Sin(Time.time * 1.5f) - 1.0f);

        // sync position with networked position
        //transform.position = Position.Value;
        transform.position = Position.Value + hover;
    }

    static Vector2 GetMovementInputs()
    {
        Vector2 move = new Vector2();
        //handle keystrokes
        if (Input.GetKeyDown("w"))
        {
            move.y += 1;
        }
        else
        if (Input.GetKeyDown("a"))
        {
            move.x -= 1;
        }
        else
        if (Input.GetKeyDown("s"))
        {
            move.y -= 1;
        }
        else
        if (Input.GetKeyDown("d"))
        {
            move.x += 1;
        }

        return move;
    }

}
