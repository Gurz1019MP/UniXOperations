using UnityEngine;

public class ShowMessageEvent : AbstractMissionEvent
{
    public ShowMessageEvent(GameDataContainer gameDataContainer) : base(gameDataContainer)
    {

    }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]メッセージ表示");
        if (GameDataContainer.EventMessasge.ContainsKey(Data))
        {
            RaiseOnShowMessage(GameDataContainer.EventMessasge[Data]);
        }

        RaiseChangeEventChain();
    }
}
