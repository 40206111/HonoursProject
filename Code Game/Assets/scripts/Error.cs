using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Error
{
    //enum for different error types
    public enum ErrorCodes
    {
        None,
        Syntax,
        InGame,      //errors that occur due to not having the full functionality of C#
        Mathematical,
        Compiler,
        TypeMismatch
    };

    //Variable declairation
    public ErrorCodes errorCode = ErrorCodes.None;
    string message = "";
    int lineNo = 0;

    public Error() { } //empty constructor

    //constructor that takes parameters
    public Error(ErrorCodes e, string m, int l)
    {
        errorCode = e;
        message = m;
        lineNo = l;
    }

    //Overriding the ToString Method
    public override string ToString()
    {
        string output = "ERROR CODE:" + errorCode + ": " +  message; //set output

        if (message == "") //if there is no message
        {
            //Give generic error message based on type
            switch (errorCode)
            {
                case ErrorCodes.Syntax:
                    output = "There is a typo in the code";
                    break;
                case ErrorCodes.InGame:
                    output = "This would work in standard C# but not in this game";
                    break;
                case ErrorCodes.Mathematical:
                    output = "There is a problem in understanding a maths equation";
                    break;
                case ErrorCodes.Compiler:
                    output = "There are problems with the layout of your code";
                    break;
                case ErrorCodes.TypeMismatch:
                    output = "The type that is trying to be set with is not the same as the type trying to be set to";
                    break;
            }
        }

        //Add line number
        if (lineNo == 0)
            output += "\nat line number: UNKNOWN";
        else
            output += "\nat line number: " + lineNo;

        return output;
    }
}