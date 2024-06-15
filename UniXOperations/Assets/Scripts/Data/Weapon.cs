public class Weapon
{
    public Weapon(WeaponSpec spec, WeaponState state)
    {
        Spec = spec;
        State = state;
    }

    public WeaponSpec Spec { get; private set; }

    public WeaponState State { get; private set; }

    public bool IsTriggerPulled { get; private set; }

    public bool IsSemiFired { get; private set; }

    public bool IsLoading { get; private set; }

    public bool IsReloading { get; private set; }

    public void NextWeapon()
    {
        Spec = Spec.NextWeapon();
    }

    public void PreWeapon()
    {
        Spec = Spec.PreWeapon();
    }

    public void ChangeFireMode()
    {
        Spec = Spec.ChangeFireMode();
    }

    public void SetState(WeaponState state)
    {
        State = state;
    }

    public void LostWeapon()
    {
        Spec = WeaponSpec.None;
        SetState(new WeaponState(0, 0));
    }

    public void AddAmmo()
    {
        SetState(State.AddAmmo(Spec));
    }

    public void Reload()
    {
        SetState(State.Reload(Spec));
    }

    public void SetFlags(bool isTriggerPulled, bool isSemiFired, bool isLoading, bool isReloading)
    {
        IsTriggerPulled = isTriggerPulled;
        IsSemiFired = isSemiFired;
        IsLoading = isLoading;
        IsReloading = isReloading;
    }
}
