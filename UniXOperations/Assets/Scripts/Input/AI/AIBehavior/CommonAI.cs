using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

public class CommonAI : AbstractAIBehavior
{
    public CommonAI(IAIBehaviorData aIComponentData) : base(aIComponentData)
    {
        _characterTransform = CharacterState.transform;
        _headTransform = CharacterState.FpsCameraAnchor;

        CharacterState.OnDamaged.Subscribe(DangerDetected).AddTo(CharacterState.gameObject);
        CharacterState.OnClosed.Subscribe(DangerDetected).AddTo(CharacterState.gameObject);
        CharacterState.OnGunSound.Subscribe(DangerDetected).AddTo(CharacterState.gameObject);

        Observable.FromCoroutine(JumpChance).Subscribe().AddTo(CharacterState);
        Observable.FromCoroutine(DetectEnemy).Subscribe().AddTo(CharacterState);

        TargetEnemy.Subscribe(e =>
        {
            _targetDiedSubscriver?.Dispose();
            _targetDiedSubscriver = null;

            if (e == null) return;

            _targetDiedSubscriver = e.OnDied.Subscribe(_ => BeginAlert(Vector3.zero)).AddTo(e);
        });
    }

    #region Property

    public ReactiveProperty<AIStateManager.StateKind> StateMode { get; private set; } = new ReactiveProperty<AIStateManager.StateKind>();
    public ReactiveProperty<Character> TargetEnemy { get; private set; } = new ReactiveProperty<Character>(null);
    public ReactiveProperty<Vector3> AlertDirection { get; private set; } = new ReactiveProperty<Vector3>();
    public bool IsPriorityRunning { get; set; }

    #endregion

    #region PrivateField

    private Transform _characterTransform;
    private Transform _headTransform;
    private System.IDisposable _targetDiedSubscriver;
    private System.IDisposable _stateModeSubscriver;

    #endregion

    private IEnumerator JumpChance()
    {
        Vector3 lastPosition = Vector3.zero;
        byte jumpDetactCount = 0;

        while (true)
        {
            if ((_characterTransform.position - lastPosition).magnitude < 0.05f &&
                (Controller.Vertical != 0 || Controller.Horizontal != 0 || Controller.Walk))
            {
                jumpDetactCount++;
                if (jumpDetactCount == 3)
                {
                    // 動きたいのに動けてない状態
                    Observable.FromCoroutine(_ => PushOne(v => Controller.Jump = v)).Subscribe().AddTo(CharacterState);
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
            if (GameDataContainer.Characters != null &&
                CharacterState.ID != 0)
            {
                // 検出範囲内の敵をすべて抽出
                // 遅延実行のためパフォーマンスへの影響はそれほど高くないことを期待
                var allEnemies = GameDataContainer.Characters
                    .Where(c => c.Team != CharacterState.Team)
                    .Where(c => c.gameObject != null);

                var detectedEnemies = allEnemies
                    .Where(c =>
                    {
                        Vector3 enemyDirection = c.FpsCameraAnchor.position - _headTransform.position;

                        return enemyDirection.magnitude < AIParameter.DetectionRange &&
                               Vector3.Angle(_headTransform.forward, Vector3.ProjectOnPlane(enemyDirection, _characterTransform.up)) < AIParameter.DetectionAngleHorizontal &&
                               Vector3.Angle(_headTransform.forward, Vector3.ProjectOnPlane(enemyDirection, _characterTransform.right)) < AIParameter.DetectionAngleVertical &&
                               !Physics.Raycast(_headTransform.position, enemyDirection, enemyDirection.magnitude, LayerMask.GetMask("Stage"));
                    }).ToArray();

                var runningEnemies = allEnemies
                    .Where(c =>
                    {
                        Vector3 enemyDirection = c.FpsCameraAnchor.position - _headTransform.position;

                        return enemyDirection.magnitude < AIParameter.RunningDetectionRange &&
                               (Mathf.Abs(c.Inputter.Horizontal) > 0 || Mathf.Abs(c.Inputter.Vertical) > 0);
                    });

                if (StateMode.Value == AIStateManager.StateKind.Safe ||
                    StateMode.Value == AIStateManager.StateKind.Alert)
                {
                    // 安全時、または警戒時
                    // 敵を一人でも発見したら戦闘状態に移行
                    if (detectedEnemies.Any())
                    {
                        TargetEnemy.Value = detectedEnemies.First();    // キャラクターの生成順序が早い方から優先的にターゲットされてしまう
                                                                        //Debug.Log($"Enemy Detected (From Team:{_characterState.Team}, Id:{_characterState.ID}, To Team:{TargetEnemy.Value?.Team}, Id:{TargetEnemy.Value?.ID}");

                        // 警戒状態に移行するコルーチンを中止
                        if (_stateModeSubscriver != null)
                        {
                            _stateModeSubscriver.Dispose();
                            _stateModeSubscriver = null;
                        }

                        if (!IsPriorityRunning)
                        {
                            StateMode.Value = AIStateManager.StateKind.Combat;
                        }
                    }
                    else if (runningEnemies.Any() && !IsPriorityRunning)
                    {
                        BeginAlert(runningEnemies.First().transform.position - _characterTransform.position);
                    }
                    else
                    {
                        TargetEnemy.Value = null;
                    }
                }
                else if (StateMode.Value == AIStateManager.StateKind.Combat)
                {
                    if (detectedEnemies.Contains(TargetEnemy.Value))
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
                            TargetEnemy.Value = detectedEnemies.First();    // キャラクターの生成順序が早い方から優先的にターゲットされてしまう
                            //Debug.Log($"Enemy Detected (From Team:{_characterState.Team}, Id:{_characterState.ID}, To Team:{_targetEnemy.Value.Team}, Id:{_targetEnemy.Value.ID}");
                        }
                        else
                        {
                            // しばらく記憶してから警戒状態に移行
                            if (_stateModeSubscriver == null)
                            {
                                _stateModeSubscriver = Observable.FromCoroutine(CombatToAlert).Subscribe().AddTo(CharacterState);
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(AIParameter.DetectionDelay);
        }
    }

    private IEnumerator CombatToAlert()
    {
        yield return new WaitForSeconds(AIParameter.MissDelay * Random.Range(0.5f, 1.5f));

        if (_stateModeSubscriver == null) yield break;

        BeginAlert(Vector3.zero);
    }

    private void BeginAlert(Vector3 direction)
    {
        TargetEnemy.Value = null;
        AlertDirection.Value = direction;

        StateMode.Value = AIStateManager.StateKind.Alert;
        _stateModeSubscriver = Observable.FromCoroutine(AlertToSafe).Subscribe().AddTo(CharacterState);
    }

    private IEnumerator AlertToSafe()
    {
        yield return new WaitForSeconds(AIParameter.AlertTime * Random.Range(0.8f, 1.5f));

        if (_stateModeSubscriver == null) yield break;

        StateMode.Value = AIStateManager.StateKind.Safe;
        _stateModeSubscriver.Dispose();
        _stateModeSubscriver = null;
    }

    private void DangerDetected(Vector3 direction)
    {
        if (StateMode.Value == AIStateManager.StateKind.Safe && !IsPriorityRunning)
        {
            BeginAlert(direction);
        }
    }
}
