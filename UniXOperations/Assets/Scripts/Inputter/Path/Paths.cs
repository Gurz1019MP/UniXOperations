using System.Linq;
using UnityEngine;

public class Paths
{
    public Paths(PointData[] pointDatas)
    {
        _pointDatas = pointDatas;
    }

    public IPath GetPath(short id, Transform characterTransform, Transform headTransform, AISpec aiSkill, Character character)
    {
        PointData pointData = _pointDatas.FirstOrDefault(p => p.Data4 == id);
        if (pointData.Data1 == 3)
        {
            switch (pointData.Data2)
            {
                case 0:
                    return new Walking(pointData, characterTransform, headTransform, aiSkill);
                case 1:
                    return new Running(pointData, characterTransform, headTransform, aiSkill);
                case 2:
                    return new Waiting(pointData, characterTransform, headTransform, aiSkill);
                case 3:
                    return new Tracking(pointData, characterTransform, headTransform, aiSkill);
                case 4:
                    return new AlertWaiting(pointData, characterTransform, headTransform, aiSkill);
                case 5:
                    return new TimeWaiting(pointData, characterTransform, headTransform, aiSkill);
                case 6:
                    return new ThrowingGrenade(pointData, character, headTransform, aiSkill);
                case 7:
                    return new PriorityRunning(pointData, characterTransform, headTransform, aiSkill, character);
                default:
                    return null;
            }
        }
        else
        {
            return new RandomPath();
        }
    }

    private PointData[] _pointDatas;
}
