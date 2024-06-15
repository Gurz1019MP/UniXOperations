using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class ChangeTeamEvent : AbstractMissionEvent
{
    public ChangeTeamEvent(PointData pointData, Characters characters) : base(pointData)
    {
        _characters = characters;
    }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]チーム変更");
        var characters = _characters.WhereId(Data);
        foreach (var character in characters)
        {
            character.SetState(character.State.ChangeTeam(new Team(0)));
        }

        RaiseChangeEventChain();
    }

    private readonly Characters _characters;
}
