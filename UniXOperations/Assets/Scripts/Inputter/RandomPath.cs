using System;

[Serializable]
public class RandomPath : AbstractPath
{
    private short NextId1 => _pointData.Data2;
    private short NextId2 => _pointData.Data3;

    public RandomPath(PointData pointData) : base(pointData)
    {
    }

    public override short GetNextPathId()
    {
        return UnityEngine.Random.Range(0, 2) == 0 ? NextId1 : NextId2;
    }
}
