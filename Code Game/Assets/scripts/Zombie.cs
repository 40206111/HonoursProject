using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour {

    private GameObject player;
    [SerializeField]
    private float speed = 2.0f;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 direction = player.transform.localPosition - gameObject.transform.localPosition;
        direction = direction.normalized;
        gameObject.transform.localPosition += direction * speed * Time.deltaTime;

	}
}
