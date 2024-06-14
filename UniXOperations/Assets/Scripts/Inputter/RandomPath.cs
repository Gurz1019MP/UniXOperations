using System;

[Serializable]
public class RandomPath : AbstractPath
{
    public short NextId1;
    public short NextId2;

    public RandomPath(short id, short nextId1, short nextId2) : base(id)
    {
        NextId1 = nextId1;
        NextId2 = nextId2;
    }

    public override PathContainer GetNextPathContainer(Paths paths)
    {
        return paths.GetPath(UnityEngine.Random.Range(0, 2) == 0 ? NextId1 : NextId2);
    }
}
