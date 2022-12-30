using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setupPlayer : MonoBehaviour
{
    public GameObject playerPrefab;
    GameObject player;
    float xPos = 0f;
    float yPos = 1f;
    float zPos = 0f;

    void Awake()
    {
        //create player
        player = GameObject.Instantiate(playerPrefab);
        player.transform.position = new Vector3(xPos, yPos, zPos);

    }

}
