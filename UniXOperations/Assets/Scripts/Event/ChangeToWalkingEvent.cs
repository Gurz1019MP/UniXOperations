using UnityEngine;

public class ChangeToWalkingEvent : AbstractMissionEvent
{
    public ChangeToWalkingEvent(PointData pointData, Paths paths) : base(pointData)
    {
        _paths = paths;
    }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]歩きに変更");
        if (_paths.GetPath(Data) is SinglePath path)
        {
            path.ChangeKindToWalk();
        }

        RaiseChangeEventChain();
    }

    private readonly Paths _paths;
}
