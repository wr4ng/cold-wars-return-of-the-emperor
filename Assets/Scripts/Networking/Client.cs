using System;
using dotSpace.Interfaces.Space;

public class Client
{
    public ISpace space;
    public bool isAlive;
    public Guid transformID;

    public Client(ISpace space, Guid transformID)
    {
        this.space = space;
        this.transformID = transformID;
        isAlive = true;
    }
}