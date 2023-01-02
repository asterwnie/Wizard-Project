using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
    [Header("Tile Instance")]
    static int count = 0;
    public int id;
    public bool selected = false;
    public Vector2 gridPosition;
    public MeshFilter model;

    [Header("Static References")]
    public List<MeshFilter> tileVariants = new List<MeshFilter>();

    public GameObject footLoc;

    // Start is called before the first frame update
    void Start()
    {
        // assign an ID to this tile
        this.id = count;
        count++;

        RandomizeMesh();
    }

    void RandomizeMesh()
    {
        // choose a random mesh from list of variants
        int index = Random.Range(0, tileVariants.Count);
        model.mesh = tileVariants[index].sharedMesh;
    }
}
