using System.Collections.Generic;
using System.Linq;

public class EventMessages
{
    public EventMessages(string[] messages)
    {
        _eventMessage = messages.Select((m, i) => new { id = i, message = m }).ToDictionary(a => (short)a.id, a => a.message);
    }

    public string GetMessage(short id)
    {
        return _eventMessage[id];
    }

    private Dictionary<short, string> _eventMessage;
}
