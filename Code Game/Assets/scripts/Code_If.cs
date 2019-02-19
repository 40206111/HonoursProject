using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Code_if : Method
{

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

    public enum VectorPart
    {
        none,
        x,
        y,
        z
    }

    public Code_if elses = null;
    public Logic ifType = Logic.NONE;
    public Code_if ifLHS = null;
    public Code_if ifRHS = null;
    public string lhs = "";
    public string rhs = "";
    public VectorPart rightV;
    public VectorPart leftV;
    public bool hasLhs = false;
    public bool hasRhs = false;
    public bool bl_lhsvalue = false;
    public bool bl_rhsvalue = false;
    public string str_lhsvalue = "";
    public string str_rhsvalue = "";
    public float nbr_lhsvalue = 0;
    public float nbr_rhsvalue = 0;
    public Mathematics mathLHS = null;
    public Mathematics mathRHS = null;
    public Variable.VariableType compareValues = Variable.VariableType.BOOL;

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

    public override bool Compute()
    {
        if (getValue())
        {
            Controller.MethodRun(methods);
        }
        else if (elses != null)
        {
            elses.Compute();
        }
        return true;
    }

    public bool getValue()
    {
        if (ifType == Logic.NONE)
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
                str_lhsvalue = Controller.vars[lhs].str_value;
            }

            if (rhs != "")
            {
                str_rhsvalue = Controller.vars[rhs].str_value;
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
            boolLHS = Controller.vars[lhs].bool_value;
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
            boolRHS = Controller.vars[rhs].bool_value;
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
        if (Controller.vars[rhs].type == Variable.VariableType.VEC3)
        {
            switch (rightV)
            {
                case VectorPart.x:
                    return Controller.vars[rhs].vec3_value.x;
                case VectorPart.y:
                    return Controller.vars[rhs].vec3_value.y;
                case VectorPart.z:
                    return Controller.vars[rhs].vec3_value.z;
            }
        }
        else if (Controller.vars[rhs].type == Variable.VariableType.INT)
        {
            return Controller.vars[rhs].int_value;
        }
        return Controller.vars[rhs].flt_value;
    }

    //helper method to get numeric value of left hand side
    private float LhsValue()
    {
        if (Controller.vars[lhs].type == Variable.VariableType.VEC3)
        {
            switch (leftV)
            {
                case VectorPart.x:
                    return Controller.vars[lhs].vec3_value.x;
                case VectorPart.y:
                    return Controller.vars[lhs].vec3_value.y;
                case VectorPart.z:
                    return Controller.vars[lhs].vec3_value.z;
            }
        }
        else if (Controller.vars[lhs].type == Variable.VariableType.INT)
        {
            return Controller.vars[lhs].int_value;
        }
        return Controller.vars[lhs].flt_value;
    }
}
