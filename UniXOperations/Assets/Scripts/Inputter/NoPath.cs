using System.ComponentModel;
using UnityEngine;

public class NoPath : AbstractPath
{
    static NoPath()
    {
        Instance = new NoPath(new PointData(Vector3.zero, Quaternion.identity, 0, 0, 0, 0));
    }

    private NoPath(PointData point) : base(point) { }

    public static NoPath Instance { get; }

    public override short GetNextPathId()
    {
        return 0;
    }
}
