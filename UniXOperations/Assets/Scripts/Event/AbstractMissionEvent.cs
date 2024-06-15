using System;
using System.Collections;
using UniRx;
using UnityEngine;

public abstract class AbstractMissionEvent
{
    public AbstractMissionEvent(PointData pointData)
    {
        _pointData = pointData;
    }

    public short Id => _pointData.Data4;

    public short Data => _pointData.Data2;

    public short Next => _pointData.Data3;

    public Vector3 Position => _pointData.Position;

    public IObservable<Unit> OnChangeEventChain => _onChangeEventChainSubject;

    public IObservable<bool> OnEndEventChain => _onEndEventChainSubject;

    public IObservable<string> OnShowMessage => _onShowMessageSubject;

    public abstract void BeginEvent(MissionEvents missionEvents);

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

    protected IEnumerator WaitForPerson(Character character, Vector3 eventPosition, bool withCase)
    {
        while (true)
        {
            if ((character.Position - eventPosition).magnitude < 1f)
            {
                if (withCase)
                {
                    if (character.State.Weapon1.Spec.IsCase || character.State.Weapon2.Spec.IsCase)
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

    private PointData _pointData;
    private Subject<Unit> _onChangeEventChainSubject = new Subject<Unit>();
    private Subject<bool> _onEndEventChainSubject = new Subject<bool>();
    private Subject<string> _onShowMessageSubject = new Subject<string>();
}
