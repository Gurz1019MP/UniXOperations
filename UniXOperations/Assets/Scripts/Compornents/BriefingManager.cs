using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BriefingManager : MonoBehaviour
{
    public Text missionName;
    public Text Content;
    public Image singlePic;
    public Image doublePic1;
    public Image doublePic2;

    private MissionInformation _selectedMissionInformation;
    private InputSystem _playerInputter;

    private void Start()
    {
        _playerInputter = new InputSystem();
        _playerInputter.Menu.Enter.performed += (_) => TransitionToGame();
        _playerInputter.Menu.Exit.performed += (_) => TransitionToMenu();
        _playerInputter.Enable();
    }

    public void Initialize(MissionInformation missionInformation)
    {
        _selectedMissionInformation = missionInformation.ReadMif();

        Texture2D pic1 = _selectedMissionInformation.GetPic1();
        Texture2D pic2 = _selectedMissionInformation.GetPic2();

        if (singlePic != null && doublePic1 != null && doublePic2 != null)
        {
            if (pic1 != null)
            {
                singlePic.sprite = Sprite.Create(pic1, new Rect(0, 0, pic1.width, pic1.height), Vector2.zero);
            }

            if (pic2 != null)
            {
                singlePic.gameObject.SetActive(false);
                doublePic1.gameObject.SetActive(true);
                doublePic2.gameObject.SetActive(true);

                doublePic1.sprite = singlePic.sprite;
                doublePic2.sprite = Sprite.Create(pic2, new Rect(0, 0, pic2.width, pic2.height), Vector2.zero);
            }
        }

        if (Content != null)
        {
            Content.text = _selectedMissionInformation.Content;
        }

        if (missionName != null)
        {
            missionName.text = _selectedMissionInformation.Name;
        }
    }

    private void TransitionToGame()
    {
        _playerInputter.Dispose();
        SceneManager.sceneLoaded += GameLoaded;
        SceneManager.LoadScene("Scene/Game");

    }

    private void GameLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SceneManager.sceneLoaded -= GameLoaded;

        GameCoreManager gameCoreManager = scene.GetRootGameObjects().Single(g => g.name.Equals("GameLogic")).GetComponent<GameCoreManager>();
        gameCoreManager.Initialize(_selectedMissionInformation);
    }

    private void TransitionToMenu()
    {
        _playerInputter.Dispose();
        SceneManager.LoadScene("Scene/Menu");
    }
}
