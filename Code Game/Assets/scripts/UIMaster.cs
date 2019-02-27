using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMaster : MonoBehaviour
{
    [SerializeField]
    Overlay over;

    [SerializeField]
    GameObject helpscreen;

    public void Begin()
    {
        SceneManager.LoadScene(NoDestroy.sceneNumber);
    }

    public void Continue()
    {
        NoDestroy.sceneNumber = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(NoDestroy.sceneNumber);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Menu()
    {        SceneManager.LoadScene("MainMenu");
    }

    public void Help()
    {
        helpscreen.transform.localPosition = new Vector3(0, 0, 0);
        if (!NoDestroy.helpButton)
        {
            over.dialogueFile = "BeginHelp";
            over.Read();
            NoDestroy.helpButton = true;
        }
    }

    public void Back()
    {
        helpscreen.transform.localPosition = new Vector3(0, -1074, 0);
    }

    public void CompileHelp()
    {
        over.dialogueFile = "Compile";
        over.Read();
    }

    public void StopHelp()
    {
        over.dialogueFile = "Stop";
        over.Read();
    }

    public void UsingHelp()
    {
        over.dialogueFile = "Using";
        over.Read();
    }

    public void NewLineHelp()
    {
        over.dialogueFile = "newline";
        over.Read();
    }

    public void ClassHelp()
    {
        over.dialogueFile = "ZombieClass";
        over.Read();
    }

    public void MethodHelp()
    {
        over.dialogueFile = "void";
        over.Read();
    }
}
