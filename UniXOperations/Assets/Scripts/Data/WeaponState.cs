using System;
using UnityEngine;

[Serializable]
public readonly struct WeaponState
{
    public WeaponState(WeaponSpec spec,
                       int ammo,
                       int magazine)
    {
        Spec = spec;
        Ammo = ammo;
        Magazine = magazine;
    }

    public WeaponSpec Spec { get; }
    public int Ammo { get; }
    public int Magazine { get; }

    public readonly bool HasAmmo => Ammo > 0;

    public readonly bool OutOfMagazine => Magazine == 0;

    public readonly bool OutOfAmmo => Ammo == 0 && OutOfMagazine;

    public static WeaponState None { get; } = new WeaponState(WeaponSpec.None, 0, 0);

    public static WeaponState EquipWeapon(WeaponSpec kind)
    {
        return new WeaponState(kind, kind.MagazineSize * 2, kind.MagazineSize);
    }

    public WeaponState ChangeFireMode()
    {
        return new WeaponState(Spec.ChangeFireMode(), Ammo, Magazine);
    }

    public WeaponState LostWeapon()
    {
        return new WeaponState(WeaponSpec.None, 0, 0);
    }

    public WeaponState Loading()
    {
        return new WeaponState(Spec, Ammo, Magazine - 1);
    }

    public WeaponState Reload()
    {
        WeaponSpec spec = Spec;
        return new WeaponState(Spec, Ammo - spec.MagazineSize, Ammo > spec.MagazineSize ? spec.MagazineSize : Ammo);
    }

    public WeaponState NextWeapon()
    {
        return new WeaponState(Spec.NextWeapon(), Ammo, Magazine);
    }

    public WeaponState PreWeapon()
    {
        return new WeaponState(Spec.PreWeapon(), Ammo, Magazine);
    }

    public WeaponState AddAmmo()
    {
        return new WeaponState(Spec, Ammo + Spec.MagazineSize, Magazine);
    }
}
