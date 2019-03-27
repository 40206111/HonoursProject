using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{

    private GameObject player;
    private float speed = 2.0f;
    public bool dead = true;
    private float SpeedTime = 20.0f;
    private float SpeedUp;
    private float hitTime = 3.0f;
    private float hitWait;

    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        SpeedUp = SpeedTime;
        hitWait = 0.0f;
    }

    //Method to reset game
    public void Reset()
    {
        //Set variables back to start point
        speed = 2.0f;
        dead = true;
        SpeedUp = SpeedTime;
        hitWait = hitTime;
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0.0f, -1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMaster.pause) //if game is paused do nothing
        {
            return;
        }
        SpeedUp -= Time.deltaTime; //decrease time until speed up

        if (dead) //if zombie is dead do nothing else
        {
            hitWait = 0.0f;
            return;
        }

        if (gameObject.transform.position.z < player.transform.position.z + 1) // if zombie is near enough to player
        {
            if (hitWait <= 0.0f) //enough time has passed since last hit
            {
                //Hit player
                --player.GetComponent<Player>().health;
                hitWait = hitTime;
            }
            hitWait -= Time.deltaTime; //decrease time until next hit
        }
        if (gameObject.transform.position.z > player.transform.position.z) // if zombie isn't at player
        {
            //Move zombie forward
            Vector3 movement = gameObject.transform.position;
            movement.z -= speed * Time.deltaTime;
            gameObject.transform.position = movement;
        }
        if (speed <= 7.0f && SpeedUp <= 0.0f) // if speed is less than max speed and it is time to speed up
        {
            //increase speed
            speed += 0.5f;
            SpeedUp = SpeedTime;
        }
    }
}
