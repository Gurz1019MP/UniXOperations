using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameCoreManager : MonoBehaviour
{
    public GameDataContainer GameDataContainer { get; private set; }

    private PlayerInputter2 _playerInputter;
    private GameCameraController _gameCameraController;
    private int _currentCharacterIndex;

    void Start()
    {
        if (GameDataContainer == null)
        {
            Initialize(null);
        }
    }

    public void Initialize(MissionInformation missionInformation)
    {
        _playerInputter = new PlayerInputter2();

        if (PlayerPrefs.HasKey(_keyBindingKey))
        {
            try
            {
                _playerInputter.LoadBindingOverridesFromJson(PlayerPrefs.GetString(_keyBindingKey));
            }
            catch
            {
                Debug.Log("Settings Load Error (KeyBindings)");
            }
        }

        _playerInputter.Menu.Exit.performed += (_) => TransitionToMenu();

        _missionInformation = missionInformation;
        GameDataContainer = new MissionDataLoader(GameObject.Find("Stage")).Load(missionInformation);

        Material skybox = SkyboxLoader.GetSkybox(missionInformation?.Sky);
        RenderSettings.skybox = skybox;

        MissionEventManager missionEventManager = GetComponent<MissionEventManager>();
        missionEventManager.InitEvents(GameDataContainer.MissionEvents);

        _gameCameraController = GameObject.Find("GameCamera").GetComponent<GameCameraController>();
        var playerCharacter = GameDataContainer.Characters.SingleOrDefault(c => c.ID == 0);
        _currentCharacterIndex = GameDataContainer.Characters.ToList().IndexOf(playerCharacter);
        if (_gameCameraController != null && playerCharacter != null)
        {
            playerCharacter.InputterContainer.EnterPlayer(_playerInputter);
            _gameCameraController.ChangeCharacter(playerCharacter);
            _gameCameraController.SetPlayerInputter(_playerInputter);
        }

        _startTime = DateTime.Now;

        _playerInputter.Player.NextCharacter.performed += NextCharacter;
        _playerInputter.Player.PreCharacter.performed += PreCharacter;

        _playerInputter.Enable();
        //JsonContainer.Save();
    }

    public void TransitionToMenu()
    {
        _playerInputter.Dispose();
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Scene/Menu");
    }

    public void TransitionToResult(bool result)
    {
        _playerInputter.Dispose();
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

    private void NextCharacter(InputAction.CallbackContext obj)
    {
        if (GameDataContainer == null) return;
        if (_gameCameraController == null) return;

        CharacterState oldCharacter = GameDataContainer.Characters[_currentCharacterIndex];

        oldCharacter.InputterContainer.LeavePlayer();

        _currentCharacterIndex = _currentCharacterIndex == (GameDataContainer.Characters.Count - 1) ? 0 : _currentCharacterIndex + 1;

        CharacterState newCharacter = GameDataContainer.Characters[_currentCharacterIndex];

        newCharacter.InputterContainer.EnterPlayer(_playerInputter);
        _gameCameraController.ChangeCharacter(newCharacter);
        _gameCameraController.SetPlayerInputter(_playerInputter);
    }

    private void PreCharacter(InputAction.CallbackContext obj)
    {
        if (GameDataContainer == null) return;
        if (_gameCameraController == null) return;

        CharacterState oldCharacter = GameDataContainer.Characters[_currentCharacterIndex];

        oldCharacter.InputterContainer.LeavePlayer();

        _currentCharacterIndex = _currentCharacterIndex == 0 ? GameDataContainer.Characters.Count - 1 : _currentCharacterIndex - 1;

        CharacterState newCharacter = GameDataContainer.Characters[_currentCharacterIndex];

        newCharacter.InputterContainer.EnterPlayer(_playerInputter);
        _gameCameraController.ChangeCharacter(newCharacter);
        _gameCameraController.SetPlayerInputter(_playerInputter);
    }

    private GameCameraController _gameCamera;
    private MissionInformation _missionInformation;
    private ResultInformation _result;
    private DateTime _startTime;
    private static readonly MissionInformation DebugMissionInformation = new MissionInformation() { Name = "Debug" };
    private static readonly string _keyBindingKey = "KeyBinding";
}
