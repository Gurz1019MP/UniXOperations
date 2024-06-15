using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;

public class WaitForDeathEvent : AbstractMissionEvent
{
    public WaitForDeathEvent(PointData pointData, Characters characters) : base(pointData)
    {
        _characters = characters;
    }

    public override void BeginEvent(MissionEvents missionEvents)
    {
        Debug.Log($"イベントチェーン変更 [{Id}]死亡待ち");
        Observable.FromCoroutine(WaitForDeath).Subscribe();
    }

    private IEnumerator WaitForDeath()
    {
        while (true)
        {
            if (_characters.NoOneId(Data))
            {
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }

        RaiseChangeEventChain();
    }

    private readonly Characters _characters;
}
