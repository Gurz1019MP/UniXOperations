using System;
using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class CombatAction : IStateModeDependedAction, IInputter
{
    public Action GetInputUpdateAction()
    {
        return CombatInputUpdate;
    }

    public Action GetEnterStateAction()
    {
        return CombatEnterState;
    }

    public Action GetLeaveStateAction()
    {
        return CombatLeaveState;
    }

    public Action<CharacterContainer[], CharacterContainer[]> GetDetectEnemyAction()
    {
        return CombatDetectEnemy;
    }

    public Action<Vector3> GetDangerDetectedAction()
    {
        return null;
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

    private CharacterContainer _characterContainer;
    private AISpec _aISkill;
    private IDisposable _targetDiedSubscriver;
    private IDisposable _stateModeSubscriver;
    private IDisposable _fireSubscriver;
    private IDisposable _evideSubscriver;
    private Transform _headTransform;
    private Vector3 _attackedDirection;
    private Vector3 _targetPosition;

    private void CombatInputUpdate()
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

    private void CombatEnterState()
    {
        if (_aISkill.IsZombie())
        {
            _characterContainer.Character.SetState(_characterContainer.Character.State.BuildWithArmTargetAngle(0));
        }
    }

    private void CombatLeaveState()
    {
        if (_aISkill.IsZombie())
        {
            _characterContainer.Character.SetState(_characterContainer.Character.State.BuildWithArmTargetAngle(90));
        }
        else
        {
            _characterContainer.Character.SetState(_characterContainer.Character.State.BuildWithArmTargetAngle(90));
        }
    }

    private void CombatDetectEnemy(CharacterContainer[] detectedEnemies, CharacterContainer[] runningEnemies)
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
                TargetEnemy = detectedEnemies[UnityEngine.Random.Range(0, detectedEnemies.Length)];
            }
            else
            {
                // しばらく記憶してから警戒状態に移行
                _stateModeSubscriver ??= Observable.FromCoroutine(CombatToAlert).Subscribe().AddTo(_characterContainer);
            }
        }
    }

    private IEnumerator PushOne(UnityAction<bool> boolSetAction)
    {
        boolSetAction(true);
        yield return new WaitForSeconds(0.2f);
        boolSetAction(false);
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

        _onChangeStateModeSubject.OnNext(StateKind.Alert);
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

    private IEnumerator EvideChance()
    {
        var randomH = UnityEngine.Random.Range(0, 20);
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

        var randomY = UnityEngine.Random.Range(0, 50);
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
