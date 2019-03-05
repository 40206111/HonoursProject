using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Overlay : MonoBehaviour
{
    //store character emotions
    [SerializeField]
    private List<GameObject> ava;
    [SerializeField]
    private List<GameObject> larry;
    [SerializeField]
    private List<GameObject> gw;
    //to store current character
    private GameObject curEnabled;

    //store name plate
    [SerializeField]
    private TMP_Text nameplate;
    //store dialogue content box
    [SerializeField]
    private TMP_Text content;

    //store dialogue file to open
    public string dialogueFile = "";

    //bool to decide if dialogue will start on scene load
    [SerializeField]
    private bool onLoad = true;

    //bool for when there is an expected key press
    private bool expect = false;
    private int count = 0;  //current line
    private string[] lines; //array of dialogue lines
    private bool skip = false;  //if user has skipped dialougue writting out
    private bool active = false;   //if dialogue box is currently writing out dialogue


    // Start is called before the first frame update
    void Start()
    {
        if (onLoad) Read(); //read text
        else gameObject.SetActive(false);   //deactivate gameobject
    }

    //Method to read in dialogue
    public void Read()
    {
        //activate gameobject
        gameObject.SetActive(true);
        //read in dialogue from file
        string dialogue = System.IO.File.ReadAllText("Assets/Dialogue/" + dialogueFile + ".txt");
        lines = dialogue.Split('\n');   //split on newline
        content.text = ""; // remove text from content box
        StartCoroutine(WaitToActivate());   //set diagloge box to active
        StartCoroutine(HelpRead(lines[count])); //Read line 0
    }

    //Method to stop user being able to skip to quickly over the dialogue
    IEnumerator WaitToActivate()
    {
        yield return new WaitForEndOfFrame();
        active = true;
    }

    private void Update()
    {
        //If expecting a key press and there is a key press
        if (expect && Input.anyKeyDown)
        {
            //If there isn't any more dialogue left
            if (count > lines.Length - 1)
            {
                //Reset values
                count = 0;
                active = false;
                gameObject.SetActive(false);
                expect = false;
            }
            else
            {
                content.text = ""; //empty text box
                expect = false;
                StartCoroutine(HelpRead(lines[count])); //Read next line
            }
        }
        //if textbox is currently being written to and there is a key press
        else if (active && Input.anyKeyDown)
        {
            skip = true;    //Skip dialogue
        }
    }

    //Method to read a line
    IEnumerator HelpRead(string line)
    {
        bool linestart = true;
        string word = "";
        //while there is still content left to write
        while (line != "" && !skip)
        {
            char c = line[0];   //get first character
            line = line.Substring(1, line.Length - 1); //remove first character

            //if the actual content hasn't started yet
            if (linestart)
            {
                if (c == ':')
                {
                    //CHANGE CHARACTER
                    if (curEnabled != null) curEnabled.SetActive(false);    //deactivate active character
                    //Set New character
                    if (word == "Ava")
                    {
                        nameplate.text = "Ava";
                        curEnabled = ava[0];
                    }
                    if (word == "AvaWet")
                    {
                        nameplate.text = "Ava";
                        curEnabled = ava[1];
                    }
                    if (word == "Larry")
                    {
                        nameplate.text = "Larry";
                        curEnabled = larry[0];
                    }
                    if (word == "GW")
                    {
                        nameplate.text = "Glitch Witch";
                        curEnabled = gw[0];
                    }
                    if (word == "GWCat")
                    {
                        nameplate.text = "Glitch Witch";
                        curEnabled = gw[1];
                    }
                    if (word == "GWEr")
                    {
                        nameplate.text = "Glitch Witch";
                        curEnabled = gw[2];
                    }
                    if (word == "GWHa")
                    {
                        nameplate.text = "Glitch Witch";
                        curEnabled = gw[3];
                    }
                    curEnabled.SetActive(true); //activate new character
                    linestart = false;  //set linestart to false
                }
                else if (c == ' ')
                {
                    //character has not changed, continue
                    linestart = false;
                    line = word + " " + line;
                }
                else
                {
                    word += c;
                }
            }
            else
            {
                content.text += c; //add character to content box
                yield return new WaitForSeconds(0.05f); //wait before adding next character
            }

        }

        //if text is skipped
        if (skip)
        {
            skip = false;
            content.text += line;   //put rest of line into content box
        }

        count++;    //increase line count
        expect = true;  //expect key press
        //if there are still lines left
        if (count < lines.Length)
        {
            active = false;
            StartCoroutine(WaitToActivate());
            expect = true;
        }
    }
}
