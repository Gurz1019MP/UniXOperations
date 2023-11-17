public interface ICharacterInputter : ICharacterInputterAxis, ICharacterInputterTrigger
{
}

public interface ICharacterInputterAxis
{
    float Horizontal { get; }
    float Vertical { get; }
    bool Fire { get; }
    bool Jump { get; }
    float MouseX { get; }
    float MouseY { get; }
    bool Walk { get; }
    bool Reload { get; }
    bool DropWeapon { get; }
    bool Zoom { get; }
    bool FireMode { get; }
    bool SwitchWeapon { get; }
    bool Weapon1 { get; }
    bool Weapon2 { get; }
}

public interface ICharacterInputterTrigger
{
    bool FireEnter { get; }
    bool JumpEnter { get; }
    bool ReloadEnter { get; }
    bool DropWeaponEnter { get; }
    bool ZoomEnter { get; }
    bool FireModeEnter { get; }
    bool SwitchWeaponEnter { get; }
    bool Weapon1Enter { get; }
    bool Weapon2Enter { get; }
}