using UnityEngine;

public class Tracking : IInputter, IPath
{
    public Tracking(PointData pointData, Transform characterTransform, Transform headTransform, AISpec aiSkill)
    {
        _pointData = pointData;
        _runToPosition = new RunToPosition(characterTransform, headTransform, aiSkill);
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

    public void InputUpdate()
    {
        Vector3 targetPosition = _characters.GetCharacter(_pointData.Data3)?.Position ?? Vector3.zero;

        _runToPosition.InputUpdate(targetPosition);
    }

    private PointData _pointData;
    private RunToPosition _runToPosition;
    private readonly Characters _characters;
}
