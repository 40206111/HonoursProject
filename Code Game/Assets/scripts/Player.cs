using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    private GameObject[] zombie;
    public int health = 3;
    private UnityEngine.UI.Text healthTxt;

    // Use this for initialization
    void Start()
    {
        zombie = GameObject.FindGameObjectsWithTag("Zombie");
        healthTxt = GameObject.FindGameObjectWithTag("Health").GetComponent<UnityEngine.UI.Text>();
        healthTxt.text = "Health: 0";
    }

    public void Reset()
    {
        Debug.Log("test");
        health = 3;
        healthTxt.text = "Health: 0";
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMaster.pause)
        {
            return;
        }
        healthTxt.text = "Health: " + health;
        if (health <= 0)
        {
            GameMaster.pause = true;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            for (int i = 0; i < 3; ++i)
            {
                if (zombie[i].transform.position.x > 0 && zombie[i].transform.position.z <= gameObject.transform.position.z + 3)
                {
                    zombie[i].GetComponent<Zombie>().dead = true;
                    zombie[i].transform.position = new Vector3(zombie[i].transform.position.x, 0.0f, -1.0f);
                    GameMaster.score += 1;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            for (int i = 0; i < 3; ++i)
            {
                if (zombie[i].transform.position.x == 0 && zombie[i].transform.position.z <= gameObject.transform.position.z + 3)
                {
                    zombie[i].GetComponent<Zombie>().dead = true;
                    zombie[i].transform.position = new Vector3(zombie[i].transform.position.x, 0.0f, -1.0f);
                    GameMaster.score += 1;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            for (int i = 0; i < 3; ++i)
            {
                if (zombie[i].transform.position.x < 0 && zombie[i].transform.position.z <= gameObject.transform.position.z + 3)
                {
                    zombie[i].GetComponent<Zombie>().dead = true;
                    zombie[i].transform.position = new Vector3(zombie[i].transform.position.x, 0.0f, -1.0f);
                    GameMaster.score += 1;
                }
            }
        }
    }
}
