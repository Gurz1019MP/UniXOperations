using System.Collections;
using UniRx;
using UnityEngine;

public class WaitForTimerEvent : AbstractMissionEvent
{
    public WaitForTimerEvent(PointData pointData) : base(pointData) { }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]時間待ち");
        Observable.FromCoroutine(WaitEventTimer).Subscribe();
    }

    private IEnumerator WaitEventTimer()
    {
        yield return new WaitForSeconds(Data);
        RaiseChangeEventChain();
    }
}
