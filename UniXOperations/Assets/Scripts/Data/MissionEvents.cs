using System.Collections.Generic;
using System.Linq;

public class MissionEvents
{
    public MissionEvents(MissionEventContainer[] missionEvents)
    {
        _missionEvents = missionEvents;
    }

    public MissionEventContainer GetMissionEventContainer(short id)
    {
        return _missionEvents.Single(me => me.MissionEvent.Id == id);
    }

    public AbstractMissionEvent GetMissionEvent(short id)
    {
        return GetMissionEventContainer(id).MissionEvent;
    }

    private MissionEventContainer[] _missionEvents;
}
