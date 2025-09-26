using System;
using System.Collections;
using UniRx;
using UnityEngine;

public class TimeWaiting : IInputter, IPath, INextPath
{
    public TimeWaiting(PointData pointData, Transform characterTransform, Transform headTransform, AISpec aiSkill)
    {
        _pointData = pointData;
        _walkToPosition = new WalkToPosition(characterTransform, headTransform, aiSkill);
        _lookDirection = new LookDirection(characterTransform, headTransform, aiSkill);
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
        if (_waitPathSubscriver == null)
        {
            _walkToPosition.InputUpdate(_pointData.Position, BeginChangeNextPathContainerWaitTime);
        }
        else
        {
            _lookDirection.InputUpdate(_pointData.Rotation * Vector3.forward);
        }

        MouseX = _walkToPosition.MouseX;
        MouseY = _walkToPosition.MouseY;
        Walk = _walkToPosition.Walk;
    }

    private void BeginChangeNextPathContainerWaitTime()
    {
        if (_waitPathSubscriver != null) return;

        _waitPathSubscriver = Observable.FromCoroutine(ChangeNextPathContainerWaitTime).Subscribe();
    }

    private IEnumerator ChangeNextPathContainerWaitTime()
    {
        yield return new WaitForSeconds(_waitPathTimer);

        _waitPathSubscriver?.Dispose();
        _waitPathSubscriver = null;

        _onChangeNextPathSubject.OnNext(_pointData.Data3);
    }

    private readonly PointData _pointData;
    private readonly WalkToPosition _walkToPosition;
    private readonly LookDirection _lookDirection;
    private readonly float _waitPathTimer = 5f;
    private IDisposable _waitPathSubscriver;
    private readonly Subject<short> _onChangeNextPathSubject = new Subject<short>();
}
