using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameCoreManager : GameCoreContainer
{
    public void Start()
    {
        if (_gameCore == null)
        {
            Initialize(MissionInformation.Fallback);
        }
    }

    public override void Initialize(MissionInformation missionInformation)
    {
        base.Initialize(missionInformation);

        _playerInputter = new InputSystem();

        if (PlayerPrefs.HasKey(ConstantsManager.KeyBindingKey))
        {
            try
            {
                _playerInputter.LoadBindingOverridesFromJson(PlayerPrefs.GetString(ConstantsManager.KeyBindingKey));
            }
            catch
            {
                Debug.Log("Settings Load Error (KeyBindings)");
            }
        }

        var playerCharacter = _gameCore._characters.Player;
        _currentCharacterIndex = _gameCore._characters.GetIndex(playerCharacter);
        playerCharacter.SetInputterState(playerCharacter.InputterState.EnterPlayer(new PlayerInputter(gameObject, _playerInputter)));

        _playerInputter.Menu.Exit.performed += (_) => TransitionToMenu();

        _startTime = DateTime.Now;

        _playerInputter.Player.NextCharacter.performed += NextCharacter;
        _playerInputter.Player.PreCharacter.performed += PreCharacter;
        _playerInputter.Player.NextWeapon.performed += NextWeapon;
        _playerInputter.Player.PreWeapon.performed += PreWeapon;
        _playerInputter.Player.AddAmmo.performed += AddAmmo;

        _playerInputter.Enable();

        _missionInformation = missionInformation;
        _gameCameraController = GameObject.Find("GameCamera").GetComponent<GameCameraController>();
        MissionEventManager missionEventManager = GetComponent<MissionEventManager>();
        OnChangeCharacter(playerCharacter, _playerInputter);

        RenderSettings.skybox = missionInformation.GetSkybox();
    }

    private void Update()
    {
        if (_playerInputter.Player.MoveUp.IsPressed())
        {
            Debug.Log("MoveUp");
            Character character = _gameCore._characters.GetCharacterAtIndex(_currentCharacterIndex);
            CharacterContainer characterContainer = FindCharacterContainer(character);
            characterContainer.transform.position = new Vector3(characterContainer.transform.position.x, characterContainer.transform.position.y + 10 * Time.deltaTime, characterContainer.transform.position.z);
            character.SetState(character.State.SetMoveDelta(Vector3.zero));
        }
    }

    public void OnChangeCharacter(Character character, InputSystem inputSystem)
    {
        _gameCameraController.ChangeCharacter(FindCharacterContainer(character));
        _gameCameraController.SetPlayerInputter(inputSystem);
    }

    public void TransitionToResult(bool result)
    {
        _playerInputter.Dispose();
        _result = new ResultInformation(result, DateTime.Now - _startTime, _gameCore._characters.PlayerCombatStatistics);

        Cursor.lockState = CursorLockMode.None;
        SceneManager.sceneLoaded += ResultLoaded;

        SceneManager.LoadScene("Scene/Result");

        StopAllCoroutines();
    }

    private void TransitionToResultAction(ResultInformation resultInformation)
    {
        SceneManager.sceneLoaded += ResultLoaded;
        _result = resultInformation;
    }

    public void TransitionToMenu()
    {
        _playerInputter.Dispose();
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Scene/Menu");
    }

    private void NextCharacter(InputAction.CallbackContext obj)
    {
        Character oldCharacter = _gameCore._characters.GetCharacterAtIndex(_currentCharacterIndex);

        oldCharacter.SetInputterState(oldCharacter.InputterState.LeavePlayer());

        _currentCharacterIndex = _gameCore._characters.NextCharacter(_currentCharacterIndex);

        Character newCharacter = _gameCore._characters.GetCharacterAtIndex(_currentCharacterIndex);

        newCharacter.SetInputterState(newCharacter.InputterState.EnterPlayer(new PlayerInputter(gameObject, _playerInputter)));
        OnChangeCharacter(newCharacter, _playerInputter);
    }

    private void PreCharacter(InputAction.CallbackContext obj)
    {
        Character oldCharacter = _gameCore._characters.GetCharacterAtIndex(_currentCharacterIndex);

        oldCharacter.SetInputterState(oldCharacter.InputterState.LeavePlayer());

        _currentCharacterIndex = _gameCore._characters.PreCharacter(_currentCharacterIndex);

        Character newCharacter = _gameCore._characters.GetCharacterAtIndex(_currentCharacterIndex);

        newCharacter.SetInputterState(newCharacter.InputterState.EnterPlayer(new PlayerInputter(gameObject, _playerInputter)));
        OnChangeCharacter(newCharacter, _playerInputter);
    }

    private void NextWeapon(InputAction.CallbackContext obj)
    {
        Character character = _gameCore._characters.GetCharacterAtIndex(_currentCharacterIndex);
        character.SetState(character.State.SetCurrentWeaponState(character.State.CurrentWeapon.NextWeapon()));
    }

    private void PreWeapon(InputAction.CallbackContext obj)
    {
        Character character = _gameCore._characters.GetCharacterAtIndex(_currentCharacterIndex);
        character.SetState(character.State.SetCurrentWeaponState(character.State.CurrentWeapon.PreWeapon()));
    }

    private void AddAmmo(InputAction.CallbackContext obj)
    {
        Character character = _gameCore._characters.GetCharacterAtIndex(_currentCharacterIndex);
        character.SetState(character.State.SetCurrentWeaponState(character.State.CurrentWeapon.AddAmmo()));
    }

    private void ResultLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SceneManager.sceneLoaded -= ResultLoaded;

        ResultManager resultManager = scene.GetRootGameObjects().Single(g => g.name.Equals("SceneManager")).GetComponent<ResultManager>();
        resultManager.Initialize(_missionInformation, _result);
    }

    private GameCameraController _gameCameraController;
    private InputSystem _playerInputter;
    private MissionInformation _missionInformation;
    private DateTime _startTime;
    private ResultInformation _result;
    private int _currentCharacterIndex;
}
