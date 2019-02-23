using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GM : MonoBehaviour
{
    public static TMP_InputField console;
    public static GameObject player;
    public static GameObject[] zombie;
    public static List<Vector3> zombieStart = new List<Vector3>();
    public static Button cnr;
    public static Button run;
    public static Button stop;

    // Start is called before the first frame update
    void Start()
    {
        console = GameObject.FindGameObjectWithTag("console").GetComponent<TMP_InputField>();
        player = GameObject.FindGameObjectWithTag("Player");
        zombie = GameObject.FindGameObjectsWithTag("Zombie");
        cnr = GameObject.FindGameObjectWithTag("cnr").GetComponent<Button>();
        run = GameObject.FindGameObjectWithTag("run").GetComponent<Button>();
        stop = GameObject.FindGameObjectWithTag("stop").GetComponent<Button>();
        zombieStart.Add(zombie[0].transform.position);
        zombieStart.Add(zombie[1].transform.position);
        zombieStart.Add(zombie[2].transform.position);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Stop()
    {
        for (int i = 0; i < 3; ++i)
        {
            GM.zombie[i].transform.position = GM.zombieStart[i];
        }
        Controller.stop = true;
        stop.interactable = false;
        run.interactable = true;
        cnr.interactable = true;
    }
}
