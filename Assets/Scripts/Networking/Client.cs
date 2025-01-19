using System;
using dotSpace.Interfaces.Space;

public class Client
{
    public ISpace space;
    public Guid transformID;

    public Client(ISpace space, Guid transformID)
    {
        this.space = space;
        this.transformID = transformID;
    }
}