using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flashline : MonoBehaviour
{

    private float time = 0.0f;
    private Color sr;

    // Use this for initialization
    void Start()
    {
        sr = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > 0.5f)
        {
            if (sr.a == 255)
            {
                sr.a = 0;
            }
            else
            {
                sr.a = 255;
            }
            time = 0.0f;
        }
    }
}
