using System;

[Serializable]
public class CharacterInformation
{
    public CharacterInformation(PointData pointData)
    {
        _pointData = pointData;
        _kind = new CharacterKind(_pointData.Data2);
        _team = new Team(_pointData.Data3);
    }

    public bool IsId(short id)
    {
        return id == _pointData.Data4;
    }

    public CharacterKind GetKind()
    {
        return _kind;
    }

    public Team GetTeam()
    {
        return _team;
    }

    private readonly PointData _pointData;
    private readonly CharacterKind _kind;
    private readonly Team _team;
}
