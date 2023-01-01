using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
