using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Method
{
    public abstract bool Compute();

}

public class Code_Debug : Method
{
    public string variablename;
    public Code_if.VectorPart vPart = Code_if.VectorPart.none;
    public string content;

    public override bool Compute()
    {
        if (variablename != "")
        {
            switch (Controller.vars[variablename].type)
            {
                case Variable.VariableType.BOOL:
                    GM.console.text += Controller.vars[variablename].bool_value + "\n";
                    break;
                case Variable.VariableType.FLOAT:
                    GM.console.text += Controller.vars[variablename].flt_value + "\n";
                    break;
                case Variable.VariableType.INT:
                    GM.console.text += Controller.vars[variablename].int_value + "\n";
                    break;
                case Variable.VariableType.STRING:
                    GM.console.text += Controller.vars[variablename].str_value + "\n";
                    break;
                case Variable.VariableType.VEC3:
                    if (vPart == Code_if.VectorPart.none)
                        GM.console.text += Controller.vars[variablename].vec3_value + "\n";
                    else if (vPart == Code_if.VectorPart.x)
                        GM.console.text += Controller.vars[variablename].vec3_value.x + "\n";
                    else if (vPart == Code_if.VectorPart.y)
                        GM.console.text += Controller.vars[variablename].vec3_value.y + "\n";
                    else if (vPart == Code_if.VectorPart.z)
                        GM.console.text += Controller.vars[variablename].vec3_value.z + "\n";
                    break;

            }
        }
        else
        {
            GM.console.text += content + "\n";
        }
        return true;
    }
}

public class Code_While : Method
{
    public Code_if checkCase;
    List<Method> inside = new List<Method>();

    public override bool Compute()
    {
        while (checkCase.getValue())
        {
            Controller.MethodRun(inside);
        }
        return true;
    }
}

public class Code_EndIf : Method
{
    public override bool Compute()
    {
        return true;
    }
}

public class Code_SetZombie : Method
{
    string variable = "";
    Mathematics x = null;
    Mathematics y = null;
    Mathematics z = null;
    float xMove = 0;
    float yMove = 0;
    float zMove = 0;

    public override bool Compute()
    {
        if (variable != "")
        {
            xMove = Controller.vars[variable].vec3_value.x;
            yMove = Controller.vars[variable].vec3_value.y;
            zMove = Controller.vars[variable].vec3_value.z;
        }
        else
        {
            if (x != null) xMove = x.Calculate();
            if (y != null) yMove = y.Calculate();
            if (z != null) zMove = z.Calculate();
        }


        return false;
    }
}

public class Code_Equation : Method
{
    public string output = "";
    public Code_if.VectorPart vPart = Code_if.VectorPart.none;
    public Mathematics maths = null;
    public bool changeType = false;
    public Variable.VariableType newType = Variable.VariableType.INT;

    public override bool Compute()
    {
        Variable temp = Controller.vars[output];
        if (changeType)
        {
            temp.type = newType;
        }
        switch (temp.type)
        {
            case Variable.VariableType.FLOAT:
                temp.flt_value = maths.Calculate();
                break;
            case Variable.VariableType.INT:
                temp.int_value = (int)maths.Calculate();
                break;
            case Variable.VariableType.VEC3:
                switch (vPart)
                {
                    case Code_if.VectorPart.x:
                        temp.vec3_value.x = maths.Calculate();
                        break;
                    case Code_if.VectorPart.y:
                        temp.vec3_value.y = maths.Calculate();
                        break;
                    case Code_if.VectorPart.z:
                        temp.vec3_value.z = maths.Calculate();
                        break;
                }
                break;
        }
        Controller.vars[output] = temp;

        return true;
    }
}

public class Code_Vec3Set : Method
{
    public string output = "";
    public Mathematics x = null;
    public Mathematics y = null;
    public Mathematics z = null;

    public override bool Compute()
    {
        Variable temp = Controller.vars[output];

        if (x != null) temp.vec3_value.x = x.Calculate();
        if (y != null) temp.vec3_value.y = y.Calculate();
        if (z != null) temp.vec3_value.z = z.Calculate();
        temp.type = Variable.VariableType.VEC3;

        Controller.vars[output] = temp;

        return true;
    }
}

public class Code_SimpleSet : Method
{
    public string output = "";
    public string input = "";
    public int iValue = 0;
    public float fValue = 0;
    public bool bValue = false;
    public string sValue = "";
    public Code_if.VectorPart outPart = Code_if.VectorPart.none;
    public Code_if.VectorPart inPart = Code_if.VectorPart.none;
    public Variable.VariableType newType = Variable.VariableType.INT;
    public bool changeType = false;

    public override bool Compute()
    {
        Variable temp = Controller.vars[output];
        if (changeType)
        {
            temp.type = newType;
        }

        switch (temp.type)
        {
            case Variable.VariableType.BOOL:
                if (input == "")
                {
                    temp.bool_value = bValue;
                }
                else
                {
                    temp.bool_value = Controller.vars[input].bool_value;
                }
                break;
            case Variable.VariableType.FLOAT:
                if (input == "")
                {
                    temp.flt_value = fValue;
                }
                else
                {
                    temp.flt_value = InputValue();
                }
                break;
            case Variable.VariableType.INT:
                if (input == "")
                {
                    temp.int_value = iValue;
                }
                else
                {
                    temp.int_value = (int)InputValue();
                }
                break;
            case Variable.VariableType.VEC3:
                if (input == "")
                {
                    switch (inPart)
                    {
                        case Code_if.VectorPart.x:
                            temp.vec3_value.x = fValue;
                            break;
                        case Code_if.VectorPart.y:
                            temp.vec3_value.y = fValue;
                            break;
                        case Code_if.VectorPart.z:
                            temp.vec3_value.z = fValue;
                            break;
                    }
                }
                else
                {
                    switch (inPart)
                    {
                        case Code_if.VectorPart.x:
                            temp.vec3_value.x = InputValue();
                            break;
                        case Code_if.VectorPart.y:
                            temp.vec3_value.y = InputValue();
                            break;
                        case Code_if.VectorPart.z:
                            temp.vec3_value.z = InputValue();
                            break;
                    }
                }
                break;
            case Variable.VariableType.STRING:
                if (input == "")
                {
                    temp.str_value = sValue;
                }
                else
                {
                    temp.str_value = Controller.vars[input].str_value;
                }
                break;
        }

        Controller.vars[output] = temp;
        return true;
    }

    private float InputValue()
    {
        if (Controller.vars[input].type == Variable.VariableType.FLOAT)
            return Controller.vars[input].flt_value;
        else if (Controller.vars[input].type == Variable.VariableType.INT)
            return Controller.vars[input].int_value;
        else if (Controller.vars[input].type == Variable.VariableType.VEC3)
        {
            switch (inPart)
            {
                case Code_if.VectorPart.x:
                    return Controller.vars[input].vec3_value.x;
                case Code_if.VectorPart.y:
                    return Controller.vars[input].vec3_value.y;
                case Code_if.VectorPart.z:
                    return Controller.vars[input].vec3_value.z;
            }
        }

        return 0;
    }
}