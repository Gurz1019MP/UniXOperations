using System;
using System.Collections.Generic;
using System.Linq;

public class EventMessages
{
    private EventMessages(string[] messages)
    {
        _eventMessage = messages.Select((m, i) => new { id = i, message = m }).ToDictionary(a => (short)a.id, a => a.message);
    }

    public static EventMessages Instance
    {
        get
        {
            if (_instance == null) throw new InvalidOperationException("EventMessagesが初期化されていません");

            return _instance;
        }
    }

    public IReadOnlyDictionary<short, string> Items => _eventMessage;

    public static void Initialize(string[] messages)
    {
        _instance = new EventMessages(messages);
    }

    private static EventMessages _instance;
    private Dictionary<short, string> _eventMessage;
}
