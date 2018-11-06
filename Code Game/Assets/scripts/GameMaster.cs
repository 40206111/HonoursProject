using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    private GameObject[] zombie;
    private float waitTime = 0.0f;
    private float maxWait = 4.0f;
    private float SpeedTime = 10.0f;
    private float SpeedUp;
    public static int score = 0;
    public static bool pause = false;
    private UnityEngine.UI.Text scoreTxt;
    public static bool reset = false;

    // Use this for initialization
    void Start ()
    {
        zombie = GameObject.FindGameObjectsWithTag("Zombie");
        SpeedUp = SpeedTime;
        scoreTxt = GameObject.FindGameObjectWithTag("Score").GetComponent<UnityEngine.UI.Text>();
        scoreTxt.text = "Score: 0";

    }

    IEnumerator Reset()
    {
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
        yield return new WaitForEndOfFrame();
        pause = false;
        reset = false;
    }

    // Update is called once per frame
    void Update ()
    {
        if (pause)
        {
            if (!reset && Input.GetKeyDown(KeyCode.Space))
            {
                reset = true;
            }
            if(reset)
            {
                StartCoroutine(Reset());
            }
            return;
        }
        scoreTxt.text = "Score: " + score;
        if (waitTime <= 0.0f)
        {
            waitTime = Random.Range(0.5f, maxWait);
            int zInt = Random.Range(0, zombie.Length);
            for (int i = zInt; i < zInt + zombie.Length; ++i)
            {
                if (zombie[i % zombie.Length].GetComponent<Zombie>().dead)
                {
                    zombie[i % zombie.Length].GetComponent<Zombie>().dead = false;
                    zombie[i % zombie.Length].transform.position = new Vector3(zombie[i % zombie.Length].transform.position.x, 0.0f, 15.0f);
                    break;
                }
            }
        }

        if (maxWait >= 1.0f && SpeedUp <= 0.0f)
        {
            maxWait -= 0.5f;
            SpeedUp = SpeedTime;
        }

        waitTime -= Time.deltaTime;
        SpeedUp -= Time.deltaTime;

    }
}
