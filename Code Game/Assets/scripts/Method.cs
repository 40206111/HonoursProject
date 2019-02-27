using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Method
{
    public List<Method> methods = new List<Method>();
    public abstract bool Compute();

}

public class Code_Debug : Method
{
    public string variablename;
    public Code_if.VectorPart vPart = Code_if.VectorPart.none;
    public string content;

    public override bool Compute()
    {
        GM.console.text += "- ";
        if (variablename != null)
        {
            switch (Controller.allVars[Controller.currentZomb][variablename].type)
            {
                case Variable.VariableType.BOOL:
                    GM.console.text += Controller.allVars[Controller.currentZomb][variablename].bool_value + "\n";
                    break;
                case Variable.VariableType.FLOAT:
                    GM.console.text += Controller.allVars[Controller.currentZomb][variablename].flt_value + "\n";
                    break;
                case Variable.VariableType.INT:
                    GM.console.text += Controller.allVars[Controller.currentZomb][variablename].int_value + "\n";
                    break;
                case Variable.VariableType.STRING:
                    GM.console.text += Controller.allVars[Controller.currentZomb][variablename].str_value + "\n";
                    break;
                case Variable.VariableType.VEC3:
                    if (vPart == Code_if.VectorPart.none)
                        GM.console.text += Controller.allVars[Controller.currentZomb][variablename].vec3_value + "\n";
                    else if (vPart == Code_if.VectorPart.x)
                        GM.console.text += Controller.allVars[Controller.currentZomb][variablename].vec3_value.x + "\n";
                    else if (vPart == Code_if.VectorPart.y)
                        GM.console.text += Controller.allVars[Controller.currentZomb][variablename].vec3_value.y + "\n";
                    else if (vPart == Code_if.VectorPart.z)
                        GM.console.text += Controller.allVars[Controller.currentZomb][variablename].vec3_value.z + "\n";
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
    public Code_if checkCase = new Code_if();

    public override bool Compute()
    {
        GameObject.FindObjectOfType<Controller>().BeginWhile(this);
        return true;
    }

    public IEnumerator NotInfinite()
    {
        while (checkCase.getValue() && !Controller.stop)
        {
            checkCase.Compute();
            yield return new WaitForEndOfFrame();
        }
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
    public string variable = "";

    public override bool Compute()
    {
        GM.zombie[Controller.currentZomb].transform.position = Controller.allVars[Controller.currentZomb][variable].vec3_value;
        Variable temp = Controller.allVars[Controller.currentZomb]["gameobject.transform.position"];
        temp.vec3_value = Controller.allVars[Controller.currentZomb][variable].vec3_value;
        Controller.allVars[Controller.currentZomb]["gameobject.transform.position"] = temp;
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
        Variable temp = Controller.allVars[Controller.currentZomb][output];
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
        Controller.allVars[Controller.currentZomb][output] = temp;

        return true;
    }
}

public class Code_Vec3Set : Method
{
    public string output = "";
    public Mathematics x = null;
    public Mathematics y = null;
    public Mathematics z = null;
    public string input = "";

    public override bool Compute()
    {
        Variable temp = Controller.allVars[Controller.currentZomb][output];

        if (x != null) temp.vec3_value.x = x.Calculate();
        else temp.vec3_value.x = Controller.allVars[Controller.currentZomb][input].vec3_value.x;
        if (y != null) temp.vec3_value.y = y.Calculate();
        else temp.vec3_value.y = Controller.allVars[Controller.currentZomb][input].vec3_value.y;
        if (z != null) temp.vec3_value.z = z.Calculate();
        else temp.vec3_value.z = Controller.allVars[Controller.currentZomb][input].vec3_value.z;
        temp.type = Variable.VariableType.VEC3;

        Controller.allVars[Controller.currentZomb][output] = temp;

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
        Variable temp = Controller.allVars[Controller.currentZomb][output];
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
                    temp.bool_value = Controller.allVars[Controller.currentZomb][input].bool_value;
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
                    temp.str_value = Controller.allVars[Controller.currentZomb][input].str_value;
                }
                break;
        }

        Controller.allVars[Controller.currentZomb][output] = temp;
        return true;
    }

    private float InputValue()
    {
        if (Controller.allVars[Controller.currentZomb][input].type == Variable.VariableType.FLOAT)
            return Controller.allVars[Controller.currentZomb][input].flt_value;
        else if (Controller.allVars[Controller.currentZomb][input].type == Variable.VariableType.INT)
            return Controller.allVars[Controller.currentZomb][input].int_value;
        else if (Controller.allVars[Controller.currentZomb][input].type == Variable.VariableType.VEC3)
        {
            switch (inPart)
            {
                case Code_if.VectorPart.x:
                    return Controller.allVars[Controller.currentZomb][input].vec3_value.x;
                case Code_if.VectorPart.y:
                    return Controller.allVars[Controller.currentZomb][input].vec3_value.y;
                case Code_if.VectorPart.z:
                    return Controller.allVars[Controller.currentZomb][input].vec3_value.z;
            }
        }

        return 0;
    }
}