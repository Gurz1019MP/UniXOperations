using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InputTrigger))]
public class GameCoreManager : MonoBehaviour
{
    public GameDataContainer GameDataContainer { get; private set; }

    void Start()
    {
        if (GameDataContainer == null)
        {
            Initialize(null);
        }

        //_inputTrigger = GetComponent<InputTrigger>();
        //_gameCamera = GameObject.Find("GameCamera").GetComponent<GameCameraController>();

        //if (GameDataContainer == null)
        //{
        //    GameDataContainer = new MissionDataLoader(GameObject.Find("Stage")).Load();

        //    MissionEventManager missionEventManager = GetComponent<MissionEventManager>();
        //    missionEventManager.InitEvents(GameDataContainer.MissionEvents);

        //    var playerCharacter = GameDataContainer.Characters.SingleOrDefault(c => c.ID == 0);
        //    if (_gameCamera != null && playerCharacter != null)
        //    {
        //        _gameCamera.ChangeCharacter(playerCharacter.gameObject);
        //    }
        //}
    }

    void Update()
    {
        if (_inputTrigger != null)
        {
            if (_inputTrigger.InputEnter("Exit"))
            {
                Cursor.lockState = CursorLockMode.None;
                SceneManager.LoadScene("Scene/Menu");
            }
        }
    }

    public void Initialize(MissionInformation missionInformation)
    {
        _inputTrigger = GetComponent<InputTrigger>();

        _missionInformation = missionInformation;
        GameDataContainer = new MissionDataLoader(GameObject.Find("Stage")).Load(missionInformation);

        Material skybox = SkyboxLoader.GetSkybox(missionInformation?.Sky);
        RenderSettings.skybox = skybox;

        MissionEventManager missionEventManager = GetComponent<MissionEventManager>();
        missionEventManager.InitEvents(GameDataContainer.MissionEvents);

        GameCameraController gameCameraController = GameObject.Find("GameCamera").GetComponent<GameCameraController>();
        var playerCharacter = GameDataContainer.Characters.SingleOrDefault(c => c.ID == 0);
        if (gameCameraController != null && playerCharacter != null)
        {
            playerCharacter.InputterContainer.EnterPlayer();
            gameCameraController.ChangeCharacter(playerCharacter);
        }

        _startTime = DateTime.Now;

        //JsonContainer.Save();
    }

    public void TransitionToResult(bool result)
    {
        _result = new ResultInformation()
        {
            IsSuccess = result,
            Time = DateTime.Now - _startTime,
            CombatStatistics = GameDataContainer.PlayerCombatStatistics,
        };

        Cursor.lockState = CursorLockMode.None;
        StopAllCoroutines();
        SceneManager.sceneLoaded += ResultLoaded;

        SceneManager.LoadScene("Scene/Result");
    }

    private void ResultLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SceneManager.sceneLoaded -= ResultLoaded;

        ResultManager resultManager = scene.GetRootGameObjects().Single(g => g.name.Equals("SceneManager")).GetComponent<ResultManager>();
        resultManager.Initialize(_missionInformation == null ? DebugMissionInformation : _missionInformation, _result);
    }

    private InputTrigger _inputTrigger;
    private GameCameraController _gameCamera;
    private MissionInformation _missionInformation;
    private ResultInformation _result;
    private DateTime _startTime;
    private static readonly MissionInformation DebugMissionInformation = new MissionInformation() { Name = "Debug" };
}
