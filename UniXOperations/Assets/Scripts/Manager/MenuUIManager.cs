using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UniRx;

public class MenuUIManager : MonoBehaviour
{
    public GameObject DefaultMissionContent;
    public GameObject AddonMissionContent;
    public GameObject StageButton;
    public RectTransform CursorV;
    public RectTransform CursorH;
    public RawImage DemoImage;

    private MissionInformation _selectedMissionInformation;
    private InputSystem _playerInputter;

    private void Start()
    {
        _playerInputter = new InputSystem();

        if (StageButton != null)
        {
            if (MissionDataLoader.ExistsXOps())
            {
                //if (StageButton != null) StageButton.onClick.AddListener(TransitionToGame);
                
                if (DefaultMissionContent != null)
                {
                    foreach (var mi in DefaultMissionInformation.GetMissionInformation())
                    {
                        var instance = Instantiate(StageButton, DefaultMissionContent.transform);
                        var presenter = instance.GetComponent<MenuMissionNodePresenter>();
                        presenter.SetMissionInformation(mi);
                        presenter.Selected.Subscribe(MissionSelected).AddTo(gameObject);
                    }
                }
            }
            else
            {
                DemoImage.enabled = false;
            }

            foreach (var mi in AddonMissionInformationLoader.GetMissionInformation())
            {
                var instance = Instantiate(StageButton, AddonMissionContent.transform);
                var presenter = instance.GetComponent<MenuMissionNodePresenter>();
                presenter.SetMissionInformation(mi);
                presenter.Selected.Subscribe(MissionSelected).AddTo(gameObject);
            }
        }

        _playerInputter.Player.Exit.performed += Exit_performed;
        _playerInputter.Enable();
    }

    private void Exit_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
#if UNITY_EDITOR
        Debug.Log("Exit");
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
            UnityEngine.Application.Quit();
#endif
    }

    private void Update()
    {
        if (CursorV != null && CursorH != null)
        {
            CursorV.position = new Vector3(_playerInputter.Menu.MouseX.ReadValue<float>(), CursorV.position.y, 0);
            CursorH.position = new Vector3(CursorH.position.x, _playerInputter.Menu.MouseY.ReadValue<float>(), 0);
        }
    }

    private void MissionSelected(MissionInformation missionInformation)
    {
        _playerInputter.Dispose();
        _selectedMissionInformation = missionInformation;

        SceneManager.sceneLoaded += BriefingLoaded;
        SceneManager.LoadScene("Scene/Briefing");
    }

    private void BriefingLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SceneManager.sceneLoaded -= BriefingLoaded;

        BriefingManager briefingManager = scene.GetRootGameObjects().Single(g => g.name.Equals("SceneManager")).GetComponent<BriefingManager>();
        briefingManager.Initialize(_selectedMissionInformation);
    }

    public void KeyConfig()
    {
        _playerInputter.Disable();
        SceneManager.LoadScene("Scene/Settings");
    }
}
