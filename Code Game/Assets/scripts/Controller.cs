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
    public bool pub;
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
        {"vector3 ", Happening.ExpectVariableName},
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
    };

    private List<List<string>> scopeVariables = new List<List<string>>();

    //monospace tag
    const string monostring = "<mspace=1.2em><noparse>";

    bool ignore = false;
    bool superIgnore = false;
    bool inClass = false;
    bool inMethod = false;
    bool allowNewLines = false;


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
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "int ", "string ", "bool ", "float ", "vector3 ", "void " });  //PublicPrivate
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "Start()", "Update()" });  //ExpectMethodName
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "Zombie", "Player", "GameMaster" });  //ExpectClassName
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { ": Monobehaviour", ":Monobehaviour" });  //MonoBehaviour
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "public ", "private ", "int ", "string ", "bool ", "float ", "vector3 ", "}", "//", "/*", "{" });  //InClass
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "int ", "string ", "bool ", "float", "vector3 ", "if", "while", "return", "for", "}", "//", "/*", "{" });  //InMethod
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "{", "//", "/*" });  //ExpectBracket
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { ";" });  //ExpectSemiColon
        allStrings.Add(new List<string>());
        allStrings[allStrings.Count - 1] = new List<string>(new string[] { "=", ";" });  //ExpectEquals

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
        bool wait = false;

        for (int i = 0; i < methods.Count(); ++i)
        {
            bool output = methods[i].Compute();

            if (methods[i].GetType() == typeof(Code_if))
            {

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
        int character = 0;
        int bracket = 0;
        string word = "";
        string closestWord = "";
        curHaps = Happening.Starting;
        List<string> current = allStrings[(int)curHaps];

        foreach (char c in input.text)
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

            word += c;
            if ((int)curHaps > allStrings.Count - 1)
            {

            }
            else
            {
                foreach (string s in current)
                {
                    if (c != s[character])
                    {
                        if (ignore || superIgnore)
                        {
                            word = "";
                        }
                        else
                        {
                            current.Remove(s);
                        }
                    }
                    else if (word == s)
                    {
                        if (ignore) ignore = false;
                        if (superIgnore) superIgnore = false;

                        previous = curHaps;
                        curHaps = keys[word];
                        if ((int)curHaps < allStrings.Count)
                        {
                            current = allStrings[(int)curHaps];
                        }

                        switch (curHaps)
                        {
                            case Happening.Ignore:
                                ignore = true;
                                break;
                            case Happening.SuperIgnore:
                                superIgnore = true;
                                break;
                            case Happening.Starting:
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
                        }
                    }
                    else
                    {
                        closestWord = s;
                    }

                }
                if (current.Count == 0)
                {
                    return new Error(Error.ErrorCodes.Syntax, "word \"" + word + "\" not understood. Closest word " + closestWord, lineNo);
                }
            }

        }

        return new Error();
    }

}
