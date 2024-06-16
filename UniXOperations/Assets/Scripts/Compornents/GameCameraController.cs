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

    public ReactiveProperty<CharacterContainer> CharacterContainer { get; private set; } = new ReactiveProperty<CharacterContainer>();
    public InputSystem PlayerInputter { get; set; }

    private GameObject _diedCamera;
    private IDisposable _onChangeZoomSubscriber;
    private IDisposable _onDiedSubscriber;

    private void Start()
    {
        Camera = GetComponent<Camera>();
        _diedCamera = AssetLoader.LoadAsset<GameObject>(ConstantsManager.PrefabDiedCamera);

        if (PlayerPrefs.HasKey(ConstantsManager.FovKey))
        {
            try
            {
                Camera.fieldOfView = PlayerPrefs.GetFloat(ConstantsManager.FovKey);
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
    public void ChangeCharacter(CharacterContainer newCharacter)
    {
        if (newCharacter == null) return;

        // 変更前のキャラクターの設定
        if (CharacterContainer.Value != null)
        {
            _onChangeZoomSubscriber.Dispose();
            _onChangeZoomSubscriber = null;
            _onDiedSubscriber.Dispose();
            _onDiedSubscriber = null;

            // 胴体と腕と武器のレイヤを変更
            foreach (var bodyTransform in new Transform[] { CharacterContainer.Value.UpBase, CharacterContainer.Value.LegBase, CharacterContainer.Value.ArmBase })
            {
                foreach (var childTransforms in bodyTransform.GetDescendantsWithParent())
                {
                    childTransforms.gameObject.layer = ConstantsManager.LayerCharacter;
                }
            }
        }

        // 変更後のキャラクターの設定
        CharacterContainer.Value = newCharacter;

        _onChangeZoomSubscriber = CharacterContainer.Value.Character.OnChangeZoom.Subscribe(sender => CharacterState_OnChangeZoom()).AddTo(CharacterContainer.Value);
        _onDiedSubscriber = CharacterContainer.Value.Character.OnDied.Subscribe(OnDiedHandler).AddTo(CharacterContainer.Value);

        // 注視点とカメラアンカーを変更
        LookTarget = CharacterContainer.Value.Target;
        ChangeAnchor();

        // 胴体のレイヤを変更
        foreach (var bodyTransform in new Transform[] { CharacterContainer.Value.UpBase, CharacterContainer.Value.LegBase })
        {
            foreach (var childTransforms in bodyTransform.GetDescendantsWithParent())
            {
                childTransforms.gameObject.layer = ConstantsManager.LayerBody;
            }
        }

        // 腕と武器のレイヤを変更
        foreach (var childTransforms in CharacterContainer.Value.ArmBase.GetDescendantsWithParent())
        {
            childTransforms.gameObject.layer = ConstantsManager.LayerArm;
        }
    }

    public void SetPlayerInputter(InputSystem inputter)
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
        if (CharacterContainer == null) return;

        Anchor = isTps ? CharacterContainer.Value.TpsCameraAnchor : CharacterContainer.Value.FpsCameraAnchor;
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

    private void CharacterState_OnChangeZoom()
    {
        bool isZoom = CharacterContainer.Value.Character.State.IsZoom;
        WeaponSpec.ZoomModeEnum zoomMode = CharacterContainer.Value.Character.State.CurrentWeapon.Spec.Scope;

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
}
