using System;
using UnityEngine;

public class RunToPosition : IInputter
{
    public RunToPosition(Transform characterTransform, Transform headTransform, AISpec aiSkill)
    {
        _lookPosition = new LookPosition(characterTransform, headTransform, aiSkill);
        _aiSkill = aiSkill;
    }

    #region IInputter

    public float Horizontal { get; private set; }

    public float Vertical { get; private set; }

    public bool Fire { get; private set; }

    public bool Jump { get; private set; }

    public float MouseX { get; private set; }

    public float MouseY { get; private set; }

    public bool Walk { get; private set; }

    public bool Reload { get; private set; }

    public bool DropWeapon { get; private set; }

    public bool Zoom { get; private set; }

    public bool FireMode { get; private set; }

    public bool SwitchWeapon { get; private set; }

    #endregion

    public void InputUpdate(Vector3 targetPosition, Action onReachAction = null)
    {
        _lookPosition.InputUpdate(targetPosition, (distance, angleXZ) =>
        {
            if (distance > _movingDistance)
            {
                if (Mathf.Abs(angleXZ) < _movingAngle * _runningMovingAngleConstant)
                {
                    Vertical = _aiSkill.GetCorrectionOperatingVolume(distance);
                }
                else
                {
                    Vertical = 0;
                }
            }
            else
            {
                Vertical = 0;
                onReachAction?.Invoke();
            }
        });
    }

    private readonly LookPosition _lookPosition;
    private readonly AISpec _aiSkill;
    private readonly float _movingAngle = 10f;
    private readonly float _runningMovingAngleConstant = 2f;
    private readonly float _movingDistance = 0.3f;
}
