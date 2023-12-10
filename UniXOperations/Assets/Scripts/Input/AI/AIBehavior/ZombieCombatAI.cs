using System.Collections;
using UniRx;
using UnityEngine;

public class ZombieCombatAI : AbstractAIBehavior, ICombatAI
{
    public ZombieCombatAI(IAIBehaviorData aIComponentData) : base(aIComponentData)
    {
        _headTransform = CharacterState.FpsCameraAnchor;
    }

    #region PrivateField

    private Transform _headTransform;
    private Character _targetEnemy;
    private System.IDisposable _fireSubscriver;

    #endregion

    public override void Update()
    {
        if (_targetEnemy != null && _targetEnemy.gameObject != null)
        {
            var enemyDir = _targetEnemy.gameObject.transform.position + _targetEnemy.gameObject.transform.up * AIParameter.AimHeightOffset - _headTransform.position;         // 現在地から敵へのベクトル
            var enemyDirXZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.up);              // 上記ベクトルをXY平面に射影
            var angleXZ = Vector3.SignedAngle(_headTransform.forward, enemyDirXZPlane, _headTransform.up); // 今向いている方向との差を計算
            var enemyDirYZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.right);
            var angleYZ = Vector3.SignedAngle(_headTransform.forward, enemyDirYZPlane, _headTransform.right);

            Controller.MouseX = angleXZ * AIParameter.PropControlConstant;
            Controller.MouseY = angleYZ * AIParameter.PropControlConstant;

            Controller.Walk = true;

            if (_fireSubscriver == null)
            {
                _fireSubscriver = Observable.FromCoroutine(FireSemi).Subscribe().AddTo(CharacterState);
            }
        }
        else
        {
            Controller.MouseY = 0;
        }
    }

    public override void EnterState()
    {
        CharacterState.ArmTargetAngle = 0;
    }

    public override void LeaveState()
    {
        CharacterState.ArmTargetAngle = 90;
    }

    public void SetTargetEnemy(Character enemy)
    {
        _targetEnemy = enemy;
    }

    private IEnumerator FireSemi()
    {
        Controller.Fire = true;
        yield return new WaitForSeconds(0.2f);
        Controller.Fire = false;
        yield return new WaitForSeconds(0.2f);
        _fireSubscriver = null;
    }
}
