using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GameObject tileHighligher;
    public GameObject tileSelected;
    public HelloWorldManager networkManager;
    public HelloWorldPlayer localPlayer;
   // public setupGrid grid;

    // spellcasting
    [Header("Spellcasting")]
    public TMP_Text selectedSpellName;
    public Spell selectedSpell;
    public Material fireballMaterial; // THIS IS SUPER PLACEHOLDER
    public GameObject projectilePrefab;
    bool isCastingAnimation = false;
    WorldTile selectedTile;
    
    // range indicator
    float lineThetaScale = 0.01f;
    float radius;
    int linePoints;
    LineRenderer lineRenderer;
    float lineTheta = 0f;
    public Material rangeIndicatorMaterial;
    public float lineWidth = 0.05f;


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

        // for radius indicator
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = rangeIndicatorMaterial;
        lineRenderer.startWidth = lineWidth; //thickness of line
        lineRenderer.endWidth = lineWidth;
    }

    // Update is called once per frame
    void Update()
    {
        MoveTileHighlighter();

        // update spell UI
        if (selectedSpell != null)
            selectedSpellName.text = selectedSpell.GetName();
        else
            selectedSpellName.text = "None";

        if (selectedSpell != null && !isCastingAnimation)
            ShowRange();
        else
            lineRenderer.enabled = false;
    }

    public void ShowRange()
    {
        // show range indicator of selected spell
        lineRenderer.enabled = true;
        lineTheta = 0f;
        linePoints = (int)((1f / lineThetaScale) + 1f);
        lineRenderer.positionCount = linePoints;
        radius = selectedSpell.GetRange();
        Vector3 playerPos = localPlayer.transform.position;
        for (int i = 0; i < linePoints; i++)
        {
            lineTheta += (2.0f * Mathf.PI * lineThetaScale);
            float x = radius * Mathf.Cos(lineTheta);
            float y = radius * Mathf.Sin(lineTheta);
            lineRenderer.SetPosition(i, new Vector3(x + playerPos.x, 0.25f, y + playerPos.z));
        }
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

                // if casting a spell & clicked, select this tile
                if(selectedSpell != null && Input.GetMouseButton(0))
                {
                    // SELECT SPELL TARGET TILE

                    // check if within the spell's radius
                    if(Vector3.Distance(localPlayer.transform.position, hoveredTile.transform.position) <= selectedSpell.GetRange())
                    {
                        // select the tile & move selection indicator
                        hoveredTile.selected = true;
                        selectedTile = hoveredTile;
                        tileSelected.SetActive(true);
                        tileSelected.transform.position = hoveredTile.footLoc.transform.position;
                    }
                   
                }

            }
            else if (hitObject.layer != 5) // 5 is the UI layer
            {
                // when clicking outside of the tilezone, deselect any tile that may be selected
                if (Input.GetMouseButton(0))
                    DeselectSpell();
            }
        }
        else
        {
            // disable the highlighter if nothing was hit
            tileHighligher.SetActive(false);

            // when clicking outside of the tilezone, deselect any tile that may be selected
            if (Input.GetMouseButton(0))
            {
                if (EventSystem.current.IsPointerOverGameObject()) return; // prevent UI clickthrough
                DeselectSpell();
            }
                
        }
    }

    public void DeselectSpell() // called when a click occurs outside the tilezone
    {
        // deselect tile
        if (selectedTile)
            selectedTile.selected = false;
        selectedTile = null;
        tileSelected.SetActive(false);

        // deselect spell
        selectedSpell = null;
    }

    //spells .... can probably be cleaned up later
    public void SelectSpellBurst()
    {
        selectedSpell = new SpellBurst();
    }

    public void SelectFireball()
    {
        selectedSpell = new SpellFireball();
        
    }

    public void ClearSpellSelection()
    {
        selectedSpell = null;
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
        isCastingAnimation = true;
        float projectileSpeed = .5f;
        float projectileHeight = 1f;
        float spellDuration = 2f;

        // placeholder.....this should be a prefab instead!

        // create the projectile
        GameObject projectile = GameObject.Instantiate(projectilePrefab);
        projectile.transform.position = localPlayer.transform.position;
        projectile.transform.LookAt(selectedTile.footLoc.transform.position);

        // move the projectile from the player to the impact zone
        float deltaTime = 0f;
        float duration = projectileSpeed;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            Vector3 xzMovement = Vector3.Lerp(localPlayer.transform.position, selectedTile.footLoc.transform.position, deltaTime / duration);
            float yMovement = Mathf.Sin((deltaTime / duration) * Mathf.PI) * projectileHeight;
            projectile.transform.position = new Vector3(xzMovement.x, xzMovement.y + yMovement, xzMovement.z);
            yield return null;
        }

        // create the impact spell effect
        GameObject spellEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spellEffect.transform.position = selectedTile.footLoc.transform.position;
        spellEffect.transform.localScale = Vector3.zero;
        Vector3 targetScale = new Vector3(selectedSpell.GetRadius() * 2f, selectedSpell.GetRadius() * 2f, selectedSpell.GetRadius() * 2f);
        spellEffect.GetComponent<Renderer>().material = fireballMaterial;

        // clear the selection after it is done
        selectedTile = null;
        selectedSpell = null;
        tileSelected.SetActive(false);
        isCastingAnimation = false;

        // grow after impact to correct size
        deltaTime = 0f;
        duration = 0.5f;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            spellEffect.transform.localScale = Vector3.Lerp(spellEffect.transform.localScale, targetScale, deltaTime / duration);
            yield return null;
        }

        // wait for spell impact to linger
        deltaTime = 0f;
        duration = spellDuration;
        while(deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            if(deltaTime >= .1f)
                GameObject.Destroy(projectile); // remove the projectile

            yield return null;
        }

        // shrink the impact zone to 0
        deltaTime = 0f;
        duration = 0.5f;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            spellEffect.transform.localScale = Vector3.Lerp(spellEffect.transform.localScale, Vector3.zero, deltaTime / duration);
            yield return null;
        }

        GameObject.Destroy(spellEffect);

        // release coroutine
        yield return null;
    }
}
