using UnityEngine;

public class ChangeToWalkingEvent : AbstractMissionEvent
{
    public ChangeToWalkingEvent(PointData pointData, Paths paths) : base(pointData)
    {
        _paths = paths;
    }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]歩きに変更");
        _paths.GetPath(Data).ChangeKindToWalk();

        RaiseChangeEventChain();
    }

    private readonly Paths _paths;
}
