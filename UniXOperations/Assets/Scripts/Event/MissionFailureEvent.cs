using UnityEngine;

public class MissionFailureEvent : AbstractMissionEvent
{
    public MissionFailureEvent(GameDataContainer gameDataContainer) : base(gameDataContainer)
    {

    }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]任務失敗");
        RaiseOnEndEventChain(false);
    }
}
