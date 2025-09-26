using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(GameSceneLogic))]
public class MissionEventManager : MonoBehaviour
{
    public Animator UIAnimator;
    public UnityEngine.UI.Text EventMessage;

    public void InitEvents(MissionEvents missionEvents, Characters characters)
    {
        characters.OnPlayerDied.Subscribe(CharacterManager_OnPlayerDied).AddTo(gameObject);
        characters.OnAllEnemyEliminated.Subscribe(CharacterManager_OnAllEnemyEliminated).AddTo(gameObject);
        EventMessage.text = string.Empty;

        _eventChain1 = new MissionEventChain(missionEvents.GetMissionEvent(-100), missionEvents);
        _eventChain1.OnEndEventChain.Subscribe(OnEndEventChain).AddTo(gameObject);
        _eventChain1.OnShowMessage.Subscribe(OnShowMessage).AddTo(gameObject);

        _eventChain2 = new MissionEventChain(missionEvents.GetMissionEvent(-110), missionEvents);
        _eventChain2.OnEndEventChain.Subscribe(OnEndEventChain).AddTo(gameObject);
        _eventChain2.OnShowMessage.Subscribe(OnShowMessage).AddTo(gameObject);

        _eventChain3 = new MissionEventChain(missionEvents.GetMissionEvent(-120), missionEvents);
        _eventChain3.OnEndEventChain.Subscribe(OnEndEventChain).AddTo(gameObject);
        _eventChain3.OnShowMessage.Subscribe(OnShowMessage).AddTo(gameObject);
    }

    private void OnEndEventChain(bool result)
    {
        if (result)
        {
            ObjectiveComplete();
        }
        else
        {
            MissionFailure();
        }
    }

    private void OnShowMessage(string message)
    {
        // メッセージ表示
        EventMessage.text = message;

        if (_coroutine1 != null)
        {
            StopCoroutine(_coroutine1);
        }

        _coroutine1 = StartCoroutine(ClearEventMessage());
    }

    private void ObjectiveComplete()
    {
        _isEnding = true;
        UIAnimator.SetTrigger("ObjectiveComplate");
    }

    private void MissionFailure()
    {
        _isEnding = true;
        UIAnimator.SetTrigger("MissionFailure");
    }

    private void CharacterManager_OnPlayerDied(Unit _)
    {
        if (_isEnding) return;

        MissionFailure();
    }

    private void CharacterManager_OnAllEnemyEliminated(Unit _)
    {
        if (_isEnding) return;

        ObjectiveComplete();
    }

    private IEnumerator ClearEventMessage()
    {
        yield return new WaitForSeconds(5f);
        EventMessage.text = string.Empty;
        _coroutine1 = null;
    }

    private MissionEventChain _eventChain1;
    private MissionEventChain _eventChain2;
    private MissionEventChain _eventChain3;
    private Coroutine _coroutine1;
    private bool _isEnding;
}
