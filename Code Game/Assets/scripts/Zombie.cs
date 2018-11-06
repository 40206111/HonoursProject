using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour {

    private GameObject player;
    private float speed = 2.0f;
    public bool dead = true;

	// Use this for initialization
	void Start ()
    {
        player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update ()
    { 
        if (!dead && gameObject.transform.position.z > player.transform.position.z + 2)
        {
            Vector3 movement = gameObject.transform.position;
            movement.z -= speed * Time.deltaTime;
            gameObject.transform.position = movement;
        }
    }
}
