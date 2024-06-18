using System;
using UnityEngine;

[Serializable]
public abstract class AbstractPath
{
    public Vector3 Position => _pointData.Position;

    public Vector3 Forward => _pointData.Rotation * Vector3.forward;

    public AbstractPath(PointData pointData)
    {
        _pointData = pointData;
    }

    public short Id => _pointData.Data4;

    public abstract short GetNextPathId();

    protected PointData _pointData;
}
