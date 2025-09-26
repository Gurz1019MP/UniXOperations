using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public static class PushOneTrigger
{
    public static IEnumerator PushOne(UnityAction<bool> boolSetAction)
    {
        boolSetAction(true);
        yield return new WaitForSeconds(_pushTime);
        boolSetAction(false);
    }

    private static readonly float _pushTime = 0.2f;
}
