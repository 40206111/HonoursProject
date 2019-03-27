using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GM : MonoBehaviour
{
    //UI variables
    public static TMP_InputField console;
    
    //Gameobject variables
    public static GameObject player;
    public static GameObject[] zombie;

    //List of zombies start positions
    public static List<Vector3> zombieStart = new List<Vector3>();

    //Button variables
    public static Button cnr;
    public static Button stop;

    // Start is called before the first frame update
    void Start()
    {
        //Get gameobjects from scene
        console = GameObject.FindGameObjectWithTag("console").GetComponent<TMP_InputField>();
        player = GameObject.FindGameObjectWithTag("Player");
        zombie = GameObject.FindGameObjectsWithTag("Zombie");
        cnr = GameObject.FindGameObjectWithTag("cnr").GetComponent<Button>();
        stop = GameObject.FindGameObjectWithTag("stop").GetComponent<Button>();
        //Add zombie start positions to list
        zombieStart.Add(zombie[0].transform.position);
        zombieStart.Add(zombie[1].transform.position);
        zombieStart.Add(zombie[2].transform.position);
    }

    // Update is called once per frame
    void Update()
    {
    }

    //Method for stopping in game program from running
    public void Stop()
    {
        //reset zombies
        for (int i = 0; i < 3; ++i)
        {
            GM.zombie[i].transform.position = GM.zombieStart[i];
        }
        //set button interactability
        Controller.stop = true;
        stop.interactable = false;
        cnr.interactable = true;
    }
}
