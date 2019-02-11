using System.Collections;
using System.Collections.Generic;

public class Mathematics
{
    public enum Operator
    {
        NONE,
        PLUS,
        MINUS,
        TIMES,
        DIVIDE
    }

    Mathematics lhs = null;
    Mathematics rhs = null;
    string varLHS = "";
    string varRHS = "";
    float fLHS;
    float fRHS;
    Operator op = Operator.NONE;
    Code_if.VectorPart vectorLHS;
    Code_if.VectorPart vectorRHS;

    public float Calculate()
    {

        switch (op)
        {
            case Operator.NONE:
                return LeftValue();
            case Operator.PLUS:
                return LeftValue() + RightValue();
            case Operator.MINUS:
                return LeftValue() - RightValue();
            case Operator.TIMES:
                return LeftValue() * RightValue();
            case Operator.DIVIDE:
                return LeftValue() / RightValue();
        }
        return 0;
    }

    private float LeftValue()
    {
        if (lhs != null)
        {
            return lhs.Calculate();
        }
        else if (varLHS != "")
        {
            switch (Controller.vars[varLHS].type)
            {
                case Variable.VariableType.FLOAT:
                    return Controller.vars[varLHS].flt_value;
                case Variable.VariableType.INT:
                    return Controller.vars[varLHS].int_value;
                case Variable.VariableType.VEC3:
                    switch (vectorLHS)
                    {
                        case Code_if.VectorPart.x:
                            return Controller.vars[varLHS].vec3_value.x;
                        case Code_if.VectorPart.y:
                            return Controller.vars[varLHS].vec3_value.y;
                        case Code_if.VectorPart.z:
                            return Controller.vars[varLHS].vec3_value.z;
                    }
                    break;
            }
        }
        return fLHS;
    }

    private float RightValue()
    {
        if (rhs != null)
        {
            return rhs.Calculate();
        }
        else if (varRHS != "")
        {
            switch (Controller.vars[varRHS].type)
            {
                case Variable.VariableType.FLOAT:
                    return Controller.vars[varRHS].flt_value;
                case Variable.VariableType.INT:
                    return Controller.vars[varRHS].int_value;
                case Variable.VariableType.VEC3:
                    switch (vectorRHS)
                    {
                        case Code_if.VectorPart.x:
                            return Controller.vars[varRHS].vec3_value.x;
                        case Code_if.VectorPart.y:
                            return Controller.vars[varRHS].vec3_value.y;
                        case Code_if.VectorPart.z:
                            return Controller.vars[varRHS].vec3_value.z;
                    }
                    break;
            }
        }
        return fRHS;
    }
}
