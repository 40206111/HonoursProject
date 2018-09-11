using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getInput : MonoBehaviour {

    public UnityEngine.UI.Text theText;

    // Use this for initialization
    void Start () {
        theText = GetComponent<UnityEngine.UI.Text>();
    }
	
	// Update is called once per frame
	void Update () {
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
