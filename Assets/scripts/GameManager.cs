using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    public GameObject tileHighligher;
    public GameObject tileSelected;
    public HelloWorldManager networkManager;


    // spellcasting
    public TMP_Text selectedSpellName;
    public Spell selectedSpell;
    WorldTile selectedTile;
    public Material fireballMaterial; // THIS IS SUPER PLACEHOLDER


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
        tileSelected.SetActive(false);
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
        //Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.transform.gameObject;    //find which object was hit
            if (hitObject.tag == "grid")
            {
                WorldTile hoveredTile = hitObject.GetComponent<WorldTile>();

                // move the highlighter to the tile hit
                tileHighligher.SetActive(true);
                tileHighligher.transform.position = hoveredTile.footLoc.transform.position;

                // if clicked, select this one
                if(Input.GetMouseButton(0))
                {
                    hoveredTile.selected = true;
                    selectedTile = hoveredTile;
                    tileSelected.SetActive(true);
                    tileSelected.transform.position = hoveredTile.footLoc.transform.position;
                }

            }
            else if (hitObject.layer != 5) // 5 is the UI layer
            {
                // when clicking outside of the tilezone, deselect any tile that may be selected
                if (Input.GetMouseButtonDown(0))
                {
                    if (selectedTile)
                        selectedTile.selected = false;
                    selectedTile = null;
                    tileSelected.SetActive(false);
                }
            }
        }
        else
        {
            // disable the highlighter if nothing was hit
            tileHighligher.SetActive(false);
        }
    }

    //spells .... can probably be cleaned up later
    public void SelectSpellBurst()
    {
        selectedSpell = new SpellBurst();
        selectedSpellName.text = selectedSpell.GetName();
    }

    public void SelectFireball()
    {
        selectedSpell = new SpellFireball();
        selectedSpellName.text = selectedSpell.GetName();
    }

    public void ClearSpellSelection()
    {
        selectedSpell = null;
        selectedSpellName.text = "None";
    }

    public void ConfirmSpell()
    {
        // check if there is a selected spell and a selected tile
        if(selectedSpell != null && selectedTile != null)
        {
            StartCoroutine(ExecuteSpell());
        }
    }

    // this is done in a coroutine to ensure everything gets done in the right order
    IEnumerator ExecuteSpell()
    {
        Debug.Log("Executing spell: " + selectedSpell.GetName());

        // placeholder.....this should be a prefab instead!
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.transform.position = selectedTile.footLoc.transform.position;
        projectile.transform.localScale = new Vector3(selectedSpell.GetRadius(), selectedSpell.GetRadius(), selectedSpell.GetRadius());
        projectile.GetComponent<Renderer>().material = fireballMaterial;

        // clear the selection after it is done
        selectedTile = null;
        selectedSpell = null;


        // delete prefab after a time
        float deltaTime = 0f;
        float duration = 2f;
        while(deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            yield return null;
        }
        GameObject.Destroy(projectile);

        // release coroutine
        yield return null;
    }
}
