using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Abstract method class
public abstract class Method
{
    public List<Method> methods = new List<Method>(); //methods within this method
    public abstract bool Compute(); //method to run stored method
}

//Class for outputting debug statements
public class Code_Debug : Method
{
    public string variablename; // variable to output
    public Code_if.VectorPart vPart = Code_if.VectorPart.none; //part of vector for that variable
    public string content; //string content of debug statement

    //Method to write debug log
    public override bool Compute()
    {
        GM.console.text += "- "; //add dash
        if (variablename != null) //if debug method contains a variable
        {
            //output correct value based on variable type
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
                    if (vPart == Code_if.VectorPart.none) //all of vec3 should be printed
                        GM.console.text += Controller.allVars[Controller.currentZomb][variablename].vec3_value + "\n";
                    else if (vPart == Code_if.VectorPart.x) // the x value should be printed
                        GM.console.text += Controller.allVars[Controller.currentZomb][variablename].vec3_value.x + "\n";
                    else if (vPart == Code_if.VectorPart.y) //the y value should be printed
                        GM.console.text += Controller.allVars[Controller.currentZomb][variablename].vec3_value.y + "\n";
                    else if (vPart == Code_if.VectorPart.z) //the z value should be printed
                        GM.console.text += Controller.allVars[Controller.currentZomb][variablename].vec3_value.z + "\n";
                    break;

            }
        }
        else
        {
            //display string content
            GM.console.text += content + "\n";
        }
        return true;
    }
}

//Class to store while loops
public class Code_While : Method
{
    //the case within the while loop
    public Code_if checkCase = new Code_if();

    //Method to compute while loop
    public override bool Compute()
    {
        GameObject.FindObjectOfType<Controller>().BeginWhile(this);
        return true;
    }

    //Coroutine to stop infinite loops
    public IEnumerator NotInfinite()
    {
        while (checkCase.getValue() && !Controller.stop) //while check case is true and the program is still running
        {
            //run the methods within the while loop
            checkCase.Compute();
            yield return new WaitForEndOfFrame();
        }
    }
}

//Class to show end of if statements
public class Code_EndIf : Method
{
    public override bool Compute()
    {
        return true;
    }
}

//Class to store the setting of the zombies position
public class Code_SetZombie : Method
{
    public string variable = ""; //variable to set zombie position to

    public override bool Compute()
    {
        //Set zombie position in Unity
        GM.zombie[Controller.currentZomb].transform.position = Controller.allVars[Controller.currentZomb][variable].vec3_value;
        //Set zombie value of stored zombie variable
        Variable temp = Controller.allVars[Controller.currentZomb]["gameobject.transform.position"];
        temp.vec3_value = Controller.allVars[Controller.currentZomb][variable].vec3_value;
        Controller.allVars[Controller.currentZomb]["gameobject.transform.position"] = temp;
        return false;
    }
}

//Class to store equations
public class Code_Equation : Method
{
    public string output = ""; //varaible name to output value to
    public Code_if.VectorPart vPart = Code_if.VectorPart.none; //part of vector to set to output value
    public Mathematics maths = null; //maths within the equation
    //Varaibles for changing variable type
    public bool changeType = false;
    public Variable.VariableType newType = Variable.VariableType.INT;

    //Method to calculate result from equation
    public override bool Compute()
    {
        //make temp variable
        Variable temp = Controller.allVars[Controller.currentZomb][output];
        //change variable type
        if (changeType)
        {
            temp.type = newType;
        }
        //Set the right part of the variable
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
        Controller.allVars[Controller.currentZomb][output] = temp; //set variable

        return true;
    }
}

//Class to store how to set the x, y and z values of a vector 3
public class Code_Vec3Set : Method
{
    //variables of what variables to set
    public string output = "";
    public Mathematics x = null;
    public Mathematics y = null;
    public Mathematics z = null;
    public string input = "";

    public override bool Compute()
    {
        Variable temp = Controller.allVars[Controller.currentZomb][output]; //create temp variable

        if (x != null) temp.vec3_value.x = x.Calculate(); //if there is a mathematical equation for the x value
        else temp.vec3_value.x = Controller.allVars[Controller.currentZomb][input].vec3_value.x; //set to input x value
        if (y != null) temp.vec3_value.y = y.Calculate(); //if there is a mathematical equation for the y value
        else temp.vec3_value.y = Controller.allVars[Controller.currentZomb][input].vec3_value.y; //set to input y value
        if (z != null) temp.vec3_value.z = z.Calculate(); //if there is a mathematical equation for the z value
        else temp.vec3_value.z = Controller.allVars[Controller.currentZomb][input].vec3_value.z; //set to input z value
        temp.type = Variable.VariableType.VEC3;

        Controller.allVars[Controller.currentZomb][output] = temp; //set output

        return true;
    }
}

//Class to stor the setting variables or one part of a vector 3
public class Code_SimpleSet : Method
{
    public string output = "";
    public string input = "";
    public int iValue = 0; //int value
    public float fValue = 0; //float value
    public bool bValue = false; //bool value
    public string sValue = ""; //string value
    public Code_if.VectorPart outPart = Code_if.VectorPart.none;
    public Code_if.VectorPart inPart = Code_if.VectorPart.none;
    //Variables for changing variable type
    public Variable.VariableType newType = Variable.VariableType.INT;
    public bool changeType = false;

    //Method to compute new value
    public override bool Compute()
    {
        Variable temp = Controller.allVars[Controller.currentZomb][output]; //create temp variable
        //Change variable type
        if (changeType)
        {
            temp.type = newType;
        }

        //set based on variable type
        switch (temp.type)
        {
            //set to bool value
            case Variable.VariableType.BOOL:
                if (input == "") //if there is an input variable
                {
                    temp.bool_value = bValue;
                }
                else
                {
                    temp.bool_value = Controller.allVars[Controller.currentZomb][input].bool_value;
                }
                break;
            //set to float value
            case Variable.VariableType.FLOAT:
                if (input == "") //if there is an input variable
                {
                    temp.flt_value = fValue;
                }
                else
                {
                    temp.flt_value = InputValue();
                }
                break;
            //set to int value
            case Variable.VariableType.INT:
                if (input == "") //if there is an input variable
                {
                    temp.int_value = iValue;
                }
                else
                {
                    temp.int_value = (int)InputValue();
                }
                break;
            //set to vector 3 value
            case Variable.VariableType.VEC3:
                if (input == "") //if there is an input variable
                {
                    //set to correct variable part
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
                    //set to correct variable part
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
            //set to string value
            case Variable.VariableType.STRING:
                if (input == "") //if there is an input variable
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

    //Method to get the value of the input variable as a float
    private float InputValue()
    {
        if (Controller.allVars[Controller.currentZomb][input].type == Variable.VariableType.FLOAT) //FLOAT
            return Controller.allVars[Controller.currentZomb][input].flt_value;
        else if (Controller.allVars[Controller.currentZomb][input].type == Variable.VariableType.INT) //INT
            return Controller.allVars[Controller.currentZomb][input].int_value;
        else if (Controller.allVars[Controller.currentZomb][input].type == Variable.VariableType.VEC3) //VEC3
        {
            //return correct vector 3 part
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