using System.Linq;
using UniRx;
using UnityEngine;

public class ChangeTeamEvent : AbstractMissionEvent
{
    public ChangeTeamEvent(GameDataContainer gameDataContainer) : base(gameDataContainer)
    {

    }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]チーム変更");
        var characters = GameDataContainer.Characters.Where(c => c.ID == Data);
        foreach (var character in characters)
        {
            character.Team = 0;
        }

        RaiseChangeEventChain();
    }
}
