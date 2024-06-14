using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using XOPS;

public class GameCoreManager : MonoBehaviour
{
    public void Start()
    {
        if (_gameCore == null)
        {
            Initialize(MissionInformation.Fallback);
        }
    }

    public void Initialize(MissionInformation missionInformation)
    {
        _gameCore = new GameCore(GameObject.Find("Stage"), LoadBlock, missionInformation, GetComponent<MissionEventManager>());

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

        _characters = new Characters(_gameCore._characters.ToArray());
        var playerCharacter = _characters.Player;
        _currentCharacterIndex = _characters.GetIndex(playerCharacter);
        playerCharacter.InputterContainer.EnterPlayer(_playerInputter);

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

    private void LoadBlock(BlockData blockData)
    {
        GameObject stage = GameObject.Find("Stage") ?? throw new InvalidOperationException("ヒエラルキーにゲームオブジェクト\"Stage\"が見つかりません。");

        stage.GetComponent<MeshFilter>().mesh = blockData.Mesh;
        stage.GetComponent<MeshCollider>().sharedMesh = blockData.Mesh;
        stage.GetComponent<MeshRenderer>().materials = blockData.Materials;
    }

    private void Update()
    {
        if (_playerInputter.Player.MoveUp.IsPressed())
        {
            Debug.Log("MoveUp");
            CharacterContainer character = _characters.GetCharacterAtIndex(_currentCharacterIndex);
            character.transform.position = new Vector3(character.transform.position.x, character.transform.position.y + 10 * Time.deltaTime, character.transform.position.z);
            character.ResetMoveDeltaY();
        }
    }

    public void OnChangeCharacter(CharacterContainer character, InputSystem inputSystem)
    {
        _gameCameraController.ChangeCharacter(character);
        _gameCameraController.SetPlayerInputter(inputSystem);
    }

    public void TransitionToResult(bool result)
    {
        _playerInputter.Dispose();
        _result = new ResultInformation(result, DateTime.Now - _startTime, _characters.PlayerCombatStatistics);

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
        CharacterContainer oldCharacter = _characters.GetCharacterAtIndex(_currentCharacterIndex);

        oldCharacter.InputterContainer.LeavePlayer();

        _currentCharacterIndex = _characters.NextCharacter(_currentCharacterIndex);

        CharacterContainer newCharacter = _characters.GetCharacterAtIndex(_currentCharacterIndex);

        newCharacter.InputterContainer.EnterPlayer(_playerInputter);
        OnChangeCharacter(newCharacter, _playerInputter);
    }

    private void PreCharacter(InputAction.CallbackContext obj)
    {
        CharacterContainer oldCharacter = _characters.GetCharacterAtIndex(_currentCharacterIndex);

        oldCharacter.InputterContainer.LeavePlayer();

        _currentCharacterIndex = _characters.PreCharacter(_currentCharacterIndex);

        CharacterContainer newCharacter = _characters.GetCharacterAtIndex(_currentCharacterIndex);

        newCharacter.InputterContainer.EnterPlayer(_playerInputter);
        OnChangeCharacter(newCharacter, _playerInputter);
    }

    private void NextWeapon(InputAction.CallbackContext obj)
    {
        CharacterContainer character = _characters.GetCharacterAtIndex(_currentCharacterIndex);
        character.CurrentWeaponState = character.CurrentWeaponState.NextWeapon();
        character.ChangeWeapon(character.CurrentWeaponState);
    }

    private void PreWeapon(InputAction.CallbackContext obj)
    {
        CharacterContainer character = _characters.GetCharacterAtIndex(_currentCharacterIndex);
        character.CurrentWeaponState = character.CurrentWeaponState.PreWeapon();
        character.ChangeWeapon(character.CurrentWeaponState);
    }

    private void AddAmmo(InputAction.CallbackContext obj)
    {
        CharacterContainer character = _characters.GetCharacterAtIndex(_currentCharacterIndex);
        character.CurrentWeaponState = character.CurrentWeaponState.AddAmmo();
    }

    private void ResultLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SceneManager.sceneLoaded -= ResultLoaded;

        ResultManager resultManager = scene.GetRootGameObjects().Single(g => g.name.Equals("SceneManager")).GetComponent<ResultManager>();
        resultManager.Initialize(_missionInformation, _result);
    }

    private GameCore _gameCore;
    private Characters _characters;
    private GameCameraController _gameCameraController;
    private InputSystem _playerInputter;
    private MissionInformation _missionInformation;
    private DateTime _startTime;
    private ResultInformation _result;
    private int _currentCharacterIndex;
}
