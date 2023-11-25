using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UnityEngine.UIElements;

public class PathAI : AbstractAIBehavior
{
    public PathAI(IAIBehaviorData aIComponentData) : base(aIComponentData)
    {
        _characterTransform = CharacterState.transform;
        _headTransform = CharacterState.FpsCameraAnchor;

        CurrentPath.Value = aIComponentData.FirstPath;
    }

    #region Public Property

    public float MovingAngle => 10f;
    public float MovingDistance => 0.3f;
    public float TrackingDistance => 1f;
    public float RunningMovingAngleConstant => 2f;
    public float WaitPathTimer => 5f;

    public ReactiveProperty<PathContainer> CurrentPath { get; private set; } = new ReactiveProperty<PathContainer>();


    #endregion

    #region PrivateField

    private Transform _characterTransform;
    private Transform _headTransform;
    private GameObject _trackingTarget;
    private bool _wasAlert;
    private System.IDisposable _lookSubscriver;
    private System.IDisposable _waitPathSubscriver;
    private CharacterState _targetEnemy;
    private System.IDisposable _fireSubscriver;

    #endregion

    public override void Update()
    {
        if (CurrentPath.Value != null)
        {
            if (CurrentPath.Value.Path is SinglePath path)
            {
                if (path.Kind == SinglePath.PathKind.Walking)
                {
                    Moving(CurrentPath.Value.transform.position, false, false, ChangeNextPathContainer);
                }
                else if (path.Kind == SinglePath.PathKind.Running)
                {
                    Moving(CurrentPath.Value.transform.position, true, false, ChangeNextPathContainer);
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
                    ThrowingGrenade(CurrentPath.Value.transform.position);
                }
                else if (path.Kind == SinglePath.PathKind.PriorityRunning)
                {
                    if (_targetEnemy == null)
                    {
                        Moving(CurrentPath.Value.transform.position, true, false, ChangeNextPathContainer);
                    }
                    else
                    {
                        CombatAndMoving(CurrentPath.Value.transform.position, ChangeNextPathContainer);
                    }
                }
            }
            else if (CurrentPath.Value.Path is RandomPath)
            {
                ChangeNextPathContainer();
            }
        }
    }

    public override void EnterState()
    {
        EndWaitLook();
    }

    public void SetWasAlert()
    {
        _wasAlert = true;
    }

    private void Moving(Vector3 targetPoint, bool isRunning, bool isTracking, UnityAction actionWhenReaching)
    {
        var pathDir = targetPoint - _characterTransform.position;              // 現在地からポイントへのベクトル
        var pathDirPlane = Vector3.ProjectOnPlane(pathDir, Vector3.up);                 // 上記ベクトルをXY平面に射影
        var angleXZ = Vector3.SignedAngle(_characterTransform.forward, pathDirPlane, _characterTransform.up);   // 今向いている方向との差を計算
        var angleYZ = Vector3.SignedAngle(_headTransform.forward, _characterTransform.forward, _headTransform.right);

        Controller.MouseX = angleXZ * AIParameter.PropControlConstant;
        Controller.MouseY = angleYZ * AIParameter.PropControlConstant;
        //MouseX = Mathf.Sign(angle) * 0.5f;

        float movingAngle = MovingAngle * (isRunning ? RunningMovingAngleConstant : 1);
        float distance = isTracking ? TrackingDistance : MovingDistance;
        if (Mathf.Abs(angleXZ) < movingAngle && pathDirPlane.magnitude > distance)
        {
            if (isRunning)
            {
                Controller.Vertical = pathDirPlane.magnitude * AIParameter.PropControlConstant;
            }
            else
            {
                Controller.Walk = true;
            }
        }
        else
        {
            Controller.Vertical = 0;
            Controller.Walk = false;

            if (pathDirPlane.magnitude <= distance)
            {
                actionWhenReaching();
            }
        }

        //Debug.Log($"angle : {angle}, distance : {pathDirPlane.magnitude}");
    }

