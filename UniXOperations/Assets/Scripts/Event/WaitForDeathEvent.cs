using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;

public class WaitForDeathEvent : AbstractMissionEvent
{
    public WaitForDeathEvent(GameDataContainer gameDataContainer) : base(gameDataContainer)
    {

    }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]死亡待ち");
        Observable.FromCoroutine(WaitForDeath).Subscribe();
    }

    private IEnumerator WaitForDeath()
    {
        while (true)
        {
            if (GameDataContainer.Characters.All(c => c.ID != Data))
            {
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }

        RaiseChangeEventChain();
    }
}
