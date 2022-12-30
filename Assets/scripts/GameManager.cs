using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject tileHighligher;
    public HelloWorldManager networkManager;

    // create a singleton so the Gamemanager can be found without Gamemanager.find
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        tileHighligher.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        MoveTileHighlighter();
    }

    public void MoveTileHighlighter()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.transform.gameObject;    //find which object was hit
            if (hitObject.tag == "grid")
            {
                // move the highlighter to the tile hit
                tileHighligher.SetActive(true);
                tileHighligher.transform.position = hitObject.GetComponent<WorldTile>().footLoc.transform.position;
            }
        }
        else
        {
            // disable the highlighter if nothing was hit
            tileHighligher.SetActive(false);
        }
    }

}
