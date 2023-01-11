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

    [Header("Spell Buttons")]
    public TMP_Text spell1Text;
    public TMP_Text spell2Text;
    public TMP_Text spell3Text;
    public TMP_Text spell4Text;

    [Header("Spell UI")]
    public List<Image> manaUI;
    public List<Image> spellUI;

    [Header("Spell References")]
    public Material fireballMaterial; // THIS IS SUPER PLACEHOLDER
    public GameObject fireballProjectilePrefab;
    public GameObject fireballImpactPrefab;
    public GameObject spellburstProjectilePrefab;
    public GameObject iceShardProjectilePrefab;
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
                ResetSpellSelectionUI();
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

    public void ResetSpellSelectionUI()
    {
        //reset spell selection UI
        foreach (Image spell in spellUI)
        {
            spell.color = new Color(spell.color.r, spell.color.g, spell.color.b, 1f);
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

            // update spells to reflect player's spell names
            spell1Text.text = localPlayer.spell1.GetName();
            spell2Text.text = localPlayer.spell2.GetName();
            spell3Text.text = localPlayer.spell3.GetName();
            spell4Text.text = localPlayer.spell4.GetName();

            // update mana to reflect player mana
            foreach(Image mana in manaUI) { mana.color = new Color(mana.color.r, mana.color.g, mana.color.b, 0f); } // reset visibility
            for(int i = 0; i < localPlayer.GetMana(); i++)
            {
                manaUI[i].color = new Color(manaUI[i].color.r, manaUI[i].color.g, manaUI[i].color.b, 1f); // show visible
            }
            
            // update mana UI if a spell is selected
            if(localPlayer.selectedSpell != null)
            {
                for(int i = localPlayer.GetMana() - 1; i > localPlayer.GetMana() - localPlayer.selectedSpell.GetManaCost() - 1; i--)
                {
                    manaUI[i].color = new Color(manaUI[i].color.r, manaUI[i].color.g, manaUI[i].color.b, Mathf.Sin(Time.time * 5f) / 4f + 0.5f); // show visible
                }
            }
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

        //update spell UI
        for(int i = 1; i <= spellUI.Count; i++)
        {
            if(i == number)
                spellUI[i - 1].color = new Color(spellUI[i - 1].color.r, spellUI[i - 1].color.g, spellUI[i - 1].color.b, 0.5f);
            else
                spellUI[i - 1].color = new Color(spellUI[i - 1].color.r, spellUI[i - 1].color.g, spellUI[i - 1].color.b, 1f);
        }
        
    }

}
