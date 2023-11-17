using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;

public class WaitForDestroyEvent : AbstractMissionEvent
{
    public WaitForDestroyEvent(GameDataContainer gameDataContainer) : base(gameDataContainer)
    {

    }

    public override void BeginEvent()
    {
        Debug.Log($"イベントチェーン変更 [{Id}]小物破壊待ち");
        Observable.FromCoroutine(WaitForDestroy).Subscribe();
    }

    private IEnumerator WaitForDestroy()
    {
        while (true)
        {
            if (GameDataContainer.Articles.All(c => c.Article.ID != Data))
            {
                break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        RaiseChangeEventChain();
    }
}
