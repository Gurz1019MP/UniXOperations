using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class SafeAction : IStateModeDependedAction, IInputter
{
    public SafeAction()
    {

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

    private CharacterContainer _characterContainer;
    private AISpec _aISkill;
    private Path _currentPath;
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
    private readonly Dictionary<PathKind, IPath> _pathMapper;

    public IObservable<StateKind> OnChangeStateMode => _onChangeStateModeSubject;

    public void InputUpdate()
    {
        _currentPath?.InputUpdate();
    }

    public void EnterState()
    {
        EndWaitLook();
    }

    public void LeaveState()
    {
        EndWaitLook();
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

    public void DangerDetected(Vector3 direction)
    {
        // IsPriorityRunning
        BeginAlert(direction);
    }

    private void BeginAlert(Vector3 direction)
    {
        TargetEnemy = null;
        _attackedDirection = direction;

        _onChangeStateModeSubject.OnNext(StateKind.Alert);
    }

    private void EndWaitLook()
    {
        if (_lookSubscriver == null) return;

        _lookSubscriver.Dispose();
        _lookSubscriver = null;
    }

    private readonly Subject<StateKind> _onChangeStateModeSubject = new Subject<StateKind>();
}
