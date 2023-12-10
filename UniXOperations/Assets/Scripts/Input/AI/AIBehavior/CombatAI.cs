using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

public class CombatAI : AbstractAIBehavior, ICombatAI
{
    public CombatAI(IAIBehaviorData aIComponentData) : base(aIComponentData)
    {
        _headTransform = CharacterState.FpsCameraAnchor;
    }

    #region PrivateField

    private Transform _headTransform;
    private Character _targetEnemy;
    private System.IDisposable _fireSubscriver;
    private System.IDisposable _evideSubscriver;

    #endregion

    public override void Update()
    {
        if (_targetEnemy != null && _targetEnemy.gameObject != null)
        {
            var enemyDir = _targetEnemy.transform.position + _targetEnemy.transform.up * AIParameter.AimHeightOffset - _headTransform.position;         // 現在地から敵へのベクトル
            var enemyDirXZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.up);              // 上記ベクトルをXY平面に射影
            var angleXZ = Vector3.SignedAngle(_headTransform.forward, enemyDirXZPlane, _headTransform.up); // 今向いている方向との差を計算
            var enemyDirYZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.right);
            var angleYZ = Vector3.SignedAngle(_headTransform.forward, enemyDirYZPlane, _headTransform.right);

            Controller.MouseX = angleXZ * AIParameter.PropControlConstant;
            Controller.MouseY = angleYZ * AIParameter.PropControlConstant;

            Controller.MouseX += Random.Range(-AIParameter.ShootError, AIParameter.ShootError);
            Controller.MouseY += Random.Range(-AIParameter.ShootError, AIParameter.ShootError);
            if (_evideSubscriver == null) _evideSubscriver = Observable.FromCoroutine(EvideChance).Subscribe().AddTo(CharacterState);
            Controller.Walk = false;

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

            if (CharacterState.CurrentWeaponState.Spec.Scope != WeaponSpec.ZoomModeEnum.None &&
                !CharacterState.IsZoom)
            {
                Observable.FromCoroutine(_ => PushOne(v => Controller.Zoom = v)).Subscribe().AddTo(CharacterState);
            }

            if (CharacterState.CurrentWeaponState.Kind == 0 &&
                CharacterState.DisableWeaponState.Kind == 0)
            {
                CharacterState.ArmTargetAngle = -90;
                if (CharacterState.IsArmLookAtMode)
                {
                    CharacterState.ArmTargetAngleMode();
                }
            }

            if (Mathf.Abs(angleXZ) < AIParameter.ShootThreshold && Mathf.Abs(angleYZ) < AIParameter.ShootThreshold &&
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
        }
        else
        {
            Controller.MouseY = 0;
        }
    }

    public override void LeaveState()
    {
        CharacterState.ArmTargetAngle = 90;
    }

    public void SetTargetEnemy(Character enemy)
    {
        _targetEnemy = enemy;
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

    private IEnumerator EvideChance()
    {
        var randomH = Random.Range(0, 20);
        if (randomH == 1)
        {
            Controller.Horizontal = 1f;
        }
        else if (randomH == 2)
        {
            Controller.Horizontal = -1f;
        }
        else
        {
            Controller.Horizontal = 0;
        }

        var randomY = Random.Range(0, 50);
        if (randomY == 1)
        {
            Controller.Vertical = 1f;
        }
        else if (randomY == 2)
        {
            Controller.Vertical = -1f;
        }
        else
        {
            Controller.Vertical = 0;
        }

        yield return new WaitForSeconds(0.1f);
        _evideSubscriver.Dispose();
        _evideSubscriver = null;
    }
}
