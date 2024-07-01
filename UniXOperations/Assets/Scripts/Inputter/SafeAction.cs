using System;
using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class SafeAction : IStateModeDependedAction, IInputter
{
    public Action GetInputUpdateAction()
    {
        return SafeInputUpdate;
    }

    public Action GetEnterStateAction()
    {
        return SafeEnterState;
    }

    public Action GetLeaveStateAction()
    {
        return SafeLeaveState;
    }

    public Action<CharacterContainer[], CharacterContainer[]> GetDetectEnemyAction()
    {
        return SafeAndAlertDetectEnemy;
    }

    public Action<Vector3> GetDangerDetectedAction()
    {
        return SafeDangerDetected;
    }

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

    private CharacterContainer _characterContainer;
    private AISpec _aISkill;
    private AbstractPath _currentPath;
    private Vector3 _targetPosition;
    private float _movingAngle = 10f;
    private float _movingDistance = 0.3f;
    private float _trackingDistance = 1f;
    private float _runningMovingAngleConstant = 2f;
    private float _waitPathTimer = 5f;
    private bool _wasAlert;
    private Transform _characterTransform;
    private Transform _headTransform;
    private Vector3 _attackedDirection;
    private Vector3 _trackingPosition;
    private IDisposable _targetDiedSubscriver;
    private IDisposable _stateModeSubscriver;
    private IDisposable _lookSubscriver;
    private IDisposable _fireSubscriver;
    private IDisposable _waitPathSubscriver;
    private readonly Characters _characters;
    private readonly Paths _paths;

    private bool IsPriorityRunning => _currentPath != null && (_currentPath is SinglePath singlePath) && singlePath.Kind == SinglePath.PathKind.PriorityRunning;

    private CharacterContainer _targetEnemy;
    private CharacterContainer TargetEnemy
    {
        get { return _targetEnemy; }
        set
        {
            if (_targetEnemy == value)
                return;

            _targetDiedSubscriver?.Dispose();
            _targetDiedSubscriver = null;

            if (value == null) return;

            _targetDiedSubscriver = value.Character.OnDied.Subscribe(_ => BeginAlert(Vector3.zero)).AddTo(value);

            _targetEnemy = value;
        }
    }

    public IObservable<StateKind> OnChangeStateMode => _onChangeStateModeSubject;

    private void SafeInputUpdate()
    {
        if (_currentPath != null)
        {
            if (_currentPath is SinglePath path)
            {
                if (path.Kind == SinglePath.PathKind.Walking)
                {
                    Moving(_currentPath.Position, false, false, ChangeNextPathContainer);
                }
                else if (path.Kind == SinglePath.PathKind.Running)
                {
                    Moving(_currentPath.Position, true, false, ChangeNextPathContainer);
                }
                else if (path.Kind == SinglePath.PathKind.Waiting)
                {
                    WalkAndWait(false, false);
                }
                else if (path.Kind == SinglePath.PathKind.Tracking)
                {
                    Tracking(path.NextId);
                }
                else if (path.Kind == SinglePath.PathKind.AlertWaiting)
                {
                    WalkAndWait(true, false);
                }
                else if (path.Kind == SinglePath.PathKind.TimeWaiting)
                {
                    WalkAndWait(false, true);
                }
                else if (path.Kind == SinglePath.PathKind.ThrowingGrenade)
                {
                    ThrowingGrenade(_currentPath.Position);
                }
                else if (path.Kind == SinglePath.PathKind.PriorityRunning)
                {
                    if (TargetEnemy == null)
                    {
                        Moving(_currentPath.Position, true, false, ChangeNextPathContainer);
                    }
                    else
                    {
                        CombatAndMoving(_currentPath.Position, ChangeNextPathContainer);
                    }
                }
            }
            else if (_currentPath is RandomPath)
            {
                ChangeNextPathContainer();
            }
        }
    }

    private void SafeEnterState()
    {
        EndWaitLook();
    }

    private void SafeLeaveState()
    {
        EndWaitLook();
    }

    private void SafeAndAlertDetectEnemy(CharacterContainer[] detectedEnemies, CharacterContainer[] runningEnemies)
    {
        // 安全時、または警戒時
        // 敵を一人でも発見したら戦闘状態に移行
        if (detectedEnemies.Any())
        {
            TargetEnemy = detectedEnemies[UnityEngine.Random.Range(0, detectedEnemies.Length)];

            // 警戒状態に移行するコルーチンを中止
            if (_stateModeSubscriver != null)
            {
                _stateModeSubscriver.Dispose();
                _stateModeSubscriver = null;
            }

            if (!IsPriorityRunning)
            {
                _onChangeStateModeSubject.OnNext(StateKind.Combat);
            }
        }
        else if (runningEnemies.Any() && !IsPriorityRunning)
        {
            BeginAlert(runningEnemies.First().transform.position - _characterTransform.position);
        }
        else
        {
            TargetEnemy = null;
        }
    }

    private void SafeDangerDetected(Vector3 direction)
    {
        BeginAlert(direction);
    }

    private void BeginAlert(Vector3 direction)
    {
        TargetEnemy = null;
        _attackedDirection = direction;

        _onChangeStateModeSubject.OnNext(StateKind.Alert);
    }

    private void Moving(Vector3 targetPoint, bool isRunning, bool isTracking, UnityAction actionWhenReaching = null)
    {
        var pathDir = targetPoint - _characterTransform.position;              // 現在地からポイントへのベクトル
        var pathDirPlane = Vector3.ProjectOnPlane(pathDir, Vector3.up);                 // 上記ベクトルをXY平面に射影
        var angleXZ = Vector3.SignedAngle(_characterTransform.forward, pathDirPlane, _characterTransform.up);   // 今向いている方向との差を計算
        var angleYZ = Vector3.SignedAngle(_headTransform.forward, _characterTransform.forward, _headTransform.right);

        MouseX = _aISkill.GetCorrectionOperatingVolume(angleXZ);
        MouseY = _aISkill.GetCorrectionOperatingVolume(angleYZ);

        float movingAngle = _movingAngle * (isRunning ? _runningMovingAngleConstant : 1);
        float distance = isTracking ? _trackingDistance : _movingDistance;
        if (Mathf.Abs(angleXZ) < movingAngle && pathDirPlane.magnitude > distance)
        {
            if (isRunning)
            {
                Vertical = _aISkill.GetCorrectionOperatingVolume(pathDirPlane.magnitude);
            }
            else
            {
                Walk = true;
            }
        }
        else
        {
            Vertical = 0;
            Walk = false;

            if (pathDirPlane.magnitude <= distance)
            {
                actionWhenReaching?.Invoke();
            }
        }
    }

    private void WalkAndWait(bool isAlertWaiting, bool isTimeWaiting)
    {
        if (_lookSubscriver == null)
        {
            if (isTimeWaiting)
            {
                Moving(_currentPath.Position, false, false, () => BeginChangeNextPathContainerWaitTimeAndLookDirection());
            }
            else
            {
                Moving(_currentPath.Position, false, false, () => BeginWaitLookAround(isAlertWaiting));
            }
        }
        else
        {
            var angleYZ = Vector3.SignedAngle(_headTransform.forward, _characterTransform.forward, _headTransform.right);
            MouseY = _aISkill.GetCorrectionOperatingVolume(angleYZ);
        }
    }

    private void Tracking(short targetId)
    {
        try
        {
            _trackingPosition = _characters.GetCharacter(targetId).Position;
        }
        catch
        {
            _trackingPosition = _characterContainer.transform.position;
            Debug.Log("追尾対象の取得に失敗");
        }

        Moving(_trackingPosition, true, true);
    }

    private void ThrowingGrenade(Vector3 targetPoint)
    {
        Stop();

        if (_characterContainer.Character.State.CurrentWeapon.Spec.IsGrenade)
        {
            var enemyDir = targetPoint - _headTransform.position;         // 現在地から敵へのベクトル
            var enemyDirXZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.up);              // 上記ベクトルをXY平面に射影
            var angleXZ = Vector3.SignedAngle(_headTransform.forward, enemyDirXZPlane, _headTransform.up); // 今向いている方向との差を計算
            var enemyDirYZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.right);
            var angleYZ = Vector3.SignedAngle(_headTransform.forward, enemyDirYZPlane, _headTransform.right);

            MouseX = _aISkill.GetCorrectionOperatingVolume(angleXZ);
            MouseY = _aISkill.GetCorrectionOperatingVolume(angleYZ);

            if (angleXZ < 0.1 && angleYZ < 0.1)
            {
                Observable.FromCoroutine(FireSemi).Subscribe().AddTo(_characterContainer);
                ChangeNextPathContainer();
            }
        }
        else if (_characterContainer.Character.State.DisableWeapon.Spec.IsGrenade)
        {
            if (!_characterContainer.Character.State.IsSwitching)
            {
                Observable.FromCoroutine(_ => PushOne(v => SwitchWeapon = v)).Subscribe().AddTo(_characterContainer);
            }
        }
        else
        {
            ChangeNextPathContainer();
        }
    }

    private void CombatAndMoving(Vector3 targetPoint, UnityAction actionWhenReaching)
    {
        if (TargetEnemy != null && TargetEnemy.gameObject != null && _targetPosition != Vector3.zero)
        {
            Combat();

            var pathDir = targetPoint - _characterTransform.position;              // 現在地からポイントへのベクトル
            var pathDirPlane = Vector3.ProjectOnPlane(pathDir, Vector3.up);                 // 上記ベクトルをXY平面に射影

            if (pathDirPlane.magnitude > _movingDistance)
            {
                Vertical = Vector3.Dot(_characterTransform.forward, pathDirPlane);
                Horizontal = Vector3.Dot(_characterTransform.right, pathDirPlane);
            }
            else
            {
                Vertical = 0;
                Walk = false;

                actionWhenReaching();
            }
        }
        else
        {
            MouseY = 0;
        }
    }

    private void Shooting()
    {
        var enemyDir = _targetPosition - _headTransform.position;                               // 現在地から敵へのベクトル
        var enemyDirXZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.up);              // 上記ベクトルをXY平面に射影
        var enemyAngleXZ = Vector3.SignedAngle(_headTransform.forward, enemyDirXZPlane, _headTransform.up); // 今向いている方向との差を計算
        var enemyDirYZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.right);
        var enemyAngleYZ = Vector3.SignedAngle(_headTransform.forward, enemyDirYZPlane, _headTransform.right);

        MouseX = _aISkill.GetCorrectionOperatingVolume(enemyAngleXZ);
        MouseY = _aISkill.GetCorrectionOperatingVolume(enemyAngleYZ);

        if (_aISkill.BeShoot(enemyAngleXZ, enemyAngleYZ) &&
            !Physics.Raycast(_headTransform.position, enemyDir, enemyDir.magnitude, LayerMask.GetMask("Stage")))
        {
            if (_fireSubscriver == null)
            {
                if (_characterContainer.Character.State.CurrentWeapon.Spec.FullAuto)
                {
                    _fireSubscriver = Observable.FromCoroutine(FireFull).Subscribe().AddTo(_characterContainer);
                }
                else
                {
                    _fireSubscriver = Observable.FromCoroutine(FireSemi).Subscribe().AddTo(_characterContainer);
                }
            }
        }
    }

    private void Combat()
    {
        Shooting();

        if (!_characterContainer.Character.State.CurrentWeapon.Spec.IsNone &&
            _characterContainer.Character.State.CurrentWeapon.State.OutOfMagazine)
        {
            if (_characterContainer.Character.State.CurrentWeapon.State.OutOfAmmo)
            {
                Observable.FromCoroutine(_ => PushOne(v => DropWeapon = v)).Subscribe().AddTo(_characterContainer);
            }
            else if (_characterContainer.Character.State.CurrentWeapon.State.HasAmmo &&
                     !_characterContainer.Character.State.CurrentWeapon.IsReloading)
            {
                Observable.FromCoroutine(_ => PushOne(v => Reload = v)).Subscribe().AddTo(_characterContainer);
            }
        }

        if (_characterContainer.Character.State.CurrentWeapon.Spec.IsNone &&
            !_characterContainer.Character.State.DisableWeapon.Spec.IsNone &&
            !_characterContainer.Character.State.IsSwitching)
        {
            Observable.FromCoroutine(_ => PushOne(v => SwitchWeapon = v)).Subscribe().AddTo(_characterContainer);
        }

        if (_characterContainer.Character.State.CurrentWeapon.Spec.HasScope &&
            !_characterContainer.Character.State.IsZoom)
        {
            Observable.FromCoroutine(_ => PushOne(v => Zoom = v)).Subscribe().AddTo(_characterContainer);
        }

        if (_characterContainer.Character.State.CurrentWeapon.Spec.IsNone &&
            _characterContainer.Character.State.DisableWeapon.Spec.IsNone)
        {
            _characterContainer.Character.SetState(_characterContainer.Character.State.BuildWithArmTargetAngle(-90));
            if (_characterContainer.Character.State.IsArmLookAtMode)
            {
                _characterContainer.ArmTargetAngleMode();
            }
        }
    }

    private void ChangeNextPathContainer()
    {
        EndWaitLook();
        _wasAlert = false;

        var nextPath = _paths.GetPath(_currentPath.GetNextPathId());

        _currentPath = nextPath;
    }

    private void EndWaitLook()
    {
        if (_lookSubscriver == null) return;

        _lookSubscriver.Dispose();
        _lookSubscriver = null;
    }

    private void BeginChangeNextPathContainerWaitTimeAndLookDirection()
    {
        BeginChangeNextPathContainerWaitTime();
        BeginLookDirection();
    }

    private void BeginWaitLookAround(bool isAlertWaiting)
    {
        if (_lookSubscriver != null) return;

        _lookSubscriver = Observable.FromCoroutine(_ => WaitLookAround(isAlertWaiting)).Subscribe().AddTo(_characterContainer);
    }

    private IEnumerator WaitLookAround(bool isAlertWaiting)
    {
        while (true)
        {
            if (isAlertWaiting && _wasAlert) break;

            MouseX = UnityEngine.Random.Range(-0.5f, 0.5f);
            yield return new WaitForSeconds(2);
        }

        ChangeNextPathContainer();
    }

    private IEnumerator ChangeNextPathContainerWaitTime()
    {
        yield return new WaitForSeconds(_waitPathTimer);
        ChangeNextPathContainer();

        _waitPathSubscriver?.Dispose();
        _waitPathSubscriver = null;
    }

    private void BeginChangeNextPathContainerWaitTime()
    {
        if (_waitPathSubscriver != null) return;

        _waitPathSubscriver = Observable.FromCoroutine(ChangeNextPathContainerWaitTime).Subscribe().AddTo(_characterContainer);
    }

    private void BeginLookDirection()
    {
        if (_lookSubscriver != null) return;

        _lookSubscriver = Observable.FromCoroutine(_ => LookDirection()).Subscribe().AddTo(_characterContainer);
    }

    private IEnumerator LookDirection()
    {
        while (true)
        {
            var angleXZ = Vector3.SignedAngle(_characterTransform.forward, _currentPath.Forward, _characterTransform.up);
            MouseX = _aISkill.GetCorrectionOperatingVolume(angleXZ);
            yield return null;
        }
    }

    private IEnumerator PushOne(UnityAction<bool> boolSetAction)
    {
        boolSetAction(true);
        yield return new WaitForSeconds(0.2f);
        boolSetAction(false);
    }

    private void Stop()
    {
        Horizontal = 0;
        Vertical = 0;
        Fire = false;
        Jump = false;
        MouseX = 0;
        MouseY = 0;
        Walk = false;
        Reload = false;
        DropWeapon = false;
        Zoom = false;
        FireMode = false;
        SwitchWeapon = false;
    }

    private IEnumerator FireFull()
    {
        Fire = true;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 1.0f));
        Fire = false;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.0f));
        _fireSubscriver = null;
    }

    private IEnumerator FireSemi()
    {
        Fire = true;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 1.0f));
        Fire = false;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.0f));
        _fireSubscriver = null;
    }

    private readonly Subject<StateKind> _onChangeStateModeSubject = new Subject<StateKind>();
}
