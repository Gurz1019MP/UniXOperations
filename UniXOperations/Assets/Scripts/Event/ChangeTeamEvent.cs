using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class ChangeTeamEvent : AbstractMissionEvent
{
    public ChangeTeamEvent(Characters characters)
    {
        _characters = characters;
    }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]チーム変更");
        var characters = _characters.WhereId(Data);
        foreach (var character in characters)
        {
            character.Team = new Team(0);
        }

        RaiseChangeEventChain();
    }

    private readonly Characters _characters;
}
