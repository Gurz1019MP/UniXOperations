using System;
using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class AlertAction : IStateModeDependedAction, IInputter
{
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

    private bool IsPriorityRunning => _currentPath != null && _currentPath.Kind == PathKind.PriorityRunning;

    private CharacterContainer _characterContainer;
    private AISpec _aISkill;
    private Path _currentPath;
    private Transform _characterTransform;
    private Transform _headTransform;
    private float _rollValue;
    private int _rollSign;
    private Vector3 _attackedDirection;
    private IDisposable _targetDiedSubscriver;
    private IDisposable _stateModeSubscriver;

    public void InputUpdate()
    {
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

    public void EnterState()
    {
        if (_attackedDirection == Vector3.zero)
        {
            _rollValue = 0.6f;
            _rollSign = UnityEngine.Random.Range(0, 2) * 2 - 1;
        }
        else
        {
            var angleXZ = Vector3.SignedAngle(_characterTransform.forward, _attackedDirection, -_characterTransform.up);
            // Debug.Log(angleXZ);
            _rollValue = 1.0f;
            _rollSign = (int)Mathf.Sign(angleXZ);
        }
    }

    public void LeaveState()
    {
        if (_attackedDirection == Vector3.zero)
        {
            _rollValue = 0.6f;
            _rollSign = UnityEngine.Random.Range(0, 2) * 2 - 1;
        }
        else
        {
            var angleXZ = Vector3.SignedAngle(_characterTransform.forward, _attackedDirection, -_characterTransform.up);
            // Debug.Log(angleXZ);
            _rollValue = 1.0f;
            _rollSign = (int)Mathf.Sign(angleXZ);
        }
    }

    public void DetectEnemy(CharacterContainer[] detectedEnemies, CharacterContainer[] runningEnemies)
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

    public void DangerDetected(Vector3 direction) { }

    private IEnumerator PushOne(UnityAction<bool> boolSetAction)
    {
        boolSetAction(true);
        yield return new WaitForSeconds(0.2f);
        boolSetAction(false);
    }

    private void BeginAlert(Vector3 direction)
    {
        TargetEnemy = null;
        _attackedDirection = direction;

        _onChangeStateModeSubject.OnNext(StateKind.Alert);
    }

    private readonly Subject<StateKind> _onChangeStateModeSubject = new Subject<StateKind>();
}
