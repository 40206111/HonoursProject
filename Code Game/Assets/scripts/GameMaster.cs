using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    //array of zombies
    private GameObject[] zombie;
    //timing variables
    private float waitTime = 0.0f;
    private float maxWait = 4.0f;
    private float SpeedTime = 10.0f;
    private float SpeedUp;
    //score
    public static int score = 0;
    private UnityEngine.UI.Text scoreTxt;
    //game state booleans
    public static bool pause = false;
    public static bool reset = false;

    // Use this for initialization
    void Start ()
    {
        zombie = GameObject.FindGameObjectsWithTag("Zombie"); //get zombies
        SpeedUp = SpeedTime; //start speed up time
        //Set initial score
        scoreTxt = GameObject.FindGameObjectWithTag("Score").GetComponent<UnityEngine.UI.Text>();
        scoreTxt.text = "Score: 0";
    }

    //co routine to reset level
    IEnumerator Reset()
    {
        //reset varables to initial values
        waitTime = 0.0f;
        maxWait = 4.0f;
        SpeedUp = SpeedTime;
        score = 0;
        scoreTxt.text = "Score: 0";
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Reset();
        for (int i = 0; i < zombie.Length; ++i)
        {
            zombie[i].GetComponent<Zombie>().Reset();
        }
        yield return new WaitForEndOfFrame(); //to make sure UI elements have been updated before the game starts up
        pause = false;
        reset = false;
    }

    // Update is called once per frame
    void Update ()
    {
        if (pause) //if game paused
        {
            //make sure game will only be reset once
            if (!reset && Input.GetKeyDown(KeyCode.Space))
            {
                reset = true;
            }
            if(reset)
            {
                StartCoroutine(Reset()); //reset game
            }
            return;
        }
        if (waitTime <= 0.0f)
        {
            waitTime = Random.Range(0.5f, maxWait); //set new wait time
            int zInt = Random.Range(0, zombie.Length); //choose random zombie
            for (int i = zInt; i < zInt + zombie.Length; ++i) //starting at random zombie for all zombies
            {
                if (zombie[i % zombie.Length].GetComponent<Zombie>().dead) //if zombie is dead
                {
                    //Bring zombie back to life
                    zombie[i % zombie.Length].GetComponent<Zombie>().dead = false;
                    zombie[i % zombie.Length].transform.position = new Vector3(zombie[i % zombie.Length].transform.position.x, 0.0f, 15.0f);
                    break;
                }
            }
        }
        scoreTxt.text = "Score: " + score; //set score

        if (maxWait >= 1.0f && SpeedUp <= 0.0f) //if time until speed up is 0
        {
            //decrease time between zombies respawning
            maxWait -= 0.5f;
            SpeedUp = SpeedTime;
        }

        //decrease timers
        waitTime -= Time.deltaTime;
        SpeedUp -= Time.deltaTime;
    }
}
