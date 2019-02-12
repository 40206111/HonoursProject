using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Error
{
    public enum ErrorCodes
    {
        None,
        Syntax,
        InGame,      //errors that occur due to not having the full functionality of C#
        Mathematical
    };

    ErrorCodes errorCode = ErrorCodes.None;
    string message = "";
    int lineNo = 0;

    public Error() { }

    //constructor that takes parameters
    public Error(ErrorCodes e, string m, int l)
    {
        errorCode = e;
        message = m;
        lineNo = l;
    }

    public override string ToString()
    {
        string output = message;

        if (output == "")
        {

            switch (errorCode)
            {
                case ErrorCodes.Syntax:
                    output = "There is a typo in this code";
                    break;
            }
        }

        if (lineNo == 0)
            output += "\nat line number: UNKNOWN";
        else
            output += "\nat line number: " + lineNo;

        return output;
    }
}