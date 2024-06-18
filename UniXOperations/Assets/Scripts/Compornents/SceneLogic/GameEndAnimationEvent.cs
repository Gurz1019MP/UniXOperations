using UnityEngine;

public class GameEndAnimationEvent : MonoBehaviour
{
    public void ObjectiveComplate()
    {
        GameObject.Find("GameLogic").GetComponent<GameSceneLogic>().TransitionToResult(true);
    }

    public void MissionFailure()
    {
        GameObject.Find("GameLogic").GetComponent<GameSceneLogic>().TransitionToResult(false);
    }
}
