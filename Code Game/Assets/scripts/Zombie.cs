using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour {

    private GameObject player;
    private float speed = 2.0f;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 direction = player.transform.position - gameObject.transform.position;
        direction = direction.normalized;
        direction = direction * speed;
        direction.y = 0.0f;
        gameObject.transform.position += direction * Time.deltaTime;

	}
}
