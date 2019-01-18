using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class Controller : MonoBehaviour
{
    [SerializeField]
    private TMP_Text numbers;
    [SerializeField]
    private TMP_Text theText;
    [SerializeField]
    private TMP_InputField input;
    [SerializeField] UnityEngine.UI.Scrollbar scroll;
    private static int lines = 1;
    private static float number_center;

    //Code Storage Variables//
    List<string> allUsing = new List<string>();

    //monospace tag
    const string monostring = "<mspace=1.2em><noparse>";


    // Start is called before the first frame update
    void Start()
    {
        number_center = numbers.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        CheckContent();
        LineNumbers();
    }

    public void ScrollNumbers()
    {
        Vector3 pos = numbers.transform.position;
        float test = numbers.textBounds.extents.y;

        Debug.Log(test);
        pos.y = number_center + scroll.value * test;

        numbers.transform.position = pos;
    }

    private void LineNumbers()
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

    private void CheckContent()
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
        Debug.Log("Compile Button Pressed");
        int currentLine = 1;
        allUsing.Clear();

        Error error = TryCompile(ref currentLine);

        Debug.Log(error);
    }

    private Error TryCompile(ref int lineNo)
    {
        int character = 0;

        //Break text by ;
        List<string> code = theText.text.Split(';').ToList();

        code[0] = code[0].Substring(monostring.Length, code[0].Length - monostring.Length);

        //until not usings
        while (code.Count > 0)
        {
            string usings = "using ";
            int usingCount = 0;

            if (code[0] == "")
            {
                return new Error(Error.ErrorCodes.Syntax, "Expected instruction before semicolon", lineNo);
            }

            //Add usings to list
            foreach (char c in code[0])
            {
                if (c == 8203)
                {
                    character++;
                    continue;
                }

                if (c == '\n')
                {
                    lineNo++;
                    character = 0;
                }
                else if (character >= 1)
                {
                    character = 0;
                    lineNo++;
                }
                //check if found using key word
                if (usingCount == usings.Length)
                {
                    //check that using is valid
                    if ((c >= 0 && c <= 31) ||  //non printable characters
                        (c >= 20 && c <= 45) || //  !"#$%'()*+,-
                        c == 47 ||              // /
                        (c >= 58 && c <= 64) ||   // :;<=>?@
                        (c >= 91 && c <= 96) || // [\]^_`
                        (c >= 123 && c <= 127))    // {|}~
                    {
                        //return error
                        return new Error(Error.ErrorCodes.Syntax, "invalid characters in using declaration", lineNo);
                    }
                    //add character to list of usings
                    allUsing[allUsing.Count() - 1] += c;
                }
                else if (c == usings[usingCount]) // character in usings key word
                {
                    //increment using count
                    usingCount++;
                    if (usingCount == usings.Length)
                    {
                        if (code.Count == 1) return new Error(Error.ErrorCodes.Syntax, "Expected semi colon", lineNo);
                        //add new using to list
                        allUsing.Add("");
                    }
                }
                else if (!(usingCount == 0 && char.IsWhiteSpace(c)))
                {
                    break;
                }
            }

            //using keyword not complete
            if (usingCount > 0 && usingCount < usings.Length - 1) return new Error(Error.ErrorCodes.Syntax, "expected keyword \"using\"", lineNo);
            //no package given after using keyword
            else if (usingCount == usings.Length - 1) return new Error(Error.ErrorCodes.Syntax, "name of package should be given after \"using\" keyword", lineNo);
            else if (allUsing.Count != 0)
            {
                if ((allUsing[allUsing.Count() - 1] == "")) return new Error(Error.ErrorCodes.Syntax, "name of package should be given after \"using\" keyword", lineNo);
            }

            //remove used line
            code.RemoveAt(0);
        }

        //for all commands after usings
        for (int i = 0; i < code.Count; ++i)
        {
            //incriment by 1 every new line and every character = 8203 (counting line numbers)
            foreach (char c in code[0])
            {
                if (c == 8203)
                {
                    character++;
                    continue;
                }

                if (c == '\n')
                {
                    lineNo++;
                    character = 0;
                }
                else if (character >= 1)
                {
                    character = 0;
                    lineNo++;
                }

                ///check for class if not already in class (if not output error on current line)
                ///check for open bracket (increment bracket count)
                ///if in class while bracketcount = 1 start class check
                ///if bracket count is zero check for class or end of file
                ///if error break loops and continue with error help
            }
        }

        return new Error();
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
