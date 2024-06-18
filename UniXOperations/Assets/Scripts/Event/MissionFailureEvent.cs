using UnityEngine;

public class MissionFailureEvent : AbstractMissionEvent
{
    public MissionFailureEvent(PointData pointData) : base(pointData) { }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]任務失敗");
        RaiseOnEndEventChain(false);
    }
}
