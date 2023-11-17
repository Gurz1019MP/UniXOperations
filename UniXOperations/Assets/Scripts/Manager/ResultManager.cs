using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(InputTrigger))]
public class ResultManager : MonoBehaviour
{
    public Text MissionName;
    public GameObject MissionSuccessfull;
    public GameObject MissionFailure;
    public Text Time;
    public Text RoundsFired;
    public Text RoundsOnTarget;
    public Text AccuracyRate;
    public Text KillHeadShot;

    private void Start()
    {
        _inputTrigger = GetComponent<InputTrigger>();
    }

    void Update()
    {
        if (_inputTrigger.InputEnter("Fire") ||
            _inputTrigger.InputEnter("Exit"))
        {
            TransitionToMenu();
        }
    }

    public void Initialize(MissionInformation missionInformation, ResultInformation result)
    {
        if (MissionName != null)
        {
            MissionName.text = missionInformation.Name;
        }

        if (MissionSuccessfull != null && MissionFailure != null)
        {
            MissionSuccessfull.SetActive(result.IsSuccess);
            MissionFailure.SetActive(!result.IsSuccess);
        }

        if (Time != null)
        {
            Time.text = $"Time : {result.Time:%m' min '%s' sec'}";
        }

        if (result.CombatStatistics != null && 
            RoundsFired != null && 
            RoundsOnTarget != null && 
            AccuracyRate != null && 
            KillHeadShot != null)
        {
            RoundsFired.text = $"Rounds fired : {result.CombatStatistics.RoundsFired}";
            RoundsOnTarget.text = $"Rounds on target : {result.CombatStatistics.RoundsOnTarget}";
            AccuracyRate.text = $"Accuracy rate : {result.CombatStatistics.AccuracyRate:##0.#}%";
            KillHeadShot.text = $"Kill : {result.CombatStatistics.Kill} / HeadShot : {result.CombatStatistics.HeadShot}";
        }
    }

    private void TransitionToMenu()
    {
        SceneManager.LoadScene("Scene/Menu");
    }

    private InputTrigger _inputTrigger;
}
