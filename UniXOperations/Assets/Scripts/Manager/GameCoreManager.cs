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

        _playerInputter.Menu.Exit.performed += (_) => TransitionToMenu();

        _missionInformation = missionInformation;
        GameDataContainer = new MissionDataLoader(GameObject.Find("Stage")).Load(missionInformation);

        Material skybox = GetSkybox(missionInformation?.Sky);
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
        _playerInputter.Player.NextWeapon.performed += NextWeapon;
        _playerInputter.Player.PreWeapon.performed += PreWeapon;
        _playerInputter.Player.AddAmmo.performed += AddAmmo;

        _playerInputter.Enable();
        //JsonContainer.Save();
    }

    private void Update()
    {
        if (_playerInputter.Player.MoveUp.IsPressed())
        {
            Debug.Log("MoveUp");
            Character character = GameDataContainer.Characters[_currentCharacterIndex];
            character.transform.position = new Vector3(character.transform.position.x, character.transform.position.y + 10 * Time.deltaTime, character.transform.position.z);
            character.ResetMoveDeltaY();
        }
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

    public Material GetSkybox(string index)
    {
        if (!string.IsNullOrEmpty(index) && ConstantsManager.SkyboxMapper.ContainsKey(index))
        {
            string assetName = ConstantsManager.SkyboxMapper[index];
            if (assetName == null)
            {
                return null;
            }
            else
            {
                return AssetLoader.LoadAsset<Material>(assetName);
            }
        }
        else
        {
            return AssetLoader.LoadAsset<Material>(ConstantsManager.SkyboxMapper["1"]);
        }
    }

    private void NextCharacter(InputAction.CallbackContext obj)
    {
        if (GameDataContainer == null) return;
        if (_gameCameraController == null) return;

        Character oldCharacter = GameDataContainer.Characters[_currentCharacterIndex];

        oldCharacter.InputterContainer.LeavePlayer();

        _currentCharacterIndex = _currentCharacterIndex == (GameDataContainer.Characters.Count - 1) ? 0 : _currentCharacterIndex + 1;

        Character newCharacter = GameDataContainer.Characters[_currentCharacterIndex];

        newCharacter.InputterContainer.EnterPlayer(_playerInputter);
        _gameCameraController.ChangeCharacter(newCharacter);
        _gameCameraController.SetPlayerInputter(_playerInputter);
    }

    private void PreCharacter(InputAction.CallbackContext obj)
    {
        if (GameDataContainer == null) return;
        if (_gameCameraController == null) return;

        Character oldCharacter = GameDataContainer.Characters[_currentCharacterIndex];

        oldCharacter.InputterContainer.LeavePlayer();

        _currentCharacterIndex = _currentCharacterIndex == 0 ? GameDataContainer.Characters.Count - 1 : _currentCharacterIndex - 1;

        Character newCharacter = GameDataContainer.Characters[_currentCharacterIndex];

        newCharacter.InputterContainer.EnterPlayer(_playerInputter);
        _gameCameraController.ChangeCharacter(newCharacter);
        _gameCameraController.SetPlayerInputter(_playerInputter);
    }

    private void NextWeapon(InputAction.CallbackContext obj)
    {
        Character character = GameDataContainer.Characters[_currentCharacterIndex];
        character.CurrentWeaponState.Kind = (short)(character.CurrentWeaponState.Kind == (JsonContainer.Instance.WeaponSpecArray.Length - 1) ? 0 : character.CurrentWeaponState.Kind + 1);
        character.ChangeWeapon(character.CurrentWeaponState);
    }

    private void PreWeapon(InputAction.CallbackContext obj)
    {
        Character character = GameDataContainer.Characters[_currentCharacterIndex];
        character.CurrentWeaponState.Kind = (short)(character.CurrentWeaponState.Kind == 0 ? JsonContainer.Instance.WeaponSpecArray.Length - 1 : character.CurrentWeaponState.Kind - 1);
        character.ChangeWeapon(character.CurrentWeaponState);
    }

    private void AddAmmo(InputAction.CallbackContext obj)
    {
        Character character = GameDataContainer.Characters[_currentCharacterIndex];
        character.CurrentWeaponState.Ammo += JsonContainer.Instance.WeaponSpecArray.First(w => w.Id == character.CurrentWeaponState.Kind).MagazineSize;
    }

    private MissionInformation _missionInformation;
    private ResultInformation _result;
    private DateTime _startTime;
    private static readonly MissionInformation DebugMissionInformation = new MissionInformation() { Name = "Debug" };
}
