using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class UIUpdater : MonoBehaviour
{
    public GameCameraController GameCameraController;
    public UnityEngine.UI.Text CurrentWeaponName;
    public UnityEngine.UI.Text DisableWeaponName;
    public UnityEngine.UI.Text Ammo;
    public UnityEngine.UI.Text MagazineAmmo;
    public UnityEngine.UI.Text HP;
    public UnityEngine.UI.Text Overlay;
    public RectTransform ReticleLeft;
    public RectTransform ReticleRight;
    public RectTransform ReticleTop;
    public RectTransform ReticleBottom;
    public GameObject NormalSight;
    public GameObject ScopeSight;
    public UnityEngine.UI.Image HitEffect;
    public Gradient HealthGradient;
    public UnityEngine.UI.Image StatePanel;
    public GameObject WeaponPreviewContainer;

    private CharacterState CharacterState;
    private WeaponSystem WeaponSystem;
    private GameObject WeaponPreviewModel;
    private IDisposable _onChangeZoomSubscriber;
    private IDisposable _onDamagedSubscriber;
    private IDisposable _onWeaponVisualUpdatedSubscriber;

    void Start()
    {
        GameCameraController.Character.Subscribe(newCharacter => ChangeCharacter(newCharacter)).AddTo(GameCameraController);
    }

    void Update()
    {
        if (CharacterState != null && WeaponSystem != null)
        {
            CurrentWeaponName.text = WeaponSpec.GetSpec((CharacterState.IsWeapon1 ? CharacterState.Weapon1 : CharacterState.Weapon2).Kind).WeaponName;
            DisableWeaponName.text = WeaponSpec.GetSpec((CharacterState.IsWeapon1 ? CharacterState.Weapon2 : CharacterState.Weapon1).Kind).WeaponName;

            Ammo.text = CharacterState.CurrentWeaponState.Ammo.ToString();
            MagazineAmmo.text = CharacterState.CurrentWeaponState.Magazine.ToString();

            if (CharacterState.HitPoint > 90) { HP.text = "健康"; }
            else if (CharacterState.HitPoint > 50) { HP.text = "軽傷"; }
            else if (CharacterState.HitPoint > 0) { HP.text = "重症"; }
            else { HP.text = "死亡"; }

            StatePanel.color = HealthGradient.Evaluate(1 - Mathf.Clamp(CharacterState.HitPoint / 100, 0, 1));

            if (CharacterState.IsSwitching) { Overlay.text = "Switching..."; }
            else if (WeaponSystem.IsReloading) { Overlay.text = "Reloading..."; }
            else { Overlay.text = string.Empty; }

            var fpsCameraTransform = CharacterState.FpsCameraAnchor;
            var frontPoint = RectTransformUtility.WorldToScreenPoint(
                GameCameraController.Camera,
                fpsCameraTransform.position + fpsCameraTransform.forward);
            var errorPoint = RectTransformUtility.WorldToScreenPoint(
                GameCameraController.Camera,
                fpsCameraTransform.position + Quaternion.AngleAxis(WeaponSystem.MaxShootingError, fpsCameraTransform.up) * fpsCameraTransform.forward);
            var reticle = (errorPoint - frontPoint).x + 25;

            ReticleLeft.localPosition = new Vector3(-reticle, 0, 0);
            ReticleRight.localPosition = new Vector3(reticle, 0, 0);
            ReticleTop.localPosition = new Vector3(0, reticle, 0);
            ReticleBottom.localPosition = new Vector3(0, -reticle, 0);
        }

        if (HitEffect.color.a > 0)
        {
            HitEffect.color = new Color(
                HitEffect.color.r,
                HitEffect.color.g,
                HitEffect.color.b,
                Mathf.Clamp(HitEffect.color.a - 1f * Time.deltaTime, 0, 1));
        }
    }

    private void ChangeCharacter(CharacterState newCharacter)
    {
        _onChangeZoomSubscriber?.Dispose();
        _onChangeZoomSubscriber = null;
        _onDamagedSubscriber?.Dispose();
        _onDamagedSubscriber = null;
        _onWeaponVisualUpdatedSubscriber?.Dispose();
        _onWeaponVisualUpdatedSubscriber = null;

        CharacterState = newCharacter;
        if (CharacterState != null)
        {
            Debug.Log($"ChangeCharacter ID:{CharacterState.ID}");

            WeaponSystem = CharacterState.GetComponent<WeaponSystem>();

            _onChangeZoomSubscriber = CharacterState.OnChangeZoom.Subscribe(sender => CharacterState_OnChangeZoom(sender.Item1, sender.Item2)).AddTo(CharacterState);
            _onDamagedSubscriber = CharacterState.OnDamaged.Subscribe(CharacterState_OnDamage).AddTo(CharacterState.gameObject);
            _onWeaponVisualUpdatedSubscriber = CharacterState.OnWeaponVisualUpdated.Subscribe(CharacterState_OnWeaponVisualUpdated).AddTo(CharacterState.gameObject);
            CharacterState_OnWeaponVisualUpdated(Unit.Default);
        }
    }

    private void CharacterState_OnChangeZoom(bool isZoom, WeaponSpec.ZoomModeEnum zoomMode)
    {
        NormalSight.SetActive(!isZoom);
        ScopeSight.SetActive(isZoom);
    }

    private void CharacterState_OnDamage(Vector3 _)
    {
        HitEffect.color = new Color(
            HitEffect.color.r,
            HitEffect.color.g,
            HitEffect.color.b,
            1);
    }

    private void CharacterState_OnWeaponVisualUpdated(Unit _)
    {
        Destroy(WeaponPreviewModel);

        if (string.IsNullOrEmpty(CharacterState.CurrentWeaponState.Spec.ModelName)) return;
        if (CharacterState.CurrentWeaponState.Spec.ModelName == "null") return;

        var model = AssetLoader.LoadAsset<GameObject>(ConstantsManager.GetResoucePathWeapon(CharacterState.CurrentWeaponState.Spec.ModelName));
        if (model == null)
        {
            Debug.Log($"ピックアップに使用する武器モデルが見つかりませんでした({CharacterState.CurrentWeaponState.Spec.ModelName})");
        }
        else
        {
            WeaponPreviewModel = Instantiate(model, WeaponPreviewContainer.transform);
            WeaponPreviewModel.layer = WeaponPreviewContainer.layer;
        }
    }
}
