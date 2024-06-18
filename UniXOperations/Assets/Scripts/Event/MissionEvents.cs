using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

public class MissionEvents
{
    public MissionEvents(AbstractMissionEvent[] missionEvents)
    {
        _missionEvents = missionEvents;
    }

    public void ForEach(UnityAction<AbstractMissionEvent> action)
    {
        foreach (AbstractMissionEvent missionEvents in _missionEvents)
        {
            action(missionEvents);
        }
    }

    public AbstractMissionEvent GetMissionEvent(short id)
    {
        return _missionEvents.SingleOrDefault(me => me.Id == id);
    }

    private AbstractMissionEvent[] _missionEvents;
}
