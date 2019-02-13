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
        bool initialise = false;
        int character = 0;
        int bracket = 0;
        string word = "";
        string closestWord = "";
        curHaps = Happening.Starting;
        List<string> current = new List<string>(allStrings[(int)curHaps]);
        vars.Clear();
        scopeVariables.Clear();

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
                            if (!initialise)
                            {
                                return new Error(Error.ErrorCodes.Syntax, "Nothing happening", lineNo);
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
                            else if (next == Happening.Starting)
                            {
                                return new Error(Error.ErrorCodes.Syntax, "Variable not initialised", lineNo);
                            }
                            if (!vars.ContainsKey(stringset))
                            {
                                Variable theNew = new Variable
                                {
                                    inScope = true
                                };
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
                            }
                            else
                            {
                                if (next != Happening.ExpectVec3)
                                {
                                    Code_SimpleSet set = new Code_SimpleSet
                                    {
                                        changeType = true,
                                        output = stringset
                                    };

                                    switch (next)
                                    {
                                        case Happening.ExpectInt:
                                            set.iValue = 0;
                                            set.newType = Variable.VariableType.INT;
                                            break;
                                        case Happening.ExpectBool:
                                            set.bValue = false;
                                            set.newType = Variable.VariableType.BOOL;
                                            break;
                                        case Happening.ExpectFloat:
                                            set.fValue = 0;
                                            set.newType = Variable.VariableType.FLOAT;
                                            break;
                                        case Happening.ExpectString:
                                            set.sValue = "";
                                            set.newType = Variable.VariableType.STRING;
                                            break;
                                    }
                                    methods.Add(set);
                                }
                                else
                                {
                                    Code_Vec3Set set = new Code_Vec3Set
                                    {
                                        output = stringset
                                    };

                                    Mathematics maths = new Mathematics();
                                    maths.lhsComplete = true;
                                    maths.fLHS = 0;
                                    set.x = maths;
                                    set.y = maths;
                                    set.z = maths;
                                }
                            }
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
                            initialise = false;
                            allowWhiteSpace = true;
                            lineUnfinished = false;
                            word = "";
                        }
                        else if (character > 0 && c == 32)
                        {
                            stringset = word.Substring(0, word.Length - 1);
                            if (initialise && vars.ContainsKey(stringset))
                            {
                                if (vars[stringset].inScope == true && next == Happening.Starting)
                                {
                                    return new Error(Error.ErrorCodes.Syntax, "cannot redefine \"" + stringset + "\"", lineNo);
                                }
                                Variable temp = new Variable();
                                temp = vars[stringset];
                                temp.inScope = true;
                                vars[stringset] = temp;
                            }
                            previous = next;
                            if (next == Happening.Starting)
                            {
                                if (vars[stringset].type == Variable.VariableType.BOOL)
                                {
                                    next = Happening.ExpectBool;
                                }
                                else if (vars[stringset].type == Variable.VariableType.FLOAT)
                                {
                                    next = Happening.ExpectFloat;
                                }
                                else if (vars[stringset].type == Variable.VariableType.INT)
                                {
                                    next = Happening.ExpectInt;
                                }
                                else if (vars[stringset].type == Variable.VariableType.STRING)
                                {
                                    next = Happening.ExpectString;
                                }
                                else if (vars[stringset].type == Variable.VariableType.VEC3)
                                {
                                    next = Happening.ExpectVec3;
                                }
                            }
                            curHaps = Happening.ExpectEquals;
                            current = new List<string>(allStrings[(int)curHaps]);
                            allowWhiteSpace = false;
                            lineUnfinished = true;
                            word = "";
                            character = -1;
                            initialise = false;
                        }
                        else if (character > 0 && c == 61)
                        {
                            stringset = word.Substring(0, word.Length - 1);
                            if (initialise && vars.ContainsKey(stringset))
                            {
                                if (vars[stringset].inScope == true)
                                {
                                    return new Error(Error.ErrorCodes.Syntax, "cannot redefine \"" + stringset + "\"", lineNo);
                                }
                                Variable temp = new Variable();
                                temp = vars[stringset];
                                temp.inScope = true;
                                vars[stringset] = temp;
                            }
                            previous = next;
                            if (next == Happening.Starting)
                            {
                                if (vars[stringset].type == Variable.VariableType.BOOL)
                                {
                                    next = Happening.ExpectBool;
                                }
                                else if (vars[stringset].type == Variable.VariableType.FLOAT)
                                {
                                    next = Happening.ExpectFloat;
                                }
                                else if (vars[stringset].type == Variable.VariableType.INT)
                                {
                                    next = Happening.ExpectInt;
                                }
                                else if (vars[stringset].type == Variable.VariableType.STRING)
                                {
                                    next = Happening.ExpectString;
                                }
                                else if (vars[stringset].type == Variable.VariableType.VEC3)
                                {
                                    next = Happening.ExpectVec3;
                                }
                            }
                            curHaps = next;
                            allowWhiteSpace = false;
                            lineUnfinished = true;
                            word = "";
                            character = -1;
                            initialise = false;
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
                            int value;
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
                                if (vars[stringset].type != Variable.VariableType.INT)
                                {
                                    temp.changeType = true;
                                    temp.newType = Variable.VariableType.INT;
                                }
                                methods.Add(temp);
                            }
                            else if (Int32.TryParse(word, out value))
                            {
                                if (!vars.ContainsKey(stringset))
                                {
                                    Variable temp = new Variable();
                                    temp.type = Variable.VariableType.INT;
                                    temp.inScope = true;
                                    temp.int_value = value;
                                    vars.Add(stringset, temp);
                                    scopeVariables[scopeVariables.Count - 1].Add(stringset);
                                }

                                if (inMethod)
                                {
                                    Code_SimpleSet set = new Code_SimpleSet
                                    {
                                        output = stringset,
                                        fValue = value
                                    };

                                    if (vars[stringset].type != Variable.VariableType.FLOAT)
                                    {
                                        set.changeType = true;
                                        set.newType = Variable.VariableType.FLOAT;
                                    }

                                    methods.Add(set);
                                }
                            }

                            curHaps = Happening.Starting;
                            if (inMethod)
                            {
                                curHaps = Happening.InMethod;
                            }
                            else if (inClass)
                            {
                                curHaps = Happening.InClass;
                            }
                            current = new List<string>(allStrings[(int)curHaps]);
                            allowWhiteSpace = true;
                            lineUnfinished = false;
                            word = "";
                            character = -1;
                            stringset = "";
                        }
                        else if (c == '+' || c == '-' || c == '*' || c == '/' || c == '(')
                        {
                            curHaps = Happening.ExpectEquation;
                            if (!vars.ContainsKey(stringset))
                            {
                                Variable temp = new Variable();
                                temp.type = Variable.VariableType.INT;
                                temp.inScope = true;
                                scopeVariables[scopeVariables.Count - 1].Add(stringset);
                                vars.Add(stringset, temp);
                            }
                        }
                        else if (c != 32 && !(c >= 65 && c <= 90) && !(c >= 97 && c <= 122) && !(c >= 48 && c <= 57))   //not letters or numbers
                        {
                            return new Error(Error.ErrorCodes.Mathematical, "Cannot convert \"" + c + "\" to float", lineNo);
                        }
                        break;
                    case Happening.ExpectFloat:
                        if (c == ';')
                        {
                            if (character == 0)
                            {
                                return new Error(Error.ErrorCodes.Syntax, "No value found", lineNo);
                            }

                            word = word.Substring(0, word.Length - 1);
                            string[] words = word.Split('.');
                            if (words.Count() > 2)
                            {
                                return new Error(Error.ErrorCodes.Syntax, "Too many decimal points found", lineNo);
                            }
                            float value;
                            if (vars.ContainsKey(words[0]))
                            {
                                if (!inMethod)
                                {
                                    return new Error(Error.ErrorCodes.Compiler, "Cannot initialise variable to variable outside of method", lineNo);
                                }
                                Code_SimpleSet temp = new Code_SimpleSet();

                                if (words.Count() == 1 && vars[word].type != Variable.VariableType.INT && vars[word].type != Variable.VariableType.FLOAT)
                                {
                                    return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert variable \"" + word + "\" to float", lineNo);
                                }
                                else if (words.Count() == 2)
                                {
                                    if (vars[word].type != Variable.VariableType.VEC3)
                                    {
                                        return new Error(Error.ErrorCodes.TypeMismatch, words[1] + " is not part of " + words[0], lineNo);
                                    }

                                    if (words[1] == "x")
                                    {
                                        temp.inPart = Code_if.VectorPart.x;
                                    }
                                    else if (words[1] == "x")
                                    {
                                        temp.inPart = Code_if.VectorPart.x;
                                    }
                                    else if (words[1] == "x")
                                    {
                                        temp.inPart = Code_if.VectorPart.x;
                                    }
                                    else
                                    {
                                        return new Error(Error.ErrorCodes.TypeMismatch, word[1] + "is not a part of " + word[0], lineNo);
                                    }
                                }
                                if (vars[stringset].type != Variable.VariableType.FLOAT)
                                {
                                    temp.changeType = true;
                                    temp.newType = Variable.VariableType.FLOAT;
                                }
                                temp.output = stringset;
                                temp.input = word;
                                methods.Add(temp);
                            }
                            else if (word[word.Length - 1] != 'f')
                            {
                                int int_value;
                                if (Int32.TryParse(word, out int_value))
                                {
                                    if (!vars.ContainsKey(stringset))
                                    {
                                        Variable temp = new Variable
                                        {
                                            type = Variable.VariableType.FLOAT,
                                            inScope = true,
                                            flt_value = int_value
                                        };
                                        vars.Add(stringset, temp);
                                        scopeVariables[scopeVariables.Count - 1].Add(stringset);
                                    }

                                    if (inMethod)
                                    {
                                        Code_SimpleSet set = new Code_SimpleSet
                                        {
                                            output = stringset,
                                            fValue = int_value
                                        };

                                        if (vars[stringset].type != Variable.VariableType.FLOAT)
                                        {
                                            set.changeType = true;
                                            set.newType = Variable.VariableType.FLOAT;
                                        }

                                        methods.Add(set);
                                    }
                                }
                                else return new Error(Error.ErrorCodes.TypeMismatch, "Expected float value", lineNo);
                            }
                            else if (float.TryParse(word.Substring(1, word.Length - 2), out value))
                            {
                                if (!vars.ContainsKey(stringset))
                                {
                                    Variable temp = new Variable
                                    {
                                        type = Variable.VariableType.FLOAT,
                                        inScope = true,
                                        flt_value = value
                                    };
                                    vars.Add(stringset, temp);
                                    scopeVariables[scopeVariables.Count - 1].Add(stringset);
                                }

                                if (inMethod)
                                {
                                    Code_SimpleSet set = new Code_SimpleSet
                                    {
                                        output = stringset,
                                        fValue = value
                                    };

                                    if (vars[stringset].type != Variable.VariableType.FLOAT)
                                    {
                                        set.changeType = true;
                                        set.newType = Variable.VariableType.FLOAT;
                                    }

                                    methods.Add(set);
                                }
                            }
                            else
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, "Invalid characters in float", lineNo);
                            }

                            curHaps = Happening.Starting;
                            if (inMethod)
                            {
                                curHaps = Happening.InMethod;
                            }
                            else if (inClass)
                            {
                                curHaps = Happening.InClass;
                            }
                            current = new List<string>(allStrings[(int)curHaps]);
                            allowWhiteSpace = true;
                            lineUnfinished = false;
                            word = "";
                            character = -1;
                            stringset = "";
                        }
                        else if (c == '+' || c == '-' || c == '*' || c == '/' || c == '(')
                        {
                            curHaps = Happening.ExpectEquation;
                            if (!vars.ContainsKey(stringset))
                            {
                                Variable temp = new Variable
                                {
                                    type = Variable.VariableType.FLOAT,
                                    inScope = true
                                };
                                scopeVariables[scopeVariables.Count - 1].Add(stringset);
                                vars.Add(stringset, temp);
                            }
                        }
                        else if (c != 46 && c != 32 && !(c >= 65 && c <= 90) && !(c >= 97 && c <= 122) && !(c >= 48 && c <= 57))   //not letters or numbers
                        {
                            return new Error(Error.ErrorCodes.Mathematical, "Cannot convert \"" + c + "\" to float", lineNo);
                        }
                        break;
                    case Happening.ExpectBool:
                        if (c == ';')
                        {
                            string[] words = { "true;", " true;", "true ;", " true ;",
                                           "false;", " false;", "false ;", " false ;" };
                            for (int i = 0; i < words.Length; ++i)
                            {
                                if (word == words[i])
                                {
                                    bool value = false;
                                    if (i < 4)
                                    {
                                        value = true;
                                    }
                                    if (!vars.ContainsKey(stringset))
                                    {
                                        Variable temp = new Variable
                                        {
                                            inScope = true,
                                            bool_value = value
                                        };
                                        vars.Add(stringset, temp);
                                        vars[stringset] = temp;
                                    }
                                    if (inMethod)
                                    {
                                        Code_SimpleSet set = new Code_SimpleSet
                                        {
                                            bValue = value,
                                            output = stringset
                                        };
                                        if (vars[stringset].type != Variable.VariableType.BOOL)
                                        {
                                            set.changeType = true;
                                            set.newType = Variable.VariableType.BOOL;
                                        }
                                    }

                                    break;
                                }

                            }
                            word = word.Substring(0, word.Length - 1);
                            word = word.Trim();
                            if (vars.ContainsKey(word))
                            {
                                if (!inMethod)
                                {
                                    return new Error(Error.ErrorCodes.Compiler, "Cannot set variable to another variable outside of class", lineNo);
                                }
                                if (vars[word].type != Variable.VariableType.BOOL)
                                {
                                    return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert value to bool type", lineNo);
                                }

                                if (!vars.ContainsKey(stringset))
                                {
                                    Variable temp = new Variable
                                    {
                                        inScope = true,
                                        bool_value = vars[word].bool_value
                                    };
                                    vars.Add(stringset, temp);
                                    vars[stringset] = temp;
                                }
                                Code_SimpleSet set = new Code_SimpleSet
                                {
                                    input = word,
                                    output = stringset
                                };
                                if (vars[stringset].type != Variable.VariableType.BOOL)
                                {
                                    set.changeType = true;
                                    set.newType = Variable.VariableType.BOOL;
                                }
                            }
                            else
                            {
                                return new Error(Error.ErrorCodes.Syntax, "Cannot convert value to bool", lineNo);
                            }

                            curHaps = Happening.Starting;
                            if (inMethod)
                            {
                                curHaps = Happening.InMethod;
                            }
                            else if (inClass)
                            {
                                curHaps = Happening.InClass;
                            }
                            current = new List<string>(allStrings[(int)curHaps]);
                            allowWhiteSpace = true;
                            lineUnfinished = false;
                            word = "";
                            character = -1;
                            stringset = "";
                        }
                        break;
                    case Happening.ExpectEquation:
                        if (c == ';')
                        {
                            Mathematics theMaths = new Mathematics();
                            int braCount = 0;
                            Error test = MakeEquation(ref theMaths, ref word, lineNo, inMethod, ref braCount);

                            if (test.errorCode != Error.ErrorCodes.None)
                            {
                                return test;
                            }
                            else
                            {
                                if (inMethod)
                                {
                                    Code_Equation eq = new Code_Equation();
                                    eq.maths = theMaths;
                                    eq.output = stringset;
                                    if (next == Happening.ExpectInt && vars[stringset].type != Variable.VariableType.INT)
                                    {
                                        eq.changeType = true;
                                        eq.newType = Variable.VariableType.INT;
                                    }
                                    else if (next == Happening.ExpectFloat && vars[stringset].type != Variable.VariableType.FLOAT)
                                    {
                                        eq.changeType = true;
                                        eq.newType = Variable.VariableType.FLOAT;
                                    }
                                    methods.Add(eq);
                                }
                                else
                                {
                                    if (vars[stringset].type == Variable.VariableType.INT)
                                    {
                                        Variable temp = vars[stringset];
                                        temp.int_value = (int)theMaths.Calculate();
                                        vars[stringset] = temp;
                                    }
                                    else if (vars[stringset].type == Variable.VariableType.FLOAT)
                                    {
                                        Variable temp = vars[stringset];
                                        temp.flt_value = theMaths.Calculate();
                                        vars[stringset] = temp;
                                    }
                                }
                            }

                            curHaps = Happening.Starting;
                            if (inMethod)
                            {
                                curHaps = Happening.InMethod;
                            }
                            else if (inClass)
                            {
                                curHaps = Happening.InClass;
                            }
                            current = new List<string>(allStrings[(int)curHaps]);
                            allowWhiteSpace = true;
                            lineUnfinished = false;
                            word = "";
                            character = -1;
                            stringset = "";
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
                            initialise = true;
                        }
                        else if (word == "string ")
                        {
                            next = Happening.ExpectString;
                            initialise = true;
                        }
                        else if (word == "float ")
                        {
                            next = Happening.ExpectFloat;
                            initialise = true;
                        }
                        else if (word == "Vector3 ")
                        {
                            next = Happening.ExpectVec3;
                            initialise = true;
                        }
                        else if (word == "bool ")
                        {
                            next = Happening.ExpectBool;
                            initialise = true;
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
                if (current.Count == 0 && !inMethod)
                {
                    return new Error(Error.ErrorCodes.Syntax, "word \"" + word + "\" not understood. Closest word " + closestWord, lineNo);
                }
                else if (current.Count == 0 && inMethod)
                {
                    next = Happening.Starting;
                    previous = curHaps;
                    curHaps = Happening.ExpectVariableName;
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

    /// <summary>
    /// Helper function that makes an equation from a line of maths
    /// </summary>
    /// <param name="maths"> the Mathematics variable you wish to store your equation in </param>
    /// <param name="eq">your equation to be stored</param>
    /// <param name="lineNo"> the current line number</param>
    /// <param name="inMethod"> if the variable is in a method</param>
    /// <param name="brackCount"> should be 0 to begin with</param>
    /// <returns></returns>
    private Error MakeEquation(ref Mathematics maths, ref string eq, int lineNo, bool inMethod, ref int brackCount)
    {
        string word = "";
        while (eq != "")
        {
            char c = eq[0];
            eq = eq.Substring(1, eq.Length - 1);
            if (c == '(')
            {
                word = word.Trim();
                brackCount++;
                if (word == "")
                {
                    if (!maths.lhsComplete)
                    {
                        maths.lhsComplete = true;
                        maths.lhs = new Mathematics();
                        Error test = MakeEquation(ref maths.lhs, ref eq, lineNo, inMethod, ref brackCount);
                        if (test.errorCode != Error.ErrorCodes.None)
                        {
                            return test;
                        }
                    }
                    else if (!maths.rhsComplete)
                    {
                        maths.rhsComplete = true;
                        maths.rhs = new Mathematics();
                        Error test = MakeEquation(ref maths.rhs, ref eq, lineNo, inMethod, ref brackCount);
                        if (test.errorCode != Error.ErrorCodes.None)
                        {
                            return test;
                        }
                    }
                    else
                    {
                        return new Error(Error.ErrorCodes.Mathematical, "Bracket Not expected", lineNo);
                    }
                }
                else
                {
                    return new Error(Error.ErrorCodes.Mathematical, "Bracket Not expected", lineNo);
                }
            }
            else if (c == '+' || c == '-' || c == '*' || c == '/')
            {
                word = word.Trim();
                string[] words = word.Split('.');
                if (words.Count() > 2)
                {
                    return new Error(Error.ErrorCodes.Mathematical, "Too many decimal places", lineNo);
                }

                if (!((maths.op == Mathematics.Operator.MINUS && c == '+') ||
                    (maths.op == Mathematics.Operator.DIVIDE && c == '*') ||
                    ((maths.op == Mathematics.Operator.TIMES || maths.op == Mathematics.Operator.DIVIDE) && (c == '+' || c == '-'))) && maths.op != Mathematics.Operator.NONE)
                {
                    Mathematics temp = new Mathematics();
                    temp.lhs = maths.rhs;
                    temp.lhsComplete = maths.rhsComplete;
                    if (c == '+')
                    {
                        temp.op = Mathematics.Operator.PLUS;
                    }
                    else if (c == '-')
                    {
                        temp.op = Mathematics.Operator.MINUS;
                    }
                    else if (c == '*')
                    {
                        temp.op = Mathematics.Operator.TIMES;
                    }
                    else if (c == '/')
                    {
                        temp.op = Mathematics.Operator.DIVIDE;
                    }

                    if (word != "")
                    {
                        if (words.Count() == 1)
                        {
                            float value;
                            if (vars.ContainsKey(word))
                            {
                                if (vars[word].type == Variable.VariableType.INT || vars[word].type == Variable.VariableType.FLOAT)
                                {
                                    if (temp.lhsComplete)
                                    {
                                        temp.varRHS = word;
                                        temp.rhsComplete = true;
                                    }
                                    else
                                    {
                                        temp.varLHS = word;
                                        temp.lhsComplete = true;
                                    }
                                }
                                else
                                {
                                    return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert variable type to float", lineNo);
                                }

                            }
                            else if (float.TryParse(word, out value))
                            {
                                if (temp.lhsComplete)
                                {
                                    temp.fRHS = value;
                                    temp.rhsComplete = true;
                                }
                                else
                                {
                                    temp.fLHS = value;
                                    temp.lhsComplete = true;
                                }
                            }
                            else
                            {
                                return new Error(Error.ErrorCodes.Mathematical, "Invalid characters in expected float", lineNo);
                            }
                        }
                        if (words.Count() == 2)
                        {
                            if (vars.ContainsKey(words[0]))
                            {
                                if (!inMethod)
                                {
                                    return new Error(Error.ErrorCodes.Compiler, "Cannot initialise variable with a variable outside of method", lineNo);
                                }

                                if (vars[words[0]].type == Variable.VariableType.VEC3)
                                {
                                    if (temp.lhsComplete)
                                    {
                                        temp.rhsComplete = true;
                                        temp.varRHS = words[0];
                                        if (words[1] == "x")
                                        {
                                            temp.vectorRHS = Code_if.VectorPart.x;
                                        }
                                        else if (words[1] == "y")
                                        {
                                            temp.vectorRHS = Code_if.VectorPart.y;
                                        }
                                        else if (words[1] == "z")
                                        {
                                            temp.vectorRHS = Code_if.VectorPart.z;
                                        }
                                        else
                                        {
                                            return new Error(Error.ErrorCodes.InGame, words[1] + "is not part of " + words[0], lineNo);
                                        }
                                    }
                                    else
                                    {
                                        temp.lhsComplete = true;
                                        temp.varLHS = words[0];
                                        if (words[1] == "x")
                                        {
                                            temp.vectorLHS = Code_if.VectorPart.x;
                                        }
                                        else if (words[1] == "y")
                                        {
                                            temp.vectorLHS = Code_if.VectorPart.y;
                                        }
                                        else if (words[1] == "z")
                                        {
                                            temp.vectorLHS = Code_if.VectorPart.z;
                                        }
                                        else
                                        {
                                            return new Error(Error.ErrorCodes.InGame, words[1] + "is not part of " + words[0], lineNo);
                                        }
                                    }
                                }
                                else
                                {
                                    return new Error(Error.ErrorCodes.TypeMismatch, words[1] + "is not part of " + words[0], lineNo);
                                }
                            }
                            else
                            {
                                float value;
                                if (word[word.Length - 1] != 'f')
                                {
                                    return new Error(Error.ErrorCodes.Mathematical, "Expected type of float", lineNo);
                                }
                                else if (float.TryParse(word.Substring(0, word.Length - 2), out value))
                                {
                                    if (temp.lhsComplete)
                                    {
                                        temp.rhsComplete = true;
                                        temp.fRHS = value;
                                    }
                                    else
                                    {
                                        temp.lhsComplete = true;
                                        temp.fLHS = value;
                                    }
                                }
                                else
                                {
                                    return new Error(Error.ErrorCodes.Mathematical, "Expected float value", lineNo);
                                }
                            }
                        }
                    }

                    maths.rhs = temp;
                    maths.rhsComplete = true;
                    return MakeEquation(ref maths.rhs, ref eq, lineNo, inMethod, ref brackCount);
                }
                if (word != "")
                {
                    if (words.Count() == 1)
                    {
                        float value;
                        if (vars.ContainsKey(word))
                        {
                            if (vars[word].type == Variable.VariableType.INT || vars[word].type == Variable.VariableType.FLOAT)
                            {
                                if (maths.lhsComplete)
                                {
                                    maths.varRHS = word;
                                    maths.rhsComplete = true;
                                }
                                else
                                {
                                    maths.varLHS = word;
                                    maths.lhsComplete = true;
                                }
                            }
                            else
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert variable type to float", lineNo);
                            }

                        }
                        else if (float.TryParse(word, out value))
                        {
                            if (maths.lhsComplete)
                            {
                                maths.fRHS = value;
                                maths.rhsComplete = true;
                            }
                            else
                            {
                                maths.fLHS = value;
                                maths.lhsComplete = true;
                            }
                        }
                        else
                        {
                            return new Error(Error.ErrorCodes.Mathematical, "Invalid characters in expected float", lineNo);
                        }
                    }
                    if (words.Count() == 2)
                    {
                        if (vars.ContainsKey(words[0]))
                        {
                            if (!inMethod)
                            {
                                return new Error(Error.ErrorCodes.Compiler, "Cannot initialise variable with a variable outside of method", lineNo);
                            }

                            if (vars[words[0]].type == Variable.VariableType.VEC3)
                            {
                                if (maths.lhsComplete)
                                {
                                    maths.rhsComplete = true;
                                    maths.varRHS = words[0];
                                    if (words[1] == "x")
                                    {
                                        maths.vectorRHS = Code_if.VectorPart.x;
                                    }
                                    else if (words[1] == "y")
                                    {
                                        maths.vectorRHS = Code_if.VectorPart.y;
                                    }
                                    else if (words[1] == "z")
                                    {
                                        maths.vectorRHS = Code_if.VectorPart.z;
                                    }
                                    else
                                    {
                                        return new Error(Error.ErrorCodes.InGame, words[1] + "is not part of " + words[0], lineNo);
                                    }
                                }
                                else
                                {
                                    maths.lhsComplete = true;
                                    maths.varLHS = words[0];
                                    if (words[1] == "x")
                                    {
                                        maths.vectorLHS = Code_if.VectorPart.x;
                                    }
                                    else if (words[1] == "y")
                                    {
                                        maths.vectorLHS = Code_if.VectorPart.y;
                                    }
                                    else if (words[1] == "z")
                                    {
                                        maths.vectorLHS = Code_if.VectorPart.z;
                                    }
                                    else
                                    {
                                        return new Error(Error.ErrorCodes.InGame, words[1] + "is not part of " + words[0], lineNo);
                                    }
                                }
                            }
                            else
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, words[1] + "is not part of " + words[0], lineNo);
                            }
                        }
                        else
                        {
                            float value;
                            if (word[word.Length - 1] != 'f')
                            {
                                return new Error(Error.ErrorCodes.Mathematical, "Expected type of float", lineNo);
                            }
                            else if (float.TryParse(word.Substring(0, word.Length - 1), out value))
                            {
                                if (maths.lhsComplete)
                                {
                                    maths.rhsComplete = true;
                                    maths.fRHS = value;
                                }
                                else
                                {
                                    maths.lhsComplete = true;
                                    maths.fLHS = value;
                                }
                            }
                            else
                            {
                                return new Error(Error.ErrorCodes.Mathematical, "Expected float value", lineNo);
                            }
                        }
                    }
                    word = "";
                }

                if (maths.op != Mathematics.Operator.NONE)
                {
                    Mathematics temp = new Mathematics(maths);
                    maths = new Mathematics();
                    maths.lhs = temp;
                    maths.lhsComplete = true;
                }


                if (c == '+')
                {
                    maths.op = Mathematics.Operator.PLUS;
                }
                else if (c == '-')
                {
                    maths.op = Mathematics.Operator.MINUS;
                }
                else if (c == '*')
                {
                    maths.op = Mathematics.Operator.TIMES;
                }
                else if (c == '/')
                {
                    maths.op = Mathematics.Operator.DIVIDE;
                }
            }
            else if (c == ')' || c == ';')
            {
                word = word.Trim();
                if (c == ')')
                {
                    brackCount--;
                }
                if (word != "")
                {
                    string[] words = word.Split('.');
                    if (words.Count() > 2)
                    {
                        return new Error(Error.ErrorCodes.TypeMismatch, "Too many decimal points", lineNo);
                    }
                    if (vars.ContainsKey(words[0]))
                    {
                        if (!inMethod)
                        {
                            return new Error(Error.ErrorCodes.Compiler, "Cannot initialise variable with a variable outside of method", lineNo);
                        }

                        if (vars[words[0]].type == Variable.VariableType.VEC3)
                        {
                            if (words.Count() != 2)
                            {
                                return new Error(Error.ErrorCodes.Mathematical, "cannot convert vector3 into float", lineNo);
                            }

                            if (maths.lhsComplete)
                            {
                                maths.rhsComplete = true;
                                maths.varRHS = words[0];
                                if (words[1] == "x")
                                {
                                    maths.vectorRHS = Code_if.VectorPart.x;
                                }
                                else if (words[1] == "y")
                                {
                                    maths.vectorRHS = Code_if.VectorPart.y;
                                }
                                else if (words[1] == "z")
                                {
                                    maths.vectorRHS = Code_if.VectorPart.z;
                                }
                                else
                                {
                                    return new Error(Error.ErrorCodes.InGame, words[1] + "is not part of " + words[0], lineNo);
                                }
                            }
                            else
                            {
                                maths.lhsComplete = true;
                                maths.varLHS = words[0];
                                if (words[1] == "x")
                                {
                                    maths.vectorLHS = Code_if.VectorPart.x;
                                }
                                else if (words[1] == "y")
                                {
                                    maths.vectorLHS = Code_if.VectorPart.y;
                                }
                                else if (words[1] == "z")
                                {
                                    maths.vectorLHS = Code_if.VectorPart.z;
                                }
                                else
                                {
                                    return new Error(Error.ErrorCodes.InGame, words[1] + "is not part of " + words[0], lineNo);
                                }
                            }
                        }
                        else if (vars[words[0]].type == Variable.VariableType.INT || vars[words[0]].type == Variable.VariableType.FLOAT)
                        {
                            if (words.Count() != 1)
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert vector3 to float", lineNo);
                            }

                            if (maths.lhsComplete)
                            {
                                maths.rhsComplete = true;
                                maths.varRHS = word;
                            }
                            else
                            {
                                maths.lhsComplete = true;
                                maths.varLHS = word;
                            }
                        }
                        else
                        {
                            return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert variable type to float", lineNo);
                        }
                    }
                    else
                    {
                        if (words.Count() == 2)
                        {
                            float value;
                            if (word[word.Length - 1] != 'f')
                            {
                                return new Error(Error.ErrorCodes.Mathematical, "Cannot convert value to float", lineNo);
                            }
                            else if (float.TryParse(word.Substring(0, word.Length - 1), out value))
                            {
                                if (maths.lhsComplete)
                                {
                                    maths.rhsComplete = true;
                                    maths.fRHS = value;
                                }
                                else
                                {
                                    maths.lhsComplete = true;
                                    maths.fLHS = value;
                                }
                            }
                            else
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert value to float", lineNo);
                            }
                        }
                        else
                        {
                            float value;
                            if (float.TryParse(word, out value))
                            {
                                if (maths.lhsComplete)
                                {
                                    maths.rhsComplete = true;
                                    maths.fRHS = value;
                                }
                                else
                                {
                                    maths.lhsComplete = true;
                                    maths.fLHS = value;
                                }
                            }
                            else
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert value to float", lineNo);
                            }
                        }
                    }
                }
                //Make sure maths part is complete
                if ((maths.op == Mathematics.Operator.NONE && !maths.lhsComplete) &&
                    !(maths.lhsComplete && maths.rhsComplete))
                {
                    return new Error(Error.ErrorCodes.Mathematical, "Equation not complete", lineNo);
                }
                return new Error();
            }
            else
            {
                word += c;
            }
        }
        if (brackCount > 0)
        {
            return new Error(Error.ErrorCodes.Mathematical, "Too many brackets found", lineNo);
        }
        else if (brackCount < 0)
        {
            return new Error(Error.ErrorCodes.Mathematical, "Too few brackets found", lineNo);
        }

        return new Error();
    }

}
