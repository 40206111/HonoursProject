using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    GameObject[] zombie;
    float waitTime = 0.0f;
    int lastZombie = 0;

    // Use this for initialization
    void Start ()
    {
        zombie = GameObject.FindGameObjectsWithTag("Zombie");
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (waitTime <= 0.0f)
        {
            waitTime = Random.Range(1.0f, 4.0f);
            for (int i = 0; i < zombie.Length; ++i)
            {
                if (zombie[i].GetComponent<Zombie>().dead)
                {
                    zombie[i].GetComponent<Zombie>().dead = false;
                    zombie[i].transform.position = new Vector3(zombie[i].transform.position.x, 0.0f, 15.0f);
                    break;
                }
            }
        }

        waitTime -= Time.deltaTime;

    }
}
