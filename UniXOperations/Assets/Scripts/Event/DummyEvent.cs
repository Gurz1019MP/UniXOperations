using System;

public class DummyEvent : AbstractMissionEvent
{
    public DummyEvent(PointData pointData) : base(pointData) { }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        throw new NotImplementedException();
    }
}
