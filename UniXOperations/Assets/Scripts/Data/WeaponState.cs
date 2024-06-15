using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public readonly struct WeaponState
{
    public WeaponState(WeaponSpec spec,
                       int ammo,
                       int magazine,
                       bool isTriggerPulled,
                       bool isSemiFired,
                       bool isLoading,
                       bool isReloading)
    {
        Spec = spec;
        Ammo = ammo;
        Magazine = magazine;
        IsTriggerPulled = isTriggerPulled;
        IsSemiFired = isSemiFired;
        IsLoading = isLoading;
        IsReloading = isReloading;
    }

    public WeaponSpec Spec { get; }

    public int Ammo { get; }

    public int Magazine { get; }

    public bool IsTriggerPulled { get; }

    public bool IsSemiFired { get; }

    public bool IsLoading { get; }

    public bool IsReloading { get; }

    public readonly bool HasAmmo => Ammo > 0;

    public readonly bool OutOfMagazine => Magazine == 0;

    public readonly bool OutOfAmmo => Ammo == 0 && OutOfMagazine;

    public static WeaponState None { get; } = new WeaponState(WeaponSpec.None, 0, 0, false, false, false, false);

    public static WeaponState EquipWeapon(WeaponSpec kind)
    {
        return new WeaponState(kind, kind.MagazineSize * 2, kind.MagazineSize, false, false, false, false);
    }

    public WeaponState ChangeFireMode()
    {
        return new WeaponState(Spec.ChangeFireMode(), Ammo, Magazine, IsTriggerPulled, IsSemiFired, IsLoading, IsReloading);
    }

    public WeaponState LostWeapon()
    {
        return None;
    }

    public WeaponState Load()
    {
        return new WeaponState(Spec, Ammo, Magazine - 1, IsTriggerPulled, IsSemiFired, IsLoading, IsReloading);
    }

    public WeaponState Reload()
    {
        return new WeaponState(Spec, Ammo - Spec.MagazineSize, Ammo > Spec.MagazineSize ? Spec.MagazineSize : Ammo, IsTriggerPulled, IsSemiFired, IsLoading, IsReloading);
    }

    public WeaponState NextWeapon()
    {
        return new WeaponState(Spec.NextWeapon(), Ammo, Magazine, IsTriggerPulled, IsSemiFired, IsLoading, IsReloading);
    }

    public WeaponState PreWeapon()
    {
        return new WeaponState(Spec.PreWeapon(), Ammo, Magazine, IsTriggerPulled, IsSemiFired, IsLoading, IsReloading);
    }

    public WeaponState AddAmmo()
    {
        return new WeaponState(Spec, Ammo + Spec.MagazineSize, Magazine, IsTriggerPulled, IsSemiFired, IsLoading, IsReloading);
    }

    public WeaponState SetIsTriggerPulled(bool trigger)
    {
        return new WeaponState(Spec, Ammo, Magazine, trigger, IsSemiFired, IsLoading, IsReloading);
    }

    public WeaponState SetIsSemiFired(bool fired)
    {
        return new WeaponState(Spec, Ammo, Magazine, IsTriggerPulled, fired, IsLoading, IsReloading);
    }

    public WeaponState SetIsLoading(bool loading)
    {
        return new WeaponState(Spec, Ammo, Magazine, IsTriggerPulled, IsSemiFired, loading, IsReloading);
    }

    public WeaponState SetIsReloading(bool reloading)
    {
        return new WeaponState(Spec, Ammo, Magazine, IsTriggerPulled, IsSemiFired, IsLoading, reloading);
    }

    public WeaponState StartLoading()
    {
        return new WeaponState(Spec, Ammo, Magazine - 1, IsTriggerPulled, IsSemiFired, true, IsReloading);
    }

    public WeaponState EndReloading()
    {
        return new WeaponState(Spec, Ammo - Spec.MagazineSize, Ammo > Spec.MagazineSize ? Spec.MagazineSize : Ammo, IsTriggerPulled, IsSemiFired, IsLoading, false);
    }

    public WeaponState StandBy()
    {
        return new WeaponState(Spec, Ammo - Spec.MagazineSize, Ammo > Spec.MagazineSize ? Spec.MagazineSize : Ammo, false, false, false, false);
    }
}
