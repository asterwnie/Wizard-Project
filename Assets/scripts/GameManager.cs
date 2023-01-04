using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class GameManager : NetworkBehaviour
{
    [Header("Networking")]
    private NetworkVariable<float> masterClock = new NetworkVariable<float>();
    public NetworkVariable<int> numClients = new NetworkVariable<int>();

    [Header("References")]
    public GameObject pointerHighligher;
    public GameObject pointerSelected;
    public HelloWorldManager networkManager;
    public PlayerData localPlayer;

    public Material fireballMaterial; // THIS IS SUPER PLACEHOLDER
    public GameObject fireballProjectilePrefab;
    public GameObject spellburstProjectilePrefab;

    public GameObject plannedSkillGraphic;

    // turns
    [Header("Rounds")]
    public Image timerBar;
    float currentTime;
    float roundStartTime = 0f;
    static float roundDuration = 6f;
    public bool roundActive = false;
    


    // create a singleton so the Gamemanager can be found without Gamemanager.find
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    public NetworkVariable<float> GetServerClock()
    {
        return masterClock;
    }

    // Start is called before the first frame update
    void Start()
    {
        roundStartTime = masterClock.Value; // start the clock...this should probably not happen RIGHT at start ******
        roundActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            //server solely dictates the clock
            masterClock.Value += Time.deltaTime;
            BroadcastTimeClientRpc(masterClock.Value, roundStartTime); // send server time to clients

            // if the round is up, reset to do next round
            if (roundActive && masterClock.Value - roundStartTime >= roundDuration)
            {
                roundActive = false;
                BroadcastRoundActiveClientRpc(roundActive);
                SpellHandler.Instance.ResolveTurn();
                ResetRoundClientRpc();
            }
        }

        UpdateUI();
    }

    public void ResetRoundTime()
    {
        if (IsServer)
        {
            roundActive = true;
            BroadcastRoundActiveClientRpc(roundActive);
            roundStartTime = masterClock.Value;
        }
    }

    [ClientRpc]
    void BroadcastTimeClientRpc(float serverTime, float roundStartTime)
    {
        if (IsClient)
        {
            currentTime = serverTime;
            this.roundStartTime = roundStartTime;
        }
    }

    [ClientRpc]
    void BroadcastRoundActiveClientRpc(bool roundActive)
    {
        if (IsClient)
        {
            this.roundActive = roundActive;
        }
    }

    [ClientRpc]
    void ResetRoundClientRpc()
    {
        if (IsClient)
        {
            localPlayer.ClearSpellSelection();
            localPlayer.RemovePlannedSpells();
        }
    }

    public void UpdateUI()
    {
        if (IsClient)
        {
            // update timer ui
            timerBar.fillAmount = 1f - ((currentTime - roundStartTime) / roundDuration);
        }

        if(IsServer)
        {
            // update timer ui
            timerBar.fillAmount = 1f - ((masterClock.Value - roundStartTime) / roundDuration);
        }
        
    }


    // sends to player to handle spell selection
    public void SelectSpell(int number)
    {
        localPlayer.SelectSpell(number);
    }

}
