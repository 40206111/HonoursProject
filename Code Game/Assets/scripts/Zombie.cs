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

    public void Reset()
    {
        speed = 2.0f;
        dead = true;
        SpeedUp = SpeedTime;
        hitWait = hitTime;
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0.0f, -1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMaster.pause)
        {
            return;
        }
        SpeedUp -= Time.deltaTime;

        if (dead)
        {
            hitWait = 0.0f;
            return;
        }

        if (gameObject.transform.position.z < player.transform.position.z + 1)
        {
            if (hitWait <= 0.0f)
            {
                --player.GetComponent<Player>().health;
                hitWait = hitTime;
            }
            hitWait -= Time.deltaTime;
        }
        if (gameObject.transform.position.z > player.transform.position.z)
        {
            Vector3 movement = gameObject.transform.position;
            movement.z -= speed * Time.deltaTime;
            gameObject.transform.position = movement;
        }
        if (speed <= 7.0f && SpeedUp <= 0.0f)
        {
            speed += 0.5f;
            SpeedUp = SpeedTime;
        }
    }
}
