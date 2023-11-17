using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

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

    private CharacterState CharacterState;
    private WeaponSystem WeaponSystem;

    //void Start()
    //{
    //    CharacterState = GameCameraController.Character;
    //    WeaponSystem = GameCameraController.Character.GetComponent<WeaponSystem>();

    //    CharacterState.OnChangeZoom.Subscribe(sender => CharacterState_OnChangeZoom(sender.Item1, sender.Item2)).AddTo(CharacterState);
    //    CharacterState.OnDamaged.Subscribe(CharacterState_OnDamage).AddTo(CharacterState);
    //}

    void Update()
    {
        if (GameCameraController != null)
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
            else
            {
                CharacterState = GameCameraController.Character;
                WeaponSystem = GameCameraController.Character.GetComponent<WeaponSystem>();

                CharacterState.OnChangeZoom.Subscribe(sender => CharacterState_OnChangeZoom(sender.Item1, sender.Item2)).AddTo(CharacterState);
                CharacterState.OnDamaged.Subscribe(CharacterState_OnDamage).AddTo(CharacterState.gameObject);
            }
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
}
