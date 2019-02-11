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

    public enum VectorPart
    {
        none,
        x,
        y,
        z
    }

    private Logic ifType = Logic.EQUAL;
    public Code_if ifLHS = null;
    public Code_if ifRHS = null;
    public string lhs;
    public string rhs;
    public VectorPart rightV;
    public VectorPart leftV;


    public override bool Compute()
    {
        return getValue();
    }

    public bool getValue()
    {
        if (Controller.vars[lhs].type == Variable.VariableType.BOOL || ifLHS != null)    //compare booleans
        {
            return CompareBools();
        }
        else if (Controller.vars[lhs].type == Variable.VariableType.FLOAT || //Compare numbers
            Controller.vars[lhs].type == Variable.VariableType.INT ||
            Controller.vars[rhs].type == Variable.VariableType.FLOAT ||
            Controller.vars[rhs].type == Variable.VariableType.INT)
        {
                return CompareFloats(LhsValue(), RhsValue());
        }
        else if (Controller.vars[lhs].type == Variable.VariableType.VEC3)    //compare vec3s
        {
            if (leftV != VectorPart.none)
            {
                return CompareFloats(LhsValue(), RhsValue());
            }
            switch (ifType)
            {
                case Logic.EQUAL:
                    return (Controller.vars[lhs].vec3_value == Controller.vars[rhs].vec3_value);
                case Logic.NOT:
                    return (Controller.vars[lhs].vec3_value != Controller.vars[rhs].vec3_value);
            }
        }
        else if (Controller.vars[lhs].type == Variable.VariableType.STRING)  //compare strings
        {
            switch (ifType)
            {
                case Logic.EQUAL:
                    return (Controller.vars[lhs].str_value == Controller.vars[rhs].str_value);
                case Logic.NOT:
                    return (Controller.vars[lhs].str_value != Controller.vars[rhs].str_value);
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
