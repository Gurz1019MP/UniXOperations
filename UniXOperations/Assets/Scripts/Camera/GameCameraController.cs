using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.ComponentModel;
using static TransformExtension;
using UniRx;

[RequireComponent(typeof(Camera))]
public class GameCameraController : MonoBehaviour
{
    public LayerMask FPSCullingMask;
    public LayerMask FPSWithArmCullingMask;
    public LayerMask TPSCullingMask;
    public float DefalutFoV;
    public float ZoomFoV;

    [ReadOnly]
    public Camera Camera;
    [ReadOnly]
    public Transform Anchor;
    [ReadOnly]
    public Transform LookTarget;
    [ReadOnly]
    public bool isTps;
    [ReadOnly]
    public bool isShowArm = false;

    public CharacterState Character { get; private set; }

    private GameObject _diedCamera;

    private void Start()
    {
        Camera = GetComponent<Camera>();
        _diedCamera = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabDiedCamera);
    }

    private void Update()
    {
        if (Input.GetButtonDown("ToggleFPS/TPS"))
        {
            isTps = !isTps;
            ChangeAnchor();
            ChangeCameraCullingMask();
        };

        if (Input.GetButtonDown("ToggleShowArm"))
        {
            isShowArm = !isShowArm;
            ChangeCameraCullingMask();
        };

        gameObject.transform.LookAt(LookTarget);
    }

    /// <summary>
    /// キャラクターを切り替える
    /// </summary>
    public void ChangeCharacter(CharacterState newCharacter)
    {
        if (newCharacter == null) return;
        Character = newCharacter;

        Character.OnChangeZoom.Subscribe(sender => CharacterState_OnChangeZoom(sender.Item1, sender.Item2)).AddTo(Character);
        Character.OnDied.Subscribe(OnDiedHandler).AddTo(Character);

        // 注視点とカメラアンカーを変更
        LookTarget = Character.Target;
        ChangeAnchor();

        // 胴体のレイヤを変更
        Transform[] bodyTransforms = new Transform[] { Character.UpBase, Character.LegBase };
        foreach (var bodyTransform in bodyTransforms)
        {
            foreach (var childTransforms in bodyTransform.GetDescendantsWithParent())
            {
                childTransforms.gameObject.layer = _bodyLayer;
            }
        }

        // 腕と武器のレイヤを変更
        foreach (var childTransforms in Character.ArmBase.GetDescendantsWithParent())
        {
            childTransforms.gameObject.layer = _armLayer;
        }
    }

    /// <summary>
    /// アンカーを切り替える
    /// </summary>
    private void ChangeAnchor()
    {
        if (Character == null) return;

        Anchor = isTps ? Character.TpsCameraAnchor : Character.FpsCameraAnchor;
        transform.SetParent(Anchor, false);

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ChangeCameraCullingMask()
    {
        if (isTps)
        {
            Camera.cullingMask = TPSCullingMask;
        }
        else
        {
            if (isShowArm)
            {
                Camera.cullingMask = FPSWithArmCullingMask;
            }
            else
            {
                Camera.cullingMask = FPSCullingMask;
            }
        }
    }

    private void OnDiedHandler(CharacterState sender)
    {
        Instantiate(_diedCamera, transform.position, transform.rotation);
    }

    private void CharacterState_OnChangeZoom(bool isZoom, WeaponSpec.ZoomModeEnum zoomMode)
    {
        if (zoomMode != WeaponSpec.ZoomModeEnum.Detail) return;

        Camera.fieldOfView = isZoom ? ZoomFoV : DefalutFoV;
    }

    private static int _bodyLayer = 11;
    private static int _armLayer = 12;
}
