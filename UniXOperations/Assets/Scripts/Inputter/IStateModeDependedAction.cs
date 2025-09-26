using System;
using UnityEngine;

public interface IStateModeDependedAction
{
    public void InputUpdate();

    public void EnterState();

    public void LeaveState();

    public void DetectEnemy(CharacterContainer[] detectedEnemies, CharacterContainer[] runningEnemies);

    public void DangerDetected(Vector3 direction);
}
