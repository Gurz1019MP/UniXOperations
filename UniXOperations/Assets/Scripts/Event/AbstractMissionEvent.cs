using System;
using System.Collections;
using UniRx;
using UnityEngine;

public abstract class AbstractMissionEvent
{
    public short Id { get; set; }

    public short Data { get; set; }

    public short Next { get; set; }

    public IObservable<Unit> OnChangeEventChain => _onChangeEventChainSubject;

    public IObservable<bool> OnEndEventChain => _onEndEventChainSubject;

    public IObservable<string> OnShowMessage => _onShowMessageSubject;

    protected GameDataContainer GameDataContainer { get; private set; }

    public AbstractMissionEvent(GameDataContainer gameDataContainer)
    {
        GameDataContainer = gameDataContainer;
    }

    public abstract void BeginEvent();

    protected void RaiseChangeEventChain()
    {
        if (Id != Next)
        {
            _onChangeEventChainSubject.OnNext(Unit.Default);
            _onChangeEventChainSubject.OnCompleted();
        }
    }

    protected void RaiseOnEndEventChain(bool result)
    {
        _onEndEventChainSubject.OnNext(result);
        _onEndEventChainSubject.OnCompleted();
    }

    protected void RaiseOnShowMessage(string message)
    {
        _onShowMessageSubject.OnNext(message);
        _onShowMessageSubject.OnCompleted();
    }

    protected IEnumerator WaitForPerson(Character character, MissionEventContainer missionEventContainer, bool withCase)
    {
        while (true)
        {
            if ((character.transform.position - missionEventContainer.transform.position).magnitude < 1f)
            {
                if (withCase)
                {
                    if (character.Weapon1.Kind == 15 || character.Weapon2.Kind == 15)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        RaiseChangeEventChain();
    }

    private Subject<Unit> _onChangeEventChainSubject = new Subject<Unit>();
    private Subject<bool> _onEndEventChainSubject = new Subject<bool>();
    private Subject<string> _onShowMessageSubject = new Subject<string>();
}
