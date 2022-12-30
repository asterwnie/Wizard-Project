using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setupPlayer : MonoBehaviour
{
    
    GameObject player;
    float xPos = 0f;
    float yPos = 0f;
    float zPos = -1.0f;

    void Start()
    {
        //create player
        player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.transform.position = new Vector3(xPos, yPos, zPos);
        player.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

    }

    // Update is called once per frame
    void Update()
    {

        //add a hovering effect
        zPos = 0.5f*Mathf.Sin(Time.time*1.5f) - 1.0f;

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






        player.transform.position = new Vector3(xPos, yPos, zPos);
    }
}
