using System;
using UnityEngine;

public interface IStateModeDependedAction
{
    public Action GetInputUpdateAction();

    public Action GetEnterStateAction();

    public Action GetLeaveStateAction();

    public Action<CharacterContainer[], CharacterContainer[]> GetDetectEnemyAction();

    public Action<Vector3> GetDangerDetectedAction();
}
