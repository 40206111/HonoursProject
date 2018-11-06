using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    GameObject[] zombie;
    float waitTime = 0.0f;
    int lastZombie = 0;
    float maxWait = 4.0f;
    float SpeedTime = 10.0f;
    float SpeedUp;

    // Use this for initialization
    void Start ()
    {
        zombie = GameObject.FindGameObjectsWithTag("Zombie");
        SpeedUp = SpeedTime;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (waitTime <= 0.0f)
        {
            waitTime = Random.Range(0.5f, maxWait);
            int zInt = Random.Range(0, zombie.Length);
            for (int i = zInt; i < zInt + zombie.Length; ++i)
            {
                if (zombie[i % zombie.Length].GetComponent<Zombie>().dead)
                {
                    zombie[i % zombie.Length].GetComponent<Zombie>().dead = false;
                    zombie[i % zombie.Length].transform.position = new Vector3(zombie[i % zombie.Length].transform.position.x, 0.0f, 15.0f);
                    break;
                }
            }
        }

        if (maxWait >= 1.0f && SpeedUp <= 0.0f)
        {
            maxWait -= 0.5f;
            SpeedUp = SpeedTime;
        }

        waitTime -= Time.deltaTime;
        SpeedUp -= Time.deltaTime;

    }
}
