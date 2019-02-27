using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoDestroy : MonoBehaviour
{
    static public int sceneNumber = 1;
    static public bool helpButton = false;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }
}
