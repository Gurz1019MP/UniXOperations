using System;

[Serializable]
public abstract class AbstractPath
{
    public AbstractPath(short id)
    {
        Id = id;
    }

    public short Id { get; }

    public abstract PathContainer GetNextPathContainer(Paths paths);
}
