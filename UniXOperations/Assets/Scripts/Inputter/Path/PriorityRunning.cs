using System;
using UniRx;
using UnityEngine;

public class PriorityRunning : IInputter, IPath, INextPath
{
    public PriorityRunning(PointData pointData, Transform characterTransform, Transform headTransform, AISpec aiSkill, Character character)
    {
        _pointData = pointData;
        _runToPosition = new RunToPosition(characterTransform, headTransform, aiSkill);
        _combat = new Combat(character, headTransform, aiSkill);
        _characterTransform = characterTransform;
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
        if (_targetPosition == Vector3.zero)
        {
            _runToPosition.InputUpdate(_pointData.Position, () => { _onChangeNextPathSubject.OnNext(_pointData.Data3); });
        }
        else
        {
            _combat.InputUpdate(_targetPosition);

            var pathDir = _targetPosition - _characterTransform.position;              // 現在地からポイントへのベクトル
            var pathDirPlane = Vector3.ProjectOnPlane(pathDir, Vector3.up);                 // 上記ベクトルをXY平面に射影

            if (pathDirPlane.magnitude > _movingDistance)
            {
                Vertical = Vector3.Dot(_characterTransform.forward, pathDirPlane);
                Horizontal = Vector3.Dot(_characterTransform.right, pathDirPlane);
            }
            else
            {
                Vertical = 0;
                Horizontal = 0;

                _onChangeNextPathSubject.OnNext(_pointData.Data3);
            }
        }
    }

    public void SetTargetPosition(Vector3 target)
    {
        _targetPosition = target;
    }

    private readonly PointData _pointData;
    private readonly RunToPosition _runToPosition;
    private readonly Combat _combat;
    private readonly Transform _characterTransform;
    private readonly float _movingDistance = 0.3f;
    private Vector3 _targetPosition;
    private readonly Subject<short> _onChangeNextPathSubject = new Subject<short>();
}
