using System.Linq;
using UniRx;
using UnityEngine;

public class WaitForCaseEvent : AbstractMissionEvent
{
    public WaitForCaseEvent(PointData pointData, Characters characters) : base(pointData)
    {
        _characters = characters;
    }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]ケース待ち");

        var missionEvent = missionEvents.GetMissionEvent(Id);
        var character = _characters.GetCharacter(Data);

        Observable.FromCoroutine(() => WaitForPerson(character, missionEvent.Position, true)).Subscribe();
    }

    private readonly Characters _characters;
}
