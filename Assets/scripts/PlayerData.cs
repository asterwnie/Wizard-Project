using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<int> numClients = new NetworkVariable<int>();

    //network action data
    string actionType = "idle";
    public Action action;
    int numPlayers;
    //public List<Action> allActions = new List<Action>();   //we will dump every action we hear into this list

    //screen pointer
    Camera camera;
    public GameObject pointer;
    public LayerMask playLayer;

    //player
    [Header("Player Stats")]
    public GameObject playerModel;
    public Image healthBarUI;
    static public int maxHealth = 100;
    static public int maxMana = 4;
    public NetworkVariable<int> CurrentHealth;
    int currentMana;

    // spellcasting
    [Header("Spellcasting")]
    List<GameObject> plannedActionIndicators = new List<GameObject>(); // rendered gameobjects that show what the player is going to do, deleted after moves have been done
    public Spell selectedSpell;
    Vector3 spellTarget;
    bool isAimingSpell = false;

    //placeholder spells - should be
    public Spell spell1 = new SpellFireball();
    public Spell spell2 = new SpellBurst();
    public Spell spell3 = new SpellIceShard();
    public Spell spell4 = new SpellOrbShield();

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
            SubmitPositionRequestServerRpc(new Vector3(Random.Range(0f, 9f), 0.5f, Random.Range(0f, 9f)));
            GameManager.Instance.localPlayer = this;
            SubmitHealthRequestServerRpc(maxHealth);
            currentMana = maxMana;
        }
    }

    public int GetMana() // gets the player's current mana
    {
        return currentMana;
    }

    public int GetHealth() // gets the player's current health
    {
        return CurrentHealth.Value;
    }

    public void ResetMana()
    {
        currentMana = maxMana;
    }

    void Update()
    {
        //calculate the number of players in the game
        numPlayers = GameObject.FindGameObjectsWithTag("Player").Length;

        if (IsClient && IsOwner)
        {

            DetectInput();

            if (selectedSpell != null)
                ShowRange();
            else
                lineRenderer.enabled = false;

            // update health bar
            healthBarUI.fillAmount = CurrentHealth.Value / maxHealth;
        }

        //hover
        float breathe = Mathf.Sin(Time.time * 2f) * 0.003f;
        playerModel.transform.position += new Vector3(0f, breathe, 0f);
        transform.position = Position.Value;
    }

    [ServerRpc(RequireOwnership=false)]
    void SendActionServerRpc(string actionType, Vector3 target, Spell.SpellType spellType = Spell.SpellType.INVALID, ServerRpcParams serverRpcParams = default) {

        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Action action = new Action(actionType, clientId, target, spellType);

        //Debug.Log(action.printInfo());
        SpellHandler.Instance.actionsQueue.Add(action); // add to the server's spell handler
        ClearSpellSelection();
    }


    [ServerRpc]
    void SubmitPositionRequestServerRpc(Vector3 pos, ServerRpcParams rpcParams = default)
    {
        Position.Value = pos;
    }

    [ServerRpc]
    void SubmitHealthRequestServerRpc(int health, ServerRpcParams rpcParams = default)
    {
        CurrentHealth.Value = health;
    }

    public void SetHealth(int health)
    {
        if (IsOwner)
            SubmitHealthRequestServerRpc(health);
    }

    public void ClearSpellSelection()
    {
        spellTarget = Vector3.zero;
        selectedSpell = null;
        pointer.SetActive(false);
        GameManager.Instance.ResetSpellSelectionUI();
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
                if(pointerLoc == Vector3.zero)
                {
                    // if the pointer is not in range, hide the pointer and deselect spell
                    ClearSpellSelection();
                }
                else if (Vector3.Distance(spellTarget, gameObject.transform.position) <= selectedSpell.GetRange())
                {
                    isAimingSpell = false;

                    // remove mana
                    currentMana -= selectedSpell.GetManaCost();

                    // make a planning indicator of the action for the client to render
                    GameObject planningGraphic = GameObject.Instantiate(GameManager.Instance.plannedSkillGraphic);
                    planningGraphic.transform.position = pointerLoc;
                    planningGraphic.GetComponentInChildren<TMP_Text>().text = selectedSpell.GetName();
                    plannedActionIndicators.Add(planningGraphic);

                    // planning line
                    GameObject planningLine = new GameObject("planning_line");
                    LineRenderer lineRenderer = planningLine.AddComponent<LineRenderer>();
                    lineRenderer.SetPosition(0,transform.position);
                    lineRenderer.SetPosition(1, pointerLoc);
                    lineRenderer.startWidth = lineWidth;
                    lineRenderer.endWidth = lineWidth;
                    lineRenderer.material = rangeIndicatorMaterial;
                    plannedActionIndicators.Add(planningLine);

                    // set target of spell
                    SendActionServerRpc(actionType, pointerLoc, selectedSpell.GetSpellType()); //send action data from the client -> server
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
            }
        } 
        else if (Input.GetKeyDown(KeyCode.Alpha1)) {
            GameManager.Instance.SelectSpell(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameManager.Instance.SelectSpell(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GameManager.Instance.SelectSpell(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GameManager.Instance.SelectSpell(4);
        }
        else if(Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            ClearSpellSelection();
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
        if (Physics.Raycast(ray, out hit, 100f, playLayer))
        {
            pointer.transform.position = hit.point + new Vector3(0f, 0.25f, 0f);

            //player look at pointer
            playerModel.transform.LookAt(new Vector3(pointer.transform.position.x, playerModel.transform.position.y, pointer.transform.position.z));

            // return pointer pos for spell targeting
            return pointer.transform.position;
        }

        return Vector3.zero;
    }

    public void SelectSpell(int number)
    {
        isAimingSpell = true;
        actionType = "spell";
        switch (number)
        {
            case 1:
                if (currentMana - spell1.GetManaCost() >= 0) { selectedSpell = spell1; }
                else { actionType = "none"; isAimingSpell = false; }
                return;
            case 2:
                if (currentMana - spell2.GetManaCost() >= 0) { selectedSpell = spell2; }
                else { actionType = "none"; isAimingSpell = false; }
                return;
            case 3:
                if (currentMana - spell3.GetManaCost() >= 0) { selectedSpell = spell3; }
                else { actionType = "none"; isAimingSpell = false; }
                return;
            case 4:
                if (currentMana - spell4.GetManaCost() >= 0) { selectedSpell = spell4; }
                else { actionType = "none"; isAimingSpell = false; }
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