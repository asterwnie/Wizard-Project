using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericDmgSpellEffect : MonoBehaviour
{
    public Spell spell;
    bool isUsed = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit!: " + other.name);
        if (other.tag == "Player" && !isUsed)
        {
            isUsed = true;
            PlayerData currentPlayer = other.gameObject.GetComponent<PlayerData>();
            currentPlayer.SetHealth(currentPlayer.GetHealth() - spell.GetDamage());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("hit!: " + other.name);
        if (other.tag == "Player" && !isUsed)
        {
            isUsed = true;
            PlayerData currentPlayer = other.gameObject.GetComponent<PlayerData>();
            currentPlayer.SetHealth(currentPlayer.GetHealth() - spell.GetDamage());
        }
    }
}
