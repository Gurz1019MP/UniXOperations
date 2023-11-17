using System.Linq;
using UniRx;
using UnityEngine;

public class WaitForArrivalEvent : AbstractMissionEvent
{
    public WaitForArrivalEvent(GameDataContainer gameDataContainer) : base(gameDataContainer)
    {

    }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]到着待ち");
        var missionEventContainer = GameDataContainer.MissionEvents.Single(m => m.MissionEvent.Id == Id);
        var character = GameDataContainer.Characters.First(c => c.ID == Data);

        Observable.FromCoroutine(() => WaitForPerson(character, missionEventContainer, false)).Subscribe().AddTo(character.gameObject);
    }
}
