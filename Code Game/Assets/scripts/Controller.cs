﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

//Struct for variables
public struct Variable
{
    //enum of variable types
    public enum VariableType
    {
        STRING,
        INT,
        FLOAT,
        VEC3,
        BOOL
    }

    //possible variable values
    public string str_value;
    public int int_value;
    public float flt_value;
    public Vector3 vec3_value;
    public bool bool_value;
    //variable type
    public VariableType type;
    public bool inScope;
}

//class to parse user inutted code
public class Controller : MonoBehaviour
{
    //objects from the unity scene
    [SerializeField]
    private TMP_Text numbers;
    [SerializeField]
    private TMP_Text theText;
    [SerializeField]
    private TMP_InputField input;
    [SerializeField]
    UnityEngine.UI.Scrollbar scroll;

    //variables for line numbers
    private int lines = 1;
    private static float number_center;

    //Methods
    IDictionary<string, List<Method>> methods = new Dictionary<string, List<Method>>();
    //Variables
    public static IDictionary<string, Variable> vars = new Dictionary<string, Variable>();
    public static List<IDictionary<string, Variable>> allVars = new List<IDictionary<string, Variable>>();
    Variable zomble = new Variable()
    {
        type = Variable.VariableType.VEC3,
        vec3_value = new Vector3(0, 0, 0),
        inScope = true
    };
    Happening curHaps = Happening.Starting;
    Happening previous = Happening.Starting;
    public static int currentZomb = 0;

