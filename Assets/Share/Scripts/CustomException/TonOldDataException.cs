using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TonOldDataException : Exception
{
    public TonOldDataException(string message): base(message)
    {
        
    }
}
public class IncorrectPassword : Exception
{
    public IncorrectPassword(string message): base(message)
    {
        
    }
}