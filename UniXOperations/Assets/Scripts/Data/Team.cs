using System;

public struct Team : IEquatable<Team>
{
    #region .ctor

    static Team()
    {
        _empty = new Team(true);
    }

    public Team(short id) : this(false)
    {
        _id = id;
        _isEmpty = false;
    }

    public Team(bool isEmpty)
    {
        _id = 0;
        _isEmpty = isEmpty;
    }

    #endregion

    #region Property

    public static Team Empty => _empty;

    #endregion

    #region Method

    public override bool Equals(object obj) => obj is Team other && this.Equals(other);

    public bool Equals(Team p) => _id == p._id && _isEmpty == p._isEmpty;

    public override int GetHashCode() => (_id, _isEmpty).GetHashCode();

    public static bool operator ==(Team lhs, Team rhs) => lhs.Equals(rhs);

    public static bool operator !=(Team lhs, Team rhs) => !(lhs == rhs);

    #endregion

    #region Field

    private static readonly Team _empty;
    private readonly short _id;
    private readonly bool _isEmpty;

    #endregion
}
