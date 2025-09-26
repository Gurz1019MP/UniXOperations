using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LookDirection : IInputter
{
    public LookDirection(Transform characterTransform, Transform headTransform, AISpec aiSkill)
    {
        _characterTransform = characterTransform;
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

    public void InputUpdate(Vector3 targetDirection)
    {
        var angleXZ = Vector3.SignedAngle(_characterTransform.forward, targetDirection, _characterTransform.up);   // 今向いている方向との差を計算
        var angleYZ = Vector3.SignedAngle(_headTransform.forward, _characterTransform.forward, _headTransform.right);

        MouseX = _aiSkill.GetCorrectionOperatingVolume(angleXZ);
        MouseY = _aiSkill.GetCorrectionOperatingVolume(angleYZ);
    }

    private Transform _characterTransform;
    private Transform _headTransform;
    private AISpec _aiSkill;
}
