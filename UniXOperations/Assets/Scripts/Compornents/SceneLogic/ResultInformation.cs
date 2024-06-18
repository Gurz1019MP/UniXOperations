using System;

public struct ResultInformation
{
    public ResultInformation(bool isSuccess,
                             TimeSpan time,
                             CombatStatistics combatStatistics)
    {
        IsSuccess = isSuccess;
        Time = time;
        CombatStatistics = combatStatistics;
    }

    public bool IsSuccess { get; }

    public TimeSpan Time { get; }

    public CombatStatistics CombatStatistics { get; }
}
