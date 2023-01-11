using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// PLEASE READ!!!!
// WHEN ADDING A NEW SPELL, MAKE SURE TO ALSO
// ADD AN ENTRY IN THE SWITCH STATEMENT
// OF SpellHandler.cs IN ExecuteSpell()!!!!!!
public abstract class Spell
{
    public enum SpellType
    { 
        INVALID = -1,
        FIREBALL,
        SPELL_BURST,
        ICE_SHARD,
        ORB_SHIELD,
        COUNT
    }

    protected SpellType spellType;
    protected string name;
    protected float range;
    protected float radius;
    protected int manaCost;
    protected int damage;
    public Spell() { }

    public SpellType GetSpellType()
    {
        return spellType;
    }
    public string GetName()
    {
        return name;
    }

    public float GetRadius()
    {
        return radius;
    }

    public float GetRange()
    {
        return range;
    }

    public int GetManaCost()
    {
        return manaCost;
    }

    public int GetDamage()
    {
        return damage;
    }

    // this is done in a coroutine to ensure everything gets done in the right order
    public abstract IEnumerator ExecuteSpell(Vector3 origin, Vector3 targetPos);
}

public class SpellFireball : Spell
{
    float projectileSpeed = 0.5f;
    float projectileHeight = 1f;
    float spellDuration = 2f;
    public SpellFireball()
    {
        spellType = SpellType.FIREBALL;
        name = "Fireball";
        range = 4.5f;
        radius = 2f;
        manaCost = 2;
        damage = 20;
    }

    public override IEnumerator ExecuteSpell(Vector3 origin, Vector3 targetPos)
    {
        //Debug.Log("Executing spell animation: " + this.GetName());

        // create the projectile
        GameObject projectile = GameObject.Instantiate(GameManager.Instance.fireballProjectilePrefab);
        projectile.transform.position = origin;
        projectile.transform.LookAt(targetPos);

        // move the projectile from the player to the impact zone
        float deltaTime = 0f;
        float duration = projectileSpeed;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            Vector3 xzMovement = Vector3.Lerp(origin, targetPos, deltaTime / duration);
            float yMovement = Mathf.Sin((deltaTime / duration) * Mathf.PI) * projectileHeight;
            projectile.transform.position = new Vector3(xzMovement.x, xzMovement.y + yMovement, xzMovement.z);
            yield return null;
        }

        // create the impact spell effect
        GameObject spellEffect = GameObject.Instantiate(GameManager.Instance.fireballImpactPrefab);
        spellEffect.transform.position = targetPos;
        //spellEffect.transform.localScale = Vector3.zero;
        //Vector3 targetScale = new Vector3(this.GetRadius() * 2f, this.GetRadius() * 2f, this.GetRadius() * 2f);
        //spellEffect.GetComponentInChildren<GenericDmgSpellEffect>().spell = this;


        /*// grow after impact to correct size
        deltaTime = 0f;
        duration = 0.5f;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            spellEffect.transform.localScale = Vector3.Lerp(spellEffect.transform.localScale, targetScale, deltaTime / duration);
            yield return null;
        }*/

        // wait for spell impact to linger
        deltaTime = 0f;
        duration = spellDuration;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            if (deltaTime >= .1f)
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

public class SpellBurst : Spell
{
    float projectileSpeed = .5f;
    float projectileHeight = 1f;
    //float spellDuration = 1f;
    public SpellBurst()
    {
        spellType = SpellType.SPELL_BURST;
        name = "Magic Burst";
        range = 7.5f;
        radius = .5f;
        manaCost = 1;
        damage = 10;
    }

    public override IEnumerator ExecuteSpell(Vector3 origin, Vector3 targetPos)
    {
        Debug.Log("Executing spell animation: " + this.GetName());

        // create the projectile
        GameObject projectile = GameObject.Instantiate(GameManager.Instance.spellburstProjectilePrefab);
        projectile.transform.position = origin;
        projectile.transform.LookAt(targetPos);

        // move the projectile from the player to the impact zone
        float deltaTime = 0f;
        float duration = projectileSpeed;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            Vector3 xzMovement = Vector3.Lerp(origin, targetPos, deltaTime / duration);
            float yMovement = Mathf.Sin((deltaTime / duration) * Mathf.PI) * projectileHeight;
            projectile.transform.position = new Vector3(xzMovement.x, xzMovement.y + yMovement, xzMovement.z);
            yield return null;
        }

        // shrink the projectile to 0
        deltaTime = 0f;
        duration = 0.5f;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            projectile.transform.localScale = Vector3.Lerp(projectile.transform.localScale, Vector3.zero, deltaTime / duration);
            yield return null;
        }

        GameObject.Destroy(projectile); // remove the projectile

        // release coroutine
        yield return null;
    }
}

