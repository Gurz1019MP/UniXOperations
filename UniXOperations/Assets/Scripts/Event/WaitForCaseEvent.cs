using System.Linq;
using UniRx;
using UnityEngine;

public class WaitForCaseEvent : AbstractMissionEvent
{
    public WaitForCaseEvent(Characters characters)
    {
        _characters = characters;
    }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]ケース待ち");

        var missionEventContainer = missionEvents.GetMissionEventContainer(Id);
        var character = _characters.GetCharacter(Data);

        Observable.FromCoroutine(() => WaitForPerson(character, missionEventContainer, true)).Subscribe().AddTo(character.gameObject);
    }

    private readonly Characters _characters;
}
