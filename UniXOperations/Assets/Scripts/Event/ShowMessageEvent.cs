using UnityEngine;

public class ShowMessageEvent : AbstractMissionEvent
{
    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]メッセージ表示");
        if (EventMessages.Instance.Items.ContainsKey(Data))
        {
            RaiseOnShowMessage(EventMessages.Instance.Items[Data]);
        }

        RaiseChangeEventChain();
    }
}
