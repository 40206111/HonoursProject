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


    //Method to reset the player
    public void Reset()
    {
        //reset variables back to their initial values
        health = 3;
        healthTxt.text = "Health: 0";
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMaster.pause) //if game is paused do nothing
        {
            return;
        }

        //Display players health
        healthTxt.text = "Health: " + health;
        if (health <= 0) // if player is dead pause game
        {
            GameMaster.pause = true;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) //user presed right arrow key
        {
            for (int i = 0; i < zombie.Length; ++i) //for all zombies
            {
                if (zombie[i].transform.position.x > 0 && zombie[i].transform.position.z <= gameObject.transform.position.z + 3) //if zombie is close enough && to the right
                {
                    //hit zombie
                    zombie[i].GetComponent<Zombie>().dead = true;
                    zombie[i].transform.position = new Vector3(zombie[i].transform.position.x, 0.0f, -1.0f);
                    GameMaster.score += 1;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) //user pressed up arrow key
        {
            for (int i = 0; i < zombie.Length; ++i) //for all zombies
            {
                if (zombie[i].transform.position.x == 0 && zombie[i].transform.position.z <= gameObject.transform.position.z + 3) //if zombie is close enough and above
                {
                    //hit zombie
                    zombie[i].GetComponent<Zombie>().dead = true;
                    zombie[i].transform.position = new Vector3(zombie[i].transform.position.x, 0.0f, -1.0f);
                    GameMaster.score += 1;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) //user pressed left arrow key
        {
            for (int i = 0; i < zombie.Length; ++i) //for all zombies
            {
                if (zombie[i].transform.position.x < 0 && zombie[i].transform.position.z <= gameObject.transform.position.z + 3) //if zombe is close enough and to the left
                {
                    //hit zombie
                    zombie[i].GetComponent<Zombie>().dead = true;
                    zombie[i].transform.position = new Vector3(zombie[i].transform.position.x, 0.0f, -1.0f);
                    GameMaster.score += 1;
                }
            }
        }
    }
}
