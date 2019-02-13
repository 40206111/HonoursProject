using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GM : MonoBehaviour
{
    public static TMP_InputField console;
    public static GameObject player;
    public static GameObject[] zombie;

    // Start is called before the first frame update
    void Start()
    {
        console = GameObject.FindGameObjectWithTag("console").GetComponent<TMP_InputField>();
        player = GameObject.FindGameObjectWithTag("Player");
        zombie = GameObject.FindGameObjectsWithTag("Zombie");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
