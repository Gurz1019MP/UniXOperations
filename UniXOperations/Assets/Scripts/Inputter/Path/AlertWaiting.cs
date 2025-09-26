using System;
using System.Collections;
using UniRx;
using UnityEngine;

public class AlertWaiting : IInputter, IPath, INextPath, IDisposable
{
    public AlertWaiting(PointData pointData, Transform characterTransform, Transform headTransform, AISpec aiSkill)
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
        if (_lookSubscriver == null)
        {
            _walkToPosition.InputUpdate(_pointData.Position, BeginWaitLookAround);
        }
        else
        {
            _lookDirection.InputUpdate(_targetDirection);
        }

        MouseX = _walkToPosition.MouseX;
        MouseY = _walkToPosition.MouseY;
        Walk = _walkToPosition.Walk;
    }

    public void BeAlert()
    {
        _wasAlert = true;
    }

    public void Dispose()
    {
        _lookSubscriver?.Dispose();
        _lookSubscriver = null;
    }

    private void BeginWaitLookAround()
    {
        if (_lookSubscriver != null) return;

        _lookSubscriver = Observable.FromCoroutine(WaitLookAround).Subscribe();
    }

    private IEnumerator WaitLookAround()
    {
        while (true)
        {
            if (_wasAlert) break;

            _targetDirection = Quaternion.Euler(0, UnityEngine.Random.Range(-120f, 120f), 0) * Vector3.forward;
            yield return new WaitForSeconds(2);
        }

        _lookSubscriver?.Dispose();
        _lookSubscriver = null;

        _onChangeNextPathSubject.OnNext(_pointData.Data3);
    }

    private readonly PointData _pointData;
    private readonly WalkToPosition _walkToPosition;
    private readonly LookDirection _lookDirection;
    private bool _wasAlert;
    private Vector3 _targetDirection;
    private IDisposable _lookSubscriver;
    private readonly Subject<short> _onChangeNextPathSubject = new Subject<short>();
}
