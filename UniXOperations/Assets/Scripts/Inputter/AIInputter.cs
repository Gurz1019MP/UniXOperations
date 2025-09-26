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
        _aISkill = aISkill;
        _characterTransform = characterContainer.transform;
        _headTransform = _characterContainer.FpsCameraAnchor;
        _paths = paths;
        _characters = characters;
        _findCharacterContainerFunction = findCharacterContainerFunction;
        _stateModeDependedActionMapper = new Dictionary<StateKind, IStateModeDependedAction>()
        {
            { StateKind.Safe, new SafeAction() },
            { StateKind.Alert, new AlertAction() },
            { StateKind.Combat, new CombatAction() },
        };

        _characterContainer.Character.OnDamaged.Subscribe(DangerDetected).AddTo(characterContainer.gameObject);
        _characterContainer.Character.OnClosed.Subscribe(DangerDetected).AddTo(characterContainer.gameObject);
        _characterContainer.Character.OnGunSound.Subscribe(DangerDetected).AddTo(characterContainer.gameObject);

        Observable.FromCoroutine(JumpChance).Subscribe().AddTo(characterContainer);
        Observable.FromCoroutine(DetectEnemy).Subscribe().AddTo(characterContainer);
        Observable.FromCoroutine(UpdateTargetPosition).Subscribe().AddTo(characterContainer);

        ChangeStateMode(StateKind.Safe);
    }

    #endregion

    #region プロパティ

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

    #endregion

    #region フィールド

    private CharacterContainer _characterContainer;
    private Transform _characterTransform;
    private Transform _headTransform;
    private AISpec _aISkill;
    private Vector3 _targetPosition;
    private Vector3 _attackedDirection;
    private System.IDisposable _targetDiedSubscriver;
    private System.IDisposable _stateModeSubscriver;
    private readonly Paths _paths;
    private readonly Characters _characters;
    private readonly System.Func<Character, CharacterContainer> _findCharacterContainerFunction;
    private StateKind _stateMode;
    private Dictionary<StateKind, IStateModeDependedAction> _stateModeDependedActionMapper;

    #endregion

    protected override void InputUpdate()
    {
        _stateModeDependedActionMapper[_stateMode].InputUpdate();

#if UNITY_EDITOR
        _characterContainer.DebugText.text = $"State : {_stateMode}\r\nHP : {_characterContainer.Character.State.HitPoint}";
#else
#endif
    }

    #region PrivateMethod

    private void ChangeStateMode(StateKind kind)
    {
        if (_stateMode == kind) return;

        _stateModeDependedActionMapper[_stateMode].LeaveState();
        _stateModeDependedActionMapper[kind].EnterState();

        _stateMode = kind;
    }

    private IEnumerator PushOne(UnityAction<bool> boolSetAction)
    {
        boolSetAction(true);
        yield return new WaitForSeconds(0.2f);
        boolSetAction(false);
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
            var allEnemies = _characters.Enemies(_characterContainer.Character.State.Team)
                                        .Where(c => _findCharacterContainerFunction(c) != null)
                                        .Select(c => _findCharacterContainerFunction(c));

            var detectedEnemies = allEnemies
                .Where(c =>
                {
                    Vector3 enemyDirection = c.FpsCameraAnchor.position - _headTransform.position;

                    return _aISkill.IsInRange(enemyDirection.magnitude, 
                                                Vector3.Angle(_headTransform.forward, Vector3.ProjectOnPlane(enemyDirection, _characterTransform.up)), 
                                                Vector3.Angle(_headTransform.forward, Vector3.ProjectOnPlane(enemyDirection, _characterTransform.right))) &&
                            !Physics.Raycast(_headTransform.position, enemyDirection, enemyDirection.magnitude, LayerMask.GetMask(ConstantsProvider.LayerMask_Stage));
                }).ToArray();

            var runningEnemies = allEnemies
                .Where(c =>
                {
                    Vector3 enemyDirection = c.FpsCameraAnchor.position - _headTransform.position;

                    return _aISkill.IsInRunningRange(enemyDirection.magnitude) &&
                            (Mathf.Abs(c.Character.InputterState.Inputter.Horizontal) > 0 || Mathf.Abs(c.Character.InputterState.Inputter.Vertical) > 0);
                }).ToArray();

            _stateModeDependedActionMapper[_stateMode].DetectEnemy(detectedEnemies, runningEnemies);

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

    private void BeginAlert(Vector3 direction)
    {
        TargetEnemy = null;
        _attackedDirection = direction;

        ChangeStateMode(StateKind.Alert);
        _stateModeSubscriver = Observable.FromCoroutine(AlertToSafe).Subscribe().AddTo(_characterContainer);
    }

    private IEnumerator AlertToSafe()
    {
        yield return _aISkill.WaitForAlertToSafe();

        if (_stateModeSubscriver == null) yield break;

        ChangeStateMode(StateKind.Safe);
        _stateModeSubscriver.Dispose();
        _stateModeSubscriver = null;
    }

    private void DangerDetected(Vector3 direction)
    {
        _stateModeDependedActionMapper[_stateMode].DangerDetected(direction);
    }

    #endregion
}
