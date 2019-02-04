using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Method
{
    public abstract bool Compute();

}

public class Code_While : Method
{
    public Code_if checkCase;

    public override bool Compute()
    {
        return checkCase.getValue();
    }
}

public class Code_MoveZombie : Method
{
    public override bool Compute()
    {
        return true;
    }
}