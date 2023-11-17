using System.Linq;
using UniRx;
using UnityEngine;

public class WaitForCaseEvent : AbstractMissionEvent
{
    public WaitForCaseEvent(GameDataContainer gameDataContainer) : base(gameDataContainer)
    {

    }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]ケース待ち");

        var missionEventContainer = GameDataContainer.MissionEvents.Single(m => m.MissionEvent.Id == Id);
        var character = GameDataContainer.Characters.Single(c => c.ID == Data);

        Observable.FromCoroutine(() => WaitForPerson(character, missionEventContainer, true)).Subscribe().AddTo(character.gameObject);
    }
}
