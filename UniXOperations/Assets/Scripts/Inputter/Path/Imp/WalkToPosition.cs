using System;
using UnityEngine;

public class WalkToPosition : IInputter
{
    public WalkToPosition(Transform characterTransform, Transform headTransform, AISpec aiSkill)
    {
        _lookDirection = new LookPosition(characterTransform, headTransform, aiSkill);
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
        _lookDirection.InputUpdate(targetPosition, (distance, angleXZ) =>
        {
            if (distance > _movingDistance)
            {
                Walk = Mathf.Abs(angleXZ) < _movingAngle;
            }
            else
            {
                Walk = false;
                onReachAction?.Invoke();
            }
        });

        MouseX = _lookDirection.MouseX;
        MouseY = _lookDirection.MouseY;
    }

    private readonly LookPosition _lookDirection;
    private readonly float _movingAngle = 10f;
    private readonly float _movingDistance = 0.3f;
}
