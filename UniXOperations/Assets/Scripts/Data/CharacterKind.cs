using System;

public readonly struct CharacterKind : IEquatable<CharacterKind>
{
    public CharacterKind(sbyte kind)
    {
        _kind = kind;
    }

    public override bool Equals(object obj) => obj is CharacterKind other && this.Equals(other);

    public bool Equals(CharacterKind ck) => _kind == ck._kind;

    public override int GetHashCode() => _kind.GetHashCode();

    public static bool operator ==(CharacterKind lhs, CharacterKind rhs) => lhs.Equals(rhs);

    public static bool operator !=(CharacterKind lhs, CharacterKind rhs) => !(lhs == rhs);

    private readonly sbyte _kind;
}
