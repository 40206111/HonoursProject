using System.Collections;
using System.Collections.Generic;

public class Mathematics
{
    //Enum of mathematical operators
    public enum Operator
    {
        NONE,
        PLUS,
        MINUS,
        TIMES,
        DIVIDE
    }

    //variables to store the left and right hand sides of the equation
    public Mathematics lhs = null;
    public Mathematics rhs = null;
    public string varLHS = "";
    public string varRHS = "";
    public float fLHS;
    public float fRHS;
    public Operator op = Operator.NONE;
    public Code_if.VectorPart vectorLHS;
    public Code_if.VectorPart vectorRHS;

    //variables for equation creation
    public bool lhsComplete = false;
    public bool rhsComplete = false;

    public Mathematics() { } //empty constructor

    public Mathematics(Mathematics m) //constructor that makes a Mathematics object based on another Mathematics object
    {
        //Make variables equal
        lhs = m.lhs;
        rhs = m.rhs;
        varLHS = m.varLHS;
        varRHS = m.varRHS;
        fLHS = m.fLHS;
        fRHS = m.fRHS;
        op = m.op;
        vectorLHS = m.vectorLHS;
        vectorRHS = m.vectorRHS;
        lhsComplete = m.lhsComplete;
        rhsComplete = m.rhsComplete;
    }

    //Method to caluculate the result of the equation
    public float Calculate()
    {
        //Use the right operator
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

    //Method to get LHS value
    private float LeftValue()
    {
        if (lhs != null) //if there is another equation on the lhs
        {
            return lhs.Calculate(); //calculate lhs
        }
        else if (varLHS != "") //if there is a variable in the lhs
        {
            //Return a the correct value based on variable type
            switch (Controller.allVars[Controller.currentZomb][varLHS].type)
            {
                case Variable.VariableType.FLOAT:
                    return Controller.allVars[Controller.currentZomb][varLHS].flt_value;
                case Variable.VariableType.INT:
                    return Controller.allVars[Controller.currentZomb][varLHS].int_value;
                case Variable.VariableType.VEC3:
                    //Return the correct vector part
                    switch (vectorLHS)
                    {
                        case Code_if.VectorPart.x:
                            return Controller.allVars[Controller.currentZomb][varLHS].vec3_value.x;
                        case Code_if.VectorPart.y:
                            return Controller.allVars[Controller.currentZomb][varLHS].vec3_value.y;
                        case Code_if.VectorPart.z:
                            return Controller.allVars[Controller.currentZomb][varLHS].vec3_value.z;
                    }
                    break;
            }
        }
        return fLHS; //return the float value of the lhs
    }

    //Method to get RHS value
    private float RightValue()
    {
        if (rhs != null) //if there is another equation on the rhs
        {
            return rhs.Calculate(); //calculate rhs
        }
        else if (varRHS != "") //if there is a variable in the rhs
        {
            //Return a the correct value based on variable type
            switch (Controller.allVars[Controller.currentZomb][varRHS].type)
            {
                case Variable.VariableType.FLOAT:
                    return Controller.allVars[Controller.currentZomb][varRHS].flt_value;
                case Variable.VariableType.INT:
                    return Controller.allVars[Controller.currentZomb][varRHS].int_value;
                case Variable.VariableType.VEC3:
                    //Return the correct vector part
                    switch (vectorRHS)
                    {
                        case Code_if.VectorPart.x:
                            return Controller.allVars[Controller.currentZomb][varRHS].vec3_value.x;
                        case Code_if.VectorPart.y:
                            return Controller.allVars[Controller.currentZomb][varRHS].vec3_value.y;
                        case Code_if.VectorPart.z:
                            return Controller.allVars[Controller.currentZomb][varRHS].vec3_value.z;
                    }
                    break;
            }
        }
        return fRHS; //return the float value of the rhs
    }
}
