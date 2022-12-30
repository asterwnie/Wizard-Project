using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setupPlayer : MonoBehaviour
{
    
    GameObject player;
    float xPos = 0f;
    float yPos = 0f;
    float zPos = -1.0f;

    GameObject camera; 
    Camera cameraComponent;

    void Awake()
    {
        //create player
        player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.transform.position = new Vector3(xPos, yPos, zPos);
        player.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

        camera = GameObject.Find("Main Camera");
        cameraComponent = camera.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        //add a hovering effect
        zPos = 0.5f*Mathf.Sin(Time.time*1.5f) - 1.0f;

        //reset the scale of the grid
       GameObject[] grid = GameObject.FindGameObjectsWithTag("grid");
       foreach (GameObject cube in grid) {
              cube.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
       }

        //handle keystrokes
        if (Input.GetKeyDown("w"))
        {
            yPos += 1;
        } else
        if (Input.GetKeyDown("a"))
        {
            xPos -= 1;
        } else
        if (Input.GetKeyDown("s"))
        {
            yPos -= 1;
        } else 
        if (Input.GetKeyDown("d"))
        {
            xPos += 1;
        }

        //raycast mouse to battlefield for movement
        
        RaycastHit hit;
        Ray ray = cameraComponent.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit)) {

            GameObject hitObject = hit.transform.gameObject;    //find which object was hit
            if (hitObject.tag == "grid") { 
                hitObject.transform.localScale = new Vector3(1f + 0.4f*zPos, 1f + 0.4f*zPos, 1f + 0.4f*zPos);     //make the focused cube breathe
            }


        } 



        player.transform.position = new Vector3(xPos, yPos, zPos);
    }
}