    //keys for what to happen when certain words are entered
    private readonly IDictionary<string, Happening> keys = new Dictionary<string, Happening>
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
        {"while", Happening.ExpectWhile},
        {"for", Happening.ExpectFor},
        {"return", Happening.ExpectSemiColon},
        {"{", Happening.Starting},
        {";", Happening.Starting},
        {"=", Happening.ExpectInt},
        {"= ", Happening.ExpectInt},
        {"Vector3(", Happening.ExpectVec3 },
        {"Vector3 (", Happening.ExpectVec3 },
        {"new ", Happening.New },
        {" new ", Happening.New },
        {"Debug.Log(", Happening.DebugLog },
        {"else", Happening.AddElse },
        {"gameobject.transform.position=",  Happening.ExpectNew },
        {"gameobject.transform.position =", Happening.ExpectNew }
    };

    //list of variables currently in scope
    private List<List<string>> scopeVariables = new List<List<string>>();

    //monospace tag
    const string monostring = "<noparse>";

    //booleans for button states
    private bool compile = false;
    static public bool stop = false;

    //enum of possible instructions
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
        ExpectNew,
        New,
        ////////////Anything below this point does not have a list of strings asociated with it
        ExpectInt,
        ExpectString,
        ExpectFloat,
        ExpectBool,
        ExpectVec3,
        ExpectEquation,
        ExpectVariableName,
        ExpectUsing,
        ExpectIf,
        ExpectWhile,
        ExpectFor,
        DebugLog,
        AddElse
    }

    //list of list of strings of expected words
    private List<List<string>> allStrings = new List<List<string>>();

    // Start is called before the first frame update
    void Start()
    {
        //add methods to methods list
        methods.Add("Update()", new List<Method>());
        methods.Add("Start()", new List<Method>());
        //Add key words to allStrings list
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
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "int ", "string ", "bool ", "float", "Vector3 ", "if", "while", "return", "for", "}", "//", "/*", "{", "Debug.Log(", "else", "gameObject.transform.position=", "gameobject.transform.position =" });  //InMethod
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "{", "//", "/*" });  //ExpectBracket
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { ";" });  //ExpectSemiColon
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "=" });  //ExpectEquals
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "new ", " new " });  //ExpectNew
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "Vector3(", "Vector3 (" });  //New

        //set number center
        number_center = numbers.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        CheckContent();
        LineNumbers();
        ScrollNumbers();
    }

    //Method to begin while loop
    public void BeginWhile(Code_While w)
    {
        StartCoroutine(w.NotInfinite());
    }

    //Method to run compiled code
    public void Run()
    {
        //set buttoons
        GM.stop.interactable = true;
        GM.cnr.interactable = false;
        
        if (!compile) return; //do nothing if not compiled
        //Clear variables
        allVars.Clear();
        //Add a variable list for each zombie
        allVars.Add(new Dictionary<string, Variable>(vars));
        allVars.Add(new Dictionary<string, Variable>(vars));
        allVars.Add(new Dictionary<string, Variable>(vars));
        while (currentZomb < 3) //for each zombie
        {
            //set variable to current zombie position
            Variable temp = allVars[currentZomb]["gameobject.transform.position"];
            temp.vec3_value = GM.zombie[currentZomb].transform.position;
            allVars[currentZomb]["gameobject.transform.position"] = temp;
            //Run start methods
            MethodRun(methods["Start()"]);
            currentZomb++; //increment current zombie
        }
        StartCoroutine(RunHelp()); //start routine to run update methods
        currentZomb = 0; //reset currentZomb
    }

    //Coroutine to run update methods to avoid infinite loops
    IEnumerator RunHelp()
    {
        while (!stop)
        {
            while (currentZomb < 3) //for each zombie
            {
                //set variable to current zombie position
                Variable temp = allVars[currentZomb]["gameobject.transform.position"];
                temp.vec3_value = GM.zombie[currentZomb].transform.position;
                allVars[currentZomb]["gameobject.transform.position"] = temp;
                //run update methods
                MethodRun(methods["Update()"]);
                currentZomb++;//increment current zombie
            }
            currentZomb = 0; //reset current zombies to 0
            yield return new WaitForEndOfFrame();
        }
    }

    //Method to run methods
    public static void MethodRun(List<Method> ms)
    {
        for (int i = 0; i < ms.Count(); ++i) //for each method given
        {
            bool output = ms[i].Compute(); //compute method
        }
    }

    //Method to scroll line numbers
    public void ScrollNumbers()
    {
        //set new line numers position
        Vector3 pos = numbers.transform.localPosition;
        pos.y = theText.transform.localPosition.y;
        numbers.transform.localPosition = pos;
    }

    //Method to Add line numbers
    private void LineNumbers()
    {
        int current = theText.textInfo.lineCount; //get current line count
        //Remove numbers until at right amount
        while (lines > current)
        {
            numbers.text = numbers.text.Substring(0, numbers.text.Length - (lines.ToString().Length + 1));
            lines--;
        }
        //add numbers until at right amount
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
        GM.console.text = "- Compile Button Pressed\n"; //output to in game console
        int currentLine = 1; //start at line 1

        //stop buttons from being interactible during compile
        GM.stop.interactable = false;
        GM.cnr.interactable = false;

        //Try to compile
        Error error = TryCompile(ref currentLine);

        if (error.errorCode != Error.ErrorCodes.None) // if there is an erroe
        {
            //set button interactability
            GM.stop.interactable = false;
            GM.cnr.interactable = true;
            compile = false;
            GM.console.text += "- " + error + "\n"; //output error
        }
        else
        {
            compile = true;
            Run(); //run code
        }
    }

    //Method to try and compile code
    private Error TryCompile(ref int lineNo)
    {
        stop = false;
        string methodType = "Start()"; //Update() or Start()
        string stringset = ""; //name of variable that will be set next
        bool ignore = false; //ignore code because of //
        bool superIgnore = false; //ignore code because of /* */
        bool inClass = false;
        bool inMethod = false;
        bool allowWhiteSpace = true; //white space allowed before next command
        bool lineUnfinished = false; //line is not finished yet
        bool initialise = false; //variable is being initialised
        bool expectCommand = false; //command is expected after \ character
        //Counts
        int character = 0; //character count
        int bracket = 0;    // { kind of brackets
        int inString = 0;   // count "
        int lilBracket = 1; // ( kind of brackets
        //Words
        string word = ""; //current word
        string closestWord = "";
        //What's happening
        curHaps = Happening.Starting; //curent part of code
        List<string> current = new List<string>(allStrings[(int)curHaps]); //list of key words that could be at this stage
        //reset variables
        vars.Clear();
        vars.Add("gameobject.transform.position", zomble);
        scopeVariables.Clear();
        methods["Update()"].Clear();
        methods["Start()"].Clear();

        Happening next = Happening.ExpectInt; //The kind of value to expect next

        string allText = input.text.Substring(monostring.Length, input.text.Length - monostring.Length); //all text

        //Loop through characters
        foreach (char c in allText)
        {
            if (c == 8203)//end of line added by text mesh pro
            {
                lineNo++;
                continue;
            }

            if (c == '\n') //new line
            {
                superIgnore = false;
                lineNo++;
            }

            if ((!allowWhiteSpace || character > 1) && (c == '\n' || (c >= 0 && c <= 31))) //if whitespace is there but not allowed
            {
                return new Error(Error.ErrorCodes.Syntax, "Too much white space found between commands", lineNo);
            }
            else if (allowWhiteSpace && character == 0 && (c == '\n' || (c >= 0 && c <= 32)))//if ignore would end
            {
                if (!ignore) //if currently in ignore
                {
                    continue;
                }
                else
                {
                    word += c; // add character to word
                }
            }
            else
            {
                word += c; //add character to word
            }


            if ((int)curHaps > allStrings.Count - 1) //if not specific key words expected
            {
                switch (curHaps)
                {
                    case Happening.ExpectWhile:
                        if (c == ')' && lilBracket == 2) //if brackets complete
                        {
                            //Remove brackets from start and end of string
                            word = word.Trim();
                            word = word.Substring(1, word.Length - 1);

                            Code_While thewhile = new Code_While(); //make new while
                            Error e = MakeIf(ref thewhile.checkCase, ref word, lineNo);//create if in while
                            //If error return errror
                            if (e.errorCode != Error.ErrorCodes.None) return e;

                            //Add method
                            List<Method> temp = methods[methodType];
                            AddMethod(ref temp, thewhile);
                            methods[methodType] = temp;

                            //Get ready for next command
                            previous = curHaps;
                            curHaps = Happening.ExpectBracket;
                            allowWhiteSpace = true;
                            word = "";
                            current = new List<string>(allStrings[(int)curHaps]);
                            character = -1;
                            lilBracket = 1;
                        }
                        else if (c == '(')
                        {
                            lilBracket++; //increment brackets
                        }
                        else if (c == ')')
                        {
                            lilBracket--; //decrement brackets
                        }
                        break;
                    case Happening.AddElse:
                        if (c == '{' && lilBracket == 1) //if else complete with no case
                        {
                            bracket++; //increment bracket
                            scopeVariables.Add(new List<string>()); //start new scope

                            //create if
                            Code_if codeif = new Code_if()
                            {
                                compareValues = Variable.VariableType.BOOL,
                                bl_lhsvalue = true,
                                bl_rhsvalue = true,
                                ifType = Code_if.Logic.EQUAL //true == true
                            };
                            //Add else to if
                            List<Method> temp = methods[methodType]; 
                            Error e = AddElse(ref temp, codeif, lineNo); 
                            methods[methodType] = temp;
                            //if error return error
                            if (e.errorCode != Error.ErrorCodes.None) return e;

                            //Get ready for next instruction
                            previous = curHaps;
                            curHaps = Happening.InMethod;
                            allowWhiteSpace = true;
                            lineUnfinished = false;
                            word = "";
                            current = new List<string>(allStrings[(int)curHaps]);
                            character = -1;
                        }
                        else if (c == ')' && lilBracket == 2) //iff else finished with case
                        {
                            //trim brackets of if
                            word = word.Trim();
                            word = word.Substring(2, word.Length - 2);
                            word = word.Trim();
                            word = word.Substring(1, word.Length - 1);

                            //make if
                            Code_if theif = new Code_if();
                            Error e = MakeIf(ref theif, ref word, lineNo);
                            //if error return error
                            if (e.errorCode != Error.ErrorCodes.None) return e;

                            //add else to if
                            List<Method> temp = methods[methodType];
                            e = AddElse(ref temp, theif, lineNo);
                            methods[methodType] = temp;

                            //if error return error
                            if (e.errorCode != Error.ErrorCodes.None) return e;

                            //get ready for next instrucion
                            previous = curHaps;
                            curHaps = Happening.ExpectBracket;
                            allowWhiteSpace = true;
                            word = "";
                            current = new List<string>(allStrings[(int)curHaps]);
                            character = -1;
                            lilBracket = 1;
                        }
                        else if (c == '(')
                        {
                            lilBracket++; //increment bracket
                        }
                        else if (c == ')')
                        {
                            lilBracket--; //decrement bracket
                        }
                        if (lilBracket == 1)
                        {
                            character = -1;
                        }
                        break;
                    case Happening.ExpectIf:
                        if (c == ')' && lilBracket == 2) //if if is complete
                        {
                            //trim brackets of if
                            word = word.Trim();
                            word = word.Substring(1, word.Length - 1);

                            //make if
                            Code_if theif = new Code_if();
                            Error e = MakeIf(ref theif, ref word, lineNo);
                            //if error return error
                            if (e.errorCode != Error.ErrorCodes.None) return e;

                            //add if method
                            List<Method> temp = methods[methodType];
                            AddMethod(ref temp, theif);
                            methods[methodType] = temp;

                            //get ready for next instruction
                            previous = curHaps;
                            curHaps = Happening.ExpectBracket;
                            allowWhiteSpace = true;
                            word = "";
                            current = new List<string>(allStrings[(int)curHaps]);
                            character = -1;
                            lilBracket = 1;
                        }
                        else if (c == '(')
                        {
                            lilBracket++; //increment bracket
                        }
                        else if (c == ')')
                        {
                            lilBracket--; //decrement bracket
                        }
                        break;
                    case Happening.DebugLog:
                        if (expectCommand) //expect escape character
                        {
                            //add correct  haracter for escape character
                            switch (c)
                            {
                                case '\\':
                                    word = word.Substring(0, word.Length - 1);
                                    break;
                                case '\"':
                                    word = word.Substring(0, word.Length - 2) + "\"";
                                    break;
                                case 'n':
                                    word = word.Substring(0, word.Length - 2) + "\n";
                                    break;
                                case 't':
                                    word = word.Substring(0, word.Length - 2) + "\t";
                                    break;
                                default:
                                    return new Error(Error.ErrorCodes.Syntax, "Unexpected character after \\", lineNo);
                            }
                            expectCommand = false;
                        }
                        else if (c == '\\') // if \
                        {
                            expectCommand = true;
                        }
                        else if (c == '\"' && inString < 2)
                        {
                            inString++; //increment "
                        }
                        else if (c == ';' && (inString == 2 || inString == 0)) //if line complete
                        {
                            if (word[word.Length - 2] != ')') //if command doesn't end in close bracket
                            {
                                return new Error(Error.ErrorCodes.Syntax, "Expected ) not found", lineNo);
                            }
                            //remove bracket and semicolon
                            word = word.Substring(0, word.Length - 2);
                            word = word.Trim();
                            if (vars.ContainsKey(word))//id variable
                            {
                                //create debug method with variable name
                                Code_Debug debug = new Code_Debug()
                                {
                                    variablename = word,
                                };
                                //add method
                                List<Method> temp = methods[methodType];
                                AddMethod(ref temp, debug);
                                methods[methodType] = temp;
                            }
                            else if (inString == 2) //if string
                            {
                                //create debug method with string
                                Code_Debug debug = new Code_Debug()
                                {
                                    content = word.Substring(1, word.Length - 2), //remove "
                                };
                                //add method
                                List<Method> temp = methods[methodType];
                                AddMethod(ref temp, debug);
                                methods[methodType] = temp;
                                inString = 0; //reset count
                            }
                            else //expect vector3 variable
                            {
                                string[] words = word.Split('.'); //split wword at .
                                //if too few or many . return error
                                if (words.Count() != 2 && words.Count() != 4) return new Error(Error.ErrorCodes.Syntax, "No variable with the name: \"" + word + "\" exsists", lineNo);

                                //make test variabe
                                int test = word.Length - 2;
                                if (test < 0) test = 0; //make sure not less than 0
                                if (vars.ContainsKey(words[0]) || word.Substring(0, test) == "gameobject.transform.position")
                                {
                                    //create debug method
                                    Code_Debug debug = new Code_Debug()
                                    {
                                        variablename = word.Substring(0, word.Length - 2),
                                    };

                                    //set correct vector part
                                    if (words[words.Length - 1] == "x")
                                    {
                                        debug.vPart = Code_if.VectorPart.x;
                                    }
                                    else if (words[words.Length - 1] == "y")
                                    {
                                        debug.vPart = Code_if.VectorPart.y;
                                    }
                                    else if (words[words.Length - 1] == "z")
                                    {
                                        debug.vPart = Code_if.VectorPart.z;
                                    }
                                    else
                                    {
                                        return new Error(Error.ErrorCodes.Syntax, words[words.Length - 1] + " is not a part of " + words[0], lineNo);
                                    }

                                    //add method
                                    List<Method> temp = methods[methodType];
                                    AddMethod(ref temp, debug);
                                    methods[methodType] = temp;
                                }
                                else
                                {
                                    return new Error(Error.ErrorCodes.Syntax, "No variable with the name: \"" + words[0] + "\" exsists", lineNo);
                                }
                            }

                            //get ready for next instructions
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
                        else if (inString > 2) //too many "
                        {
                            return new Error(Error.ErrorCodes.Syntax, "cannot output values", lineNo);
                        }
                        break;
                    case Happening.ExpectUsing:
                        if (character > 0 && c == ';') //if end of line
                        {
                            //get ready for next instruction
                            word = "";
                            character = -1;
                            curHaps = Happening.Starting;
                            current = new List<string>(allStrings[(int)Happening.Starting]);
                            allowWhiteSpace = true;
                            lineUnfinished = false;
                        }
                        else if (c != 46 && !(c >= 65 && c <= 90) && !(c >= 97 && c <= 122)) //unexpected character
                        {
                            return new Error(Error.ErrorCodes.Syntax, "Invalid character found in using declairation", lineNo);
                        }
                        break;
                    case Happening.ExpectVariableName:
                        if (c == ';')//if end of line
                        {
                            stringset = word.Substring(0, word.Length - 1); //remove ; from end
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
                                //Make sure variable is in scope
                                if (vars[stringset].inScope == true)
                                {
                                    return new Error(Error.ErrorCodes.Syntax, "cannot redefine \"" + stringset + "\"", lineNo);
                                }
                            }
                            else if (next == Happening.Starting) //variable has not been initialised
                            {
                                return new Error(Error.ErrorCodes.Syntax, "Variable not initialised", lineNo);
                            }
                            if (!vars.ContainsKey(stringset)) //if variabe does not exsist yet
                            {
                                //create variable
                                Variable theNew = new Variable
                                {
                                    inScope = true
                                };
                                //set setting variable value based on type
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
                                vars.Add(stringset, theNew); //add variable
                            }
                            else
                            {
                                if (next != Happening.ExpectVec3) //not expecting a vector3
                                {
                                    //create simpleset method
                                    Code_SimpleSet set = new Code_SimpleSet
                                    {
                                        changeType = true,
                                        output = stringset
                                    };

                                    //set setting value based on type
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
                                    //add method
                                    List<Method> temp = methods[methodType];
                                    AddMethod(ref temp, set);
                                    methods[methodType] = temp;
                                }
                                else
                                {
                                    //create vector 3 set method
                                    Code_Vec3Set set = new Code_Vec3Set
                                    {
                                        output = stringset
                                    };

                                    //add maths
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
                                if (vars[stringset].inScope == true && next != Happening.Starting)
                                {
                                    return new Error(Error.ErrorCodes.Syntax, "cannot redefine \"" + stringset + "\"", lineNo);
                                }
                                Variable temp = new Variable();
                                temp = vars[stringset];
                                temp.inScope = true;
                                vars[stringset] = temp;
                            }
                            if (!initialise && !vars.ContainsKey(stringset))
                            {
                                return new Error(Error.ErrorCodes.Syntax, "\"" + stringset + "\" does not exist to be set", lineNo);
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
                            stringset = stringset.Trim();
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
                            if (next == Happening.ExpectVec3)
                            {
                                curHaps = Happening.ExpectNew;
                                current = new List<string>(allStrings[(int)curHaps]);
                            }
                            else
                            {
                                curHaps = next;
                            }
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
                                List<Method> mtemp = methods[methodType];
                                AddMethod(ref mtemp, temp);
                                methods[methodType] = mtemp;
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

                                    List<Method> temp = methods[methodType];
                                    AddMethod(ref temp, set);
                                    methods[methodType] = temp;
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

                            float value;
                            word = word.Substring(0, word.Length - 1);
                            word = word.Trim();
                            string[] words = word.Split('.');
                            int test = word.Length - 2;
                            if (test < 0) test = 0;
                            if (word.Substring(0, test) == "gameobject.transform.position")
                            {
                                word = word.Substring(0, word.Length - 2);
                                if (!inMethod)
                                {
                                    return new Error(Error.ErrorCodes.Compiler, "Cannot initialise variable to variable outside of method", lineNo);
                                }
                                Code_SimpleSet temp = new Code_SimpleSet();

                                if (words[words.Count() - 1] == "x")
                                {
                                    temp.inPart = Code_if.VectorPart.x;
                                }
                                else if (words[words.Count() - 1] == "y")
                                {
                                    temp.inPart = Code_if.VectorPart.y;
                                }
                                else if (words[words.Count() - 1] == "z")
                                {
                                    temp.inPart = Code_if.VectorPart.z;
                                }
                                else
                                {
                                    return new Error(Error.ErrorCodes.TypeMismatch, words[words.Count() - 1] + "is not a part of " + words[0], lineNo);
                                }
                                if (vars[stringset].type != Variable.VariableType.FLOAT)
                                {
                                    temp.changeType = true;
                                    temp.newType = Variable.VariableType.FLOAT;
                                }
                                temp.output = stringset;
                                temp.input = word;
                                List<Method> mtemp = methods[methodType];
                                AddMethod(ref mtemp, temp);
                                methods[methodType] = mtemp;
                            }
                            else if (words.Count() > 2)
                            {
                                return new Error(Error.ErrorCodes.Syntax, "Too many decimal points found", lineNo);
                            }
                            else if (vars.ContainsKey(words[0]))
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
                                    else if (words[1] == "y")
                                    {
                                        temp.inPart = Code_if.VectorPart.y;
                                    }
                                    else if (words[1] == "z")
                                    {
                                        temp.inPart = Code_if.VectorPart.z;
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
                                List<Method> mtemp = methods[methodType];
                                AddMethod(ref mtemp, temp);
                                methods[methodType] = mtemp;
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

                                        List<Method> temp = methods[methodType];
                                        AddMethod(ref temp, set);
                                        methods[methodType] = temp;
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

                                    List<Method> temp = methods[methodType];
                                    AddMethod(ref temp, set);
                                    methods[methodType] = temp;
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
                            bool complete = false;
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
                                            bool_value = value,
                                            type = Variable.VariableType.BOOL
                                        };
                                        scopeVariables[scopeVariables.Count - 1].Add(stringset);
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
                                        List<Method> temp = methods[methodType];
                                        AddMethod(ref temp, set);
                                        methods[methodType] = temp;
                                    }

                                    complete = true;
                                    break;
                                }

                            }
                            if (!complete)
                            {
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
                                            bool_value = vars[word].bool_value,
                                            type = Variable.VariableType.BOOL
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
                    case Happening.ExpectString:
                        if (expectCommand)
                        {
                            switch (c)
                            {
                                case '\\':
                                    word = word.Substring(0, word.Length - 1);
                                    break;
                                case '\"':
                                    word = word.Substring(0, word.Length - 2) + "\"";
                                    break;
                                case 'n':
                                    word = word.Substring(0, word.Length - 2) + "\n";
                                    break;
                                case 't':
                                    word = word.Substring(0, word.Length - 2) + "\t";
                                    break;
                                default:
                                    return new Error(Error.ErrorCodes.Syntax, "Unexpected character after \\", lineNo);
                            }
                            expectCommand = false;
                        }
                        else if (c == '\\')
                        {
                            expectCommand = true;
                        }
                        else if (c == '\"' && inString < 2)
                        {
                            inString++;
                        }
                        else if (c == ';' && (inString == 2 || inString == 0))
                        {
                            inString = 0;
                            word = word.Substring(0, word.Length - 1);
                            word = word.Trim();

                            if (vars.ContainsKey(word))
                            {
                                if (vars[word].type != Variable.VariableType.STRING)
                                {
                                    return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert value to string", lineNo);
                                }
                                if (!inMethod)
                                {
                                    return new Error(Error.ErrorCodes.Compiler, "Varaible cannot be initialised to another variable outside of class", lineNo);
                                }

                                if (!vars.ContainsKey(stringset))
                                {
                                    Variable temp = new Variable
                                    {
                                        str_value = vars[word].str_value,
                                        inScope = true,
                                        type = Variable.VariableType.STRING
                                    };
                                    scopeVariables[scopeVariables.Count - 1].Add(stringset);
                                    vars.Add(stringset, temp);
                                }

                                if (inMethod)
                                {
                                    Code_SimpleSet set = new Code_SimpleSet
                                    {
                                        input = word,
                                        output = stringset
                                    };

                                    if (vars[stringset].type != Variable.VariableType.STRING)
                                    {
                                        set.changeType = true;
                                        set.newType = Variable.VariableType.STRING;
                                    }
                                    List<Method> temp = methods[methodType];
                                    AddMethod(ref temp, set);
                                    methods[methodType] = temp;
                                }
                            }
                            else if (word[0] == '\"' && word[word.Length - 1] == '\"')
                            {
                                if (!vars.ContainsKey(stringset))
                                {
                                    Variable temp = new Variable
                                    {
                                        str_value = word.Substring(1, word.Length - 2),
                                        inScope = true,
                                        type = Variable.VariableType.STRING
                                    };

                                    scopeVariables[scopeVariables.Count - 1].Add(stringset);
                                    vars.Add(stringset, temp);
                                }

                                if (inMethod)
                                {
                                    Code_SimpleSet set = new Code_SimpleSet
                                    {
                                        sValue = word.Substring(1, word.Length - 2),
                                        output = stringset
                                    };

                                    if (vars[stringset].type != Variable.VariableType.STRING)
                                    {
                                        set.changeType = true;
                                        set.newType = Variable.VariableType.STRING;
                                    }
                                    List<Method> temp = methods[methodType];
                                    AddMethod(ref temp, set);
                                    methods[methodType] = temp;
                                }
                            }
                            else
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert value int to string", lineNo);
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
                        else if (inString > 2)
                        {
                            return new Error(Error.ErrorCodes.Syntax, "cannot convert value into string", lineNo);
                        }
                        break;
                    case Happening.ExpectVec3:
                        if (c == '(')
                        {
                            lilBracket++;
                        }
                        else if (c == ')')
                        {
                            lilBracket--;
                        }
                        else if (c == ';' && lilBracket == 0)
                        {
                            word = word.Substring(0, word.Length - 2);
                            word = word.Trim();
                            string[] xyz = word.Split(',');
                            lilBracket = 1;
                            if (word == "")
                            {
                                if (!vars.ContainsKey(stringset))
                                {
                                    Variable temp = new Variable()
                                    {
                                        type = Variable.VariableType.VEC3,
                                        vec3_value = new Vector3(0, 0, 0),
                                        inScope = true
                                    };
                                    scopeVariables[scopeVariables.Count - 1].Add(stringset);
                                    vars.Add(stringset, temp);
                                }
                                if (inMethod)
                                {
                                    Code_Vec3Set eq = new Code_Vec3Set
                                    {
                                        output = stringset,
                                        x = new Mathematics(),
                                        y = new Mathematics(),
                                        z = new Mathematics()
                                    };
                                    List<Method> temp = methods[methodType];
                                    AddMethod(ref temp, eq);
                                    methods[methodType] = temp;
                                }
                            }
                            else if (xyz.Count() == 1)
                            {
                                if (!vars.ContainsKey(word))
                                {
                                    return new Error(Error.ErrorCodes.Syntax, "Variable does not exist", lineNo);
                                }

                                if (!vars.ContainsKey(stringset))
                                {
                                    Variable temp = new Variable()
                                    {
                                        type = Variable.VariableType.VEC3,
                                        vec3_value = vars[word].vec3_value,
                                        inScope = true
                                    };

                                    scopeVariables[scopeVariables.Count - 1].Add(stringset);
                                    vars.Add(stringset, temp);
                                }

                                if (inMethod)
                                {
                                    if (stringset == "gameobject.transform.position")
                                    {
                                        Code_SetZombie eq = new Code_SetZombie
                                        {
                                            variable = word,
                                        };
                                        List<Method> temp = methods[methodType];
                                        AddMethod(ref temp, eq);
                                        methods[methodType] = temp;
                                    }
                                    else
                                    {
                                        Code_Vec3Set eq = new Code_Vec3Set
                                        {
                                            output = stringset,
                                            input = word,
                                        };
                                        List<Method> temp = methods[methodType];
                                        AddMethod(ref temp, eq);
                                        methods[methodType] = temp;
                                    }
                                }
                            }
                            else if (xyz.Count() != 3)
                            {
                                return new Error(Error.ErrorCodes.Syntax, "Invalid Vector3 format", lineNo);
                            }
                            else
                            {

                                int bracCount = 0;
                                xyz[0] += ';';
                                xyz[1] += ';';
                                xyz[2] += ';';

                                Mathematics mathsx = new Mathematics();
                                Error e = MakeEquation(ref mathsx, ref xyz[0], lineNo, inMethod, ref bracCount);
                                if (e.errorCode != Error.ErrorCodes.None) return e;

                                Mathematics mathsy = new Mathematics();
                                e = MakeEquation(ref mathsy, ref xyz[1], lineNo, inMethod, ref bracCount);
                                if (e.errorCode != Error.ErrorCodes.None) return e;

                                Mathematics mathsz = new Mathematics();
                                e = MakeEquation(ref mathsz, ref xyz[2], lineNo, inMethod, ref bracCount);
                                if (e.errorCode != Error.ErrorCodes.None) return e;

                                if (!vars.ContainsKey(stringset))
                                {
                                    Variable temp = new Variable()
                                    {
                                        type = Variable.VariableType.VEC3,
                                        vec3_value = new Vector3(0, 0, 0),
                                        inScope = true
                                    };

                                    if (!inMethod)
                                    {
                                        temp.vec3_value.x = mathsx.Calculate();
                                        temp.vec3_value.y = mathsy.Calculate();
                                        temp.vec3_value.z = mathsz.Calculate();
                                    }

                                    scopeVariables[scopeVariables.Count - 1].Add(stringset);
                                    vars.Add(stringset, temp);
                                }

                                if (inMethod)
                                {
                                    if (stringset == "gameobject.transform.position")
                                    {
                                        return new Error(Error.ErrorCodes.Compiler, "position must be set with exsisting variable", lineNo);
                                    }
                                    else
                                    {
                                        Code_Vec3Set eq = new Code_Vec3Set
                                        {
                                            output = stringset,
                                            x = mathsx,
                                            y = mathsy,
                                            z = mathsz
                                        };
                                        List<Method> temp = methods[methodType];
                                        AddMethod(ref temp, eq);
                                        methods[methodType] = temp;
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
                                    List<Method> temp = methods[methodType];
                                    AddMethod(ref temp, eq);
                                    methods[methodType] = temp;
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
            ////////////////////// Non Special Cases //////////////////////////
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
                        if (curHaps == Happening.ExpectMethodName)
                        {

                            if (word == "Start()") methodType = word;
                            if (word == "Update()") methodType = word;
                        }
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

                        if (word == "gameobject.transform.position =" || word == "gameobject.transform.position=")
                        {
                            stringset = "gameobject.transform.position";
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

                            if (bracket < 0)
                            {
                                return new Error(Error.ErrorCodes.Syntax, "Curly brackets do not all match up", lineNo);
                            }
                            List<Method> mtemp = methods[methodType];
                            AddMethod(ref mtemp, new Code_EndIf());
                            methods[methodType] = mtemp;
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
                            case Happening.AddElse:
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
                                    curHaps = Happening.ExpectNew;
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

    private void DownElses(ref Code_if a, Code_if b)
    {
        if (a.elses == null)
        {
            a.elses = b;
        }
        else
        {
            DownElses(ref a.elses, b);
        }
    }

    private Error AddElse(ref List<Method> meths, Code_if codeif, int lineNo)
    {
        if (meths.Count > 0)
        {
            if (meths[meths.Count - 1].GetType() == typeof(Code_EndIf))
            {
                meths.Remove(meths[meths.Count - 1]);

                if (meths[meths.Count - 1].GetType() == typeof(Code_if))
                {
                    Code_if temp = (Code_if)meths[meths.Count - 1];
                    DownElses(ref temp, codeif);
                    meths[meths.Count - 1] = temp;
                }
                else if (meths[meths.Count - 1].GetType() == typeof(Code_While))
                {
                    Code_While temp = (Code_While)meths[meths.Count - 1];
                    DownElses(ref temp.checkCase, codeif);

                    meths[meths.Count - 1] = temp;
                }
                else
                {
                    return new Error(Error.ErrorCodes.Syntax, "No if to add else to", lineNo);
                }
            }
            else if (meths[meths.Count - 1].GetType() == typeof(Code_if))
            {
                return AddElse(ref meths[meths.Count - 1].methods, codeif, lineNo);
            }
            else if (meths[meths.Count - 1].GetType() == typeof(Code_While))
            {
                Code_While temp = (Code_While)meths[meths.Count - 1];
                Error e = AddElse(ref temp.checkCase.methods, codeif, lineNo);
                meths[meths.Count - 1] = temp;
                return e;
            }
            else
            {
                return new Error(Error.ErrorCodes.Syntax, "No if to add else to", lineNo);
            }
        }
        else
        {
            return new Error(Error.ErrorCodes.Syntax, "No if to add else to", lineNo);
        }
        return new Error();
    }

    private void AddElsesMethod(ref Code_if codeif, Method m)
    {
        if (codeif.elses == null)
        {
            codeif.methods.Add(m);
        }
        else
        {
            AddElsesMethod(ref codeif.elses, m);
        }
    }

    private void AddMethod(ref List<Method> meths, Method m)
    {
        if (m.GetType() == typeof(Code_EndIf))
        {
            if (meths.Count > 0)
            {
                if (meths[meths.Count - 1].GetType() == typeof(Code_While))
                {
                    Code_While temp = (Code_While)meths[meths.Count - 1];

                    if (temp.checkCase.methods.Count > 0)
                    {
                        if (temp.checkCase.methods[temp.checkCase.methods.Count - 1].GetType() == typeof(Code_if) ||
                        temp.checkCase.methods[temp.checkCase.methods.Count - 1].GetType() == typeof(Code_While))
                        {
                            AddMethod(ref temp.checkCase.methods, m);
                            meths[meths.Count - 1] = temp;
                            return;
                        }
                    }
                }
                else if (meths[meths.Count - 1].methods.Count > 0)
                {
                    if (meths[meths.Count - 1].methods[meths[meths.Count - 1].methods.Count - 1].GetType() == typeof(Code_if) ||
                    meths[meths.Count - 1].methods[meths[meths.Count - 1].methods.Count - 1].GetType() == typeof(Code_While))
                    {
                        AddMethod(ref meths[meths.Count - 1].methods, m);
                        return;
                    }
                }
            }
        }
        else
        {
            if (meths.Count > 0)
            {
                if (meths[meths.Count - 1].GetType() == typeof(Code_if))
                {
                    Code_if temp = (Code_if)meths[meths.Count - 1];

                    AddElsesMethod(ref temp, m);
                    meths[meths.Count - 1] = temp;
                    return;
                }
                if (meths[meths.Count - 1].GetType() == typeof(Code_While))
                {
                    Code_While temp = (Code_While)meths[meths.Count - 1];

                    AddMethod(ref temp.checkCase.methods, m);
                    meths[meths.Count - 1] = temp;
                    return;
                }
            }
        }
        meths.Add(m);
    }

    private Error MakeIf(ref Code_if codeif, ref string eq, int lineNo)
    {
        int partCount = 0;
        string word = "";
        bool isMaths = false;
        Mathematics maths = new Mathematics();
        bool expectCommand = false;
        int inString = 0;

        while (eq != "")
        {
            Variable.VariableType setType = Variable.VariableType.VEC3;
            char c = eq[0];
            eq = eq.Substring(1, eq.Length - 1);
            word += c;
            if (inString != 1 && c == '(')
            {
                Error e;
                if (!codeif.hasLhs)
                {
                    codeif.ifLHS = new Code_if();
                    codeif.compareValues = Variable.VariableType.BOOL;
                    e = MakeIf(ref codeif.ifLHS, ref eq, lineNo);
                    codeif.hasLhs = true;
                }
                else
                {
                    if (codeif.compareValues != Variable.VariableType.BOOL)
                    {
                        return new Error(Error.ErrorCodes.TypeMismatch, "cannot compare value with bool", lineNo);
                    }
                    codeif.ifRHS = new Code_if();
                    e = MakeIf(ref codeif.ifRHS, ref eq, lineNo);
                    codeif.hasRhs = true;
                }
                if (e.errorCode != Error.ErrorCodes.None)
                {
                    return e;
                }
                word = "";
            }
            if (inString == 1 && expectCommand)
            {
                switch (c)
                {
                    case '\\':
                        break;
                    case '\"':
                        word = word.Substring(0, word.Length - 2) + "\"";
                        break;
                    case 'n':
                        word = word.Substring(0, word.Length - 2) + "\n";
                        break;
                    case 't':
                        word = word.Substring(0, word.Length - 2) + "\t";
                        break;
                    default:
                        return new Error(Error.ErrorCodes.Syntax, "unexpected character after \\", lineNo);
                }
                expectCommand = false;
            }
            else if (c == '\\' && inString == 1)
            {
                expectCommand = true;
            }
            else if (c == '\"')
            {
                inString++;
                if (inString > 2)
                {
                    return new Error(Error.ErrorCodes.Syntax, "Too many speach marks found", lineNo);
                }
            }
            else if (c == '=' || c == '!' || c == '|' || c == '&' || c == '>' || c == '<' || c == ')')
            {
                float value = 0;
                partCount++;
                if (partCount == 2)
                {
                    if (word == "==")
                    {
                        codeif.ifType = Code_if.Logic.EQUAL;
                    }
                    else if (word == "!=")
                    {
                        codeif.ifType = Code_if.Logic.NOT;
                    }
                    else if (word == "||")
                    {
                        if (codeif.compareValues == Variable.VariableType.FLOAT || codeif.compareValues == Variable.VariableType.STRING)
                        {
                            return new Error(Error.ErrorCodes.TypeMismatch, "Value type does not work with this if operator", lineNo);
                        }
                        codeif.ifType = Code_if.Logic.OR;
                    }
                    else if (word == "&&")
                    {
                        if (codeif.compareValues == Variable.VariableType.FLOAT || codeif.compareValues == Variable.VariableType.STRING)
                        {
                            return new Error(Error.ErrorCodes.TypeMismatch, "Value type does not work with this if operator", lineNo);
                        }
                        codeif.ifType = Code_if.Logic.AND;
                    }
                    else
                    {
                        return new Error(Error.ErrorCodes.Syntax, "If operator not recognised", lineNo);
                    }
                    partCount = 0;
                    word = "";
                }
                else
                {
                    word = word.Substring(0, word.Length - 1);
                    if (c == '>' && codeif.ifType == Code_if.Logic.NONE)
                    {
                        if (codeif.compareValues == Variable.VariableType.BOOL || codeif.compareValues == Variable.VariableType.STRING)
                        {
                            return new Error(Error.ErrorCodes.TypeMismatch, "Value type does not work with this if operator", lineNo);
                        }
                        codeif.ifType = Code_if.Logic.MORETHAN;
                        partCount = 0;
                    }
                    else if (c == '<' && codeif.ifType == Code_if.Logic.NONE)
                    {
                        if (codeif.compareValues == Variable.VariableType.BOOL || codeif.compareValues == Variable.VariableType.STRING)
                        {
                            return new Error(Error.ErrorCodes.TypeMismatch, "Value type does not work with this if operator", lineNo);
                        }
                        codeif.ifType = Code_if.Logic.LESSTHAN;
                        partCount = 0;
                    }
                    word = word.Trim();
                    string[] words = word.Split('.');

                    if (float.TryParse(word, out value) && words.Count() == 2)
                    {
                        return new Error(Error.ErrorCodes.TypeMismatch, "do not understand type", lineNo);
                    }
                    if (float.TryParse(word.Substring(0, word.Length - 1), out value) && words.Count() == 2 && word[word.Length - 1] == 'f')
                    {
                        word = word.Substring(0, word.Length - 1);
                    }
                    int test = word.Length - 2;
                    if (test < 0) test = 0;
                    if (vars.ContainsKey(words[0]) || word.Substring(0, test) == "gameobject.transform.position")
                    {
                        if (words.Count() == 1)
                        {
                            if (!codeif.hasLhs)
                            {
                                if (vars[words[0]].type == Variable.VariableType.FLOAT ||
                                    vars[words[0]].type == Variable.VariableType.INT)
                                    codeif.compareValues = Variable.VariableType.FLOAT;
                                else codeif.compareValues = vars[word].type;
                                codeif.lhs = word;
                                codeif.hasLhs = true;
                            }
                            else
                            {
                                if (((c == '<' || c == '>') && codeif.compareValues != Variable.VariableType.BOOL) &&
                                    (codeif.compareValues != vars[words[0]].type ||
                                    (codeif.compareValues == Variable.VariableType.FLOAT && vars[words[0]].type == Variable.VariableType.INT)))
                                {
                                    return new Error(Error.ErrorCodes.TypeMismatch, "cannot compare these values of different types", lineNo);
                                }
                                if (vars[words[0]].type == Variable.VariableType.STRING ||
                                    vars[words[0]].type == Variable.VariableType.BOOL)
                                    setType = vars[words[0]].type;
                                else
                                    setType = Variable.VariableType.FLOAT;
                                codeif.rhs = word;
                                codeif.hasRhs = true;
                            }
                        }
                        else if (words.Count() == 2 || words.Count() == 4)
                        {
                            if (words[words.Length - 1] == "x")
                            {
                                if (!codeif.hasLhs)
                                {
                                    codeif.leftV = Code_if.VectorPart.x;
                                }
                                else
                                {
                                    codeif.rightV = Code_if.VectorPart.x;
                                }
                            }
                            else if (words[words.Length - 1] == "y")
                            {
                                if (!codeif.hasLhs)
                                {
                                    codeif.leftV = Code_if.VectorPart.y;
                                }
                                else
                                {
                                    codeif.rightV = Code_if.VectorPart.y;
                                }
                            }
                            else if (words[words.Length - 1] == "z")
                            {
                                if (!codeif.hasLhs)
                                {
                                    codeif.leftV = Code_if.VectorPart.z;
                                }
                                else
                                {
                                    codeif.rightV = Code_if.VectorPart.z;
                                }
                            }
                            else
                            {
                                return new Error(Error.ErrorCodes.Syntax, words[words.Length - 1] + " is not a part of " + words[0], lineNo);
                            }

                            if (!codeif.hasLhs)
                            {
                                codeif.compareValues = Variable.VariableType.FLOAT;
                                if (word.Substring(0, test) == "gameobject.transform.position") codeif.lhs = word.Substring(0, test);
                                else codeif.lhs = words[0];
                                codeif.hasLhs = true;
                            }
                            else
                            {
                                if (((c == '<' || c == '>') && codeif.compareValues != Variable.VariableType.BOOL) &&
                                    codeif.compareValues != Variable.VariableType.FLOAT)
                                {
                                    return new Error(Error.ErrorCodes.TypeMismatch, "cannot compare these values of different types", lineNo);
                                }
                                setType = Variable.VariableType.FLOAT;
                                if (word.Substring(0, test) != "gameobject.transform.position")
                                {
                                    if (vars[words[0]].type == Variable.VariableType.STRING ||
                                        vars[words[0]].type == Variable.VariableType.BOOL)
                                        setType = vars[words[0]].type;
                                }
                                if (word.Substring(0, test) == "gameobject.transform.position") codeif.rhs = word.Substring(0, test);
                                else codeif.rhs = words[0];
                                codeif.hasRhs = true;
                            }
                        }
                    }
                    else if (word[0] == '\"' && word[word.Length - 1] == '\"')
                    {
                        if (inString > 2)
                        {
                            return new Error(Error.ErrorCodes.Syntax, "Too many speach marks found", lineNo);
                        }

                        codeif.compareValues = Variable.VariableType.STRING;
                        if (!codeif.hasLhs)
                        {
                            codeif.str_lhsvalue = word.Substring(1, word.Length - 2);
                            codeif.hasLhs = true;
                        }
                        else
                        {
                            if (((c == '<' || c == '>') && codeif.compareValues != Variable.VariableType.BOOL) &&
                                codeif.compareValues != Variable.VariableType.STRING)
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, "cannot compare these values of different types", lineNo);
                            }
                            setType = Variable.VariableType.STRING;
                            codeif.str_rhsvalue = word.Substring(1, word.Length - 2);
                            codeif.hasRhs = true;
                        }

                        inString = 0;
                    }
                    else if (isMaths)
                    {
                        int b = 0;
                        maths = new Mathematics();
                        word += ';';
                        Error e = MakeEquation(ref maths, ref word, lineNo, true, ref b);
                        if (e.errorCode != Error.ErrorCodes.None) return e;

                        if (!codeif.hasLhs)
                        {
                            codeif.compareValues = Variable.VariableType.FLOAT;
                            codeif.mathLHS = maths;
                            codeif.hasLhs = true;
                        }
                        else
                        {
                            if (((c == '<' || c == '>') && codeif.compareValues != Variable.VariableType.BOOL) &&
                                codeif.compareValues != Variable.VariableType.FLOAT)
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, "cannot compare these values of different types", lineNo);
                            }
                            setType = Variable.VariableType.FLOAT;
                            codeif.mathRHS = maths;
                            codeif.hasRhs = true;
                        }
                        isMaths = false;
                    }
                    else if (float.TryParse(word, out value))
                    {
                        if (!codeif.hasLhs)
                        {
                            codeif.compareValues = Variable.VariableType.FLOAT;
                            codeif.nbr_lhsvalue = value;
                            codeif.hasLhs = true;
                        }
                        else
                        {
                            if (((c == '<' || c == '>') && codeif.compareValues != Variable.VariableType.BOOL) ||
                                codeif.compareValues != Variable.VariableType.FLOAT)
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, "cannot compare these values of different types", lineNo);
                            }
                            setType = Variable.VariableType.FLOAT;
                            codeif.nbr_rhsvalue = value;
                            codeif.hasRhs = true;
                        }
                    }
                    else if (word == "true" || word == "false")
                    {
                        bool boolValue = false;
                        if (word == "true")
                        {
                            boolValue = true;
                        }
                        if (!codeif.hasLhs)
                        {
                            codeif.bl_lhsvalue = boolValue;
                            codeif.nbr_lhsvalue = value;
                            codeif.hasLhs = true;
                        }
                        else
                        {
                            if (((c == '<' || c == '>') && codeif.compareValues != Variable.VariableType.BOOL) ||
                                codeif.compareValues != Variable.VariableType.BOOL)
                            {
                                return new Error(Error.ErrorCodes.TypeMismatch, "cannot compare these values of different types", lineNo);
                            }
                            setType = Variable.VariableType.BOOL;
                            codeif.bl_rhsvalue = boolValue;
                            codeif.hasRhs = true;
                        }
                    }
                    else
                    {
                        return new Error(Error.ErrorCodes.Syntax, "Value cannot be parsed", lineNo);
                    }

                    if (c == ')')
                    {
                        if (codeif.ifType == Code_if.Logic.NONE)
                        {
                            return new Error(Error.ErrorCodes.Syntax, "If operator not found", lineNo);
                        }

                        return new Error();
                    }
                    else if (codeif.hasLhs && codeif.hasRhs)
                    {
                        Code_if temp = new Code_if();

                        word = "" + c + eq[0];

                        if (c == '>')
                        {
                            temp.ifType = Code_if.Logic.MORETHAN;
                            temp.compareValues = setType;
                            codeif.SetLHSToRHS(ref temp);
                            Error e = MakeIf(ref temp, ref eq, lineNo);
                            if (e.errorCode != Error.ErrorCodes.None) return e;
                            codeif.ifRHS = temp;
                        }
                        else if (c == '<')
                        {
                            temp.ifType = Code_if.Logic.LESSTHAN;
                            temp.compareValues = setType;
                            codeif.SetLHSToRHS(ref temp);
                            Error e = MakeIf(ref temp, ref eq, lineNo);
                            if (e.errorCode != Error.ErrorCodes.None) return e;
                            codeif.ifRHS = temp;
                        }
                        else if (word == "==")
                        {
                            temp.ifLHS = codeif;
                            temp.hasLhs = true;
                            temp.ifType = Code_if.Logic.EQUAL;
                            temp.compareValues = Variable.VariableType.BOOL;
                            eq = eq.Substring(1, eq.Length - 1);
                            codeif = temp;
                        }
                        else if (word == "!=")
                        {
                            temp.ifLHS = codeif;
                            temp.hasLhs = true;
                            temp.ifType = Code_if.Logic.NOT;
                            temp.compareValues = Variable.VariableType.BOOL;
                            eq = eq.Substring(1, eq.Length - 1);
                            codeif = temp;
                        }
                        else if (word == "||")
                        {
                            temp.ifLHS = codeif;
                            temp.hasLhs = true;
                            temp.ifType = Code_if.Logic.OR;
                            temp.compareValues = Variable.VariableType.BOOL;
                            eq = eq.Substring(1, eq.Length - 1);
                            codeif = temp;
                        }
                        else if (word == "&&")
                        {
                            temp.ifLHS = codeif;
                            temp.hasLhs = true;
                            temp.ifType = Code_if.Logic.AND;
                            temp.compareValues = Variable.VariableType.BOOL;
                            eq = eq.Substring(1, eq.Length - 1);
                            codeif = temp;
                        }
                        else
                        {
                            return new Error(Error.ErrorCodes.Syntax, "If operator not recognised", lineNo);
                        }
                        partCount = 0;
                        word = "";
                    }
                    else
                    {
                        if (c != '<' && c != '>')
                            word = "" + c;
                        else
                            word = "";
                    }
                }
            }
            else if (c == '+' || c == '-' || c == '/' || c == '*' || c == ')')
            {
                isMaths = true;
            }
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
                if (words.Count() > 2 && words.Count() != 4)
                {
                    return new Error(Error.ErrorCodes.Mathematical, "Too many decimal places", lineNo);
                }

                if (!((maths.op == Mathematics.Operator.MINUS && c == '+') ||
                    (maths.op == Mathematics.Operator.DIVIDE && c == '*') ||
                    ((maths.op == Mathematics.Operator.TIMES || maths.op == Mathematics.Operator.DIVIDE) && (c == '+' || c == '-'))) && maths.op != Mathematics.Operator.NONE)
                {
                    Mathematics temp = new Mathematics
                    {
                        lhs = maths.rhs,
                        lhsComplete = maths.rhsComplete
                    };
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
                                    return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert variable type to float or int", lineNo);
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
                                return new Error(Error.ErrorCodes.Mathematical, "Invalid characters in expected float or int", lineNo);
                            }
                        }
                        if (words.Count() == 2 || words.Count() == 4)
                        {
                            int test = word.Length - 2;
                            if (test < 0) test = 0;
                            if (vars.ContainsKey(words[0]) || word.Substring(0, test) == "gameobject.transform.position")
                            {
                                if (!inMethod)
                                {
                                    return new Error(Error.ErrorCodes.Compiler, "Cannot initialise variable with a variable outside of method", lineNo);
                                }

                                if (vars[words[0]].type == Variable.VariableType.VEC3 || word.Substring(0, test) == "gameobject.transform.position")
                                {
                                    if (temp.lhsComplete)
                                    {
                                        temp.rhsComplete = true;
                                        if (word.Substring(0, test) == "gameobject.transform.position") temp.varRHS = word.Substring(0, word.Length - 2);
                                        else temp.varRHS = words[0];

                                        if (words[word.Length - 1] == "x")
                                        {
                                            temp.vectorRHS = Code_if.VectorPart.x;
                                        }
                                        else if (words[word.Length - 1] == "y")
                                        {
                                            temp.vectorRHS = Code_if.VectorPart.y;
                                        }
                                        else if (words[word.Length - 1] == "z")
                                        {
                                            temp.vectorRHS = Code_if.VectorPart.z;
                                        }
                                        else
                                        {
                                            return new Error(Error.ErrorCodes.InGame, words[word.Length - 1] + "is not part of " + words[0], lineNo);
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
                                return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert variable type to float or int", lineNo);
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
                            return new Error(Error.ErrorCodes.Mathematical, "Invalid characters in expected float or int", lineNo);
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
                            int test = word.Length - 2;
                            if (test < 0) test = 0;
                            if (vars[words[0]].type == Variable.VariableType.VEC3 || word.Substring(0, test) == "gameobject.transform.position")
                            {
                                if (maths.lhsComplete)
                                {
                                    maths.rhsComplete = true;
                                    if (word.Substring(0, test) == "gameobject.transform.position") maths.varRHS = word.Substring(0, word.Length - 2);
                                    else maths.varRHS = words[0];

                                    if (words[word.Length - 1] == "x")
                                    {
                                        maths.vectorRHS = Code_if.VectorPart.x;
                                    }
                                    else if (words[word.Length - 1] == "y")
                                    {
                                        maths.vectorRHS = Code_if.VectorPart.y;
                                    }
                                    else if (words[word.Length - 1] == "z")
                                    {
                                        maths.vectorRHS = Code_if.VectorPart.z;
                                    }
                                    else
                                    {
                                        return new Error(Error.ErrorCodes.InGame, words[word.Length - 1] + "is not part of " + words[0], lineNo);
                                    }
                                }
                                else
                                {
                                    maths.lhsComplete = true;
                                    if (word.Substring(0, test) == "gameobject.transform.position") maths.varLHS = word.Substring(0, word.Length - 2);
                                    else maths.varLHS = words[0];
                                    if (words[words.Length - 1] == "x")
                                    {
                                        maths.vectorLHS = Code_if.VectorPart.x;
                                    }
                                    else if (words[words.Length - 1] == "y")
                                    {
                                        maths.vectorLHS = Code_if.VectorPart.y;
                                    }
                                    else if (words[words.Length - 1] == "z")
                                    {
                                        maths.vectorLHS = Code_if.VectorPart.z;
                                    }
                                    else
                                    {
                                        return new Error(Error.ErrorCodes.InGame, words[words.Length - 1] + "is not part of " + words[0], lineNo);
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
                    if (words.Count() > 2 && words.Count() != 4)
                    {
                        return new Error(Error.ErrorCodes.TypeMismatch, "Too many decimal points", lineNo);
                    }
                    int test = word.Length - 2;
                    if (test < 0) test = 0;
                    if (vars.ContainsKey(words[0]) || word.Substring(0, test) == "gameobject.transform.position")
                    {
                        if (!inMethod)
                        {
                            return new Error(Error.ErrorCodes.Compiler, "Cannot initialise variable with a variable outside of method", lineNo);
                        }

                        if (word.Substring(0, test) == "gameobject.transform.position" || vars[words[0]].type == Variable.VariableType.VEC3)
                        {
                            if (words.Count() != 2 && words.Count() != 4)
                            {
                                return new Error(Error.ErrorCodes.Mathematical, "cannot convert vector3 into float", lineNo);
                            }

                            if (maths.lhsComplete)
                            {
                                maths.rhsComplete = true;
                                if (word.Substring(0, test) == "gameobject.transform.position") maths.varRHS = word.Substring(0, word.Length - 2);
                                else maths.varRHS = words[0];

                                if (words[words.Length - 1] == "x")
                                {
                                    maths.vectorRHS = Code_if.VectorPart.x;
                                }
                                else if (words[words.Length - 1] == "y")
                                {
                                    maths.vectorRHS = Code_if.VectorPart.y;
                                }
                                else if (words[words.Length - 1] == "z")
                                {
                                    maths.vectorRHS = Code_if.VectorPart.z;
                                }
                                else
                                {
                                    return new Error(Error.ErrorCodes.InGame, words[words.Length - 1] + "is not part of " + words[0], lineNo);
                                }
                            }
                            else
                            {
                                maths.lhsComplete = true;
                                if (word.Substring(0, test) == "gameobject.transform.position") maths.varLHS = word.Substring(0, word.Length - 2);
                                else maths.varLHS = words[0];
                                if (words[words.Length - 1] == "x")
                                {
                                    maths.vectorLHS = Code_if.VectorPart.x;
                                }
                                else if (words[words.Length - 1] == "y")
                                {
                                    maths.vectorLHS = Code_if.VectorPart.y;
                                }
                                else if (words[words.Length - 1] == "z")
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
                                return new Error(Error.ErrorCodes.TypeMismatch, "Cannot convert value to float or int", lineNo);
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
