using System;
using UnityEngine;

[Serializable]
public abstract class AbstractPath
{
    public Vector3 Position { get; private set; }

    public Vector3 Forward { get; }

    public AbstractPath(PointData pointData)
    {
        _pointData = pointData;
        Position = pointData.Position;
        Forward = pointData.Rotation * Vector3.forward;
    }

    public short Id => _pointData.Data4;

    public abstract short GetNextPathId();

    public void ScalePosition(float scale)
    {
        Position *= scale;
    }

    protected PointData _pointData;
}
