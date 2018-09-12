using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getInput : MonoBehaviour
{

    private UnityEngine.UI.Text theText;
    [SerializeField]
    private GameObject line;
    private int textPlace = 0;
    private float xVal = 0;

    // Use this for initialization
    void Start()
    {
        //get text
        theText = GetComponent<UnityEngine.UI.Text>();
        //resize line to match text
        line.transform.localScale = new Vector2(theText.fontSize / 8, theText.fontSize);
        //change colour of line to match text
        line.GetComponent<SpriteRenderer>().color = theText.color;
        //Place line in correct place
        RectTransform temp = theText.GetComponent<RectTransform>();
        line.transform.localPosition = new Vector2((temp.localPosition.x - temp.rect.width/2) - theText.fontSize / 3, (temp.localPosition.y + temp.rect.height/2) + theText.fontSize / 4);
        //store x value
        xVal = line.transform.localPosition.x;
    }

    // Update is called once per frame
    void Update()
    {
        // loop through characters inputed
        foreach (char c in Input.inputString)
        {
            // check for backspace
            if (c == '\b')
            {
                if (theText.text.Length != 0)
                {
                    char temp = theText.text[theText.text.Length - 1];
                    if ((temp == '\n') || (temp == '\r'))
                    {
                        //move line up to right positon
                        line.transform.localPosition = new Vector3(xVal, line.transform.localPosition.y + (theText.fontSize * 1.08f), line.transform.localPosition.z);
                        lastLine();
                    }
                    else
                    {
                        //wait frame to move line left to ensure correct values
                        StartCoroutine(MoveLineLeft(temp));
                    }
                    //remove last character
                    theText.text = theText.text.Substring(0, theText.text.Length - 1);
                }
            }
            //check for new line
            else if ((c == '\n') || (c == '\r'))
            {
                theText.text += '\n';
                line.transform.localPosition = new Vector3(xVal, line.transform.localPosition.y - (theText.fontSize * 1.08f), line.transform.localPosition.z);
            }
            //add character
            else
            {
                theText.text += c;
                //wait for frame before movng line to ensure correct values
                StartCoroutine(MoveLineRight(c));
            }
        }
    }

    //Coroutine to move flashing line right
    private IEnumerator MoveLineRight(char c)
    {
        yield return null;
        CharacterInfo charInfo;
        theText.font.GetCharacterInfo(c, out charInfo);
        line.transform.localPosition = new Vector3(line.transform.localPosition.x + (charInfo.advance * 2.165f), line.transform.localPosition.y, line.transform.localPosition.z);
    }

    //Coroutine to move flashing line left
    private IEnumerator MoveLineLeft(char c)
    {
        yield return null;
        CharacterInfo charInfo;
        theText.font.GetCharacterInfo(c, out charInfo);
        line.transform.localPosition = new Vector3(line.transform.localPosition.x - (charInfo.advance * 2.165f), line.transform.localPosition.y, line.transform.localPosition.z);
    }

    //Method to move flashing line's x position to correct place
    private void lastLine()
    {
        char[] newlines = { '\r', '\n' };
        string[] temp = theText.text.Split(newlines);
        string theOne = temp[temp.Length - 2];
        //move right for ever character on last line
        for (int i = 0; i < theOne.Length; i ++)
        {
            StartCoroutine(MoveLineRight(theOne[i]));
        }
    }
}
