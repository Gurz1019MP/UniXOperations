using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public readonly struct WeaponState
{
    public WeaponState(int ammo,
                       int magazine)
    {
        Ammo = ammo;
        Magazine = magazine;
    }

    public int Ammo { get; }

    public int Magazine { get; }

    public readonly bool HasAmmo => Ammo > 0;

    public readonly bool OutOfMagazine => Magazine == 0;

    public readonly bool OutOfAmmo => Ammo == 0 && OutOfMagazine;

    public static WeaponState None { get; } = new WeaponState(0, 0);

    public static WeaponState EquipWeapon(WeaponSpec kind)
    {
        return new WeaponState(kind.MagazineSize * 2, kind.MagazineSize);
    }

    public WeaponState AddAmmo(WeaponSpec spec)
    {
        return new WeaponState(Ammo + spec.MagazineSize, Magazine);
    }

    public WeaponState Load()
    {
        return new WeaponState(Ammo, Magazine - 1);
    }

    public WeaponState Reload(WeaponSpec spec)
    {
        return new WeaponState(Ammo - spec.MagazineSize, Ammo > spec.MagazineSize ? spec.MagazineSize : Ammo);
    }
}
