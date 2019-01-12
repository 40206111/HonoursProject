﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class Controller : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField numbers;
    [SerializeField]
    private TMP_Text theText;
    [SerializeField]
    private TMP_InputField input;
    private static int lines = 1;

    //monospace tag
    const string monostring = "<mspace=1.2em><noparse>";


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        ////fix scrollbar jumping to top
        //}
        CheckContent();
        LineNumbers();
    }

    public void LineNumbers()
    {
        int current = theText.textInfo.lineCount;
        while (lines > current)
        {
            numbers.text = numbers.text.Substring(0, numbers.text.Length - (lines.ToString().Length + 1));
            lines--;
        }
        while (lines < current)
        {
            lines++;
            numbers.text += lines + "\n";
        }
    }

    public void CheckContent()
    {
        //stop from going out of scope
        int amount = 23;
        if (input.text.Length < 23)
        {
            amount = input.text.Length;
        }

        //check if monospace tag has been deleted
        if (input.text.Substring(0, amount) != monostring)
        {
            //remove extra letters from tag
            if (input.text.Length > 22)
            {
                input.text = input.text.Substring(22);
            }
            else
            {
                input.text = "";
            }
            //add tag back
            input.text = monostring + input.text;
            input.caretPosition = 23;
            Canvas.ForceUpdateCanvases();
        }
    }

    public void Compile()
    {
        int currentLine = 0;
        int character = 0;

        ////
        ///Break text by ;
        List<string> code = theText.text.Split(';').ToList();
        ///until not usings
        ///Add usings to list
        ///for all commands after usings
        ///incriment by 1 every new line and every 50 (dependednt on point size -> 1120/pointsize) characters (counting line numbers)
        ///If line does not equal using check for class (if not output error on current line)
        ///check for open bracket (increment bracket count)
        ///if in class while bracketcount = 1 start class check
        ///if bracket count is zero check for class or end of file
        ///if error break loops and continue with error help
    }

    private bool ClassCheck(int lineNo)
    {
        ///if close bracket decrement bracket count
        ///check for variable store variables
        ///check for method store methods
        ///check for open bracket (increment bracket count)
        ///if in method while bracket count = 2 start method check
        ///if error return false
        return false;
    }

    private bool MethodCheck(int lineNo)
    {
        //method class or struct that contains variables
        //check for variables and store within method
        //if error retur false
        return false;
    }

    public void Run()
    {
        ////
        ///run start method once
        ///run update method until stopped pressed (multithread?)
        ///
    }
}
