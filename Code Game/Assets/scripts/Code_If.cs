using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Code_if : Method
{

    private enum Logic
    {
        AND,
        OR,
        NOT,
        EQUAL,
        LESSTHAN,
        MORETHAN
    }

    public enum Type
    {
        Int,
        Bool,
        Float,
        String,
        IF
    }

    private Logic ifType = Logic.EQUAL;
    public Code_if ifLHS = null;
    public Code_if ifRHS = null;
    public Type typeLHS;
    public Type typeRHS;
    public string contentLHS = "";
    public string contentRHS = "";
    public bool boolLHS = false;
    public bool boolRHS = false;
    public int intLHS;
    public int intRHS;
    public string stringLHS;
    public string stringRHS;
    public float floatLHS;
    public float floatRHS;


    public override bool Compute()
    {
        return getValue();
    }

    public bool getValue()
    {
        bool boolCompare = false;

        //set LHS
        if (typeLHS == Type.IF)
        {
            boolLHS = ifLHS.getValue();
            boolCompare = true;
        }
        else if (typeLHS == Type.Bool)
        {
            if (Controller.bools.ContainsKey(contentLHS))
            {
                boolLHS = Controller.bools[contentLHS];
            }
            boolCompare = true;
        }
        else if (typeLHS == Type.Float)
        {
            if (Controller.floats.ContainsKey(contentLHS))
            {
                floatLHS = Controller.floats[contentLHS];
            }
        }
        else if (typeLHS == Type.Int)
        {
            if (Controller.ints.ContainsKey(contentLHS))
            {
                intLHS = Controller.ints[contentLHS];
            }
        }
        else if (typeLHS == Type.String)
        {
            if (Controller.strings.ContainsKey(contentLHS))
            {
                stringLHS = Controller.strings[contentLHS];
            }
        }

        //Set RHS
        if (typeRHS == Type.IF)
        {
            boolRHS = ifRHS.getValue();
            boolCompare = true;
        }
        else if (typeRHS == Type.Bool)
        {
            if (Controller.bools.ContainsKey(contentRHS))
            {
                boolRHS = Controller.bools[contentRHS];
            }
            boolCompare = true;
        }
        else if (typeRHS == Type.Float)
        {
            if (Controller.floats.ContainsKey(contentRHS))
            {
                floatRHS = Controller.floats[contentRHS];
            }
        }
        else if (typeRHS == Type.Int)
        {
            if (Controller.ints.ContainsKey(contentRHS))
            {
                intRHS = Controller.ints[contentRHS];
            }
        }
        else if (typeRHS == Type.String)
        {
            if (Controller.strings.ContainsKey(contentRHS))
            {
                stringRHS = Controller.strings[contentRHS];
            }
        }

        if (boolCompare)
        {
            switch (ifType)
            {
                case Logic.AND:
                    return (boolLHS && boolRHS);
                case Logic.EQUAL:
                    return (boolLHS == boolRHS);
                case Logic.NOT:
                    return (boolLHS != boolRHS);
                case Logic.OR:
                    return (boolLHS || boolRHS);
            }
        }
        else if (typeLHS == Type.Float && typeRHS == Type.Float)
        {
            switch (ifType)
            {
                case Logic.EQUAL:
                    return (floatLHS == floatRHS);
                case Logic.NOT:
                    return (floatLHS != floatRHS);
                case Logic.LESSTHAN:
                    return (floatLHS < floatRHS);
                case Logic.MORETHAN:
                    return (floatLHS > floatRHS);
            }
        }
        else if (typeLHS == Type.Float && typeRHS == Type.Int)
        {
            switch (ifType)
            {
                case Logic.EQUAL:
                    return (floatLHS == intRHS);
                case Logic.NOT:
                    return (floatLHS != intRHS);
                case Logic.LESSTHAN:
                    return (floatLHS < intRHS);
                case Logic.MORETHAN:
                    return (floatLHS > intRHS);
            }
        }
        else if (typeLHS == Type.Int && typeRHS == Type.Float)
        {
            switch (ifType)
            {
                case Logic.EQUAL:
                    return (intLHS == floatRHS);
                case Logic.NOT:
                    return (intLHS != floatRHS);
                case Logic.LESSTHAN:
                    return (intLHS < floatRHS);
                case Logic.MORETHAN:
                    return (intLHS > floatRHS);
            }
        }
        else if (typeLHS == Type.Int && typeRHS == Type.Int)
        {
            switch (ifType)
            {
                case Logic.EQUAL:
                    return (intLHS == intRHS);
                case Logic.NOT:
                    return (intLHS != intRHS);
                case Logic.LESSTHAN:
                    return (intLHS < intRHS);
                case Logic.MORETHAN:
                    return (intLHS > intRHS);
            }
        }
        else if (typeLHS == Type.String && typeRHS == Type.String)
        {
            switch (ifType)
            {
                case Logic.EQUAL:
                    return (stringLHS == stringRHS);
                case Logic.NOT:
                    return (stringLHS != stringRHS);
            }
        }

        return false;
    }
}
