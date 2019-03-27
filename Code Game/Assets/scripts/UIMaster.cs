using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Class to deal with UI elements
public class UIMaster : MonoBehaviour
{
    [SerializeField]
    Overlay over;

    [SerializeField]
    GameObject helpscreen;

    // Update is called once per frame
    public void Update()
    {
        if (SceneManager.GetActiveScene().name == "Credits") //if in the credits scene
        {
            //Continue when key pressed
            if (Input.anyKeyDown)
            {
                Continue();
            }
        }
    }

    //Method that loads the scene with the current scene number
    public void Begin()
    {
        SceneManager.LoadScene(NoDestroy.sceneNumber);
    }

    //Method to move onto the next scene
    public void Continue()
    {
        NoDestroy.sceneNumber = SceneManager.GetActiveScene().buildIndex + 1;
        Begin();
    }

    //Method to quit the application
    public void Exit()
    {
        Application.Quit();
    }

    //Method to load the menu scene
    public void Menu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    //Method to open the help menu
    public void Help()
    {
        helpscreen.transform.localPosition = new Vector3(0, 0, 0);
        if (!NoDestroy.helpButton) //if the user has never opened the help menu before
        {
            //Read dialogue for first time
            over.dialogueFile = "BeginHelp";
            over.Read();
            NoDestroy.helpButton = true;
        }
    }

    //Method to go back from the helpscreen to the main screen
    public void Back()
    {
        helpscreen.transform.localPosition = new Vector3(0, -1112, 0);
    }

    //Method to read help with compiling
    public void CompileHelp()
    {
        over.dialogueFile = "Compile";
        over.Read();
    }

    //Method to read help with stopping
    public void StopHelp()
    {
        over.dialogueFile = "Stop";
        over.Read();
    }

    //Method to read help with Using instructions
    public void UsingHelp()
    {
        over.dialogueFile = "Using";
        over.Read();
    }

    //Method to read help with new lines and other escape characters
    public void NewLineHelp()
    {
        over.dialogueFile = "newline";
        over.Read();
    }

    //Method to read help with classes
    public void ClassHelp()
    {
        over.dialogueFile = "ZombieClass";
        over.Read();
    }

    //Method to read help with the void keyword
    public void MethodHelp()
    {
        over.dialogueFile = "void";
        over.Read();
    }

    //Method to read help with strings
    public void StringHelp()
    {
        over.dialogueFile = "String";
        over.Read();
    }

    //Method to read help with debuging
    public void DebugHelp()
    {
        over.dialogueFile = "Debug";
        over.Read();
    }

    //Method to read help with variables
    public void VaraibleHelp()
    {
        over.dialogueFile = "Variable";
        over.Read();
    }

    //Method to read help with commenting
    public void CommentHelp()
    {
        over.dialogueFile = "Comment";
        over.Read();
    }

    //Method to read help with brackets
    public void BracketHelp()
    {
        over.dialogueFile = "Brackets";
        over.Read();
    }

    //Method to read help with semicolons
    public void SemiColonHelp()
    {
        over.dialogueFile = "SemiColon";
        over.Read();
    }

    //Method to read help with Errors
    public void ErrorHelp()
    {
        over.dialogueFile = "Error";
        over.Read();
    }

    //Method to read help with Unity
    public void UnityHelp()
    {
        over.dialogueFile = "Unity";
        over.Read();
    }

    //Method to read help with int variables
    public void IntHelp()
    {
        over.dialogueFile = "Int";
        over.Read();
    }

    //Method to read help with If statements
    public void IfHelp()
    {
        over.dialogueFile = "If";
        over.Read();
    }

    //Method to read help with while loops
    public void WhileHelp()
    {
        over.dialogueFile = "While";
        over.Read();
    }

    //Method to read help with writing maths
    public void MathsHelp()
    {
        over.dialogueFile = "Maths";
        over.Read();
    }

    //Method to read help with float variables
    public void FloatHelp()
    {
        over.dialogueFile = "Float";
        over.Read();
    }

    //Method to read help with vector 3s
    public void Vec3Help()
    {
        over.dialogueFile = "Vec3";
        over.Read();
    }

    //Method to read help with booleans
    public void BoolHelp()
    {
        over.dialogueFile = "Bool";
        over.Read();
    }

    //Method to read help with transform positions
    public void TransformHelp()
    {
        over.dialogueFile = "ZombiePos";
        over.Read();
    }

    //Method to read help with the start method
    public void StartHelp()
    {
        over.dialogueFile = "StartMethod";
        over.Read();
    }

    //Method to read help with the update method
    public void UpdateHelp()
    {
        over.dialogueFile = "UpdateMethod";
        over.Read();
    }

    //Method to read help with the end of the game
    public void EndHelp()
    {
        over.dialogueFile = "theEnd";
        over.Read();
    }
}