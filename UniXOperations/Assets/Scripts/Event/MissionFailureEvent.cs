using UnityEngine;

public class MissionFailureEvent : AbstractMissionEvent
{
    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]任務失敗");
        RaiseOnEndEventChain(false);
    }
}
