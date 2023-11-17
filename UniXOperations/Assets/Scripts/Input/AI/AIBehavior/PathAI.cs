using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

public class PathAI : AbstractAIBehavior
{
    public PathAI(IAIBehaviorData aIComponentData) : base(aIComponentData)
    {
        _characterTransform = CharacterState.transform;
        _headTransform = CharacterState.FpsCameraAnchor;

        CurrentPath = aIComponentData.FirstPath;
    }

    #region Public Property

    public float MovingAngle => 10f;
    public float MovingDistance => 0.3f;
    public float TrackingDistance => 1f;
    public float RunningMovingAngleConstant => 2f;
    public float WaitPathTimer => 5f;

    public PathContainer CurrentPath { get; private set; }


    #endregion

    #region PrivateField

    private Transform _characterTransform;
    private Transform _headTransform;
    private GameObject _trackingTarget;
    private bool _wasAlert;
    private System.IDisposable _lookAroundSubscriver;
    private System.IDisposable _waitPathSubscriver;

    #endregion

    public override void Update()
    {
        if (CurrentPath != null)
        {
            if (CurrentPath.Path is SinglePath path)
            {
                if (path.Kind == SinglePath.PathKind.Walking)
                {
                    Moving(CurrentPath.transform.position, false, false, ChangeNextPathContainer);
                }
                else if (path.Kind == SinglePath.PathKind.Running)
                {
                    Moving(CurrentPath.transform.position, true, false, ChangeNextPathContainer);
                }
                else if (path.Kind == SinglePath.PathKind.Waiting)
                {
                    WalkAndWait(false);
                }
                else if (path.Kind == SinglePath.PathKind.Tracking)
                {
                    Tracking(path.NextId);
                }
                else if (path.Kind == SinglePath.PathKind.AlertWaiting)
                {
                    WalkAndWait(true);
                }
                else if (path.Kind == SinglePath.PathKind.TimeWaiting)
                {
                    Moving(CurrentPath.transform.position, false, false, BeginChangeNextPathContainerWaitTime);
                }
                else if (path.Kind == SinglePath.PathKind.ThrowingGrenade)
                {
                    ThrowingGrenade(CurrentPath.transform.position);
                }
                else if (path.Kind == SinglePath.PathKind.PriorityRunning)
                {
                    Moving(CurrentPath.transform.position, true, false, ChangeNextPathContainer); // 未実装に付き一時
                }
            }
            else if (CurrentPath.Path is RandomPath)
            {
                ChangeNextPathContainer();
            }
        }
    }

    public override void EnterState()
    {
        EndWaitLookAround();
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

    private void WalkAndWait(bool isAlertWaiting)
    {
        if (_lookAroundSubscriver == null)
        {
            Moving(CurrentPath.transform.position, false, false, () => BeginWaitLookAround(isAlertWaiting));
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
        EndWaitLookAround();
        _wasAlert = false;

        var nextPath = CurrentPath.Path.GetNextPathContainer();

        //Debug.Log($"ChangePath : {CurrentPath.Path.Id}, NextPath : {nextPath.Path.Id}");

        CurrentPath = nextPath;
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
        if (_lookAroundSubscriver != null) return;

        _lookAroundSubscriver = Observable.FromCoroutine(_ => WaitLookAround(isAlertWaiting)).Subscribe().AddTo(CharacterState);
    }

    private void EndWaitLookAround()
    {
        if (_lookAroundSubscriver == null) return;

        _lookAroundSubscriver.Dispose();
        _lookAroundSubscriver = null;
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

    private IEnumerator FireSemi()
    {
        Controller.Fire = true;
        yield return new WaitForSeconds(0.2f);
        Controller.Fire = false;
        yield return new WaitForSeconds(0.2f);
    }
}
