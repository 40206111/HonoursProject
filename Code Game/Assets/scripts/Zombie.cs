using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour {

    private GameObject player;
    private float speed = 2.0f;
    public bool dead = true;
    private float SpeedTime = 20.0f;
    private float SpeedUp;

	// Use this for initialization
	void Start ()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        SpeedUp = SpeedTime;
	}

    // Update is called once per frame
    void Update()
    {
        if (!dead && gameObject.transform.position.z > player.transform.position.z)
        {
            Vector3 movement = gameObject.transform.position;
            movement.z -= speed * Time.deltaTime;
            gameObject.transform.position = movement;
        }
        if (speed <= 5.0f && SpeedUp <= 0.0f)
        {
            speed += 0.5f;
            SpeedUp = SpeedTime;
        }
        SpeedUp -= Time.deltaTime;
    }
}
