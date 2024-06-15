using UnityEngine;

public class ShowMessageEvent : AbstractMissionEvent
{
    public ShowMessageEvent(PointData pointData, EventMessages eventMessages) : base(pointData)
    {
        _eventMessages = eventMessages;
    }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]メッセージ表示");
        RaiseOnShowMessage(_eventMessages.GetMessage(Data));

        RaiseChangeEventChain();
    }

    private EventMessages _eventMessages;
}
