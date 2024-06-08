using System;

[Serializable]
public abstract class AbstractPath
{
    public AbstractPath(short id, GameDataContainer gameDataContainer)
    {
        Id = id;
        GameDataContainer = gameDataContainer;
    }

    public short Id { get; }
    protected GameDataContainer GameDataContainer { get; }

    public abstract PathContainer GetNextPathContainer();

}
