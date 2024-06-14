using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;

public class WaitForDestroyEvent : AbstractMissionEvent
{
    public WaitForDestroyEvent(Articles articles)
    {
        _articles = articles;
    }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]小物破壊待ち");
        Observable.FromCoroutine(WaitForDestroy).Subscribe();
    }

    private IEnumerator WaitForDestroy()
    {
        while (true)
        {
            if (_articles.NoOneId(Data))
            {
                break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        RaiseChangeEventChain();
    }

    private readonly Articles _articles;
}
