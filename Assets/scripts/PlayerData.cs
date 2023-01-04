using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerData : NetworkBehaviour
{
    //public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<int> numClients = new NetworkVariable<int>();

    //network action data
    string actionType = "idle";
    public Action action;

    //screen pointer
    Camera camera;
    public GameObject pointer;
    public LayerMask playLayer;

    //player
    [Header("Player Stats")]
    public GameObject playerPrefab;
    public GameObject player;
    static int maxHealth = 100;
    int currentHealth;

    // spellcasting
    [Header("Spellcasting")]
    public Spell selectedSpell;
    Vector3 spellTarget;
    bool isAimingSpell = false;
    List<GameObject> plannedActionIndicators = new List<GameObject>(); // rendered gameobjects that show what the player is going to do, deleted after moves have been done

    //placeholder spells - should be
    Spell spell1 = new SpellFireball();
    Spell spell2 = new SpellBurst();
    Spell spell3 = new SpellFireball();
    Spell spell4 = new SpellFireball();


    // range indicator
    float lineThetaScale = 0.01f;
    float radius;
    int linePoints;
    LineRenderer lineRenderer;
    float lineTheta = 0f;
    public Material rangeIndicatorMaterial;
    public float lineWidth = 0.05f;

    void Start() {
        //grab camera and instantiate screen pointer sphere
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        pointer = GameManager.Instance.pointerSelected;

        // for radius indicator
        if(IsClient)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = rangeIndicatorMaterial;
            lineRenderer.startWidth = lineWidth; //thickness of line
            lineRenderer.endWidth = lineWidth;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // move player to random location
            SubmitPositionRequestServerRpc(new Vector3(Random.Range(0f, 9f), 1f, Random.Range(0f, 9f)));
            GameManager.Instance.localPlayer = this;
            currentHealth = maxHealth;
        }
    }

    void Update()
    {
            if (IsClient && IsOwner)
            {

                DetectInput();

                if (selectedSpell != null)
                    ShowRange();
                else
                    lineRenderer.enabled = false;
            }

            //hover
            float breathe = Mathf.Sin(2 * Time.time) * 0.2f;
            //transform.position = Position.Value + new Vector3(0.0f, breathe, 0.0f);
    }

    [ServerRpc(RequireOwnership=false)]
    void SendActionServerRpc(string actionType, Vector3 target, Spell.SpellType spellType = Spell.SpellType.INVALID, ServerRpcParams serverRpcParams = default) {

        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Action action = new Action(actionType, clientId, target, spellType);

        Debug.Log(action.printInfo());
        SpellHandler.Instance.actionsQueue.Add(action); // add to the server's spell handler
        ClearSpellSelection();
    }


    [ServerRpc]
     void SubmitPositionRequestServerRpc(Vector3 pos, ServerRpcParams rpcParams = default)
     {
        //Position.Value = pos;
     }

    public void ClearSpellSelection()
    {
        spellTarget = Vector3.zero;
        selectedSpell = null;
        pointer.SetActive(false);
        isAimingSpell = false;
    }

    void DetectInput() {

        if(!GameManager.Instance.roundActive) { return; } // if the round is not active, abort

        //detect input and send actions to server
        Vector3 pointerLoc = MovePointer();

        if (Input.GetMouseButtonDown(0)) // if mouse is clicked
        {
            // check if aiming spell or just moving
            if (isAimingSpell)
            {
                spellTarget = pointerLoc;
                // check if target is in range of the spell
                if (Vector3.Distance(spellTarget, gameObject.transform.position) <= selectedSpell.GetRange())
                {
                    // make a planning indicator of the action for the client to render
                    GameObject planningGraphic = GameObject.Instantiate(GameManager.Instance.plannedSkillGraphic);
                    planningGraphic.transform.position = pointer.transform.position;
                    planningGraphic.GetComponentInChildren<TMP_Text>().text = selectedSpell.GetName();
                    plannedActionIndicators.Add(planningGraphic);

                    // set target of spell
                    SendActionServerRpc(actionType, pointer.transform.position, selectedSpell.GetSpellType()); //send action data from the client -> server
                }
                else
                {
                    // if the pointer is not in range, hide the pointer and deselect spell
                    ClearSpellSelection();
                }
                
            }
            else
            {
                //move
                actionType = "move";
                SendActionServerRpc(actionType, pointer.transform.position);
            }
        } 
        else if (Input.GetKeyDown(KeyCode.Alpha1)) {
            actionType = "spell";
            SelectSpell(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            actionType = "spell";
            SelectSpell(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            actionType = "spell";
            SelectSpell(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            actionType = "spell";
            SelectSpell(3);
        }
    }

    Vector3 MovePointer()
    {
        // if casting a spell, only set active when the pointer is in casting range
        pointer.SetActive(false);
        if (selectedSpell != null)
        {
            if (Vector3.Distance(pointer.transform.position, gameObject.transform.position) <= selectedSpell.GetRange())
            {
                pointer.SetActive(true);
            }
        }

        //move pointer indicator to mouse location
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, playLayer))
        {
            pointer.transform.position = hit.point + new Vector3(0f, 0.25f, 0f);
            return pointer.transform.position;
        }

        return Vector3.zero;
    }

    public void SelectSpell(int number)
    {
        isAimingSpell = true;
        switch(number)
        {
            case 1: 
                selectedSpell = spell1;
                return;
            case 2:
                selectedSpell = spell2;
                return;
            case 3:
                selectedSpell = spell3;
                return;
            case 4:
                selectedSpell = spell4;
                return;
        }
            
    }

    public void ShowRange()
    {
        // show range indicator of selected spell
        lineRenderer.enabled = true;
        lineTheta = 0f;
        linePoints = (int)((1f / lineThetaScale) + 1f);
        lineRenderer.positionCount = linePoints;
        radius = selectedSpell.GetRange();
        Vector3 playerPos = transform.position;
        for (int i = 0; i < linePoints; i++)
        {
            lineTheta += (2.0f * Mathf.PI * lineThetaScale);
            float x = radius * Mathf.Cos(lineTheta);
            float y = radius * Mathf.Sin(lineTheta);
            lineRenderer.SetPosition(i, new Vector3(x + playerPos.x, 0.25f, y + playerPos.z));
        }
    }

    public void RemovePlannedSpells()
    {
        foreach(GameObject obj in plannedActionIndicators)
        {
            Destroy(obj);
        }
        plannedActionIndicators.Clear();
    }
}