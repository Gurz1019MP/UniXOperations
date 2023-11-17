using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class AlertAI : AbstractAIBehavior
{
    public AlertAI(IAIBehaviorData aIComponentData) : base(aIComponentData)
    {
        _characterTransform = CharacterState.transform;
        _headTransform = CharacterState.FpsCameraAnchor;
    }

    #region PrivateField

    private Transform _characterTransform;
    private Transform _headTransform;
    private float _rollValue;
    private int _rollSign;
    private Vector3 _attackedDirection;

    #endregion

    public override void Update()
    {
        Stop();
        var angleYZ = Vector3.SignedAngle(_headTransform.forward, _characterTransform.forward, _headTransform.right);

        Controller.MouseX = _rollValue * _rollSign;
        Controller.MouseY = angleYZ * AIParameter.PropControlConstant;

        if (CharacterState.CurrentWeaponState.Kind == 0 &&
            CharacterState.DisableWeaponState.Kind != 0 &&
            !CharacterState.IsSwitching)
        {
            Observable.FromCoroutine(_ => PushOne(v => Controller.SwitchWeapon = v)).Subscribe().AddTo(CharacterState);
        }
    }

    public override void EnterState()
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

    public void SetAttackedDirection(Vector3 direction)
    {
        _attackedDirection = direction;
    }
}
