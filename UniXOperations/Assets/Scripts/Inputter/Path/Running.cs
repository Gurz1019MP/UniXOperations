using System;
using UniRx;
using UnityEngine;

public class Running : IInputter, IPath, INextPath
{
    public Running(PointData pointData, Transform characterTransform, Transform headTransform, AISpec aiSkill)
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

    public IObservable<short> OnChangeNextPath => _onChangeNextPathSubject;

    public void InputUpdate()
    {
        _runToPosition.InputUpdate(_pointData.Position, () => { _onChangeNextPathSubject.OnNext(_pointData.Data3); });

        MouseX = _runToPosition.MouseX;
        MouseY = _runToPosition.MouseY;
        Vertical = _runToPosition.Vertical;
    }

    private PointData _pointData;
    private RunToPosition _runToPosition;
    private Subject<short> _onChangeNextPathSubject = new Subject<short>();
}
