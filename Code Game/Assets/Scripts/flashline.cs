using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flashline : MonoBehaviour
{

    private float time = 0.0f;
    private SpriteRenderer sr;

    // Use this for initialization
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //increment time
        time += Time.deltaTime;
        //temp colour variable
        Color col = sr.color;

        //do every 0.3 seconds
        if (time > 0.3f)
        {
            //Change opacity to cause line to flash
            if (col.a == 255)
            {
                col.a = 0;
            }
            else
            {
                col.a = 255;
            }
            sr.color = col;

            //reset time to 0
            time = 0.0f;
        }
    }
}
