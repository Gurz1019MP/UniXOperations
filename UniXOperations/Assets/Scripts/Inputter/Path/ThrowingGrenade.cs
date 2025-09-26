using System;
using UniRx;
using UnityEngine;

public class ThrowingGrenade : IInputter, IPath, INextPath
{
    public ThrowingGrenade(PointData pointData, Character character, Transform headTransform, AISpec aiSkill)
    {
        _pointData = pointData;
        _fireTrigger = new FireTrigger();
        _character = character;
        _headTransform = headTransform;
        _aiSkill = aiSkill;
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

    public IObservable<short> OnChangeNextPath => _onChangeNextPathSubject;

    public void InputUpdate()
    {
        if (_character.State.CurrentWeapon.Spec.IsGrenade)
        {
            var targetDirection = _pointData.Position - _headTransform.position;         // 現在地から敵へのベクトル
            var targetDirectionXZPlane = Vector3.ProjectOnPlane(targetDirection, _headTransform.up);              // 上記ベクトルをXY平面に射影
            var angleXZ = Vector3.SignedAngle(_headTransform.forward, targetDirectionXZPlane, _headTransform.up); // 今向いている方向との差を計算
            var targetDirectionYZPlane = Vector3.ProjectOnPlane(targetDirection, _headTransform.right);
            var angleYZ = Vector3.SignedAngle(_headTransform.forward, targetDirectionYZPlane, _headTransform.right);

            MouseX = _aiSkill.GetCorrectionOperatingVolume(angleXZ);
            MouseY = _aiSkill.GetCorrectionOperatingVolume(angleYZ);

            if (angleXZ < 0.1 && angleYZ < 0.1)
            {
                Observable.FromCoroutine(_fireTrigger.FireSemi).Subscribe(_ => { _onChangeNextPathSubject.OnNext(_pointData.Data3); });
            }
        }
        else if (_character.State.DisableWeapon.Spec.IsGrenade)
        {
            if (!_character.State.IsSwitching)
            {
                Observable.FromCoroutine(_ => PushOneTrigger.PushOne(v => SwitchWeapon = v)).Subscribe();
            }
        }
        else
        {
            _onChangeNextPathSubject.OnNext(_pointData.Data3);
        }

        Fire = _fireTrigger.Fire;
    }

    private readonly PointData _pointData;
    private readonly FireTrigger _fireTrigger;
    private readonly Character _character;
    private readonly Transform _headTransform;
    private readonly AISpec _aiSkill;
    private readonly Subject<short> _onChangeNextPathSubject = new Subject<short>();
}
