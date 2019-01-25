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
    IDictionary<string, string> strings = new Dictionary<string, string>();

    //monospace tag
    const string monostring = "<mspace=1.2em><noparse>";

    string stringset = "";
    string type = "";

    //keywords
    const string usings = "using ";
    const string pub = "public";
    const string aClass = "class";
    const string unityClass = "MonoBehaviour";
    const string theVoid = "void";
    const string start = "Start()";
    const string update = "Update()";


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

        //Clear Global Values
        allUsing.Clear();
        strings.Clear();
        stringset = "";
        type = "";

        Error error = TryCompile(ref currentLine);

        Debug.Log(error);
    }

    private Error TryCompile(ref int lineNo)
    {
        int character = 0;
        bool end = false;

        //Break text by ;
        List<string> code = theText.text.Split(';').ToList();

        code[0] = code[0].Substring(monostring.Length, code[0].Length - monostring.Length);

        //until not usings
        while (code.Count > 0)
        {
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
                    end = true;
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

            if (end)
            {
                break;
            }
            //remove used line
            code.RemoveAt(0);
        }

        /////////////////////////AFTER USINGS/////////////////////////////////////

        int bracCount = 0; // bracket count
        string word = "";
        string prevWord = "";

        bool instring = false;
        bool expectCommand = false;
        bool ignore = false;
        bool superIgnore = false;

        //for all commands after usings
        for (int i = 0; i < code.Count; ++i)
        {

            if (code[i] == "")
            {
                return new Error(Error.ErrorCodes.Syntax, "Expected instruction before semicolon", lineNo);
            }
            code[i] += " ";

            //incriment by 1 every new line and every character = 8203 (counting line numbers)
            foreach (char c in code[i])
            {
                if (c == 8203)
                {
                    character++;
                    continue;
                }

                if (c == '\n')
                {
                    ignore = false;
                    if (instring)
                    {
                        return new Error(Error.ErrorCodes.Syntax, "Newline found in middle of string", lineNo);
                    }
                    lineNo++;
                    character = 0;
                }
                else if (character >= 1)
                {
                    character = 0;
                    lineNo++;
                }

                if (superIgnore && c=='*')
                {
                    expectCommand = true;
                    continue;
                }
                else if (superIgnore && expectCommand)
                {
                    expectCommand = false;
                    if (c == '/')
                    {
                        superIgnore = false;
                    }
                    continue;
                }

                if (expectCommand && !instring)
                {
                    expectCommand = false;

                    if (c == '/')
                    {
                        ignore = true;
                        continue;
                    }
                    else if (c == '*')
                    {
                        superIgnore = true;
                        continue;
                    }
                    else
                    {
                        word += "/";
                    }
                }

                if (ignore || superIgnore)
                {
                    continue;
                }
                else if (c == '/' && word == "" && !instring)
                {
                    expectCommand = true;
                    continue;
                }
                else if (c == '"' && !(instring && expectCommand))
                {
                    instring = !instring;
                    continue;
                }
                else if (c == '}')
                {
                    bracCount--;
                    word = "";
                    prevWord = "";
                    continue;
                }
                else if (word == unityClass && c == '\n')
                {
                    continue;
                }
                else if (c == '{')
                {
                    bracCount++;

                    prevWord = "{";
                    word = "";

                    if (bracCount == 0 && !((word == unityClass && prevWord == ":") || prevWord == unityClass))
                    {
                        return new Error(Error.ErrorCodes.InGame, "Expected derevition from monobehaviour", lineNo);
                    }
                }
                else if (c == ':' && (prevWord == "Zombie" || prevWord == "GameMaster" || prevWord == "Player"))
                {
                    prevWord = ":";
                    word = "";
                    continue;
                }
                else if (!instring && word.Length > 1 && ((c >= 0 && c <= 31) ||  //non printable characters
                        (c >= 33 && c <= 47) || //  !"#$%'()*+,-./
                        (c >= 58 && c <= 64) ||   // :;<=>?@
                        (c >= 91 && c <= 96) || // [\]^_`
                        (c >= 123 && c <= 127)))   // {|}~)
                {
                    return new Error(Error.ErrorCodes.Syntax, "Invalid character found in middle of expected keyword begining " + word, lineNo);
                }
                else if (instring || !((c >= 0 && c <= 31) ||  //non printable characters
                        (c >= 20 && c <= 47) || //  !"#$%'()*+,-./
                        (c >= 58 && c <= 60) ||   // :;<
                        (c >= 62 && c <= 64) ||   // >?@
                        (c >= 91 && c <= 96) || // [\]^_`
                        (c >= 123 && c <= 127)))   // {|}~)
                {
                    if (c == '\\' && !expectCommand && instring)
                    {
                        expectCommand = true;
                    }
                    else if (expectCommand && instring)
                    {
                        expectCommand = false;
                        switch(c)
                        {
                            case 'n':
                                word += '\n';
                                break;
                            case '\\':
                                word += '\\';
                                break;
                            case 't':
                                word += '\t';
                                break;
                            case '\"':
                                word += '\"';
                                break;
                        }
                    }
                    else
                    {
                        word += c;
                    }
                    continue;
                }

                ///check for class if not already in class
                if (bracCount == 0)
                {
                    if (prevWord == ":" && word == unityClass)
                    {
                        prevWord = word;
                        word = "";
                    }
                    else if (prevWord == aClass)
                    {
                        switch (word)
                        {
                            case "Zombie":
                                break;
                            case "GameMaster":
                                break;
                            case "Player":
                                break;
                            default:
                                return new Error(Error.ErrorCodes.InGame, "Class Must be called \"Zombie\", \"Player\" or \"GameMaster\"", lineNo);
                        }
                        prevWord = word;
                        word = "";
                    }
                    else if (word == aClass && prevWord == pub)
                    {
                        prevWord = word;
                        word = "";
                    }
                    else if (word == pub)
                    {
                        prevWord = word;
                        word = "";
                    }
                    else if (prevWord != ":"  && word != "")
                    {
                        return new Error(Error.ErrorCodes.Syntax, "unrecognised keyword for class declairation " + "\"" + word + "\"", lineNo);
                    }
                }
                else
                {
                    string error = ClassCheck(word, ref prevWord, ref bracCount);
                    if (error != "")
                    {
                        return new Error(Error.ErrorCodes.Syntax, error, lineNo);
                    }
                    word = "";
                }
            }

            if (word != "")
            {
                return new Error(Error.ErrorCodes.Syntax, "Invalid instruction before semi colon", lineNo);
            }
            word = "";
            prevWord = "";
        }

        return new Error();
    }

    private string ClassCheck(string word, ref string prevWord, ref int bracCount)
    {

        if (word.Length > 0)
        {
            if (word[0] == '=')
            {
                prevWord = "=";
                word = word.Substring(1, word.Length - 1);
                if (word == "")
                {
                    return "";
                }
            }
        }

        ///check for variable store variables
        if (prevWord == "=")
        {
            if (type == "string")
            {
                if (stringset == "" || !strings.ContainsKey(stringset))
                {
                    return "string does not exsist to be set";
                }

                strings[stringset] = word;
                prevWord = "";
                type = "";
                stringset = "";
            }
        }
        else if (word == "string")
        {
            prevWord = word;
        }
        else if (prevWord == "string")
        {
            if (word[word.Length - 1] == '=')
            {
                prevWord = "=";
                word = word.Substring(0, word.Length - 1);
            }
            type = "string";
            strings.Add(word, "");
            stringset = word;
            prevWord = "";
        }
        ///check for method store methods
        ///check for open bracket (increment bracket count)
        ///if in method while bracket count = 2 start method check
        ///if error return false
        return "";
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
