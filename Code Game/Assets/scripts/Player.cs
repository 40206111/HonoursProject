using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    GameObject[] zombie;

    // Use this for initialization
    void Start ()
    {
        zombie = GameObject.FindGameObjectsWithTag("Zombie");
    }

    // Update is called once per frame
    void Update ()
    {
		if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            for (int i = 0; i < 3; ++i)
            {
                if (zombie[i].transform.position.x > 0 && zombie[i].transform.position.z <= gameObject.transform.position.z + 5)
                {
                    zombie[i].GetComponent<Zombie>().dead = true;
                    zombie[i].transform.position = new Vector3(zombie[i].transform.position.x, 0.0f, -1.0f);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            for (int i = 0; i < 3; ++i)
            {
                if (zombie[i].transform.position.x == 0 && zombie[i].transform.position.z <= gameObject.transform.position.z + 5)
                {
                    zombie[i].GetComponent<Zombie>().dead = true;
                    zombie[i].transform.position = new Vector3(zombie[i].transform.position.x, 0.0f, -1.0f);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            for (int i = 0; i < 3; ++i)
            {
                if (zombie[i].transform.position.x < 0 && zombie[i].transform.position.z <= gameObject.transform.position.z + 5)
                {
                    zombie[i].GetComponent<Zombie>().dead = true;
                    zombie[i].transform.position = new Vector3(zombie[i].transform.position.x, 0.0f, -1.0f);
                }
            }
        }
    }
}