    private void WalkAndWait(bool isAlertWaiting, bool isTimeWaiting)
    {
        if (_lookSubscriver == null)
        {
            if (isTimeWaiting)
            {
                Moving(CurrentPath.Value.transform.position, false, false, () => BeginChangeNextPathContainerWaitTimeAndLookDirection());
            }
            else
            {
                Moving(CurrentPath.Value.transform.position, false, false, () => BeginWaitLookAround(isAlertWaiting));
            }
        }
        else
        {
            var angleYZ = Vector3.SignedAngle(_headTransform.forward, _characterTransform.forward, _headTransform.right);
            Controller.MouseY = angleYZ * AIParameter.PropControlConstant;
        }
    }

    private void Tracking(short targetId)
    {
        try
        {
            _trackingTarget = GameDataContainer.Characters.FirstOrDefault(c => c.ID == targetId).gameObject;
        }
        catch
        {
            Debug.Log("追尾対象の取得に失敗");
        }

        if (_trackingTarget != null)
        {
            Moving(_trackingTarget.transform.position, true, true, () => { });
        }
        else
        {
            CurrentPath = null;
        }
    }

    private void ThrowingGrenade(Vector3 targetPoint)
    {
        Stop();

        if (CharacterState.CurrentWeaponState.Kind == 13)
        {
            var enemyDir = targetPoint - _headTransform.position;         // 現在地から敵へのベクトル
            var enemyDirXZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.up);              // 上記ベクトルをXY平面に射影
            var angleXZ = Vector3.SignedAngle(_headTransform.forward, enemyDirXZPlane, _headTransform.up); // 今向いている方向との差を計算
            var enemyDirYZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.right);
            var angleYZ = Vector3.SignedAngle(_headTransform.forward, enemyDirYZPlane, _headTransform.right);

            Controller.MouseX = angleXZ * AIParameter.PropControlConstant;
            Controller.MouseY = angleYZ * AIParameter.PropControlConstant;

