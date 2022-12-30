using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breathe : MonoBehaviour
{
    public float speed = 2f;
    public float minSize = 0.8f;
    public float fluctuation = .2f;

    // Update is called once per frame
    void Update()
    {
        float breathe = Mathf.Abs(Mathf.Sin(Time.time * speed)) * fluctuation + minSize;
        transform.localScale = new Vector3(breathe, breathe, 1f);
    }
}
