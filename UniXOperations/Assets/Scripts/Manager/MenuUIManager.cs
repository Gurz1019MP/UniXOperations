using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UniRx;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MenuUIManager : MonoBehaviour
{
    public GameObject DefaultMissionContent;
    public GameObject AddonMissionContent;
    public GameObject StageButton;
    public RectTransform CursorV;
    public RectTransform CursorH;
    public RawImage DemoImage;
    public RectTransform Cursor;
    public UnityEventContainer MenuSwitchButton;
    public UnityEventContainer KeyConfigButton;
    public ScrollRect scrollRect1;
    public ScrollRect scrollRect2;

    private MissionInformation _selectedMissionInformation;
    private InputSystem _playerInputter;
    private List<Collider2D> _colliders1 = new List<Collider2D>();
    private List<Collider2D> _colliders2 = new List<Collider2D>();

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
                        _colliders1.Add(instance.GetComponent<Collider2D>());
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
                _colliders2.Add(instance.GetComponent<Collider2D>());
            }
        }

        Observable.Sample(Observable.EveryUpdate(), System.TimeSpan.FromSeconds(0.2f)).Subscribe((_) => ScrollRectColliderUpdate(scrollRect1, _colliders1.ToArray())).AddTo(gameObject);
        Observable.Sample(Observable.EveryUpdate(), System.TimeSpan.FromSeconds(0.2f)).Subscribe((_) => ScrollRectColliderUpdate(scrollRect2, _colliders2.ToArray())).AddTo(gameObject);

        _playerInputter.Player.Exit.performed += Exit_performed;
        _playerInputter.Menu.Enter.performed += Enter_performed;
        Observable.EveryUpdate().Subscribe(_ => StickScroll()).AddTo(gameObject);

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

    private void Enter_performed(InputAction.CallbackContext obj)
    {
        RaycastHit2D hit = Physics2D.Raycast(Cursor.position, Vector3.forward, float.PositiveInfinity, LayerMask.GetMask("UI"));
        Debug.Log(hit.collider?.gameObject.name);
        hit.collider?.gameObject.GetComponent<UnityEventContainer>()?.OnClick?.Invoke();
    }

    private void Update()
    {
        Cursor.position = new Vector3(Cursor.position.x + _playerInputter.Menu.MouseX.ReadValue<float>(),
                                      Cursor.position.y + _playerInputter.Menu.MouseY.ReadValue<float>(),
                                      0);

        CursorV.position = new Vector3(Cursor.position.x, CursorV.position.y, 0);
        CursorH.position = new Vector3(CursorH.position.x, Cursor.position.y, 0);
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

    private void ScrollRectColliderUpdate(ScrollRect scrollRect, Collider2D[] colliders)
    {
        RectTransform viewportRect = scrollRect.viewport;

        DisplayWorldCorners(viewportRect);

        foreach (Collider2D collider in colliders)
        {
            bool isVisible = IsColliderVisible(viewportRect, collider);
            collider.enabled = isVisible;
        }
    }

    private bool IsColliderVisible(RectTransform viewport, Collider2D collider)
    {
        // コライダーの中心座標をワールド座標に変換
        Vector3 colliderCenter = collider.bounds.center;

        DrawPoint(colliderCenter, Color.yellow, 5f, 10);

        // ビューポート内にあるかどうかを判定
        //bool isVisible = viewport.rect.Contains(colliderCenter, true);
        bool isVisible = RectTransformUtility.RectangleContainsScreenPoint(viewport, colliderCenter);

        return isVisible;
    }

    public void KeyConfig()
    {
        _playerInputter.Disable();
        SceneManager.LoadScene("Scene/Settings");
    }

    private void StickScroll()
    {
        ScrollRect scrollRect = scrollRect1.enabled ? scrollRect1 : scrollRect2;
        scrollRect.verticalNormalizedPosition += _playerInputter.Menu.Scroll.ReadValue<float>() * Time.deltaTime;
    }

    void DisplayWorldCorners(RectTransform rt)
    {
        Vector3[] v = new Vector3[4];
        rt.GetWorldCorners(v);

        Debug.Log("World Corners");
        for (var i = 0; i < 4; i++)
        {
            Debug.Log("World Corner " + i + " : " + v[i]);
        }
    }

    private void DrawDebugLine(Rect rect, Color color, float duration)
    {
        Debug.DrawLine(new Vector3(rect.xMin, rect.yMin, 0), new Vector3(rect.xMax, rect.yMin), color, duration);
        Debug.DrawLine(new Vector3(rect.xMax, rect.yMin, 0), new Vector3(rect.xMax, rect.yMax), color, duration);
        Debug.DrawLine(new Vector3(rect.xMax, rect.yMax, 0), new Vector3(rect.xMin, rect.yMax), color, duration);
        Debug.DrawLine(new Vector3(rect.xMin, rect.yMax, 0), new Vector3(rect.xMin, rect.yMin), color, duration);
    }

    private void DrawPoint(Vector3 point, Color color, float duration, float size)
    {
        Debug.DrawLine(new Vector3(point.x - size, point.y, point.z), new Vector3(point.x + size, point.y, point.z), color, duration);
        Debug.DrawLine(new Vector3(point.x, point.y - size, point.z), new Vector3(point.x, point.y + size, point.z), color, duration);
        Debug.DrawLine(new Vector3(point.x, point.y, point.z - size), new Vector3(point.x, point.y, point.z + size), color, duration);
    }
}
