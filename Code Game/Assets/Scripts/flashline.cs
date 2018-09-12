using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flashline : MonoBehaviour
{

    private float time = 0.0f;
    private SpriteRenderer sr;
    private int counter = 1;

    // Use this for initialization
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        Color col = sr.color;

        if (time > 0.3f)
        {
            if (col.a == 255)
            {
                col.a = 0;
            }
            else
            {
                col.a = 255;
            }
            sr.color = col;
            time = 0.0f;
            counter++;
        }
    }
}
