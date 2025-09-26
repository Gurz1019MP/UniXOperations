using System;

[Serializable]
public readonly struct CharacterInformation
{
    #region .ctor

    public CharacterInformation(PointData pointData)
    {
        _id = pointData.Data4;
        Kind = new CharacterKind(pointData.Data2);
        Team = new Team(pointData.Data3);
    }

    #endregion

    #region Property

    public CharacterKind Kind { get; }
    public Team Team { get; }

    #endregion

    #region Method

    public bool IsId(short id)
    {
        return id == _id;
    }

    #endregion

    #region Field

    private readonly short _id;
    
    #endregion
}