public class SpellOrbShield : Spell
{
    //float projectileSpeed = .5f;
    float projectileHeight = 1f;
    float spellDuration = 6f;
    public SpellOrbShield()
    {
        spellType = SpellType.ORB_SHIELD;
        name = "Orb Shield";
        range = 1f;
        radius = 1f;
        manaCost = 2;
        damage = 5;
    }

    public override IEnumerator ExecuteSpell(Vector3 origin, Vector3 targetPos)
    {
        Debug.Log("Executing spell animation: " + this.GetName());

        int numProjectiles = 3;
        float radiansBetweenProjectiles = (Mathf.PI * 2) / numProjectiles;
        Vector3 height = new Vector3(0f, projectileHeight, 0f);

        // create the projectile3
        GameObject projectile1 = GameObject.Instantiate(GameManager.Instance.spellburstProjectilePrefab);
        projectile1.transform.position = origin + height;

        GameObject projectile2 = GameObject.Instantiate(GameManager.Instance.spellburstProjectilePrefab);
        projectile2.transform.position = origin + height;

        GameObject projectile3 = GameObject.Instantiate(GameManager.Instance.spellburstProjectilePrefab);
        projectile3.transform.position = origin + height;

        // move the projectiles to rotate around player for duration
        float deltaTime = 0f;
        float duration = spellDuration;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            float xMovement1 = Mathf.Sin((deltaTime / duration) * Mathf.PI) * radius;
            float zMovement1 = Mathf.Cos((deltaTime / duration) * Mathf.PI) * radius;
            projectile1.transform.position = new Vector3(xMovement1, origin.y - 1f, zMovement1) + origin + height;

            deltaTime += Time.deltaTime;
            float xMovement2 = Mathf.Sin((deltaTime / duration) * Mathf.PI + radiansBetweenProjectiles) * radius;
            float zMovement2 = Mathf.Cos((deltaTime / duration) * Mathf.PI + radiansBetweenProjectiles) * radius;
            projectile2.transform.position = new Vector3(xMovement2, origin.y - 1f, zMovement2) + origin + height;

            deltaTime += Time.deltaTime;
            float xMovement3 = Mathf.Sin((deltaTime / duration) * Mathf.PI + (radiansBetweenProjectiles * 2f)) * radius;
            float zMovement3 = Mathf.Cos((deltaTime / duration) * Mathf.PI + (radiansBetweenProjectiles * 2f)) * radius;
            projectile3.transform.position = new Vector3(xMovement3, origin.y - 1f, zMovement3) + origin + height;
            yield return null;
        }

/*        // shrink after a time
        deltaTime = 0f;
        duration = 0.2f;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            projectile1.transform.localScale = Vector3.Lerp(projectile1.transform.localScale, Vector3.zero, deltaTime / duration);
            projectile2.transform.localScale = Vector3.Lerp(projectile2.transform.localScale, Vector3.zero, deltaTime / duration);
            projectile3.transform.localScale = Vector3.Lerp(projectile3.transform.localScale, Vector3.zero, deltaTime / duration);
            yield return null;
        }
*/
        // remove the projectiles
        GameObject.Destroy(projectile1);
        GameObject.Destroy(projectile2);
        GameObject.Destroy(projectile3);

        // release coroutine
        yield return null;
    }
}

public class SpellIceShard : Spell
{
    float projectileSpeed = .7f;
    //float spellDuration = 5f;
    public SpellIceShard()
    {
        spellType = SpellType.ICE_SHARD;
        name = "Ice Shard";
        range = 6f;
        radius = 2f;
        manaCost = 1;
        damage = 15;
    }

    public override IEnumerator ExecuteSpell(Vector3 origin, Vector3 targetPos)
    {
        Debug.Log("Executing spell animation: " + this.GetName());

        // create the projectile
        GameObject projectile = GameObject.Instantiate(GameManager.Instance.iceShardProjectilePrefab);
        projectile.transform.position = origin;
        projectile.transform.LookAt(targetPos);

        // move the projectile from the player to the impact zone
        float deltaTime = 0f;
        float duration = projectileSpeed;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            Vector3 xzMovement = Vector3.Lerp(origin, targetPos, deltaTime / duration);
            projectile.transform.position = new Vector3(xzMovement.x, xzMovement.y, xzMovement.z);
            yield return null;
        }

        // shrink the projectile to 0
        deltaTime = 0f;
        duration = 0.5f;
        while (deltaTime < duration)
        {
            deltaTime += Time.deltaTime;
            projectile.transform.localScale = Vector3.Lerp(projectile.transform.localScale, Vector3.zero, deltaTime / duration);
            yield return null;
        }

        GameObject.Destroy(projectile); // remove the projectile

        // release coroutine
        yield return null;
    }
}