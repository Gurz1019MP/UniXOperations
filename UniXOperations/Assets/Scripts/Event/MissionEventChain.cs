using System;
using System.Linq;
using UniRx;

public class MissionEventChain
{
    public AbstractMissionEvent CurrentEvent { get; private set; }

    public IObservable<bool> OnEndEventChain => _onEndEventChainSubject;

    public IObservable<string> OnShowMessage => _onShowMessage;

    public MissionEventChain(AbstractMissionEvent firstEvent, GameDataContainer gameDataContainer)
    {
        _gameDataContainer = gameDataContainer;
        ChangeEventChain(firstEvent);
    }

    private void ChangeEventChain(AbstractMissionEvent nextEvent)
    {
        if (nextEvent != null)
        {
            CurrentEvent = nextEvent;

            CurrentEvent.OnChangeEventChain.Subscribe(OnChangeEventChainHandler);
            CurrentEvent.OnEndEventChain.Subscribe(OnEndEventChainHandler);
            CurrentEvent.OnShowMessage.Subscribe(OnShowMessageHandler);

            CurrentEvent.BeginEvent();
        }
    }

    private void OnChangeEventChainHandler(Unit _)
    {
        ChangeEventChain(_gameDataContainer.MissionEvents.Single(e => e.MissionEvent.Id == CurrentEvent.Next).MissionEvent);
    }
    private void OnEndEventChainHandler(bool result)
    {
        _onEndEventChainSubject.OnNext(result);
    }

    private void OnShowMessageHandler(string message)
    {
        _onShowMessage.OnNext(message);
    }

    private GameDataContainer _gameDataContainer;
    private Subject<bool> _onEndEventChainSubject = new Subject<bool>();
    private Subject<string> _onShowMessage = new Subject<string>();
}
