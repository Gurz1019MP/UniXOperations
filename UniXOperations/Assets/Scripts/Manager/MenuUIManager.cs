using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UniRx;

[RequireComponent(typeof(InputTrigger))]
public class MenuUIManager : MonoBehaviour
{
    public GameObject DefaultMissionContent;
    public GameObject AddonMissionContent;
    public GameObject StageButton;
    public RectTransform CursorV;
    public RectTransform CursorH;
    public RawImage DemoImage;

    private MissionInformation _selectedMissionInformation;

    private void Start()
    {
        _inputTrigger = GetComponent<InputTrigger>();

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
    }

    private void Update()
    {
        if (_inputTrigger != null)
        {
            if (_inputTrigger.InputEnter("Exit"))
            {
#if UNITY_EDITOR
                Debug.Log("Exit");
                UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
            UnityEngine.Application.Quit();
#endif
            }
        }

        if (CursorV != null && CursorH != null)
        {
            CursorV.position = new Vector3(Input.mousePosition.x, CursorV.position.y, 0);
            CursorH.position = new Vector3(CursorH.position.x, Input.mousePosition.y, 0);
        }
    }

    private void MissionSelected(MissionInformation missionInformation)
    {
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

    public void TransitionToGame()
    {
        SceneManager.LoadScene("Scene/Briefing");
    }

    private InputTrigger _inputTrigger;
}
