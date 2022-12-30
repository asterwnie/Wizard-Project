using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setupGrid : MonoBehaviour
{
    
    public int xSize = 20;       
    public int ySize = 10;

    public GameObject tilePrefab;
    GameObject cube;

    // Start is called before the first frame update
    void Start()
    {

/*        //for each tile of grid, make a cube
        for (int i = 0; i < xSize; i++) {
            for (int j = 0; j < ySize; j++) {
                //cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube = GameObject.Instantiate(tilePrefab);
                cube.tag = "grid";
                //cube.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                cube.transform.position = new Vector3(i, j, 0.0f);
            }
        }
        */

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
