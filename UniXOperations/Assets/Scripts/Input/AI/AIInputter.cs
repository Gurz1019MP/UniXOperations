using UniRx;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Collections;
using Newtonsoft.Json.Linq;

public class AIInputter : InputterBase
{
    #region .ctor

    public AIInputter(Character characterState, PathContainer firstPath, GameDataContainer gameDataContainer, short aISkill) : base(characterState.gameObject)
    {
        // フィールドの初期化
        _character = characterState;
        _firstPath = firstPath;
        _gameDataContainer = gameDataContainer;
        _aISkill = AISkill.GetAISkill(aISkill);
        _currentPath = _firstPath;
        _characterTransform = _character.transform;
        _headTransform = _character.FpsCameraAnchor;
        _isZombie = _aISkill.Id == -1;

        _character.OnDamaged.Subscribe(DangerDetected).AddTo(_character.gameObject);
        _character.OnClosed.Subscribe(DangerDetected).AddTo(_character.gameObject);
        _character.OnGunSound.Subscribe(DangerDetected).AddTo(_character.gameObject);

        Observable.FromCoroutine(JumpChance).Subscribe().AddTo(_character);
        Observable.FromCoroutine(DetectEnemy).Subscribe().AddTo(_character);
        Observable.FromCoroutine(UpdateTargetPosition).Subscribe().AddTo(_character);

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

    private Character _targetEnemy;
    private Character TargetEnemy
    {
        get { return _targetEnemy; }
        set
        {
            if (_targetEnemy == value)
                return;

            _targetDiedSubscriver?.Dispose();
            _targetDiedSubscriver = null;

            if (value == null) return;

            _targetDiedSubscriver = value.OnDied.Subscribe(_ => BeginAlert(Vector3.zero)).AddTo(value);

            _targetEnemy = value;
        }
    }

    private bool IsPriorityRunning => _currentPath != null && (_currentPath.Path is SinglePath singlePath) && singlePath.Kind == SinglePath.PathKind.PriorityRunning;

    #endregion

    #region フィールド

    private Character _character;
    private PathContainer _firstPath;
    private GameDataContainer _gameDataContainer;
    private AISkill _aISkill;
    private PathContainer _currentPath;
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
    private bool _isZombie;
    private System.IDisposable _targetDiedSubscriver;
    private System.IDisposable _stateModeSubscriver;
    private System.IDisposable _lookSubscriver;
    private System.IDisposable _waitPathSubscriver;
    private System.IDisposable _fireSubscriver;
    private System.IDisposable _evideSubscriver;

    #endregion

    protected override void InputUpdate()
    {
        if (StateMode == StateKind.Safe)
        {
            if (_currentPath != null)
            {
                if (_currentPath.Path is SinglePath path)
                {
                    if (path.Kind == SinglePath.PathKind.Walking)
                    {
                        Moving(_currentPath.transform.position, false, false, ChangeNextPathContainer);
                    }
                    else if (path.Kind == SinglePath.PathKind.Running)
                    {
                        Moving(_currentPath.transform.position, true, false, ChangeNextPathContainer);
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
                        ThrowingGrenade(_currentPath.transform.position);
                    }
                    else if (path.Kind == SinglePath.PathKind.PriorityRunning)
                    {
                        if (TargetEnemy == null)
                        {
                            Moving(_currentPath.transform.position, true, false, ChangeNextPathContainer);
                        }
                        else
                        {
                            CombatAndMoving(_currentPath.transform.position, ChangeNextPathContainer);
                        }
                    }
                }
                else if (_currentPath.Path is RandomPath)
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
            MouseY = angleYZ * _aISkill.PropControlConstant;

            if (_character.CurrentWeaponState.Kind == 0 &&
                _character.DisableWeaponState.Kind != 0 &&
                !_character.IsSwitching)
            {
                Observable.FromCoroutine(_ => PushOne(v => SwitchWeapon = v)).Subscribe().AddTo(_character);
            }
        }
        else if (StateMode == StateKind.Combat)
        {
            if (TargetEnemy != null && TargetEnemy.gameObject != null && _targetPosition != Vector3.zero)
            {
                if (_isZombie)
                {
                    Shooting();

                    Walk = true;
                }
                else
                {
                    Combat();

                    _evideSubscriver ??= Observable.FromCoroutine(EvideChance).Subscribe().AddTo(_character);
                    Walk = false;
                }
            }
            else
            {
                MouseY = 0;
            }
        }

#if UNITY_EDITOR
        _character.DebugText.text = $"State : {_stateMode}\r\nHP : {_character.HitPoint}";
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
            if (_isZombie)
            {
                _character.ArmTargetAngle = 0;
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
            if (_isZombie)
            {
                _character.ArmTargetAngle = 90;
            }
            else
            {
                _character.ArmTargetAngle = 90;
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
                    Observable.FromCoroutine(_ => PushOne(v => Jump = v)).Subscribe().AddTo(_character);
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
            if (_gameDataContainer != null && _gameDataContainer.Characters != null)
            {
                // 検出範囲内の敵をすべて抽出
                // 遅延実行のためパフォーマンスへの影響はそれほど高くないことを期待
                var allEnemies = _gameDataContainer.Characters
                    .Where(c => c.Team != _character.Team)
                    .Where(c => c.gameObject != null);

                var detectedEnemies = allEnemies
                    .Where(c =>
                    {
                        Vector3 enemyDirection = c.FpsCameraAnchor.position - _headTransform.position;

                        return enemyDirection.magnitude < _aISkill.DetectionRange &&
                               Vector3.Angle(_headTransform.forward, Vector3.ProjectOnPlane(enemyDirection, _characterTransform.up)) < _aISkill.DetectionAngleHorizontal &&
                               Vector3.Angle(_headTransform.forward, Vector3.ProjectOnPlane(enemyDirection, _characterTransform.right)) < _aISkill.DetectionAngleVertical &&
                               !Physics.Raycast(_headTransform.position, enemyDirection, enemyDirection.magnitude, LayerMask.GetMask(ConstantsManager.LayerMask_Stage));
                    }).ToArray();

                var runningEnemies = allEnemies
                    .Where(c =>
                    {
                        Vector3 enemyDirection = c.FpsCameraAnchor.position - _headTransform.position;

                        return enemyDirection.magnitude < _aISkill.RunningDetectionRange &&
                               (Mathf.Abs(c.Inputter.Horizontal) > 0 || Mathf.Abs(c.Inputter.Vertical) > 0);
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
                            _stateModeSubscriver ??= Observable.FromCoroutine(CombatToAlert).Subscribe().AddTo(_character);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(_aISkill.DetectionDelay);
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
                float errorScale = (TargetEnemy.transform.position - _characterTransform.position).magnitude / _aISkill.DetectionRange;
                float error = _aISkill.ShootError * errorScale;
                _targetPosition = TargetEnemy.transform.position +
                                  TargetEnemy.transform.up * _aISkill.AimHeightOffset +
                                  new Vector3(Random.Range(-error, error), Random.Range(-error, error), Random.Range(-error, error));
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator CombatToAlert()
    {
        yield return new WaitForSeconds(_aISkill.MissDelay * Random.Range(0.5f, 1.5f));

        if (_stateModeSubscriver == null) yield break;

        BeginAlert(Vector3.zero);
    }

    private void BeginAlert(Vector3 direction)
    {
        TargetEnemy = null;
        _attackedDirection = direction;

        StateMode = StateKind.Alert;
        _stateModeSubscriver = Observable.FromCoroutine(AlertToSafe).Subscribe().AddTo(_character);
    }

    private IEnumerator AlertToSafe()
    {
        yield return new WaitForSeconds(_aISkill.AlertTime * Random.Range(0.8f, 1.5f));

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

        MouseX = angleXZ * _aISkill.PropControlConstant;
        MouseY = angleYZ * _aISkill.PropControlConstant;

        float movingAngle = _movingAngle * (isRunning ? _runningMovingAngleConstant : 1);
        float distance = isTracking ? _trackingDistance : _movingDistance;
        if (Mathf.Abs(angleXZ) < movingAngle && pathDirPlane.magnitude > distance)
        {
            if (isRunning)
            {
                Vertical = pathDirPlane.magnitude * _aISkill.PropControlConstant;
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
                Moving(_currentPath.transform.position, false, false, () => BeginChangeNextPathContainerWaitTimeAndLookDirection());
            }
            else
            {
                Moving(_currentPath.transform.position, false, false, () => BeginWaitLookAround(isAlertWaiting));
            }
        }
        else
        {
            var angleYZ = Vector3.SignedAngle(_headTransform.forward, _characterTransform.forward, _headTransform.right);
            MouseY = angleYZ * _aISkill.PropControlConstant;
        }
    }

    private void Tracking(short targetId)
    {
        try
        {
            _trackingPosition = _gameDataContainer.Characters.FirstOrDefault(c => c.ID == targetId).transform.position;
        }
        catch
        {
            _trackingPosition = _character.transform.position;
            Debug.Log("追尾対象の取得に失敗");
        }

        Moving(_trackingPosition, true, true);
    }

    private void ThrowingGrenade(Vector3 targetPoint)
    {
        Stop();

        if (_character.CurrentWeaponState.Kind == 13)
        {
            var enemyDir = targetPoint - _headTransform.position;         // 現在地から敵へのベクトル
            var enemyDirXZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.up);              // 上記ベクトルをXY平面に射影
            var angleXZ = Vector3.SignedAngle(_headTransform.forward, enemyDirXZPlane, _headTransform.up); // 今向いている方向との差を計算
            var enemyDirYZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.right);
            var angleYZ = Vector3.SignedAngle(_headTransform.forward, enemyDirYZPlane, _headTransform.right);

            MouseX = angleXZ * _aISkill.PropControlConstant;
            MouseY = angleYZ * _aISkill.PropControlConstant;

            if (angleXZ < 0.1 && angleYZ < 0.1)
            {
                Observable.FromCoroutine(FireSemi).Subscribe().AddTo(_character);
                ChangeNextPathContainer();
            }
        }
        else if (_character.DisableWeaponState.Kind == 13)
        {
            if (!_character.IsSwitching)
            {
                Observable.FromCoroutine(_ => PushOne(v => SwitchWeapon = v)).Subscribe().AddTo(_character);
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

        var nextPath = _currentPath.Path.GetNextPathContainer();

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

        _waitPathSubscriver = Observable.FromCoroutine(ChangeNextPathContainerWaitTime).Subscribe().AddTo(_character);
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

        _lookSubscriver = Observable.FromCoroutine(_ => WaitLookAround(isAlertWaiting)).Subscribe().AddTo(_character);
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

        _lookSubscriver = Observable.FromCoroutine(_ => LookDirection()).Subscribe().AddTo(_character);
    }

    private IEnumerator LookDirection()
    {
        while (true)
        {
            var angleXZ = Vector3.SignedAngle(_characterTransform.forward, _currentPath.transform.forward, _characterTransform.up);
            MouseX = angleXZ * _aISkill.PropControlConstant;
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

        MouseX = enemyAngleXZ * _aISkill.PropControlConstant;
        MouseY = enemyAngleYZ * _aISkill.PropControlConstant;

        if (Mathf.Abs(enemyAngleXZ) < _aISkill.ShootThreshold && Mathf.Abs(enemyAngleYZ) < _aISkill.ShootThreshold &&
            !Physics.Raycast(_headTransform.position, enemyDir, enemyDir.magnitude, LayerMask.GetMask("Stage")))
        {
            if (_fireSubscriver == null)
            {
                if (_character.CurrentWeaponState.Spec.FullAuto)
                {
                    _fireSubscriver = Observable.FromCoroutine(FireFull).Subscribe().AddTo(_character);
                }
                else
                {
                    _fireSubscriver = Observable.FromCoroutine(FireSemi).Subscribe().AddTo(_character);
                }
            }
        }
    }

    private void Combat()
    {
        Shooting();

        if (_character.CurrentWeaponState.Kind != 0 &&
            _character.CurrentWeaponState.Magazine == 0)
        {
            if (_character.CurrentWeaponState.Ammo == 0)
            {
                Observable.FromCoroutine(_ => PushOne(v => DropWeapon = v)).Subscribe().AddTo(_character);
            }
            else if (_character.CurrentWeaponState.Ammo != 0 &&
                     !_character.IsReloading)
            {
                Observable.FromCoroutine(_ => PushOne(v => Reload = v)).Subscribe().AddTo(_character);
            }
        }

        if (_character.CurrentWeaponState.Kind == 0 &&
            _character.DisableWeaponState.Kind != 0 &&
            !_character.IsSwitching)
        {
            Observable.FromCoroutine(_ => PushOne(v => SwitchWeapon = v)).Subscribe().AddTo(_character);
        }

        if (_character.CurrentWeaponState.Spec.Scope != WeaponSpec.ZoomModeEnum.None &&
            !_character.IsZoom)
        {
            Observable.FromCoroutine(_ => PushOne(v => Zoom = v)).Subscribe().AddTo(_character);
        }

        if (_character.CurrentWeaponState.Kind == 0 &&
            _character.DisableWeaponState.Kind == 0)
        {
            _character.ArmTargetAngle = -90;
            if (_character.IsArmLookAtMode)
            {
                _character.ArmTargetAngleMode();
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
