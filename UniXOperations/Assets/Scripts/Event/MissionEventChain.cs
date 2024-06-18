using System;
using System.Linq;
using UniRx;

public class MissionEventChain
{
    public AbstractMissionEvent CurrentEvent { get; private set; }

    public IObservable<bool> OnEndEventChain => _onEndEventChainSubject;

    public IObservable<string> OnShowMessage => _onShowMessage;

    public MissionEventChain(AbstractMissionEvent firstEvent, MissionEvents missionEvents)
    {
        _missionEvents = missionEvents;
        ChangeEventChain(firstEvent);
    }

    private void ChangeEventChain(AbstractMissionEvent nextEvent)
    {
        _onChangeEventChainSubscriber?.Dispose();
        _onChangeEventChainSubscriber = null;
        _onEndEventChainSubscriber?.Dispose();
        _onEndEventChainSubscriber = null;
        _onShowMessageSubscriber?.Dispose();
        _onShowMessageSubscriber = null;

        if (nextEvent != null)
        {
            CurrentEvent = nextEvent;

            _onChangeEventChainSubscriber = CurrentEvent.OnChangeEventChain.Subscribe(OnChangeEventChainHandler);
            _onEndEventChainSubscriber = CurrentEvent.OnEndEventChain.Subscribe(OnEndEventChainHandler);
            _onShowMessageSubscriber = CurrentEvent.OnShowMessage.Subscribe(OnShowMessageHandler);

            CurrentEvent.BeginEvent();
        }
    }

    private void OnChangeEventChainHandler(Unit _)
    {
        ChangeEventChain(_missionEvents.GetMissionEvent(CurrentEvent.Next));
    }
    private void OnEndEventChainHandler(bool result)
    {
        _onEndEventChainSubject.OnNext(result);
        _onEndEventChainSubject.OnCompleted();
    }

    private void OnShowMessageHandler(string message)
    {
        _onShowMessage.OnNext(message);
    }

    private Subject<bool> _onEndEventChainSubject = new Subject<bool>();
    private Subject<string> _onShowMessage = new Subject<string>();
    private MissionEvents _missionEvents;
    private IDisposable _onChangeEventChainSubscriber;
    private IDisposable _onEndEventChainSubscriber;
    private IDisposable _onShowMessageSubscriber;
}
