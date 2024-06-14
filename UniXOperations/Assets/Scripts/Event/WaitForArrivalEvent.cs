using System.Linq;
using UniRx;
using UnityEngine;

public class WaitForArrivalEvent : AbstractMissionEvent
{
    public WaitForArrivalEvent(Characters characters)
    {
        _characters = characters;
    }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]到着待ち");
        var missionEventContainer = missionEvents.GetMissionEventContainer(Id);
        var character = _characters.GetCharacter(Data);

        Observable.FromCoroutine(() => WaitForPerson(character, missionEventContainer, false)).Subscribe().AddTo(character.gameObject);
    }

    private readonly Characters _characters;
}
