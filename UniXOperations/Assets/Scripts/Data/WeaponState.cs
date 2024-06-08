using System;

[Serializable]
public class WeaponState
{
    public short Kind;
    public int Ammo;
    public int Magazine;

    public WeaponSpec Spec
    {
        get
        {
            return WeaponSpec.GetSpec(Kind);
        }
    }

    public void ChangeWeapon(WeaponState newWeapon)
    {
        Kind = newWeapon.Kind;
        Ammo = newWeapon.Ammo;
        Magazine = newWeapon.Magazine;
    }

    public void ChangeFireMode()
    {
        if (Spec.ChangeFireModeWeapon == 0) return;

        Kind = Spec.ChangeFireModeWeapon;
    }

    public void LostWeapon()
    {
        Kind = 0;
        Ammo = 0;
        Magazine = 0;
    }

    public static readonly WeaponState None = new WeaponState();
}
