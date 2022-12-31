using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setupGrid : MonoBehaviour
{
    
    public int xSize = 20;       
    public int ySize = 10;

    /*10x10 room tiles, reduced to a boolean array
        ex.           0 0 0 1  
                      O 1 1 1
                      O O O 1
                      1 1 1 1
    */
    bool[] cicleRoom = {false,  false,  false,  true,  true,  true,  true,  false,  false,  false,  false,  false,  true,  true,  true,  true,  true,  true,  false,  false,  false,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  false,  false,  false,  true,  true,  true,  true,  true,  true,  false,  false,  false,  false,  false,  true,  true,  true,  true,  false,  false,  false};
    bool[] donutRoom = {false,  false,  false,  true,  true,  true,  true,  false,  false,  false,  false,  false,  true,  true,  true,  true,  true,  true,  false,  false,  false,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  false,  false,  true,  true,  true,  true,  true,  true,  true,  false,  false,  false,  false,  true,  true,  true,  true,  true,  true,  false,  false,  false,  false,  true,  true,  true,  true,  true,  true,  true,  false,  false,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  false,  false,  false,  true,  true,  true,  true,  true,  true,  false,  false,  false,  false,  false,  true,  true,  true,  true,  false,  false,  false};
    bool[] largeRoom = {false,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false,  true,  true,  true,  true,  true,  true,  true,  true,  false};
    bool[] tRoom = {false,  false,  false,  false,  true,  true,  false,  false,  false,  false,  false,  false,  false,  false,  true,  true,  false,  false,  false,  false,  false,  false,  false,  false,  true,  true,  false,  false,  false,  false,  false,  false,  false,  false,  true,  true,  false,  false,  false,  false,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false};

    
    
    public GameObject tilePrefab;
    GameObject cube;

    // Start is called before the first frame update
    void Start()
    {

        //for each tile of grid, make a cube
        for (int i = 0; i < xSize; i++) {
            for (int j = 0; j < ySize; j++) {
                if (donutRoom[i*10 + j] == true) {      //room select
                    cube = GameObject.Instantiate(tilePrefab);
                    cube.transform.position = new Vector3(i, 0.0f, j);
                }
            }
        }
  
    }
 }