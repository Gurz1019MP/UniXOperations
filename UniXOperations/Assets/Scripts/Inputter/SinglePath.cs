using System;

[Serializable]
public class SinglePath : AbstractPath
{
    public short NextId;
    public PathKind Kind;

    public SinglePath(short id, short nextId, PathKind kind, GameDataContainer gameDataContainer) : base(id, gameDataContainer)
    {
        NextId = nextId;
        Kind = kind;
    }

    public override PathContainer GetNextPathContainer()
    {
        return GameDataContainer.GetPath(NextId);
    }

    public void ChangeKindToWalk()
    {
        Kind = PathKind.Walking;
    }

    public enum PathKind : short
    {
        Walking = 0,
        Running = 1,
        Waiting = 2,
        Tracking = 3,
        AlertWaiting = 4,
        TimeWaiting = 5,
        ThrowingGrenade = 6,
        PriorityRunning = 7,
    }
}
