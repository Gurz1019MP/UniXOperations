public interface IInputter
{
    public float Horizontal { get; }

    public float Vertical { get; }

    public bool Fire { get; }

    public bool Jump { get; }

    public float MouseX { get; }

    public float MouseY { get; }

    public bool Walk { get; }

    public bool Reload { get; }

    public bool DropWeapon { get; }

    public bool Zoom { get; }

    public bool FireMode { get; }

    public bool SwitchWeapon { get; }
}
