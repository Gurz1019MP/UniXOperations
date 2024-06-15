using System.Collections.Generic;
using System.Linq;

public class MissionEvents
{
    public MissionEvents(AbstractMissionEvent[] missionEvents)
    {
        _missionEvents = missionEvents;
    }

    public AbstractMissionEvent GetMissionEvent(short id)
    {
        return _missionEvents.Single(me => me.Id == id);
    }

    private AbstractMissionEvent[] _missionEvents;
}
