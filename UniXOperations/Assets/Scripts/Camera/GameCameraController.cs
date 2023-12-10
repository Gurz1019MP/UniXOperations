using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.ComponentModel;
using static TransformExtension;
using UniRx;
using System;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(Camera))]
public class GameCameraController : MonoBehaviour
{
    public LayerMask FPSCullingMask;
    public LayerMask FPSWithArmCullingMask;
    public LayerMask TPSCullingMask;
    public float DefaultFoV;
    public float DetailFoV;
    public float LowFoV;

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

    public ReactiveProperty<Character> Character { get; private set; } = new ReactiveProperty<Character>();
    public PlayerInputter2 PlayerInputter { get; set; }

    private GameObject _diedCamera;
    private IDisposable _onChangeZoomSubscriber;
    private IDisposable _onDiedSubscriber;

    private void Start()
    {
        Camera = GetComponent<Camera>();
        _diedCamera = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabDiedCamera);

        if (PlayerPrefs.HasKey(_fovKey))
        {
            try
            {
                Camera.fieldOfView = PlayerPrefs.GetFloat(_fovKey);
            }
            catch
            {
                Debug.Log("Settings Load Error (FoV)");
            }
        }
    }

    private void Update()
    {
        gameObject.transform.LookAt(LookTarget);
    }

    /// <summary>
    /// キャラクターを切り替える
    /// </summary>
    public void ChangeCharacter(Character newCharacter)
    {
        if (newCharacter == null) return;

        // 変更前のキャラクターの設定
        if (Character.Value != null)
        {
            _onChangeZoomSubscriber.Dispose();
            _onChangeZoomSubscriber = null;
            _onDiedSubscriber.Dispose();
            _onDiedSubscriber = null;

            // 胴体と腕と武器のレイヤを変更
            foreach (var bodyTransform in new Transform[] { Character.Value.UpBase, Character.Value.LegBase, Character.Value.ArmBase })
            {
                foreach (var childTransforms in bodyTransform.GetDescendantsWithParent())
                {
                    childTransforms.gameObject.layer = _characterLayer;
                }
            }
        }

        // 変更後のキャラクターの設定
        Character.Value = newCharacter;

        _onChangeZoomSubscriber = Character.Value.OnChangeZoom.Subscribe(sender => CharacterState_OnChangeZoom(sender.Item1, sender.Item2)).AddTo(Character.Value);
        _onDiedSubscriber = Character.Value.OnDied.Subscribe(OnDiedHandler).AddTo(Character.Value);

        // 注視点とカメラアンカーを変更
        LookTarget = Character.Value.Target;
        ChangeAnchor();

        // 胴体のレイヤを変更
        foreach (var bodyTransform in new Transform[] { Character.Value.UpBase, Character.Value.LegBase })
        {
            foreach (var childTransforms in bodyTransform.GetDescendantsWithParent())
            {
                childTransforms.gameObject.layer = _bodyLayer;
            }
        }

        // 腕と武器のレイヤを変更
        foreach (var childTransforms in Character.Value.ArmBase.GetDescendantsWithParent())
        {
            childTransforms.gameObject.layer = _armLayer;
        }
    }

    public void SetPlayerInputter(PlayerInputter2 inputter)
    {
        PlayerInputter = inputter;

        if (PlayerInputter != null)
        {
            PlayerInputter.Disable();

            PlayerInputter.Player.ToggleFPSTPS.performed += (_) =>
            {
                isTps = !isTps;
                ChangeAnchor();
                ChangeCameraCullingMask();
            };
            PlayerInputter.Player.ToggleShowArm.performed += (_) =>
            {
                isShowArm = !isShowArm;
                ChangeCameraCullingMask();
            };

            PlayerInputter.Enable();
        }

    }

    /// <summary>
    /// アンカーを切り替える
    /// </summary>
    private void ChangeAnchor()
    {
        if (Character == null) return;

        Anchor = isTps ? Character.Value.TpsCameraAnchor : Character.Value.FpsCameraAnchor;
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

    private void OnDiedHandler(Character sender)
    {
        Instantiate(_diedCamera, transform.position, transform.rotation);
    }

    private void CharacterState_OnChangeZoom(bool isZoom, WeaponSpec.ZoomModeEnum zoomMode)
    {
        if (isZoom)
        {
            if (zoomMode == WeaponSpec.ZoomModeEnum.Detail)
            {
                Camera.fieldOfView = DetailFoV;
            }
            else if (zoomMode == WeaponSpec.ZoomModeEnum.Low)
            {
                Camera.fieldOfView = LowFoV;
            }
            else
            {
                Camera.fieldOfView = DefaultFoV;
            }
        }
        else
        {
            Camera.fieldOfView = DefaultFoV;
        }
    }

    private static int _bodyLayer = 11;
    private static int _armLayer = 12;
    private static int _characterLayer = 8;
    private static readonly string _fovKey = "FoV";
}
