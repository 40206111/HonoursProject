using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Overlay : MonoBehaviour
{
    //Character assets
    [SerializeField]
    private List<GameObject> ava;
    [SerializeField]
    private List<GameObject> larry;
    [SerializeField]
    private List<GameObject> gw;
    //current enabled character asset
    private GameObject curEnabled;

    //Dialogue box data
    [SerializeField]
    private TMP_Text nameplate;
    [SerializeField]
    private TMP_Text content;

    //file to read dialgue from
    public string dialogueFile = "";
    //string to read dialogue into
    string dialogue;

    //Decide of overlay should run on load
    [SerializeField]
    private bool onLoad = true;

    //variables for helping to write dialogue
    private bool expect = false;
    private int count = 0;
    private string[] lines;
    private bool skip = false;
    private bool active = false;


    // Start is called before the first frame update
    void Start()
    {
        //begin if to be read on load
        if (onLoad) Read();
        else gameObject.SetActive(false);
    }

    //string that the file path will be put into
    public string filePath;
    IEnumerator ReadFile()
    {
        //find file path
        filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Dialogue");
        filePath = System.IO.Path.Combine(filePath, dialogueFile + ".txt");
        if (filePath.Contains("://")) //if on web
        {
            WWW www = new WWW(filePath);
            yield return www;
            dialogue = www.text;
        }
        else
            dialogue = System.IO.File.ReadAllText(filePath);

        //split lines of dialogue
        lines = dialogue.Split('\n');
        //empty dialogue
        content.text = "";

        //begin dialogue read
        StartCoroutine(WaitToActivate());
        StartCoroutine(HelpRead(lines[count]));
    }

    //Method to begin dialogue read
    public void Read()
    {
        gameObject.SetActive(true);
        StartCoroutine(ReadFile());
    }

    //Method to stop user from being able to skip through dialogue too fast
    IEnumerator WaitToActivate()
    {
        yield return new WaitForSeconds(1);
        active = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (expect && Input.anyKeyDown) //if waiting for button press and button pressed
        {
            if (count > lines.Length - 1) // if there are no lines left
            {
                //deactivate overlay
                count = 0;
                active = false;
                gameObject.SetActive(false);
                expect = false;
            }
            else
            {
                //Start reading next line
                content.text = "";
                expect = false;
                StartCoroutine(HelpRead(lines[count]));
            }
        }
        else if (active && Input.anyKeyDown) //if currently in dialogue and button pressed
        {
            skip = true;
        }
    }

    //Method to read a line
    IEnumerator HelpRead(string line)
    {
        bool linestart = true;
        string word = "";

        //while there is still more characters in line and line has not been skipped
        while (line != "" && !skip)
        {
            //remove character
            char c = line[0];
            line = line.Substring(1, line.Length - 1);

            if (linestart) // if this is the first word in the line
            {
                if (c == ':') // if character name is given
                {
                    if (curEnabled != null) curEnabled.SetActive(false);
                    //Set new Character based on word given
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
                    //enable new sprite
                    curEnabled.SetActive(true);
                    linestart = false;
                }
                else if (c == ' ') //new character name not given
                {
                    linestart = false;
                    //add word back to line
                    line = word + " " + line;
                }
                else
                {
                    word += c; //add character to word
                }
            }
            else
            {
                //display characters one at a time
                content.text += c;
                yield return new WaitForSeconds(0.05f);
            }

        }

        if (skip)
        {
            //skip to end of text
            skip = false;
            content.text += line;
        }

        count++; //increase line count
        expect = true; //expect button press
        if (count < lines.Length) //if there are more lines
        {
            active = false;
            StartCoroutine(WaitToActivate());
        }
    }
}
