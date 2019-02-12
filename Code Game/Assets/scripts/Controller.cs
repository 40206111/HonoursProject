using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

public struct Variable
{
    public enum VariableType
    {
        STRING,
        INT,
        FLOAT,
        VEC3,
        BOOL
    }

    public string str_value;
    public int int_value;
    public float flt_value;
    public Vector3 vec3_value;
    public bool bool_value;
    public VariableType type;
    public bool inScope;
}

public class Controller : MonoBehaviour
{
    private enum MethodType
    {
        None,
        Start,
        Update
    }

    [SerializeField]
    private TMP_Text numbers;
    [SerializeField]
    private TMP_Text theText;
    [SerializeField]
    private TMP_InputField input;
    [SerializeField] UnityEngine.UI.Scrollbar scroll;
    private static int lines = 1;
    private static float number_center;

    //Methods
    List<Method> methods = new List<Method>();
    //Variables
    public static IDictionary<string, Variable> vars = new Dictionary<string, Variable>();
    Happening curHaps = Happening.Starting;
    Happening previous = Happening.Starting;

    private IDictionary<string, Happening> keys = new Dictionary<string, Happening>
    {
        {"//", Happening.Ignore},
        {"/*", Happening.SuperIgnore},
        {"using ", Happening.ExpectUsing},
        {"public ", Happening.PublicClass},
        {"class ", Happening.ExpectClassName},
        {"\n", Happening.Starting},
        {"*/", Happening.Starting},
        {"int ", Happening.ExpectVariableName},
        {"string ", Happening.ExpectVariableName},
        {"bool ", Happening.ExpectVariableName},
        {"float ", Happening.ExpectVariableName},
        {"Vector3 ", Happening.ExpectVariableName},
        {"void ", Happening.ExpectMethodName},
        {"Start()", Happening.ExpectBracket},
        {"Update()", Happening.ExpectBracket},
        {"Zombie", Happening.MonoBehaviour},
        {"Player", Happening.MonoBehaviour},
        {"GameMaster", Happening.MonoBehaviour},
        {": MonoBehaviour", Happening.ExpectBracket},
        {":MonoBehaviour", Happening.ExpectBracket},
        {"private ", Happening.PublicPrivate},
        {"}", Happening.Starting},
        {"if", Happening.ExpectIf},
        {"while", Happening.ExpectIf},
        {"for", Happening.ExpectFor},
        {"return", Happening.ExpectSemiColon},
        {"{", Happening.Starting},
        {";", Happening.Starting},
        {"=", Happening.ExpectInt},
        {"= ", Happening.ExpectInt},
    };

    private List<List<string>> scopeVariables = new List<List<string>>();

    //monospace tag
    const string monostring = "<noparse>";


    private enum Happening
    {
        Starting,
        PublicClass,
        Ignore,
        SuperIgnore,
        PublicPrivate,
        ExpectMethodName,
        ExpectClassName,
        MonoBehaviour,
        InClass,
        InMethod,
        ExpectBracket,
        ExpectSemiColon,
        ExpectEquals,
        ////////////
        ExpectInt,
        ExpectString,
        ExpectFloat,
        ExpectBool,
        ExpectVec3,
        ExpectEquation,
        ExpectVariableName,
        ExpectUsing,
        ExpectIf,
        ExpectFor
    }

    private List<List<string>> allStrings = new List<List<string>>();

