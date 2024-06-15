using UniRx;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class AIInputter : InputterBase
{
    #region .ctor

    public AIInputter(CharacterContainer characterContainer, AISpec aISkill, Paths paths, Characters characters, System.Func<Character, CharacterContainer> findCharacterContainerFunction) : base(characterContainer.gameObject)
    {
        // フィールドの初期化
        _characterContainer = characterContainer;
        _firstPath = paths.GetPath(characterContainer.Character.PointData.Data3);
        _aISkill = aISkill;
        _currentPath = _firstPath;
        _characterTransform = characterContainer.transform;
        _headTransform = _characterContainer.FpsCameraAnchor;
        _paths = paths;
        _characters = characters;
        _findCharacterContainerFunction = findCharacterContainerFunction;

        _characterContainer.Character.OnDamaged.Subscribe(DangerDetected).AddTo(characterContainer.gameObject);
        _characterContainer.Character.OnClosed.Subscribe(DangerDetected).AddTo(characterContainer.gameObject);
        _characterContainer.Character.OnGunSound.Subscribe(DangerDetected).AddTo(characterContainer.gameObject);

        Observable.FromCoroutine(JumpChance).Subscribe();
        Observable.FromCoroutine(DetectEnemy).Subscribe();
        Observable.FromCoroutine(UpdateTargetPosition).Subscribe();

        EnterState(StateMode);
    }

    #endregion

    #region プロパティ

    private StateKind _stateMode;
    private StateKind StateMode
    {
        get { return _stateMode; }
        set
        {
            if (_stateMode == value)
                return;

            LeaveState(_stateMode);
            EnterState(value);

            if (value == StateKind.Alert)
            {
                _wasAlert = true;
            }

            _stateMode = value;
        }
    }

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

    private bool IsPriorityRunning => _currentPath != null && (_currentPath is SinglePath singlePath) && singlePath.Kind == SinglePath.PathKind.PriorityRunning;

    #endregion

    #region フィールド

    private CharacterContainer _characterContainer;
    private AbstractPath _firstPath;
    private AISpec _aISkill;
    private AbstractPath _currentPath;
    private Vector3 _targetPosition;
    private float _movingAngle = 10f;
    private float _movingDistance = 0.3f;
    private float _trackingDistance = 1f;
    private float _runningMovingAngleConstant = 2f;
    private float _waitPathTimer = 5f;
    private Transform _characterTransform;
    private Transform _headTransform;
    private Vector3 _trackingPosition;
    private bool _wasAlert;
    private float _rollValue;
    private int _rollSign;
    private Vector3 _attackedDirection;
    private System.IDisposable _targetDiedSubscriver;
    private System.IDisposable _stateModeSubscriver;
    private System.IDisposable _lookSubscriver;
    private System.IDisposable _waitPathSubscriver;
    private System.IDisposable _fireSubscriver;
    private System.IDisposable _evideSubscriver;
    private readonly Paths _paths;
    private readonly Characters _characters;
    private System.Func<Character, CharacterContainer> _findCharacterContainerFunction;

    #endregion

    protected override void InputUpdate()
    {
        if (StateMode == StateKind.Safe)
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
        else if (StateMode == StateKind.Alert)
        {
            Stop();
            var angleYZ = Vector3.SignedAngle(_headTransform.forward, _characterTransform.forward, _headTransform.right);

            MouseX = _rollValue * _rollSign;
            MouseY = _aISkill.GetCorrectionOperatingVolume(angleYZ);

            if (_characterContainer.Character.State.CurrentWeapon.Spec.IsNone &&
                !_characterContainer.Character.State.DisableWeapon.Spec.IsNone &&
                !_characterContainer.Character.State.IsSwitching)
            {
                Observable.FromCoroutine(_ => PushOne(v => SwitchWeapon = v)).Subscribe().AddTo(_characterContainer);
            }
        }
        else if (StateMode == StateKind.Combat)
        {
            if (TargetEnemy != null && TargetEnemy.gameObject != null && _targetPosition != Vector3.zero)
            {
                if (_aISkill.IsZombie())
                {
                    Shooting();

                    Walk = true;
                }
                else
                {
                    Combat();

                    _evideSubscriver ??= Observable.FromCoroutine(EvideChance).Subscribe().AddTo(_characterContainer);
                    Walk = false;
                }
            }
            else
            {
                MouseY = 0;
            }
        }

#if UNITY_EDITOR
        _characterContainer.DebugText.text = $"State : {_stateMode}\r\nHP : {_characterContainer.Character.State.HitPoint}";
#else
#endif
    }

    #region PrivateMethod

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
        Weapon1 = false;
        Weapon2 = false;
    }

    private void EnterState(StateKind kind)
    {
        if (kind == StateKind.Safe)
        {
            EndWaitLook();
        }
        else if (kind == StateKind.Alert)
        {
            if (_attackedDirection == Vector3.zero)
            {
                _rollValue = 0.6f;
                _rollSign = Random.Range(0, 2) * 2 - 1;
            }
            else
            {
                var angleXZ = Vector3.SignedAngle(_characterTransform.forward, _attackedDirection, -_characterTransform.up);
                // Debug.Log(angleXZ);
                _rollValue = 1.0f;
                _rollSign = (int)Mathf.Sign(angleXZ);
            }
        }
        else if (kind == StateKind.Combat)
        {
            if (_aISkill.IsZombie())
            {
                _characterContainer.Character.SetState(_characterContainer.Character.State.SetArmTargetAngle(0));
            }
        }
    }

    private void LeaveState(StateKind kind)
    {
        if (kind == StateKind.Safe)
        {
            EndWaitLook();
        }
        else if (kind == StateKind.Alert)
        {
            if (_attackedDirection == Vector3.zero)
            {
                _rollValue = 0.6f;
                _rollSign = Random.Range(0, 2) * 2 - 1;
            }
            else
            {
                var angleXZ = Vector3.SignedAngle(_characterTransform.forward, _attackedDirection, -_characterTransform.up);
                // Debug.Log(angleXZ);
                _rollValue = 1.0f;
                _rollSign = (int)Mathf.Sign(angleXZ);
            }
        }
        else if (kind == StateKind.Combat)
        {
            if (_aISkill.IsZombie())
            {
                _characterContainer.Character.SetState(_characterContainer.Character.State.SetArmTargetAngle(90));
            }
            else
            {
                _characterContainer.Character.SetState(_characterContainer.Character.State.SetArmTargetAngle(90));
            }
        }
    }

    private IEnumerator JumpChance()
    {
        Vector3 lastPosition = Vector3.zero;
        byte jumpDetactCount = 0;

        while (true)
        {
            if ((_characterTransform.position - lastPosition).magnitude < 0.05f &&
                (Vertical != 0 || Horizontal != 0 || Walk))
            {
                jumpDetactCount++;
                if (jumpDetactCount == 3)
                {
                    // 動きたいのに動けてない状態
                    Observable.FromCoroutine(_ => PushOne(v => Jump = v)).Subscribe().AddTo(_characterContainer);
                    jumpDetactCount = 0;
                }
            }
            else
            {
                jumpDetactCount = 0;
            }

            lastPosition = _characterTransform.position;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator DetectEnemy()
    {
        while (true)
        {
            // 検出範囲内の敵をすべて抽出
            // 遅延実行のためパフォーマンスへの影響はそれほど高くないことを期待
            var allEnemies = _characters.Enemies(_characterContainer.Character.State.Team).Select(c => _findCharacterContainerFunction(c));

            var detectedEnemies = allEnemies
                .Where(c =>
                {
                    Vector3 enemyDirection = c.FpsCameraAnchor.position - _headTransform.position;

                    return _aISkill.IsInRange(enemyDirection.magnitude, 
                                                Vector3.Angle(_headTransform.forward, Vector3.ProjectOnPlane(enemyDirection, _characterTransform.up)), 
                                                Vector3.Angle(_headTransform.forward, Vector3.ProjectOnPlane(enemyDirection, _characterTransform.right))) &&
                            !Physics.Raycast(_headTransform.position, enemyDirection, enemyDirection.magnitude, LayerMask.GetMask(ConstantsManager.LayerMask_Stage));
                }).ToArray();

            var runningEnemies = allEnemies
                .Where(c =>
                {
                    Vector3 enemyDirection = c.FpsCameraAnchor.position - _headTransform.position;

                    return _aISkill.IsInRunningRange(enemyDirection.magnitude) &&
                            (Mathf.Abs(c.Character.InputterState.Inputter.Horizontal) > 0 || Mathf.Abs(c.Character.InputterState.Inputter.Vertical) > 0);
                });

            if (StateMode == StateKind.Safe ||
                StateMode == StateKind.Alert)
            {
                // 安全時、または警戒時
                // 敵を一人でも発見したら戦闘状態に移行
                if (detectedEnemies.Any())
                {
                    TargetEnemy = detectedEnemies[Random.Range(0, detectedEnemies.Length)];

                    // 警戒状態に移行するコルーチンを中止
                    if (_stateModeSubscriver != null)
                    {
                        _stateModeSubscriver.Dispose();
                        _stateModeSubscriver = null;
                    }

                    if (!IsPriorityRunning)
                    {
                        StateMode = StateKind.Combat;
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
            else if (StateMode == StateKind.Combat)
            {
                if (detectedEnemies.Contains(TargetEnemy))
                {
                    // 検出された敵にターゲットが含まれる
                    // 警戒状態に移行するコルーチンを中止
                    if (_stateModeSubscriver != null)
                    {
                        _stateModeSubscriver.Dispose();
                        _stateModeSubscriver = null;
                    }
                }
                else
                {
                    // 検出された敵にターゲットが含まれない → ターゲットを見失った
                    if (detectedEnemies.Any())
                    {
                        // 他の敵がいる場合はターゲットを直ちに変更
                        TargetEnemy = detectedEnemies[Random.Range(0, detectedEnemies.Length)];
                    }
                    else
                    {
                        // しばらく記憶してから警戒状態に移行
                        _stateModeSubscriver ??= Observable.FromCoroutine(CombatToAlert).Subscribe().AddTo(_characterContainer);
                    }
                }
            }

            yield return _aISkill.WaitForDetectionDelay();
        }
    }

    private IEnumerator UpdateTargetPosition()
    {
        while(true)
        {
            if (TargetEnemy == null)
            {
                _targetPosition = Vector3.zero;
            }
            else
            {
                float distanceScale = _aISkill.GetDistanceScale((TargetEnemy.transform.position - _characterTransform.position).magnitude);
                float error = _aISkill.GetError((TargetEnemy.transform.position - _characterTransform.position).magnitude);
                _targetPosition = TargetEnemy.transform.position +
                                  _aISkill.AimHeightOffsetVector(TargetEnemy.transform.up) +
                                  TargetEnemy.Character.State.MoveDelta * distanceScale * 2;
                                  new Vector3(Random.Range(-error, error), Random.Range(-error, error), Random.Range(-error, error));
            }

            yield return new WaitForSeconds(Random.Range(0.1f, 1f));
        }
    }

    private IEnumerator CombatToAlert()
    {
        yield return _aISkill.WaitForMiss();

        if (_stateModeSubscriver == null) yield break;

        BeginAlert(Vector3.zero);
    }

    private void BeginAlert(Vector3 direction)
    {
        TargetEnemy = null;
        _attackedDirection = direction;

        StateMode = StateKind.Alert;
        _stateModeSubscriver = Observable.FromCoroutine(AlertToSafe).Subscribe().AddTo(_characterContainer);
    }

    private IEnumerator AlertToSafe()
    {
        yield return _aISkill.WaitForAlertToSafe();

        if (_stateModeSubscriver == null) yield break;

        StateMode = StateKind.Safe;
        _stateModeSubscriver.Dispose();
        _stateModeSubscriver = null;
    }

    private void DangerDetected(Vector3 direction)
    {
        if (StateMode == StateKind.Safe && !IsPriorityRunning)
        {
            BeginAlert(direction);
        }
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

    private void ChangeNextPathContainer()
    {
        EndWaitLook();
        _wasAlert = false;

        var nextPath = _paths.GetPath(_currentPath.GetNextPathId());

        _currentPath = nextPath;
    }

    private void BeginChangeNextPathContainerWaitTimeAndLookDirection()
    {
        BeginChangeNextPathContainerWaitTime();
        BeginLookDirection();
    }

    private void BeginChangeNextPathContainerWaitTime()
    {
        if (_waitPathSubscriver != null) return;

        _waitPathSubscriver = Observable.FromCoroutine(ChangeNextPathContainerWaitTime).Subscribe().AddTo(_characterContainer);
    }

    private IEnumerator ChangeNextPathContainerWaitTime()
    {
        yield return new WaitForSeconds(_waitPathTimer);
        ChangeNextPathContainer();

        _waitPathSubscriver?.Dispose();
        _waitPathSubscriver = null;
    }

    private void BeginWaitLookAround(bool isAlertWaiting)
    {
        if (_lookSubscriver != null) return;

        _lookSubscriver = Observable.FromCoroutine(_ => WaitLookAround(isAlertWaiting)).Subscribe().AddTo(_characterContainer);
    }

    private void EndWaitLook()
    {
        if (_lookSubscriver == null) return;

        _lookSubscriver.Dispose();
        _lookSubscriver = null;
    }

    private IEnumerator WaitLookAround(bool isAlertWaiting)
    {
        while (true)
        {
            if (isAlertWaiting && _wasAlert) break;

            MouseX = Random.Range(-0.5f, 0.5f);
            yield return new WaitForSeconds(2);
        }

        ChangeNextPathContainer();
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
            _characterContainer.Character.SetState(_characterContainer.Character.State.SetArmTargetAngle(-90));
            if (_characterContainer.Character.State.IsArmLookAtMode)
            {
                _characterContainer.ArmTargetAngleMode();
            }
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

    private IEnumerator FireFull()
    {
        Fire = true;
        yield return new WaitForSeconds(Random.Range(0.2f, 1.0f));
        Fire = false;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
        _fireSubscriver = null;
    }

    private IEnumerator FireSemi()
    {
        Fire = true;
        yield return new WaitForSeconds(Random.Range(0.2f, 1.0f));
        Fire = false;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
        _fireSubscriver = null;
    }

    private IEnumerator EvideChance()
    {
        var randomH = Random.Range(0, 20);
        if (randomH == 1)
        {
            Horizontal = 1f;
        }
        else if (randomH == 2)
        {
            Horizontal = -1f;
        }
        else
        {
            Horizontal = 0;
        }

        var randomY = Random.Range(0, 50);
        if (randomY == 1)
        {
            Vertical = 1f;
        }
        else if (randomY == 2)
        {
            Vertical = -1f;
        }
        else
        {
            Vertical = 0;
        }

        yield return new WaitForSeconds(0.1f);
        _evideSubscriver.Dispose();
        _evideSubscriver = null;
    }

    #endregion

    #region Type

    public enum StateKind
    {
        Safe,
        Alert,
        Combat,
    }

    #endregion
}
