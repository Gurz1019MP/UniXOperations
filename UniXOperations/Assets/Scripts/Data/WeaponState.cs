using System;
using UnityEngine;
using UnityEngine.TextCore.Text;

[Serializable]
public struct WeaponState : IEquatable<WeaponState>
{
    public WeaponState(short kind,
                       int ammo,
                       int magazine)
    {
        Kind = kind;
        Ammo = ammo;
        Magazine = magazine;
    }

    public short Kind { get; }
    public int Ammo { get; }
    public int Magazine { get; }

    public static WeaponState None { get; } = new WeaponState(0, 0, 0);

    public static WeaponState EquipWeapon(short kind)
    {
        return new WeaponState(kind, WeaponSpec.GetWeaponSpec(kind).MagazineSize * 2, WeaponSpec.GetWeaponSpec(kind).MagazineSize);
    }

    public bool CanChangeFireMode()
    {
        return WeaponSpec.CanChangeFireMode(Kind);
    }

    public WeaponState ChangeFireMode()
    {
        if (WeaponSpec.GetWeaponSpec(Kind).ChangeFireModeWeapon == 0) return this;

        return new WeaponState(WeaponSpec.GetWeaponSpec(Kind).ChangeFireModeWeapon, Ammo, Magazine);
    }

    public bool HasScope()
    {
        return WeaponSpec.HasScope(Kind);
    }

    public WeaponState LostWeapon()
    {
        return new WeaponState(0, 0, 0);
    }

    public float GetCorrectedMinAccuracyRatio(bool isZoom)
    {
        float ratio = WeaponSpec.GetWeaponSpec(Kind).BulletCount > 1 ? 0.5f : 0;
        if (WeaponSpec.GetWeaponSpec(Kind).Scope == WeaponSpec.ZoomModeEnum.Detail && !isZoom)
        {
            ratio = 1f;
        }
        return ratio;
    }

    public AnimationCurve GetAccuracyAnimationCurve()
    {
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0, WeaponSpec.GetWeaponSpec(Kind).AccuracyMin);
        curve.AddKey(1, WeaponSpec.GetWeaponSpec(Kind).AccuracyMax);

        return curve;
    }

    public string GetHolding()
    {
        return WeaponSpec.GetWeaponSpec(Kind).Holding.ToString();
    }

    public bool IsHoldingBody()
    {
        return WeaponSpec.GetWeaponSpec(Kind).Holding == WeaponSpec.HoldingMethod.Body;
    }

    public Vector3 GetHandlingPosition()
    {
        return WeaponSpec.GetWeaponSpec(Kind).HandlingPosition;
    }

    public Vector3 GetMuzzlePosition()
    {
        return WeaponSpec.GetWeaponSpec(Kind).MuzzlePosition;
    }

    public bool CanGetResourceWeaponModel()
    {
        string modelName = WeaponSpec.GetWeaponSpec(Kind).ModelName;
        return !string.IsNullOrEmpty(modelName) && modelName != "null";
    }

    public string GetResourceWeaponModel()
    {
        return WeaponSpec.GetResourceWeaponModel(Kind);
    }

    public bool GetIsGrenade()
    {
        return WeaponSpec.GetWeaponSpec(Kind).IsGrenade;
    }

    public void SetBullet(IBullet bullet, Team team)
    {
        WeaponSpec spec = WeaponSpec.GetWeaponSpec(Kind);

        bullet.Damage = spec.FirePower;
        bullet.Penetration = spec.PenetrationPower;
        bullet.Speed = spec.BulletSpeed;
        bullet.Team = team;
        bullet.Sound = spec.Sound;
    }

    public short GetBulletCount()
    {
        return WeaponSpec.GetWeaponSpec(Kind).BulletCount;
    }

    public float GetCameraMuzzleJump()
    {
        return WeaponSpec.GetWeaponSpec(Kind).FirePower * 0.1f;
    }

    public float GetMuzzleJump()
    {
        return WeaponSpec.GetWeaponSpec(Kind).MuzzleJump;
    }

    public string GetFireAudio()
    {
        return WeaponSpec.GetWeaponSpec(Kind).FireAudio;
    }

    public bool GetIsFullAuto()
    {
        return WeaponSpec.GetWeaponSpec(Kind).FullAuto;
    }

    public WaitForSeconds WaitForFireInterval()
    {
        return new WaitForSeconds(WeaponSpec.GetWeaponSpec(Kind).FiringInterval);
    }

    public WaitForSeconds WaitForReloadTime()
    {
        return new WaitForSeconds(WeaponSpec.GetWeaponSpec(Kind).ReloadTime);
    }

    public WeaponState Reload()
    {
        WeaponSpec spec = WeaponSpec.GetWeaponSpec(Kind);
        return new WeaponState(Kind, Ammo - Magazine, Ammo > spec.MagazineSize ? spec.MagazineSize : Ammo);
    }

    public WeaponState NextWeapon()
    {
        return new WeaponState(WeaponSpec.NextWeaponId(Kind), Ammo, Magazine);
    }

    public WeaponState PreWeapon()
    {
        return new WeaponState(WeaponSpec.PreWeaponId(Kind), Ammo, Magazine);
    }

    public WeaponState AddAmmo()
    {
        return new WeaponState(Kind, Ammo + WeaponSpec.WeaponAmmo(Kind), Magazine);
    }

    #region IEquatable

    public override bool Equals(object obj) => obj is WeaponState other && this.Equals(other);

    public bool Equals(WeaponState p) => Kind == p.Kind && Ammo == p.Ammo && Magazine == p.Magazine;

    public override int GetHashCode() => (Kind, Ammo, Magazine).GetHashCode();

    public static bool operator ==(WeaponState lhs, WeaponState rhs) => lhs.Equals(rhs);

    public static bool operator !=(WeaponState lhs, WeaponState rhs) => !(lhs == rhs);

    #endregion
}
