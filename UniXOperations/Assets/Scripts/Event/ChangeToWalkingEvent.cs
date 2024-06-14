using UnityEngine;

public class ChangeToWalkingEvent : AbstractMissionEvent
{
    public ChangeToWalkingEvent(Paths paths)
    {
        _paths = paths;
    }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]歩きに変更");
        SinglePath path = _paths.GetPath(Data).Path as SinglePath;
        if (path != null)
        {
            path.ChangeKindToWalk();
        }

        RaiseChangeEventChain();
    }

    private readonly Paths _paths;
}
