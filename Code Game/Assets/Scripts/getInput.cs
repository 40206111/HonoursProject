using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getInput : MonoBehaviour
{

    public UnityEngine.UI.Text theText;
    [SerializeField]
    private GameObject line;
    private int textPlace = 0;

    // Use this for initialization
    void Start()
    {
        theText = GetComponent<UnityEngine.UI.Text>();
        line.transform.localScale = new Vector2(theText.fontSize / 8, theText.fontSize);
        line.GetComponent<SpriteRenderer>().color = theText.color;
        RectTransform temp = theText.GetComponent<RectTransform>();
        line.transform.localPosition = new Vector2((temp.localPosition.x - temp.rect.width/2) - theText.fontSize / 3, (temp.localPosition.y + temp.rect.height/2) + theText.fontSize / 4);
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
                    theText.text = theText.text.Substring(0, theText.text.Length - 1);
                }
            }
            else if ((c == '\n') || (c == '\r')) // enter/return
            {
                theText.text += '\n';
            }
            else
            {
                theText.text += c;
            }
        }
    }
}