            if (angleXZ < 0.1 && angleYZ < 0.1)
            {
                Observable.FromCoroutine(FireSemi).Subscribe().AddTo(CharacterState);
                ChangeNextPathContainer();
            }
        }
        else if (CharacterState.DisableWeaponState.Kind == 13)
        {
            if (!CharacterState.IsSwitching)
            {
                Observable.FromCoroutine(_ => PushOne(v => Controller.SwitchWeapon = v)).Subscribe().AddTo(CharacterState);
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

        var nextPath = CurrentPath.Value.Path.GetNextPathContainer();

        //Debug.Log($"ChangePath : {CurrentPath.Path.Id}, NextPath : {nextPath.Path.Id}");

        CurrentPath.Value = nextPath;
    }

    private void BeginChangeNextPathContainerWaitTimeAndLookDirection()
    {
        BeginChangeNextPathContainerWaitTime();
        BeginLookDirection();
    }

    private void BeginChangeNextPathContainerWaitTime()
    {
        if (_waitPathSubscriver != null) return;

        _waitPathSubscriver = Observable.FromCoroutine(ChangeNextPathContainerWaitTime).Subscribe().AddTo(CharacterState);
    }

    private IEnumerator ChangeNextPathContainerWaitTime()
    {
        yield return new WaitForSeconds(WaitPathTimer);
        ChangeNextPathContainer();

        _waitPathSubscriver = null;
    }

    private void BeginWaitLookAround(bool isAlertWaiting)
    {
        if (_lookSubscriver != null) return;

        _lookSubscriver = Observable.FromCoroutine(_ => WaitLookAround(isAlertWaiting)).Subscribe().AddTo(CharacterState);
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

            Controller.MouseX = Random.Range(-0.5f, 0.5f);
            yield return new WaitForSeconds(2);
        }

        ChangeNextPathContainer();
    }

    private void BeginLookDirection()
    {
        if (_lookSubscriver != null) return;

        _lookSubscriver = Observable.FromCoroutine(_ => LookDirection()).Subscribe().AddTo(CharacterState);
    }

    private IEnumerator LookDirection()
    {
        while (true)
        {
            var angleXZ = Vector3.SignedAngle(_characterTransform.forward, CurrentPath.Value.transform.forward, _characterTransform.up);
            Controller.MouseX = angleXZ * AIParameter.PropControlConstant;
            yield return null;
        }
    }

    private IEnumerator FireFull()
    {
        Controller.Fire = true;
        yield return new WaitForSeconds(Random.Range(0.2f, 1.0f));
        Controller.Fire = false;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
        _fireSubscriver = null;
    }

    private IEnumerator FireSemi()
    {
        Controller.Fire = true;
        yield return new WaitForSeconds(Random.Range(0.2f, 1.0f));
        Controller.Fire = false;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
        _fireSubscriver = null;
    }

    public void SetTargetEnemy(CharacterState enemy)
    {
        _targetEnemy = enemy;
    }

    private void CombatAndMoving(Vector3 targetPoint, UnityAction actionWhenReaching)
    {
        if (_targetEnemy != null && _targetEnemy.gameObject != null)
        {
            var enemyDir = _targetEnemy.transform.position + _targetEnemy.transform.up * AIParameter.AimHeightOffset - _headTransform.position;         // 現在地から敵へのベクトル
            var enemyDirXZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.up);              // 上記ベクトルをXY平面に射影
            var enemyAngleXZ = Vector3.SignedAngle(_headTransform.forward, enemyDirXZPlane, _headTransform.up); // 今向いている方向との差を計算
            var enemyDirYZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.right);
            var enemyAngleYZ = Vector3.SignedAngle(_headTransform.forward, enemyDirYZPlane, _headTransform.right);

            Controller.MouseX = enemyAngleXZ * AIParameter.PropControlConstant;
            Controller.MouseY = enemyAngleYZ * AIParameter.PropControlConstant;

            Controller.MouseX += Random.Range(-AIParameter.ShootError, AIParameter.ShootError);
            Controller.MouseY += Random.Range(-AIParameter.ShootError, AIParameter.ShootError);

            if (CharacterState.CurrentWeaponState.Kind != 0 &&
                CharacterState.CurrentWeaponState.Magazine == 0)
            {
                if (CharacterState.CurrentWeaponState.Ammo == 0)
                {
                    Observable.FromCoroutine(_ => PushOne(v => Controller.DropWeapon = v)).Subscribe().AddTo(CharacterState);
                }
                else if (CharacterState.CurrentWeaponState.Ammo != 0 &&
                         !CharacterState.IsReloading)
                {
                    Observable.FromCoroutine(_ => PushOne(v => Controller.Reload = v)).Subscribe().AddTo(CharacterState);
                }
            }

            if (CharacterState.CurrentWeaponState.Kind == 0 &&
                CharacterState.DisableWeaponState.Kind != 0 &&
                !CharacterState.IsSwitching)
            {
                Observable.FromCoroutine(_ => PushOne(v => Controller.SwitchWeapon = v)).Subscribe().AddTo(CharacterState);
            }

            if (CharacterState.CurrentWeaponState.Kind == 0 &&
                CharacterState.DisableWeaponState.Kind == 0)
            {
                CharacterState.ArmController.TargetAngle = -90;
                if (CharacterState.ArmController._isLookAtMode)
                {
                    CharacterState.ArmController.TargetAngleMode();
                }
            }

            if (Mathf.Abs(enemyAngleXZ) < AIParameter.ShootThreshold && Mathf.Abs(enemyAngleYZ) < AIParameter.ShootThreshold &&
                !Physics.Raycast(_headTransform.position, enemyDir, enemyDir.magnitude, LayerMask.GetMask("Stage")))
            {
                if (_fireSubscriver == null)
                {
                    if (CharacterState.CurrentWeaponState.Spec.FullAuto)
                    {
                        _fireSubscriver = Observable.FromCoroutine(FireFull).Subscribe().AddTo(CharacterState);
                    }
                    else
                    {
                        _fireSubscriver = Observable.FromCoroutine(FireSemi).Subscribe().AddTo(CharacterState);
                    }
                }
            }

            var pathDir = targetPoint - _characterTransform.position;              // 現在地からポイントへのベクトル
            var pathDirPlane = Vector3.ProjectOnPlane(pathDir, Vector3.up);                 // 上記ベクトルをXY平面に射影
            var vertical = Vector3.Dot(_characterTransform.forward, pathDirPlane);

            if (pathDirPlane.magnitude > MovingDistance)
            {
                Controller.Vertical = Vector3.Dot(_characterTransform.forward, pathDirPlane);
                Controller.Horizontal = Vector3.Dot(_characterTransform.right, pathDirPlane);
            }
            else
            {
                Controller.Vertical = 0;
                Controller.Walk = false;

                actionWhenReaching();
            }
        }
        else
        {
            Controller.MouseY = 0;
        }
    }
}
