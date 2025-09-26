using UnityEngine;

public class Waiting : IInputter, IPath
{
    public Waiting(PointData pointData, Transform characterTransform, Transform headTransform, AISpec aiSkill)
    {
        _pointData = pointData;
        _walkToPosition = new WalkToPosition(characterTransform, headTransform, aiSkill);
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
        _walkToPosition.InputUpdate(_pointData.Position);

        MouseX = _walkToPosition.MouseX;
        MouseY = _walkToPosition.MouseY;
        Walk = _walkToPosition.Walk;
    }

    private readonly PointData _pointData;
    private readonly WalkToPosition _walkToPosition;
}
