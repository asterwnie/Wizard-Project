using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
    static int count = 0;
    public int id;

    public GameObject footLoc;

    // Start is called before the first frame update
    void Start()
    {
        // assign an ID to this tile
        this.id = count;
        count++;


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
