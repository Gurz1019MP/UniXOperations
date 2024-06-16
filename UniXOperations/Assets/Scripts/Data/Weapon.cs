public class Weapon
{
    public Weapon(PointData pointData, WeaponSpec spec, WeaponState state)
    {
        PointData = pointData;
        Spec = spec;
        State = state;
    }

    public Weapon(WeaponSpec spec, WeaponState state)
    {
        Spec = spec;
        State = state;
    }

    public PointData PointData { get; }

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

    public void SetIsTriggerPulled(bool isTriggerPulled)
    {
        IsTriggerPulled = isTriggerPulled;
    }

    public void SetIsSemiFired(bool isSemiFired)
    {
        IsSemiFired = isSemiFired;
    }

    public void SetIsLoading(bool isLoading)
    {
        IsLoading = isLoading;
    }

    public void SetIsReloading(bool isReloading)
    {
        IsReloading = isReloading;
    }
}
