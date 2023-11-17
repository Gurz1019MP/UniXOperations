using UnityEditor;
using UnityEngine;

public class PointVisualizer : MonoBehaviour
{
    public TextMesh TextMesh;
    public GameObject pointText;

    private Camera _targetCamera;

    private PointData _pointData;

    public PointData PointData
    {
        get { return _pointData; }
        set
        {
            _pointData = value;
            UpdatePointMesh();
        }
    }


    private void Start()
    {
#if UNITY_EDITOR
        _targetCamera = SceneView.lastActiveSceneView.camera;
#endif
    }

    private void Update()
    {
        if (_targetCamera != null)
        {
            pointText.transform.LookAt(_targetCamera.transform);
        }
    }

    private void UpdatePointMesh()
    {
        if (TextMesh == null) return;
        if (PointData == null) return;

        TextMesh.text = PointData.ToString();
    }
}
