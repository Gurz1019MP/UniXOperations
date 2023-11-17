using System;

public class DummyEvent : AbstractMissionEvent
{
    public DummyEvent(GameDataContainer gameDataContainer) : base(gameDataContainer)
    {

    }

    public override void BeginEvent()
    {
        throw new NotImplementedException();
    }
}
