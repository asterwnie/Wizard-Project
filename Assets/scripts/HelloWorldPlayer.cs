using Unity.Netcode;
using UnityEngine;

public class HelloWorldPlayer : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public int intendedMove = 0;

    static int maxHealth = 100;
    int currentHealth;
    WorldTile currentTile;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            GameManager.Instance.localPlayer = this;
            currentHealth = maxHealth;
           // MoveRandomTile();
        }
    }
/*
    public void MoveRandomTile()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Vector3 randPos = GetRandomTilePosition();
            transform.position = randPos;
            Position.Value = randPos;
        }
        else
        {
            SubmitPositionRequestServerRpc(GetRandomTilePosition());
        }
    }*/

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

    public void MoveToTile(WorldTile tile)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            transform.position = tile.footLoc.transform.position;
            Position.Value = tile.footLoc.transform.position;
        }
        else
        {
            SubmitPositionRequestServerRpc(tile.footLoc.transform.position);
        }
        currentTile = tile;
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        Position.Value = pos;
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(0, 10), 0, Random.Range(0, 10));
    }

    void Update()
    {
        /*// move player with WASD
        if(IsOwner && GameManager.Instance.grid != null)
        {
            Vector2 movementInput = GetMovementInputs();
            //check if any real movement happened
            if (movementInput.magnitude > 0)
            {
                // check if there is a valid tile to move on
                Vector2 nextTilePos = currentTile.gridPosition + movementInput;
                WorldTile nextTile = GameManager.Instance.grid.gridTiles[nextTilePos] as WorldTile;
                if(nextTile != null)
                {
                    // submit a movement request
                    MoveToTile(nextTile);
                }
            }
        }*/
       

        //add a hovering effect
        Vector3 hover = new Vector3(0f, 0.5f * Mathf.Sin(Time.time * 1.5f) + 1.0f, 0f);

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
