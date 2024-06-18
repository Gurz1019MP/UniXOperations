using UnityEngine;

public class ObjectiveComplateEvent : AbstractMissionEvent
{
    public ObjectiveComplateEvent(PointData pointData) : base(pointData) { }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]任務達成");
        RaiseOnEndEventChain(true);
    }
}
