using System;

[Serializable]
public class SinglePath : AbstractPath
{
    public short NextId => _pointData.Data3;
    public PathKind Kind { get; private set; }

    public SinglePath(PointData pointData) : base(pointData)
    {
        Kind = (PathKind)pointData.Data2;
    }

    public override short GetNextPathId()
    {
        return NextId;
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
