using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public abstract class Spell
{
    protected string name;
    protected float range;
    protected float radius;
    public Spell() { }

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

    // this is done in a coroutine to ensure everything gets done in the right order
    public IEnumerator ExecuteSpell(Vector3 origin, Vector3 targetPos)
    {
        Debug.Log("Executing spell: " + this.GetName());
        //isCastingAnimation = true;
        float projectileSpeed = .5f;
        float projectileHeight = 1f;
        float spellDuration = 2f;

        // placeholder.....this should be a prefab instead!

        // create the projectile
        GameObject projectile = GameObject.Instantiate(GameManager.Instance.projectilePrefab);
        //projectile.GetComponent<NetworkObject>().Spawn();
        //projectile.transform.position = player.transform.position;
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
        GameObject spellEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        spellEffect.transform.position = targetPos;
        spellEffect.transform.localScale = Vector3.zero;
        Vector3 targetScale = new Vector3(this.GetRadius() * 2f, this.GetRadius() * 2f, this.GetRadius() * 2f);
        spellEffect.GetComponent<Renderer>().material = GameManager.Instance.fireballMaterial;

        // clear the selection after it is done
        /*selectedTile = null;
        selectedSpell = null;
        tileSelected.SetActive(false);
        isCastingAnimation = false;*/

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

public class SpellFireball : Spell
{
    public SpellFireball()
    {
        name = "Fireball";
        range = 4.5f;
        radius = 2f;
    }
}

public class SpellBurst : Spell
{
    public SpellBurst()
    {
        name = "Magic Burst";
        range = 7.5f;
        radius = .5f;
    }
}