    // Start is called before the first frame update
    void Start()
    {
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "using ", "public ", "//", "/*" }); //Starting
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "class " }); //PublicClass
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "\n" }); // Ignore
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "*/" });  //SuperIgnore
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "int ", "string ", "bool ", "float ", "Vector3 ", "void " });  //PublicPrivate
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "Start()", "Update()" });  //ExpectMethodName
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "Zombie", "Player", "GameMaster" });  //ExpectClassName
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { ": MonoBehaviour", ":MonoBehaviour" });  //MonoBehaviour
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "public ", "private ", "int ", "string ", "bool ", "float ", "Vector3 ", "}", "//", "/*" });  //InClass
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "int ", "string ", "bool ", "float", "Vector3 ", "if", "while", "return", "for", "}", "//", "/*", "{", "Debug.Log(" });  //InMethod
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "{", "//", "/*" });  //ExpectBracket
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { ";" });  //ExpectSemiColon
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "=" });  //ExpectEquals

        number_center = numbers.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        CheckContent();
        LineNumbers();
        ScrollNumbers();
    }

    public void Run()
    {
        MethodRun(methods);
    }

    public static void MethodRun(List<Method> ms)
    {
        bool wait = false;

        for (int i = 0; i < ms.Count(); ++i)
        {
            Type waitType = null;
            Type countType = null;
            int count = 0;
            if (!wait)
            {
                bool output = ms[i].Compute();

                if (!output)
                {
                    if (ms[i].GetType() == typeof(Code_if))
                    {
                        wait = true;
                        count = 0;
                        waitType = typeof(Code_EndIf);
                        countType = typeof(Code_if);
                    }
                }
            }
            else
            {
                if (ms[i].GetType() == waitType)
                {
                    if (count == 0)
                    {
                        wait = false;
                    }
                    else
                    {
                        count--;
                    }
                }
                else if (ms[i].GetType() == countType)
                {
                    count++;
                }
            }
        }
    }

    //Method to scroll line numbers
    public void ScrollNumbers()
    {
        Vector3 pos = numbers.transform.localPosition;
        pos.y = theText.transform.localPosition.y;
        numbers.transform.localPosition = pos;
    }

    //Method to Add line numbers
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

    //Method to keep tags in input
    private void CheckContent()
    {
        //stop from going out of scope
        int amount = monostring.Length;
        if (input.text.Length < monostring.Length)
        {
            amount = input.text.Length;
        }

        //check if monospace tag has been deleted
        if (input.text.Substring(0, amount) != monostring)
        {
            //remove extra letters from tag
            if (input.text.Length > monostring.Length - 1)
            {
                input.text = input.text.Substring(monostring.Length - 1);
            }
            else
            {
                input.text = "";
            }
            //add tag back
            input.text = monostring + input.text;
            input.caretPosition = monostring.Length;
            Canvas.ForceUpdateCanvases();
        }
    }

    //Method to compile code
    public void Compile()
    {
        Debug.Log("Compile Button Pressed");
        int currentLine = 1;

        Error error = TryCompile(ref currentLine);

        Debug.Log(error);
    }

    private Error TryCompile(ref int lineNo)
    {
        string stringset = "";
        bool ignore = false;
        bool superIgnore = false;
        bool inClass = false;
        bool inMethod = false;
        bool allowWhiteSpace = true;
        bool lineUnfinished = false;
        int character = 0;
        int bracket = 0;
        string word = "";
        string closestWord = "";
        curHaps = Happening.Starting;
        List<string> current = new List<string>(allStrings[(int)curHaps]);
        vars.Clear();
        Mathematics theMaths;

        Happening next = Happening.ExpectInt;

        string allText = input.text.Substring(monostring.Length, input.text.Length - monostring.Length);

        foreach (char c in allText)
        {
            if (c == 8203)
            {
                lineNo++;
                continue;
            }

            if (c == '\n')
            {
                lineNo++;
            }

            if ((!allowWhiteSpace || character > 1) && (c == '\n' || (c >= 0 && c <= 31)))
            {
                return new Error(Error.ErrorCodes.Syntax, "Too much white space found between commands", lineNo);
            }
            else if (allowWhiteSpace && character == 0 && (c == '\n' || (c >= 0 && c <= 32)))
            {
                continue;
            }
            else
            {
                word += c;
            }


            if ((int)curHaps > allStrings.Count - 1)
            {
                switch (curHaps)
                {
                    case Happening.ExpectUsing:
                        if (character > 0 && c == ';')
                        {
                            word = "";
                            character = -1;
                            curHaps = Happening.Starting;
                            current = new List<string>(allStrings[(int)Happening.Starting]);
                            allowWhiteSpace = true;
                            lineUnfinished = false;
                        }
                        else if (c != 46 && !(c >= 65 && c <= 90) && !(c >= 97 && c <= 122))
                        {
                            return new Error(Error.ErrorCodes.Syntax, "Invalid character found in using declairation", lineNo);
                        }
                        break;
                    case Happening.ExpectVariableName:
                        if (c == ';')
                        {
                            stringset = word.Substring(0, word.Length - 1);
                            if (character == 0)
                            {
                                return new Error(Error.ErrorCodes.Syntax, "No variable name found", lineNo);
                            }
                            lineUnfinished = false;
                            allowWhiteSpace = true;
                            if (vars.ContainsKey(stringset))
                            {
                                if (vars[stringset].inScope == true)
                                {
                                    return new Error(Error.ErrorCodes.Syntax, "cannot redefine \"" + stringset + "\"", lineNo);
                                }
                            }
                            Variable theNew = new Variable();
                            theNew.inScope = true;
                            switch (next)
                            {
                                case Happening.ExpectInt:
                                    theNew.type = Variable.VariableType.INT;
                                    theNew.int_value = 0;
                                    break;
                                case Happening.ExpectBool:
                                    theNew.type = Variable.VariableType.BOOL;
                                    theNew.bool_value = false;
                                    break;
                                case Happening.ExpectFloat:
                                    theNew.type = Variable.VariableType.FLOAT;
                                    theNew.flt_value = 0;
                                    break;
                                case Happening.ExpectVec3:
                                    theNew.type = Variable.VariableType.VEC3;
                                    theNew.vec3_value = new Vector3();
                                    break;
                                case Happening.ExpectString:
                                    theNew.type = Variable.VariableType.STRING;
                                    theNew.str_value = "";
                                    break;
                            }
                            vars.Add(stringset, theNew);
                            scopeVariables[scopeVariables.Count - 1].Add(stringset);
                            stringset = "";
                            character = -1;
                            if (inMethod)
                            {
                                curHaps = Happening.InMethod;
                            }
                            else if (inClass)
                            {
                                curHaps = Happening.InClass;
                            }
                            current = new List<string>(allStrings[(int)curHaps]);
                        }
                        else if (character > 0 && c == 32)
                        {
                            stringset = word.Substring(0, word.Length - 1);
                            curHaps = Happening.ExpectEquals;
                            current = new List<string>(allStrings[(int)curHaps]);
                            allowWhiteSpace = false;
                            lineUnfinished = true;
                            word = "";
                            character = -1;
                        }
                        else if (character > 0 && c == 61)
                        {
                            stringset = word.Substring(0, word.Length - 1);
                            curHaps = next;
                            allowWhiteSpace = false;
                            lineUnfinished = true;
                            word = "";
                            character = -1;
                        }
                        else if (!(c >= 65 && c <= 90) && !(c >= 97 && c <= 122) && (character == 0 && (c >= 48 && c <= 57)))
                        {
                            return new Error(Error.ErrorCodes.Syntax, "Invalid character found in variableName", lineNo);
                        }
                        break;
                    case Happening.ExpectInt:
                        if (c == ';')
                        {
                            if (character == 0)
                            {
                                return new Error(Error.ErrorCodes.Syntax, "No value found", lineNo);
                            }

                            word = word.Substring(0, word.Length - 1);

                            if (vars.ContainsKey(word))
                            {
                                if (!inMethod)
                                {
                                    return new Error(Error.ErrorCodes.Compiler, "Cannot initialise variable to variable outside of method", lineNo);
                                }
                                if (vars[word].type != Variable.VariableType.INT)
                                {
                                    return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert variable \"" + word + "\" to int", lineNo);
                                }
                                Code_SimpleSet temp = new Code_SimpleSet();
                                temp.output = stringset;
                                temp.input = word;
                                methods.Add(temp);
                            }

                            current = new List<string>(allStrings[(int)curHaps]);
                            allowWhiteSpace = true;
                            lineUnfinished = false;
                            word = "";
                            character = -1;
                        }
                        else if (c == '+' || c == '-' || c == '*' || c == '/')
                        {

                        }
                        else if (!(c >= 65 && c <= 90) && !(c >= 97 && c <= 122) && (c >= 48 && c <= 57))   //not letters or numbers
                        {
                            return new Error(Error.ErrorCodes.Mathematical, "Cannot convert \"" + c + "\" to int", lineNo);
                        }
                        break;
                }
            }
            else
            {
                for (int i = 0; i < current.Count; ++i)
                {
                    if (c != current[i][character])
                    {
                        if (ignore || superIgnore)
                        {
                            word = "";
                            character = -1;
                        }
                        else
                        {
                            current.Remove(current[i]);
                            i--;
                        }
                    }
                    else if (word == current[i])
                    {
                        if (word == "{")
                        {
                            bracket++;

                            if (previous == Happening.MonoBehaviour)
                            {
                                inClass = true;
                            }
                            else if (previous == Happening.ExpectMethodName)
                            {
                                inMethod = true;
                            }
                            scopeVariables.Add(new List<string>());
                        }

                        if (word == "int ")
                        {
                            next = Happening.ExpectInt;
                        }
                        else if (word == "string ")
                        {
                            next = Happening.ExpectString;
                        }
                        else if (word == "float ")
                        {
                            next = Happening.ExpectFloat;
                        }
                        else if (word == "Vector3 ")
                        {
                            next = Happening.ExpectVec3;
                        }

                        character = -1;
                        if (ignore) ignore = false;
                        if (superIgnore) superIgnore = false;
                        if (!(curHaps == Happening.ExpectSemiColon)) lineUnfinished = true;
                        allowWhiteSpace = false;

                        if (word == "}")
                        {
                            bracket--;
                            if (bracket == 0)
                            {
                                lineUnfinished = false;
                                inClass = false;
                            }

                            if (inMethod && bracket == 1)
                            {
                                inMethod = false;
                            }

                            foreach (string s in scopeVariables[scopeVariables.Count - 1])
                            {
                                Variable temp = vars[s];
                                temp.inScope = false;
                            }

                            scopeVariables.Remove(scopeVariables.Last());
                        }

                        previous = curHaps;
                        curHaps = keys[word];

                        switch (curHaps)
                        {
                            case Happening.Ignore:
                                allowWhiteSpace = true;
                                ignore = true;
                                break;
                            case Happening.SuperIgnore:
                                allowWhiteSpace = true;
                                superIgnore = true;
                                break;
                            case Happening.Starting:
                                allowWhiteSpace = true;
                                if (inMethod)
                                {
                                    curHaps = Happening.InMethod;
                                }
                                else if (inClass)
                                {
                                    curHaps = Happening.InClass;
                                }
                                break;
                            case Happening.PublicClass:
                                if (inClass)
                                {
                                    curHaps = Happening.PublicPrivate;
                                }
                                break;
                            case Happening.MonoBehaviour:
                                allowWhiteSpace = true;
                                break;
                            case Happening.ExpectBracket:
                                allowWhiteSpace = true;
                                break;
                        }

                        if (previous == Happening.ExpectEquals)
                        {
                            switch (next)
                            {
                                case Happening.ExpectInt:
                                    curHaps = Happening.ExpectInt;
                                    break;
                                case Happening.ExpectBool:
                                    curHaps = Happening.ExpectBool;
                                    break;
                                case Happening.ExpectFloat:
                                    curHaps = Happening.ExpectFloat;
                                    break;
                                case Happening.ExpectVec3:
                                    curHaps = Happening.ExpectVec3;
                                    break;
                                case Happening.ExpectString:
                                    curHaps = Happening.ExpectString;
                                    break;
                            }
                        }

                        if ((int)curHaps < allStrings.Count)
                        {
                            current = new List<string>(allStrings[(int)curHaps]);
                        }
                        word = "";
                        closestWord = "";
                        break;
                    }
                    else
                    {
                        closestWord = current[i];
                    }

                }
                if (current.Count == 0)
                {
                    return new Error(Error.ErrorCodes.Syntax, "word \"" + word + "\" not understood. Closest word " + closestWord, lineNo);
                }
            }
            character++;
        }

        if (lineUnfinished == true)
        {
            return new Error(Error.ErrorCodes.Syntax, "Line not complete", lineNo);
        }

        return new Error();
    }

}
