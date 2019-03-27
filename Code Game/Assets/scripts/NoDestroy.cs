using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoDestroy : MonoBehaviour
{
    //variables that need stored between levels
    static public int sceneNumber = 1;
    static public bool helpButton = false;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this); //stop music from being destroyed every scene
    }
}
