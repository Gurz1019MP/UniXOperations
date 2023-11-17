using UnityEngine;

public class ChangeToWalkingEvent : AbstractMissionEvent
{
    public ChangeToWalkingEvent(GameDataContainer gameDataContainer) : base(gameDataContainer)
    {

    }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]歩きに変更");
        SinglePath path = GameDataContainer.GetPath(Data).Path as SinglePath;
        if (path != null)
        {
            path.ChangeKindToWalk();
        }

        RaiseChangeEventChain();
    }
}
