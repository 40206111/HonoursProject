using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getInput : MonoBehaviour
{

    public UnityEngine.UI.Text theText;
    [SerializeField]
    private GameObject line;
    private int textPlace = 0;
    private float xVal = 0;

    // Use this for initialization
    void Start()
    {
        theText = GetComponent<UnityEngine.UI.Text>();
        line.transform.localScale = new Vector2(theText.fontSize / 8, theText.fontSize);
        line.GetComponent<SpriteRenderer>().color = theText.color;
        RectTransform temp = theText.GetComponent<RectTransform>();
        line.transform.localPosition = new Vector2((temp.localPosition.x - temp.rect.width/2) - theText.fontSize / 3, (temp.localPosition.y + temp.rect.height/2) + theText.fontSize / 4);
        xVal = line.transform.localPosition.x;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // has backspace/delete been pressed?
            {
                if (theText.text.Length != 0)
                {
                    char temp = theText.text[theText.text.Length - 1];
                    if ((temp == '\n') || (temp == '\r'))
                    {
                        line.transform.localPosition = new Vector3(xVal, line.transform.localPosition.y + (theText.fontSize * 1.08f), line.transform.localPosition.z);
                        lastLine();
                    }
                    else
                    {
                        StartCoroutine(MoveLineLeft(temp));
                    }
                    theText.text = theText.text.Substring(0, theText.text.Length - 1);
                }
            }
            else if ((c == '\n') || (c == '\r')) // enter/return
            {
                theText.text += '\n';
                line.transform.localPosition = new Vector3(xVal, line.transform.localPosition.y - (theText.fontSize * 1.08f), line.transform.localPosition.z);
            }
            else
            {
                theText.text += c;
                StartCoroutine(MoveLineRight(c));
            }
        }
    }


    private IEnumerator MoveLineRight(char c)
    {
        yield return null;
        CharacterInfo charInfo;
        theText.font.GetCharacterInfo(c, out charInfo);
        line.transform.localPosition = new Vector3(line.transform.localPosition.x + (charInfo.advance * 2.165f), line.transform.localPosition.y, line.transform.localPosition.z);
    }

    private IEnumerator MoveLineLeft(char c)
    {
        yield return null;
        CharacterInfo charInfo;
        theText.font.GetCharacterInfo(c, out charInfo);
        line.transform.localPosition = new Vector3(line.transform.localPosition.x - (charInfo.advance * 2.165f), line.transform.localPosition.y, line.transform.localPosition.z);
    }

    private void lastLine()
    {
        char[] newlines = { '\r', '\n' };
        string[] temp = theText.text.Split(newlines);
        string theOne = temp[temp.Length - 2];
        for (int i = 0; i < theOne.Length; i ++)
        {
            StartCoroutine(MoveLineRight(theOne[i]));
        }
    }
}
