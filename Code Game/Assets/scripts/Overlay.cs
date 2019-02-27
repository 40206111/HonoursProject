using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Overlay : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> ava;
    [SerializeField]
    private List<GameObject> larry;
    [SerializeField]
    private List<GameObject> gw;
    private GameObject curEnabled;

    [SerializeField]
    private TMP_Text nameplate;
    [SerializeField]
    private TMP_Text content;

    public string dialogueFile = "";

    [SerializeField]
    private bool onLoad = true;

    private bool expect = false;
    private int count = 0;
    private string[] lines;
    private bool skip = false;
    private bool active = false;


    // Start is called before the first frame update
    void Start()
    {
        if (onLoad) Read();
        else gameObject.SetActive(false);
    }

    public void Read()
    {
        gameObject.SetActive(true);
        string dialogue = System.IO.File.ReadAllText("Assets/Dialogue/" + dialogueFile + ".txt");
        lines = dialogue.Split('\n');
        content.text = "";
        StartCoroutine(WaitToActivate());
        StartCoroutine(HelpRead(lines[count]));
    }

    IEnumerator WaitToActivate()
    {
        yield return new WaitForEndOfFrame();
        active = true;
    }

    private void Update()
    {
        if (expect && Input.anyKeyDown)
        {
            if (count > lines.Length - 1)
            {
                count = 0;
                active = false;
                gameObject.SetActive(false);
                expect = false;
            }
            else
            {
                content.text = "";
                expect = false;
                StartCoroutine(HelpRead(lines[count]));
            }
        }

        else if (active && Input.anyKeyDown)
        {
            skip = true;
        }
    }

    IEnumerator HelpRead(string line)
    {
        bool linestart = true;
        string word = "";
        while (line != "" && !skip)
        {
            char c = line[0];
            line = line.Substring(1, line.Length - 1);

            if (linestart)
            {
                if (c == ':')
                {
                    if (curEnabled != null) curEnabled.SetActive(false);
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
                    curEnabled.SetActive(true);
                    linestart = false;
                }
                else if (c == ' ')
                {
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
                content.text += c;
                yield return new WaitForSeconds(0.05f);
            }

        }

        if (skip)
        {
            skip = false;
            content.text += line;
        }

        count++;
        expect = true;
        if (count < lines.Length)
        {
            active = false;
            StartCoroutine(WaitToActivate());
            expect = true;
        }
    }
}
