using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;

public class Combat : IInputter
{
    public Combat(Character character, Transform headTransform, AISpec aiSkill)
    {
        _character = character;
        _headTransform = headTransform;
        _aiSkill = aiSkill;
        _fireTrigger = new FireTrigger();
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

    public void InputUpdate(Vector3 targetPosition)
    {
        var enemyDir = targetPosition - _headTransform.position;                               // 現在地から敵へのベクトル
        var enemyDirXZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.up);              // 上記ベクトルをXY平面に射影
        var enemyAngleXZ = Vector3.SignedAngle(_headTransform.forward, enemyDirXZPlane, _headTransform.up); // 今向いている方向との差を計算
        var enemyDirYZPlane = Vector3.ProjectOnPlane(enemyDir, _headTransform.right);
        var enemyAngleYZ = Vector3.SignedAngle(_headTransform.forward, enemyDirYZPlane, _headTransform.right);

        MouseX = _aiSkill.GetCorrectionOperatingVolume(enemyAngleXZ);
        MouseY = _aiSkill.GetCorrectionOperatingVolume(enemyAngleYZ);

        if (_aiSkill.BeShoot(enemyAngleXZ, enemyAngleYZ) &&
            !Physics.Raycast(_headTransform.position, enemyDir, enemyDir.magnitude, LayerMask.GetMask(ConstantsProvider.LayerMask_Stage)))
        {
            if (_fireSubscriber == null)
            {
                if (_character.State.CurrentWeapon.Spec.FullAuto)
                {
                    _fireSubscriber = Observable.FromCoroutine(_fireTrigger.FireFull).Subscribe(_ => { _fireSubscriber = null; });
                }
                else
                {
                    _fireSubscriber = Observable.FromCoroutine(_fireTrigger.FireSemi).Subscribe(_ => { _fireSubscriber = null; });
                }
            }
        }

        if (!_character.State.CurrentWeapon.Spec.IsNone &&
            _character.State.CurrentWeapon.State.OutOfMagazine)
        {
            if (_character.State.CurrentWeapon.State.OutOfAmmo)
            {
                _fireSubscriber = Observable.FromCoroutine(_ => PushOneTrigger.PushOne(v => DropWeapon = v)).Subscribe(_ => { _fireSubscriber = null; });
            }
            else if (_character.State.CurrentWeapon.State.HasAmmo &&
                     !_character.State.CurrentWeapon.IsReloading)
            {
                _fireSubscriber = Observable.FromCoroutine(_ => PushOneTrigger.PushOne(v => Reload = v)).Subscribe(_ => { _fireSubscriber = null; });
            }
        }

        if (_character.State.CurrentWeapon.Spec.IsNone &&
            !_character.State.DisableWeapon.Spec.IsNone &&
            !_character.State.IsSwitching)
        {
            _fireSubscriber = Observable.FromCoroutine(_ => PushOneTrigger.PushOne(v => SwitchWeapon = v)).Subscribe(_ => { _fireSubscriber = null; });
        }

        if (_character.State.CurrentWeapon.Spec.HasScope &&
            !_character.State.IsZoom)
        {
            _fireSubscriber = Observable.FromCoroutine(_ => PushOneTrigger.PushOne(v => Zoom = v)).Subscribe(_ => { _fireSubscriber = null; });
        }

        if (_character.State.CurrentWeapon.Spec.IsNone &&
            _character.State.DisableWeapon.Spec.IsNone)
        {
            _character.SetState(_character.State.BuildWithArmTargetAngle(-90));
            if (_character.State.IsArmLookAtMode)
            {
                _character.SetState(_character.State.BuildWithIsArmLookAtMode(false));
            }
        }
    }

    private readonly Character _character;
    private readonly Transform _headTransform;
    private readonly AISpec _aiSkill;
    private readonly FireTrigger _fireTrigger;
    private IDisposable _fireSubscriber = null;
}
