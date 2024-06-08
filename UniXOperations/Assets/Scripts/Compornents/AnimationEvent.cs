using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationEvent : MonoBehaviour
{
    public void ObjectiveComplate()
    {
        GameObject.Find("GameLogic").GetComponent<GameCoreManager>().TransitionToResult(true);
    }

    public void MissionFailure()
    {
        GameObject.Find("GameLogic").GetComponent<GameCoreManager>().TransitionToResult(false);
    }
}
