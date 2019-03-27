using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class to store if statements
public class Code_if : Method
{
    //Enum for the logic of if statements
    public enum Logic
    {
        NONE,
        AND,
        OR,
        NOT,
        EQUAL,
        LESSTHAN,
        MORETHAN
    }

    //enum for parts of a vector
    public enum VectorPart
    {
        none,
        x,
        y,
        z
    }

    public Code_if elses = null; //Variable for elses
    public Logic ifType = Logic.NONE; //Logic of if statement
    public Variable.VariableType compareValues = Variable.VariableType.VEC3; //types to compare

    //Left hand side and right hand side values
    public Code_if ifLHS = null; //IF
    public Code_if ifRHS = null;
    public string lhs = ""; //VARIABLE
    public string rhs = "";
    public VectorPart rightV; //VECTORPART
    public VectorPart leftV;
    public bool bl_lhsvalue = false; //BOOLS
    public bool bl_rhsvalue = false;
    public string str_lhsvalue = ""; //STRINGS
    public string str_rhsvalue = "";
    public float nbr_lhsvalue = 0; //NUMBER
    public float nbr_rhsvalue = 0;
    public Mathematics mathLHS = null;  //MATHS
    public Mathematics mathRHS = null;

    //Variables for creation
    public bool hasLhs = false;
    public bool hasRhs = false;

    //Method to set the lhs of the given if to the rhs of this if
    public void SetLHSToRHS(ref Code_if theLHS)
    {
        theLHS.bl_lhsvalue = bl_rhsvalue;
        theLHS.hasLhs = hasRhs;
        theLHS.ifLHS = ifRHS;
        theLHS.leftV = rightV;
        theLHS.str_lhsvalue = str_rhsvalue;
        theLHS.mathLHS = mathRHS;
        theLHS.nbr_lhsvalue = nbr_rhsvalue;
        theLHS.lhs = rhs;
    }

    //Method to run the if statement
    public override bool Compute()
    {
        if (getValue()) //if the checkcase is true
        {
            Controller.MethodRun(methods); //run methods in if
        }
        else if (elses != null) //if there are elses
        {
            elses.Compute(); //compute elses
        }
        return true;
    }

    //get true or false value if if
    public bool getValue()
    {
        if (ifType == Logic.NONE) //if there is no logic for this if
        {
            return ifLHS.getValue();
        }

        if (compareValues == Variable.VariableType.BOOL)    //compare booleans
        {
            return CompareBools();
        }
        else if (compareValues == Variable.VariableType.FLOAT) //compare float, int or vec3
        {
            if (lhs != "")
            {
                nbr_lhsvalue = LhsValue();
            }
            else if (mathLHS != null)
            {
                nbr_lhsvalue = mathLHS.Calculate();
            }

            if (rhs != "")
            {
                nbr_rhsvalue = RhsValue();
            }
            else if (mathRHS != null)
            {
                nbr_rhsvalue = mathRHS.Calculate();
            }

            return CompareFloats(nbr_lhsvalue, nbr_rhsvalue);
        }
        else if (compareValues == Variable.VariableType.STRING)  //compare strings
        {
            if (lhs != "")
            {
                str_lhsvalue = Controller.allVars[Controller.currentZomb][lhs].str_value;
            }

            if (rhs != "")
            {
                str_rhsvalue = Controller.allVars[Controller.currentZomb][rhs].str_value;
            }

            switch (ifType)
            {
                case Logic.EQUAL:
                    return (str_lhsvalue == str_rhsvalue);
                case Logic.NOT:
                    return (str_lhsvalue != str_rhsvalue);
            }
        }

        //if all else fails to return anything return false
        return false;
    }

    //helper method to compare bools
    private bool CompareBools()
    {
        //get lhs bool
        bool boolLHS = false;
        if (ifLHS != null)
        {
            boolLHS = ifLHS.getValue();
        }
        else if (lhs == "")
        {
            boolLHS = bl_lhsvalue;
        }
        else
        {
            boolLHS = Controller.allVars[Controller.currentZomb][lhs].bool_value;
        }

        //get rhs bool
        bool boolRHS = false;
        if (ifRHS != null)
        {
            boolRHS = ifRHS.getValue();
        }
        else if (rhs == "")
        {
            boolRHS = bl_rhsvalue;
        }
        else
        {
            boolRHS = Controller.allVars[Controller.currentZomb][rhs].bool_value;
        }

        //compare bools
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
        return false;
    }

    //helper method to compare floats
    private bool CompareFloats(float l, float r)
    {
        switch (ifType)
        {
            case Logic.EQUAL:
                return (l == r);
            case Logic.NOT:
                return (l != r);
            case Logic.LESSTHAN:
                return (l < r);
            case Logic.MORETHAN:
                return (l > r);
        }
        return false;
    }

    //helper method to get numeric value of right hand side
    private float RhsValue()
    {
        if (Controller.allVars[Controller.currentZomb][rhs].type == Variable.VariableType.VEC3)
        {
            switch (rightV)
            {
                case VectorPart.x:
                    return Controller.allVars[Controller.currentZomb][rhs].vec3_value.x;
                case VectorPart.y:
                    return Controller.allVars[Controller.currentZomb][rhs].vec3_value.y;
                case VectorPart.z:
                    return Controller.allVars[Controller.currentZomb][rhs].vec3_value.z;
            }
        }
        else if (Controller.allVars[Controller.currentZomb][rhs].type == Variable.VariableType.INT)
        {
            return Controller.allVars[Controller.currentZomb][rhs].int_value;
        }
        return Controller.allVars[Controller.currentZomb][rhs].flt_value;
    }

    //helper method to get numeric value of left hand side
    private float LhsValue()
    {
        if (Controller.allVars[Controller.currentZomb][lhs].type == Variable.VariableType.VEC3)
        {
            switch (leftV)
            {
                case VectorPart.x:
                    return Controller.allVars[Controller.currentZomb][lhs].vec3_value.x;
                case VectorPart.y:
                    return Controller.allVars[Controller.currentZomb][lhs].vec3_value.y;
                case VectorPart.z:
                    return Controller.allVars[Controller.currentZomb][lhs].vec3_value.z;
            }
        }
        else if (Controller.allVars[Controller.currentZomb][lhs].type == Variable.VariableType.INT)
        {
            return Controller.allVars[Controller.currentZomb][lhs].int_value;
        }
        return Controller.allVars[Controller.currentZomb][lhs].flt_value;
    }
}
